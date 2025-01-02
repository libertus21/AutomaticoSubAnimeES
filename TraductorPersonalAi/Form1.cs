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
                string[] lines = File.ReadAllLines(inputFilePath);
                StringBuilder translatedContent = new StringBuilder();

                // Procesar el archivo en bloques de 45 líneas
                const int batchSize = 80; // Cambiar a 45 según se requiera
                for (int i = 0; i < lines.Length; i += batchSize)
                {
                    var block = lines.Skip(i).Take(batchSize).ToArray();

                    // Crear una lista de textos para traducir
                    List<string> textsToTranslate = new List<string>();
                    foreach (var line in block)
                    {
                        if (!string.IsNullOrWhiteSpace(line) && line.Contains("0000,0000,0000,,"))
                        {
                            // Validar que la división contiene un texto después del separador
                            var splitParts = line.Split(new string[] { "0000,0000,0000,," }, StringSplitOptions.None);
                            if (splitParts.Length > 1 && !string.IsNullOrWhiteSpace(splitParts[1]))
                            {
                                string textToTranslate = splitParts[1];
                                textsToTranslate.Add(textToTranslate);
                            }
                            else
                            {
                                // Si no hay texto después del separador, copiar línea original
                                translatedContent.AppendLine(line);
                            }
                        }
                        else
                        {
                            // Copiar líneas que no necesitan traducción
                            translatedContent.AppendLine(line);
                        }
                    }

                    // Traducir los textos en bloque
                    if (textsToTranslate.Any())
                    {
                        var translatedTexts = await TranslateTextAsync(textsToTranslate);

                        int translationIndex = 0;
                        foreach (var line in block)
                        {
                            if (line.Contains("0000,0000,0000,,"))
                            {
                                var splitParts = line.Split(new string[] { "0000,0000,0000,," }, StringSplitOptions.None);
                                if (splitParts.Length > 1 && !string.IsNullOrWhiteSpace(splitParts[1]))
                                {
                                    if (translationIndex < translatedTexts.Count)
                                    {
                                        string textToTranslate = splitParts[1];
                                        translatedContent.AppendLine(line.Replace(textToTranslate, translatedTexts[translationIndex++]));
                                    }
                                    else
                                    {
                                        // Manejar caso donde no hay traducción suficiente
                                        translatedContent.AppendLine(line);
                                    }
                                }
                                else
                                {
                                    translatedContent.AppendLine(line);
                                }
                            }
                            else
                            {
                                translatedContent.AppendLine(line);
                            }
                        }
                    }

                    // Actualizar progreso
                    int progress = (int)(((i + batchSize) / (float)lines.Length) * 100);
                    progressBar.Value = Math.Min(progress, 100);
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