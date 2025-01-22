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
            openFileDialog.Filter = "Archivos PDF (*.pdf)|*.pdf"; // Cambiar filtro
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = openFileDialog.FileName;
            }
        }

        private async void btnTranslate_Click(object sender, EventArgs e)
        {
            btnTranslate.Enabled = false;
            string inputFilePath = txtFilePath.Text;

            if (string.IsNullOrWhiteSpace(inputFilePath))
            {
                MessageBox.Show("Por favor, selecciona un archivo primero.");
                btnTranslate.Enabled = true;
                return;
            }

            string outputFilePath = Path.Combine(
                Path.GetDirectoryName(inputFilePath),
                $"{Path.GetFileNameWithoutExtension(inputFilePath)}_traducido.pdf"
            );

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                // Leer PDF usando PdfPig
                var translatedParagraphs = new List<string>();
                using (var pdf = UglyToad.PdfPig.PdfDocument.Open(inputFilePath))
                {
                    int totalPages = pdf.NumberOfPages;
                    int currentPage = 0;

                    foreach (var page in pdf.GetPages())
                    {
                        currentPage++;
                        var text = page.Text;

                        // Dividir en chunks de 500 caracteres para manejar mejor la traducción
                        var chunks = SplitText(text, 500);

                        // Traducir cada chunk
                        var translatedChunks = await TranslateTextAsync(chunks);

                        // Unir los chunks traducidos
                        translatedParagraphs.Add(string.Join(" ", translatedChunks));

                        // Actualizar progreso
                        progressBar.Value = (int)((currentPage / (double)totalPages) * 100);
                    }
                }

                // Crear PDF traducido usando iTextSharp
                CreateTranslatedPdf(outputFilePath, translatedParagraphs);
                
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
        private List<string> SplitText(string text, int chunkSize)
        {
            var chunks = new List<string>();
            for (int i = 0; i < text.Length; i += chunkSize)
            {
                chunks.Add(text.Substring(i, Math.Min(chunkSize, text.Length - i)));
            }
            return chunks;
        }

        private void CreateTranslatedPdf(string outputPath, List<string> paragraphs)
        {
            using (var fs = new FileStream(outputPath, FileMode.Create))
            {
                var document = new Document();
                var writer = PdfWriter.GetInstance(document, fs);

                document.Open();

                var font = FontFactory.GetFont(FontFactory.HELVETICA, 12);

                foreach (var paragraph in paragraphs)
                {
                    document.Add(new Paragraph(paragraph, font));
                    document.Add(Chunk.NEWLINE);
                }

                document.Close();
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