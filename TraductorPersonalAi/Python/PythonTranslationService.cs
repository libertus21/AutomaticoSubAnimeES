using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraductorPersonalAi.Python
{
    //public class PythonTranslationService
    //{
    //    private readonly string _pythonInterpreter;
    //    private readonly string _pythonScript;

    //    public PythonTranslationService(string pythonInterpreter, string pythonScript)
    //    {
    //        _pythonInterpreter = pythonInterpreter ?? throw new ArgumentNullException(nameof(pythonInterpreter));
    //        _pythonScript = pythonScript ?? throw new ArgumentNullException(nameof(pythonScript));
    //    }
    //    public async Task<List<string>> TranslateAsync(List<string> texts)
    //    {
    //        ValidatePaths();
    //        string inputText = string.Join("|||", texts);

    //        using (var process = new Process())
    //        {
    //            ConfigureProcess(process, inputText);
    //            process.Start();

    //            // Leer la salida de forma asincrónica
    //            string output = await process.StandardOutput.ReadToEndAsync();

    //            // Esperar la finalización del proceso de forma asincrónica
    //            await Task.Run(() => process.WaitForExit());

    //            ValidateExitCode(process);

    //            return SplitOutput(output);
    //        }
    //    }



    //    private void ValidatePaths()
    //    {
    //        if (!File.Exists(_pythonInterpreter))
    //            throw new FileNotFoundException("Intérprete Python no encontrado", _pythonInterpreter);

    //        if (!File.Exists(_pythonScript))
    //            throw new FileNotFoundException("Script Python no encontrado", _pythonScript);
    //    }

    //    private void ConfigureProcess(Process process, string inputText)
    //    {
    //        process.StartInfo = new ProcessStartInfo
    //        {
    //            FileName = _pythonInterpreter,
    //            Arguments = $"\"{_pythonScript}\" \"{inputText}\"",
    //            RedirectStandardOutput = true,
    //            UseShellExecute = false,
    //            CreateNoWindow = true,
    //            StandardOutputEncoding = Encoding.UTF8
    //        };
    //    }

    //    private void ValidateExitCode(Process process)
    //    {
    //        if (process.ExitCode != 0)
    //            throw new InvalidOperationException($"Error en ejecución Python (Código: {process.ExitCode})");
    //    }

    //    private List<string> SplitOutput(string output)
    //    {
    //        return output.Split(new[] { "|||" }, StringSplitOptions.RemoveEmptyEntries)
    //                     .Select(t => t.Trim())
    //                     .ToList();
    //    }
    //}
    public class PythonTranslationService
    {
        private readonly string _pythonInterpreter;
        private readonly string _pythonScript;

        public PythonTranslationService(string pythonInterpreter, string pythonScript)
        {
            _pythonInterpreter = pythonInterpreter;
            _pythonScript = pythonScript;
        }

        public async Task<List<string>> TranslateAsync(List<string> texts)
        {
            ValidatePaths();

            string inputText = string.Join("|||", texts);

            using (var process = new Process())
            {
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                process.StartInfo = CreateProcessInfo(inputText);
                process.EnableRaisingEvents = true;

                // Configurar manejadores de salida
                process.OutputDataReceived += (sender, e) => {
                    if (!string.IsNullOrEmpty(e.Data)) outputBuilder.AppendLine(e.Data);
                };

                process.ErrorDataReceived += (sender, e) => {
                    if (!string.IsNullOrEmpty(e.Data)) errorBuilder.AppendLine(e.Data);
                };

                process.Start();

                // Lectura asincrónica de streams
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Espera con timeout de 5 minutos
                await Task.Run(() => {
                    if (!process.WaitForExit(300000)) // 300,000 ms = 5 minutos
                    {
                        throw new TimeoutException("Tiempo excedido en ejecución de Python");
                    }
                });

                // Verificar errores
                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException(
                        $"Error Python (Código {process.ExitCode}):\n{errorBuilder.ToString()}");
                }

                // Forzar encoding UTF-8 y limpiar salida
                return CleanOutput(outputBuilder.ToString());
            }
        }

        private ProcessStartInfo CreateProcessInfo(string inputText)
        {
            return new ProcessStartInfo
            {
                FileName = _pythonInterpreter,
                Arguments = $"\"{_pythonScript}\" \"{inputText}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                Environment = { ["PYTHONUTF8"] = "1" } // Forzar UTF-8 en Python
            };
        }

        private List<string> CleanOutput(string output)
        {
            return output.Split(new[] { "|||" }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim().Normalize(NormalizationForm.FormC))
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .ToList();
        }

        private void ValidatePaths()
        {
            if (!File.Exists(_pythonInterpreter))
                throw new FileNotFoundException("Intérprete Python no encontrado", _pythonInterpreter);

            if (!File.Exists(_pythonScript))
                throw new FileNotFoundException("Script Python no encontrado", _pythonScript);
        }
    }
}
