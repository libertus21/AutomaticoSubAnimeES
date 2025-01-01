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

namespace TraductorPersonalAi
{
    public partial class Form1 : Form
    {
        private const string HuggingFaceAPIUrl = "https://api-inference.huggingface.co/models/Helsinki-NLP/opus-mt-en-es";
        private const string ApiKey = "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";  // Tu token de Hugging Face
        public Form1()
        {
            InitializeComponent();
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
        private void txtFilePath_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Archivos de subtítulos (*.ass)|*.ass";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = openFileDialog.FileName;
            }
        }

        private async void btnTranslate_Click(object sender, EventArgs e)
        {
            string inputFilePath = txtFilePath.Text; // Ruta del archivo .ass
            string outputFilePath = "output_translated.ass"; // Ruta de salida para el archivo traducido

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                // Leer todas las líneas una vez
                string[] lines = File.ReadAllLines(inputFilePath);
                StringBuilder translatedContent = new StringBuilder();

                // Definir el tamaño del lote grande para reducir iteraciones
                const int batchSize = 100; // Lotes más grandes para minimizar llamadas repetitivas
                List<Task<List<string>>> translationTasks = new List<Task<List<string>>>();

                for (int i = 0; i < lines.Length; i += batchSize)
                {
                    // Tomar un bloque de líneas
                    var block = lines.Skip(i).Take(batchSize).ToArray();

                    // Crear una lista de textos para traducir en paralelo
                    List<string> textsToTranslate = new List<string>();
                    Dictionary<int, string> lineMap = new Dictionary<int, string>();

                    for (int j = 0; j < block.Length; j++)
                    {
                        var line = block[j];
                        if (!string.IsNullOrWhiteSpace(line) && line.Contains("0000,0000,0000,,"))
                        {
                            var splitParts = line.Split(new string[] { "0000,0000,0000,," }, StringSplitOptions.None);
                            if (splitParts.Length > 1 && !string.IsNullOrWhiteSpace(splitParts[1]))
                            {
                                textsToTranslate.Add(splitParts[1]);
                                lineMap[j] = splitParts[1];
                            }
                        }
                    }

                    if (textsToTranslate.Any())
                    {
                        // Llamar a la traducción en paralelo
                        translationTasks.Add(TranslateTextAsync(textsToTranslate)
                            .ContinueWith(task =>
                            {
                                var translatedTexts = task.Result;
                                for (int j = 0; j < block.Length; j++)
                                {
                                    if (lineMap.ContainsKey(j))
                                    {
                                        block[j] = block[j].Replace(lineMap[j], translatedTexts[j]);
                                    }
                                }
                                return block.ToList();
                            }));
                    }
                    else
                    {
                        // Si no hay nada que traducir, añadir directamente al contenido traducido
                        translatedContent.AppendLine(string.Join(Environment.NewLine, block));
                    }
                }

                // Esperar todas las traducciones en paralelo
                var allTranslatedBlocks = await Task.WhenAll(translationTasks);

                foreach (var translatedBlock in allTranslatedBlocks)
                {
                    foreach (var line in translatedBlock)
                    {
                        translatedContent.AppendLine(line);
                    }
                }

                // Mostrar el contenido traducido en txtOutput
                txtOutput.Text = translatedContent.ToString();

                // Guardar el archivo traducido
                File.WriteAllText(outputFilePath, translatedContent.ToString());
                MessageBox.Show("Traducción completa!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                stopwatch.Stop();
                TimeSpan elapsed = stopwatch.Elapsed;
                string elapsedTime = string.Format("{0} minutos con {1} segundos", elapsed.Minutes, elapsed.Seconds);
                MessageBox.Show($"Tiempo de ejecución: {elapsedTime}");
            }
        }




        private void progressBar_Click(object sender, EventArgs e)
        {

        }

        private void txtOutput_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivos de subtítulos (*.ass)|*.ass";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Guardar el contenido de txtOutput (que ahora contiene el archivo traducido)
                File.WriteAllText(saveFileDialog.FileName, txtOutput.Text);
                MessageBox.Show("Archivo guardado exitosamente.");
            }
        }

        private void txtOutput_TextChanged_1(object sender, EventArgs e)
        {

        }
    }
}
