using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestSharp;
using Newtonsoft.Json;
using System.Diagnostics;

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
        private async Task<string> TranslateTextAsync(string text)
        {
            // Obtener el texto a traducir desde un cuadro de texto (txtInputText por ejemplo)
            string inputText = text;

            // Ejecutar el script de Python
            string pythonScript = @"D:\Programacion\Visual Studio\Modelo_AI\translate.py"; // Ruta completa al script de Python
            string pythonInterpreter = @"C:\Users\Thecnomax\AppData\Local\Programs\Python\Python312\python.exe"; // Ruta al ejecutable de Python

            // Crear un proceso para ejecutar el script de Python
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = pythonInterpreter,
                Arguments = $"\"{pythonScript}\" \"{inputText}\"", // Pasar el texto de entrada como argumento
                RedirectStandardOutput = true,  // Capturar la salida
                UseShellExecute = false,  // No usar la shell
                CreateNoWindow = true  // No mostrar la ventana de la consola
            };

            Process process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();

            // Leer la salida del proceso
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Mostrar la traducción en un cuadro de texto (txtTranslatedText por ejemplo)
            txtOutput.Text = output;
            return txtOutput.Text;
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

            try
            {
                string[] lines = File.ReadAllLines(inputFilePath);
                StringBuilder translatedContent = new StringBuilder();

                // Procesar el archivo en bloques de 5 líneas
                for (int i = 0; i < lines.Length; i += 1)
                {
                    var block = new string[1];
                    // Copiar hasta 30 líneas, o menos si llegamos al final del archivo
                    Array.Copy(lines, i, block, 0, Math.Min(1, lines.Length - i));

                    foreach (var line in block)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue; // Saltar líneas vacías
                        }

                        if (line.Contains("0000,0000,0000,,")) // Si contiene el marcador
                        {
                            string textToTranslate = line.Split(new string[] { "0000,0000,0000,," }, StringSplitOptions.None)[1];
                            string translatedText = await TranslateTextAsync(textToTranslate);
                            translatedContent.AppendLine(line.Replace(textToTranslate, translatedText));
                        }
                        else
                        {
                            translatedContent.AppendLine(line);
                        }
                    }

                    // Actualizar progreso
                    int progress = (int)((i / (float)lines.Length) * 100);
                    progressBar.Value = progress;
                }

                // Mostrar el contenido traducido en txtOutput
                txtOutput.Text = translatedContent.ToString();

                // Guardar el archivo traducido automáticamente si lo deseas
                File.WriteAllText(outputFilePath, translatedContent.ToString());
                MessageBox.Show("Traducción completa!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
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
