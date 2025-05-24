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
        private readonly Action<int> _progressHandler;
        private const float LineGroupTolerance = 1f;
        private const int MaxRetries = 3;
        private const int ChunkSize = 10;
        private int _consecutiveErrors;

        private async Task<List<string>> ProcessTranslationsWithChunksAsync(List<string> originalLines)
        {
            var translatedLines = new List<string>();
            var chunks = SplitIntoChunks(originalLines, ChunkSize);

            foreach (var chunk in chunks)
            {
                bool success = false;
                List<string> translatedChunk = null;
                int attempt;

                for (attempt = 0; attempt < MaxRetries; attempt++)
                {
                    try
                    {
                        translatedChunk = await _translateTextAsync(chunk);

                        // Permitir diferencia de hasta 1 línea si es el último intento
                        if (translatedChunk.Count == chunk.Count ||
                           (attempt == MaxRetries - 1 && Math.Abs(translatedChunk.Count - chunk.Count) <= 1))
                        {
                            translatedLines.AddRange(translatedChunk);
                            success = true;
                            _consecutiveErrors = 0;
                            break;
                        }

                        await Task.Delay(500 * (attempt + 1));
                    }
                    catch
                    {
                        if (++_consecutiveErrors >= 3)
                        {
                            throw new TranslationException("Demasiados errores consecutivos. Deteniendo traducción.");
                        }

                        if (attempt == MaxRetries - 1) break;
                    }
                }

                if (!success)
                {
                    // Estrategia de recuperación mejorada
                    HandleFailedChunk(chunk, translatedChunk, translatedLines);
                }
            }

            return translatedLines;
        }

        private void HandleFailedChunk(List<string> originalChunk, List<string> translatedChunk, List<string> outputLines)
        {
            if (translatedChunk == null || translatedChunk.Count != originalChunk.Count)
            {
                // Intentar alinear manualmente las líneas

                outputLines.AddRange(translatedChunk != null && translatedChunk.Count > 0 ? translatedChunk : originalChunk);
                // Log detallado
                Console.WriteLine($"Chunk fallido. Original: {originalChunk.Count} líneas, Traducido: {translatedChunk?.Count ?? 0}");

                // Añadir marcadores para identificar discrepancias
                if (translatedChunk?.Count > 0)
                {
                    outputLines.AddRange(originalChunk.Skip(translatedChunk.Count)
                        .Select(l => $"[ERROR DE TRADUCCIÓN] {l}"));
                }
            }
            else
            {
                outputLines.AddRange(originalChunk);
            }
        }
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

                    var baseFont = await GetUnicodeFontAsync();

                    foreach (var page in pdf.GetPages())
                    {
                        currentPage++;
                        await ProcessPageAsync(page, document, writer, baseFont);
                        UpdateProgress(currentPage, totalPages);
                    }

                    document.Close();
                }
            }
            catch (TranslationException ex)
            {
                HandleTranslationError(ex);
                throw;
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        private async Task<BaseFont> GetUnicodeFontAsync()
        {
            var fontPaths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arialuni.ttf"),
                Path.Combine(AppContext.BaseDirectory, "fonts", "NotoSans-Regular.ttf")
            };

            foreach (var path in fontPaths)
            {
                if (File.Exists(path))
                {
                    return BaseFont.CreateFont(path, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                }
            }
            throw new FileNotFoundException("No suitable Unicode font found");
        }

        private async Task ProcessPageAsync(Page page, Document document, PdfWriter writer, BaseFont baseFont)
        {
            var words = page.GetWords().ToList();
            var lineGroups = GroupWordsIntoLines(words);

            var originalLines = lineGroups
                .Select(g => string.Join(" ", g.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text)))
                .ToList();

            var translatedLines = await ProcessTranslationsWithChunksAsync(originalLines);

            if (translatedLines.Count != originalLines.Count)
            {
                throw new TranslationException(
                    $"Final line count mismatch. Original: {originalLines.Count}, Translated: {translatedLines.Count}");
            }

            document.SetPageSize(GetPageSize(page));
            document.NewPage();

            var positions = lineGroups.Select(g => (
                X: (float)g.Min(w => w.BoundingBox.Left),
                Y: (float)g.Key,
                Right: (float)g.Max(w => w.BoundingBox.Right)
            )).ToList();

            RenderTranslations(writer.DirectContent, translatedLines, positions, baseFont);
        }

        //private async Task<List<string>> ProcessTranslationsWithChunksAsync(List<string> originalLines)
        //{
        //    var translatedLines = new List<string>();
        //    var chunks = SplitIntoChunks(originalLines, ChunkSize);

        //    foreach (var chunk in chunks)
        //    {
        //        bool success = false;
        //        List<string> translatedChunk = null;

        //        for (int attempt = 0; attempt < MaxRetries; attempt++)
        //        {
        //            try
        //            {
        //                translatedChunk = await _translateTextAsync(chunk);

        //                if (translatedChunk.Count == chunk.Count)
        //                {
        //                    translatedLines.AddRange(translatedChunk);
        //                    success = true;
        //                    _consecutiveErrors = 0;
        //                    break;
        //                }

        //                if (attempt == MaxRetries - 1)
        //                {
        //                    throw new TranslationException(
        //                        $"Chunk translation failed after {MaxRetries} attempts. " +
        //                        $"Original lines: {chunk.Count}, Translated: {translatedChunk?.Count ?? 0}");
        //                }

        //                await Task.Delay(500 * (attempt + 1));
        //            }
        //            catch
        //            {
        //                if (++_consecutiveErrors >= 3)
        //                {
        //                    throw new TranslationException("Too many consecutive errors. Stopping translation.");
        //                }

        //                if (attempt == MaxRetries - 1) throw;
        //            }
        //        }

        //        if (!success)
        //        {
        //            translatedLines.AddRange(chunk);
        //            _consecutiveErrors = 0;
        //        }
        //    }

        //    return translatedLines;
        //}

        private List<List<string>> SplitIntoChunks(List<string> source, int chunkSize)
        {
            return source.Select((x, i) => new { Index = i, Value = x })
                        .GroupBy(x => x.Index / chunkSize)
                        .Select(g => g.Select(x => x.Value).ToList())
                        .ToList();
        }

        private void RenderTranslations(PdfContentByte cb, List<string> translations,
                                      List<(float X, float Y, float Right)> positions, BaseFont font)
        {
            if (translations.Count != positions.Count)
            {
                throw new TranslationException(
                    $"Render mismatch: {translations.Count} translations vs {positions.Count} positions");
            }

            for (int i = 0; i < translations.Count; i++)
            {
                var text = translations[i];
                var (x, y, right) = positions[i];
                float maxWidth = right - x;

                var fontSize = CalculateOptimalFontSize(text, maxWidth, font);
                var wrappedLines = WrapText(text, maxWidth, font, fontSize);

                float currentY = y;
                foreach (var line in wrappedLines)
                {
                    if (string.IsNullOrEmpty(line)) continue;

                    cb.BeginText();
                    cb.SetFontAndSize(font, fontSize);
                    cb.ShowTextAligned(
                        PdfContentByte.ALIGN_LEFT,
                        line,
                        x,
                        currentY,
                        0
                    );
                    cb.EndText();
                    currentY -= fontSize * 1.2f;
                }
            }
        }

        private float CalculateOptimalFontSize(string text, float maxWidth, BaseFont font,
                                             float initialSize = 12f, float minSize = 6f)
        {
            float fontSize = initialSize;
            while (font.GetWidthPoint(text, fontSize) > maxWidth && fontSize > minSize)
            {
                fontSize -= 0.5f;
            }
            return fontSize;
        }

        private List<string> WrapText(string text, float maxWidth, BaseFont font, float fontSize)
        {
            var lines = new List<string>();
            var words = text.Split(' ');
            var currentLine = new System.Text.StringBuilder();

            foreach (var word in words)
            {
                var testLine = currentLine.Length > 0
                    ? currentLine.ToString() + " " + word
                    : word;

                if (font.GetWidthPoint(testLine, fontSize) <= maxWidth)
                {
                    currentLine.Append(currentLine.Length > 0 ? " " + word : word);
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
            lines.Add(currentLine.ToString());

            return lines;
        }

        private List<IGrouping<float, Word>> GroupWordsIntoLines(List<Word> words)
        {
            return words
                .GroupBy(w => FindLineGroup(w.BoundingBox.Bottom))
                .OrderByDescending(g => g.Key)
                .ToList();
        }

        private float FindLineGroup(double y)
        {
            return (float)Math.Floor(y / LineGroupTolerance) * LineGroupTolerance;
        }

        private Rectangle GetPageSize(Page page)
        {
            var mediaBox = page.MediaBox;
            return new Rectangle(
                (float)mediaBox.Bounds.Left,
                (float)mediaBox.Bounds.Bottom,
                (float)mediaBox.Bounds.Right,
                (float)mediaBox.Bounds.Top
            );
        }

        private void UpdateProgress(int current, int total)
        {
            int progress = (int)((current / (double)total) * 100);
            _progressHandler?.Invoke(progress);
        }

        private void HandleTranslationError(TranslationException ex)
        {
            // Implementar lógica de logging o notificación
            Console.WriteLine($"Error de traducción: {ex.Message}");
        }
    }

    public class TranslationException : Exception
    {
        public TranslationException(string message) : base(message) { }
    }
}