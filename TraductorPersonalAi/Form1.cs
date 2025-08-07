using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestSharp;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using UglyToad.PdfPig; // Nuevo para leer PDFs
using iTextSharp.text; // Para escribir PDFs
using iTextSharp.text.pdf;
using UglyToad.PdfPig.Content;
using TraductorPersonalAi.Traduccion.Ass;
using TraductorPersonalAi.Traduccion.PDF;
using TraductorPersonalAi.Traduccion.SRT;
using TraductorPersonalAi.Python;
namespace TraductorPersonalAi
{
    public partial class Form1 : Form
    {
        private const string HuggingFaceAPIUrl = "https://api-inference.huggingface.co/models/Helsinki-NLP/opus-mt-en-es";
        private const string ApiKey = "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";  // Tu token de Hugging Face
        private string outputFilePath; // ← Añadir esta línea

        private AssTranslator _assTranslator;
        private PdfTranslator _pdfTranslator;
        private SrtTranslator _srtTranslator;
        private readonly PythonTranslationService _pythonService;

        public Form1()
        {
            InitializeComponent();
            _pythonService = InitializePythonService(); // Asignación directa
            InitializeTranslators();
        }

        private PythonTranslationService InitializePythonService()
        {
            const string pythonInterpreter = @"C:\Users\Thecnomax\AppData\Local\Programs\Python\Python312\python.exe";
            const string pythonScript = @"D:\Programacion\Visual Studio\Modelo_AI\ModeloRapido.py";

            return new PythonTranslationService(pythonInterpreter, pythonScript);
        }


        private void InitializeTranslators()
        {
            _assTranslator = new AssTranslator(
     _pythonService.TranslateAsync,
     progress => progressBar.Value = progress,
     content => txtOutput.Text = content
 );

            _pdfTranslator = new PdfTranslator(
                _pythonService.TranslateAsync,
                progress => progressBar.Value = progress
            );

            _srtTranslator = new SrtTranslator(
                _pythonService.TranslateAsync,
                progress => progressBar.Value = progress,
                content => txtOutput.Text = content
            );


            //version con traduciones exacta
           // _assTranslator = new AssTranslator(
           //    TranslateTextAsync,
           //    progress => progressBar.Value = progress,
           //    content => txtOutput.Text = content
           //);

           // _pdfTranslator = new PdfTranslator(
           //     TranslateTextAsync,
           //     progress => progressBar.Value = progress
           // );
        }
        private async Task<List<string>> TranslateTextAsync(List<string> texts)
        {
            string pythonScript = @"D:\Programacion\Visual Studio\Modelo_AI\ModeloRapido.py";
            string pythonInterpreter = @"C:\Users\Thecnomax\AppData\Local\Programs\Python\Python312\python.exe";

            // Combinar textos con separador
            string inputText = string.Join("|||", texts);

            // Configurar el proceso de Python
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = pythonInterpreter,
                Arguments = $"\"{pythonScript}\" \"{inputText}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process { StartInfo = startInfo };
            process.Start();

            // Leer y dividir la salida
            string output = await process.StandardOutput.ReadToEndAsync();
            process.WaitForExit();

            return output.Split(new string[] { "|||" }, StringSplitOptions.None).ToList();
        }
        private void ShowNotification(string message)
        {
            // Crear un objeto de notificación en la bandeja del sistema
            NotifyIcon notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Information, // Puedes cambiar el ícono si lo deseas
                Visible = true,
                BalloonTipTitle = "Traducción",
                BalloonTipText = message,
                BalloonTipIcon = ToolTipIcon.Info
            };

            // Mostrar la notificación
            notifyIcon.ShowBalloonTip(3000); // 3000ms = 3 segundos

            // Asegurarse de ocultar el icono después de mostrar la notificación
            notifyIcon.Dispose();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (radioSingleFile.Checked)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                
                if (radioAss.Checked)
                    saveFileDialog.Filter = "Archivos de subtítulos (*.ass)|*.ass";
                else if (radioSrt.Checked)
                    saveFileDialog.Filter = "Archivos de subtítulos (*.srt)|*.srt";
                else
                    saveFileDialog.Filter = "Archivos PDF (*.pdf)|*.pdf";
                    
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Guardar el contenido de txtOutput (que ahora contiene el archivo traducido)
                    File.WriteAllText(saveFileDialog.FileName, txtOutput.Text);
                    MessageBox.Show("Archivo guardado exitosamente.");
                }
            }
            else if (radioFolder.Checked)
            {
                MessageBox.Show("Los archivos traducidos se guardan automáticamente en la misma carpeta con el sufijo '_traducido'.");
            }
        }

      

        private async void btnBrowse_Click_1(object sender, EventArgs e)
        {
            if (radioSingleFile.Checked)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();

                if (radioAss.Checked)
                    openFileDialog.Filter = "Archivos de subtítulos (*.ass)|*.ass";
                else if (radioSrt.Checked)
                    openFileDialog.Filter = "Archivos de subtítulos (*.srt)|*.srt";
                else
                    openFileDialog.Filter = "Archivos PDF (*.pdf)|*.pdf";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = openFileDialog.FileName;
                }
            }
            else if (radioFolder.Checked)
            {
                FolderBrowserDialog folderDialog = new FolderBrowserDialog();
                folderDialog.Description = "Selecciona la carpeta con los archivos a traducir";

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = folderDialog.SelectedPath;
                }
            }
        }

        private async void btnTranslate_Click_1(object sender, EventArgs e)
        {
            btnTranslate.Enabled = false;
            string inputPath = txtFilePath.Text;
            progressBar.Value = 0;

            //Verificar cuanto tiempo dura la ejeccucion codigo
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //
            try
            {
                if (string.IsNullOrWhiteSpace(inputPath))
                {
                    MessageBox.Show("Por favor, selecciona un archivo o carpeta primero.");
                    return;
                }

                if (radioSingleFile.Checked)
                {
                    await TranslateSingleFile(inputPath);
                }
                else if (radioFolder.Checked)
                {
                    await TranslateFolder(inputPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                btnTranslate.Enabled = true;
                stopwatch.Stop();
                MessageBox.Show($"Tiempo Ejeccucion: {stopwatch.Elapsed:mm\\:ss} minutos");
            }
        }

        private async Task TranslateSingleFile(string inputFilePath)
        {
            outputFilePath = GetOutputPath(inputFilePath);

            if (radioAss.Checked)
            {
                await _assTranslator.TranslateAsync(inputFilePath, outputFilePath);
                ShowNotification("Traducción ASS completada!");
            }
            else if (radioSrt.Checked)
            {
                await _srtTranslator.TranslateAsync(inputFilePath, outputFilePath);
                ShowNotification("Traducción SRT completada!");
            }
            else
            {
                await _pdfTranslator.TranslateAsync(inputFilePath, outputFilePath);
                ShowNotification("Traducción PDF completada!");
            }
        }

        private async Task TranslateFolder(string folderPath)
        {
            string[] files = GetFilesToTranslate(folderPath);
            
            if (files.Length == 0)
            {
                MessageBox.Show("No se encontraron archivos para traducir en la carpeta seleccionada.");
                return;
            }

            int totalFiles = files.Length;
            int processedFiles = 0;

            foreach (string file in files)
            {
                try
                {
                    string outputPath = GetOutputPath(file);
                    
                    if (radioAss.Checked)
                    {
                        await _assTranslator.TranslateAsync(file, outputPath);
                    }
                    else if (radioSrt.Checked)
                    {
                        await _srtTranslator.TranslateAsync(file, outputPath);
                    }
                    else
                    {
                        await _pdfTranslator.TranslateAsync(file, outputPath);
                    }

                    processedFiles++;
                    progressBar.Value = (int)((processedFiles / (float)totalFiles) * 100);
                    
                    // Actualizar el texto de salida con el progreso
                    txtOutput.Text = $"Procesando archivo {processedFiles} de {totalFiles}: {Path.GetFileName(file)}";
                    Application.DoEvents();
                }
                catch (Exception ex)
                {
                    txtOutput.AppendText($"\nError procesando {Path.GetFileName(file)}: {ex.Message}");
                }
            }

            ShowNotification($"Procesamiento completado! {processedFiles} archivos traducidos.");
            txtOutput.Text = $"Procesamiento completado exitosamente.\n{processedFiles} archivos traducidos de {totalFiles} encontrados.";
        }

        private string[] GetFilesToTranslate(string folderPath)
        {
            List<string> files = new List<string>();

            if (radioAss.Checked)
            {
                files.AddRange(Directory.GetFiles(folderPath, "*.ass", SearchOption.TopDirectoryOnly));
            }
            else if (radioSrt.Checked)
            {
                files.AddRange(Directory.GetFiles(folderPath, "*.srt", SearchOption.TopDirectoryOnly));
            }
            else
            {
                files.AddRange(Directory.GetFiles(folderPath, "*.pdf", SearchOption.TopDirectoryOnly));
            }

            return files.ToArray();
        }
        private string GetOutputPath(string inputPath)
        {
            string extension;
            if (radioAss.Checked)
                extension = ".ass";
            else if (radioSrt.Checked)
                extension = ".srt";
            else
                extension = ".pdf";
                
            return Path.Combine(
                Path.GetDirectoryName(inputPath),
                $"{Path.GetFileNameWithoutExtension(inputPath)}_traducido{extension}"
            );
        }

    }
}