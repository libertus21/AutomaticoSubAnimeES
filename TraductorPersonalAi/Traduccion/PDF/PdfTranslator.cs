using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace TraductorPersonalAi.Traduccion.PDF
{
    public class PdfTranslator
    {
        private readonly Func<List<string>, Task<List<string>>> _translateTextAsync;
        private Action<int> _progressHandler;

        public PdfTranslator(Func<List<string>, Task<List<string>>> translateTextAsync,
                           Action<int> progressHandler)
        {
            _translateTextAsync = translateTextAsync;
            _progressHandler = progressHandler;
        }

        public async Task TranslateAsync(string inputFilePath, string outputFilePath)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

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

                    var baseFont = GetBaseFont();

                    // Cambiar a lista de tareas y procesar secuencialmente
                    foreach (var page in pdf.GetPages())
                    {
                        currentPage++;
                        await ProcessPageAsync(page, document, writer, baseFont); // Ahora es async Task
                        UpdateProgress(currentPage, totalPages);
                    }

                    document.Close();
                }
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        private BaseFont GetBaseFont()
        {
            var fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
            return File.Exists(fontPath) ?
                BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED) :
                BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.EMBEDDED);
        }

        private async Task ProcessPageAsync(Page page, Document document, PdfWriter writer, BaseFont baseFont)
        {
            var words = page.GetWords().ToList();
            var lineGroups = words
                .GroupBy(w => Math.Round(w.BoundingBox.Bottom, 1))
                .OrderByDescending(g => g.Key);

            var originalLines = lineGroups
                .Select(g => string.Join(" ", g.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text)))
                .ToList();

            var translatedLines = await _translateTextAsync(originalLines);

            document.SetPageSize(new iTextSharp.text.Rectangle((float)page.Width, (float)page.Height));
            document.NewPage();
            var cb = writer.DirectContent;

            var linePositions = lineGroups.Select(g => (
                X: (float)g.Min(w => w.BoundingBox.Left),
                Y: (float)g.Key,
                Right: (float)g.Max(w => w.BoundingBox.Right)
            )).ToList();

            RenderTranslations(cb, translatedLines, linePositions, baseFont);
        }

        private void RenderTranslations(PdfContentByte cb, List<string> translations,
                                      List<(float X, float Y, float Right)> positions, BaseFont font)
        {
            for (int i = 0; i < translations.Count; i++)
            {
                float maxWidth = positions[i].Right - positions[i].X;
                float fontSize = CalculateFontSize(translations[i], maxWidth, font);

                cb.BeginText();
                cb.SetFontAndSize(font, fontSize);
                cb.ShowTextAligned(
                    PdfContentByte.ALIGN_LEFT,
                    translations[i],
                    positions[i].X,
                    positions[i].Y,
                    0
                );
                cb.EndText();
            }
        }

        private float CalculateFontSize(string text, float maxWidth, BaseFont font,
                                      float initialSize = 12f, float minSize = 8f)
        {
            float fontSize = initialSize;
            while (font.GetWidthPoint(text, fontSize) > maxWidth && fontSize > minSize)
            {
                fontSize -= 0.5f;
            }
            return fontSize;
        }

        private void UpdateProgress(int current, int total)
        {
            int progress = (int)((current / (double)total) * 100);
            _progressHandler?.Invoke(progress);
        }
    }
}
