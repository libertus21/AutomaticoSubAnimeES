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
namespace TraductorPersonalAi
{
    public partial class Form1 : Form
    {
        private const string HuggingFaceAPIUrl = "https://api-inference.huggingface.co/models/Helsinki-NLP/opus-mt-en-es";
        private const string ApiKey = "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";  // Tu token de Hugging Face
        private string outputFilePath; // ← Añadir esta línea

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
   

        private async Task TranslateAssFile(string inputFilePath)
        {
            outputFilePath = Path.Combine(
                Path.GetDirectoryName(inputFilePath),
                $"{Path.GetFileNameWithoutExtension(inputFilePath)}_traducido.ass"
            );

            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                string[] lines = File.ReadAllLines(inputFilePath);
                StringBuilder translatedContent = new StringBuilder();

                const int batchSize = 80;
                for (int i = 0; i < lines.Length; i += batchSize)
                {
                    var block = lines.Skip(i).Take(batchSize).ToArray();
                    List<string> textsToTranslate = new List<string>();
                    List<int> linesToTranslateIndices = new List<int>();

                    for (int j = 0; j < block.Length; j++)
                    {
                        var line = block[j];
                        if (line.Contains("0000,0000,0000,,"))
                        {
                            var splitParts = line.Split(new[] { "0000,0000,0000,," }, StringSplitOptions.None);
                            if (splitParts.Length > 1 && !string.IsNullOrWhiteSpace(splitParts[1]))
                            {
                                textsToTranslate.Add(splitParts[1].Trim());
                                linesToTranslateIndices.Add(j);
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

                    if (textsToTranslate.Any())
                    {
                        var translatedTexts = await TranslateTextAsync(textsToTranslate);

                        for (int j = 0; j < linesToTranslateIndices.Count; j++)
                        {
                            int lineIndex = linesToTranslateIndices[j];
                            var originalLine = block[lineIndex];
                            var splitParts = originalLine.Split(new[] { "0000,0000,0000,," }, StringSplitOptions.None);

                            if (j < translatedTexts.Count)
                            {
                                block[lineIndex] = $"{splitParts[0]}0000,0000,0000,,{translatedTexts[j]}";
                            }
                        }
                    }

                    foreach (var line in block)
                    {
                        translatedContent.AppendLine(line);
                    }

                    progressBar.Value = Math.Min((int)((i + batchSize) / (float)lines.Length * 100), 100);
                }

                txtOutput.Text = translatedContent.ToString();
                File.WriteAllText(outputFilePath, translatedContent.ToString());
                ShowNotification("Traducción ASS completada!");
            }
            finally
            {
                stopwatch.Stop();
                MessageBox.Show($"Tiempo ASS: {stopwatch.Elapsed:mm\\:ss} minutos");
            }
        }

        private async Task TranslatePdfFile(string inputFilePath)
        {
            outputFilePath = Path.Combine(
                Path.GetDirectoryName(inputFilePath),
                $"{Path.GetFileNameWithoutExtension(inputFilePath)}_traducido.pdf"
            );

            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                using (var pdf = UglyToad.PdfPig.PdfDocument.Open(inputFilePath))
                using (var fs = new FileStream(outputFilePath, FileMode.Create))
                {
                    var document = new Document();
                    var writer = PdfWriter.GetInstance(document, fs);
                    document.Open();

                    int totalPages = pdf.NumberOfPages;
                    int currentPage = 0;

                    var fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                    BaseFont baseFont = File.Exists(fontPath) ?
                        BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED) :
                        BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.EMBEDDED);

                    foreach (var page in pdf.GetPages())
                    {
                        currentPage++;
                        var words = page.GetWords().ToList();

                        var lineGroups = words
                            .GroupBy(w => Math.Round(w.BoundingBox.Bottom, 1))
                            .OrderByDescending(g => g.Key);

                        var originalLines = lineGroups
                            .Select(g => string.Join(" ", g.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text)))
                            .ToList();

                        var translatedLines = await TranslateTextAsync(originalLines);

                        document.SetPageSize(new iTextSharp.text.Rectangle((float)page.Width, (float)page.Height));
                        document.NewPage();
                        var cb = writer.DirectContent;

                        var linePositions = lineGroups.Select(g => (
                            X: (float)g.Min(w => w.BoundingBox.Left),
                            Y: (float)g.Key,
                            Right: (float)g.Max(w => w.BoundingBox.Right)
                        )).ToList();

                        for (int i = 0; i < translatedLines.Count; i++)
                        {
                            float maxWidth = linePositions[i].Right - linePositions[i].X;
                            float fontSize = 12f;
                            string translatedText = translatedLines[i];

                            // Lógica de ajuste de texto
                            while (baseFont.GetWidthPoint(translatedText, fontSize) > maxWidth && fontSize > 8)
                            {
                                fontSize -= 0.5f;
                            }

                            cb.BeginText();
                            cb.SetFontAndSize(baseFont, fontSize);
                            cb.ShowTextAligned(
                                PdfContentByte.ALIGN_LEFT,
                                translatedText,
                                linePositions[i].X,
                                linePositions[i].Y,
                                0
                            );
                            cb.EndText();
                        }

                        progressBar.Value = (int)((currentPage / (double)totalPages) * 100);
                    }

                    document.Close();
                }

                ShowNotification("Traducción PDF completada!");
            }
            finally
            {
                stopwatch.Stop();
                MessageBox.Show($"Tiempo PDF: {stopwatch.Elapsed:mm\\:ss} minutos");
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

      

        private async void btnBrowse_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (radioAss.Checked)
                openFileDialog.Filter = "Archivos de subtítulos (*.ass)|*.ass";
            else
                openFileDialog.Filter = "Archivos PDF (*.pdf)|*.pdf";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = openFileDialog.FileName;
            }

        }

        private async void btnTranslate_Click_1(object sender, EventArgs e)
        {
            btnTranslate.Enabled = false;
            string inputFilePath = txtFilePath.Text;

            if (string.IsNullOrWhiteSpace(inputFilePath))
            {
                MessageBox.Show("Por favor, selecciona un archivo primero.");
                btnTranslate.Enabled = true;
                return;
            }

            try
            {
                if (radioAss.Checked)
                {
                    // Lógica de traducción para ASS
                    await TranslateAssFile(inputFilePath);
                }
                else
                {
                    // Lógica de traducción para PDF
                    await TranslatePdfFile(inputFilePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                btnTranslate.Enabled = true;
            }
        }
    }
}