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
            // Desactivar el botón para evitar múltiples ejecuciones
            btnTranslate.Enabled = false;

            string inputFilePath = txtFilePath.Text; // Tomar la ruta desde el TextBox

            if (string.IsNullOrWhiteSpace(inputFilePath))
            {
                MessageBox.Show("Por favor, selecciona un archivo primero.");
                btnTranslate.Enabled = true; // Reactivar el botón
                return;
            }

            // Generar el nombre del archivo de salida con el sufijo "_translated"
            string outputFilePath = Path.Combine(
                Path.GetDirectoryName(inputFilePath),
                $"{Path.GetFileNameWithoutExtension(inputFilePath)}_traducido{Path.GetExtension(inputFilePath)}"
            );

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                string[] lines = File.ReadAllLines(inputFilePath);
                StringBuilder translatedContent = new StringBuilder();

                // Procesar el archivo en bloques de 80 líneas
                const int batchSize = 80;
                for (int i = 0; i < lines.Length; i += batchSize)
                {
                    var block = lines.Skip(i).Take(batchSize).ToArray();

                    // Crear una lista de textos para traducir
                    List<string> textsToTranslate = new List<string>();
                    List<int> linesToTranslateIndices = new List<int>(); // Índices de las líneas que requieren traducción

                    for (int j = 0; j < block.Length; j++)
                    {
                        var line = block[j];
                        if (line.Contains("0000,0000,0000,,"))
                        {
                            var splitParts = line.Split(new string[] { "0000,0000,0000,," }, StringSplitOptions.None);
                            if (splitParts.Length > 1 && !string.IsNullOrWhiteSpace(splitParts[1]))
                            {
                                string textToTranslate = splitParts[1].Trim();
                                textsToTranslate.Add(textToTranslate);
                                linesToTranslateIndices.Add(j); // Guardar el índice de la línea para actualización
                            }
                            else
                            {
                                translatedContent.AppendLine(line); // Si no hay texto, copiar línea original
                            }
                        }
                        else
                        {
                            translatedContent.AppendLine(line); // Copiar líneas que no necesitan traducción
                        }
                    }

                    // Traducir los textos en bloque
                    if (textsToTranslate.Any())
                    {
                        var translatedTexts = await TranslateTextAsync(textsToTranslate);

                        for (int j = 0; j < linesToTranslateIndices.Count; j++)
                        {
                            int lineIndex = linesToTranslateIndices[j];
                            var originalLine = block[lineIndex];
                            var splitParts = originalLine.Split(new string[] { "0000,0000,0000,," }, StringSplitOptions.None);

                            // Sustituir el texto original por la traducción
                            if (j < translatedTexts.Count)
                            {
                                var translatedLine = $"{splitParts[0]}0000,0000,0000,,{translatedTexts[j]}";
                                block[lineIndex] = translatedLine;
                            }
                        }
                    }

                    // Agregar el bloque procesado al contenido traducido
                    foreach (var line in block)
                    {
                        translatedContent.AppendLine(line);
                    }

                    // Actualizar progreso
                    int progress = (int)(((i + batchSize) / (float)lines.Length) * 100);
                    progressBar.Value = Math.Min(progress, 100);
                }

                // Mostrar el contenido traducido en txtOutput
                txtOutput.Text = translatedContent.ToString();

                // Guardar el archivo traducido
                File.WriteAllText(outputFilePath, translatedContent.ToString());
                ShowNotification($"Traducción completa! Archivo guardado en");
                MessageBox.Show($"Traducción completa! Archivo guardado en:\n{outputFilePath}");
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

                // Reactivar el botón después de completar el proceso
                btnTranslate.Enabled = true;
            }
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