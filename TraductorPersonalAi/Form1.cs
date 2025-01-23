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
                using (var pdf = UglyToad.PdfPig.PdfDocument.Open(inputFilePath))
                using (var fs = new FileStream(outputFilePath, FileMode.Create))
                {
                    var document = new Document();
                    var writer = PdfWriter.GetInstance(document, fs);
                    document.Open();

                    int totalPages = pdf.NumberOfPages;
                    int currentPage = 0;

                    // Configurar fuente Unicode (Arial)
                    var fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                    var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                    foreach (var page in pdf.GetPages())
                    {
                        currentPage++;
                        var words = page.GetWords().ToList();

                        // Agrupar por líneas (posición Y)
                        var lineGroups = words
                            .GroupBy(w => Math.Round(w.BoundingBox.Bottom, 1))
                            .OrderByDescending(g => g.Key);

                        var originalLines = new List<string>();
                        var linePositions = new List<(float X, float Y, float Right)>();

                        foreach (var group in lineGroups)
                        {
                            var orderedWords = group.OrderBy(w => w.BoundingBox.Left);
                            originalLines.Add(string.Join(" ", orderedWords.Select(w => w.Text)));
                            var firstWord = orderedWords.First();
                            var lastWord = orderedWords.Last();
                            linePositions.Add((
                                (float)firstWord.BoundingBox.Left,
                                (float)group.Key,
                                (float)lastWord.BoundingBox.Right
                            ));
                        }

                        // Traducir líneas
                        var translatedLines = await TranslateTextAsync(originalLines);

                        // Crear nueva página
                        document.SetPageSize(new iTextSharp.text.Rectangle((float)page.Width, (float)page.Height));
                        document.NewPage();
                        var cb = writer.DirectContent;

                        for (int i = 0; i < translatedLines.Count; i++)
                        {
                            float maxWidth = linePositions[i].Right - linePositions[i].X;
                            float fontSize = 12f;
                            string translatedText = translatedLines[i];

                            // 1. Ajustar tamaño de fuente
                            float textWidth = baseFont.GetWidthPoint(translatedText, fontSize);
                            while (textWidth > maxWidth && fontSize > 8)
                            {
                                fontSize -= 0.5f;
                                textWidth = baseFont.GetWidthPoint(translatedText, fontSize);
                            }

                            // 2. Dividir línea si aún no cabe
                            if (textWidth > maxWidth)
                            {
                                var lines = SplitLongLine(translatedText, baseFont, fontSize, maxWidth);
                                float lineHeight = fontSize * 1.2f;

                                for (int lineIdx = 0; lineIdx < lines.Count; lineIdx++)
                                {
                                    cb.BeginText();
                                    cb.SetFontAndSize(baseFont, fontSize);
                                    cb.ShowTextAligned(
                                        PdfContentByte.ALIGN_LEFT,
                                        lines[lineIdx],
                                        linePositions[i].X,
                                        linePositions[i].Y - (lineIdx * lineHeight),
                                        0
                                    );
                                    cb.EndText();
                                }
                            }
                            else
                            {
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
                        }

                        progressBar.Value = (int)((currentPage / (double)totalPages) * 100);
                    }

                    document.Close();
                }

                MessageBox.Show($"Traducción completa! Archivo guardado en:\n{outputFilePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                stopwatch.Stop();
                MessageBox.Show($"Tiempo total: {stopwatch.Elapsed:mm\\:ss} minutos");
                btnTranslate.Enabled = true;
            }
        }
        private List<string> SplitLongLine(string text, BaseFont font, float fontSize, float maxWidth)
        {
            var lines = new List<string>();
            var words = text.Split(' ');
            var currentLine = new StringBuilder();

            foreach (var word in words)
            {
                string testLine = currentLine.Length > 0 ? $"{currentLine} {word}" : word;
                float testWidth = font.GetWidthPoint(testLine, fontSize);

                if (testWidth <= maxWidth)
                {
                    currentLine.Append(currentLine.Length > 0 ? $" {word}" : word);
                }
                else
                {
                    if (currentLine.Length > 0)
                    {
                        lines.Add(currentLine.ToString());
                        currentLine.Clear();
                    }
                    currentLine.Append(word);
                }
            }

            if (currentLine.Length > 0) lines.Add(currentLine.ToString());
            return lines;
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