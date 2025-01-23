using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraductorPersonalAi.Traduccion.Ass
{
    public class AssTranslator
    {
        private readonly Func<List<string>, Task<List<string>>> _translateTextAsync;
        private Action<int> _progressHandler;
        private Action<string> _outputHandler;

        public AssTranslator(Func<List<string>, Task<List<string>>> translateTextAsync,
                           Action<int> progressHandler, Action<string> outputHandler)
        {
            _translateTextAsync = translateTextAsync;
            _progressHandler = progressHandler;
            _outputHandler = outputHandler;
        }

        public async Task TranslateAsync(string inputFilePath, string outputFilePath)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                string[] lines = File.ReadAllLines(inputFilePath);
                StringBuilder translatedContent = new StringBuilder();

                const int batchSize = 80;
                for (int i = 0; i < lines.Length; i += batchSize)
                {
                    var block = lines.Skip(i).Take(batchSize).ToArray();
                    var (textsToTranslate, linesToTranslateIndices) = ProcessBlock(block);

                    if (textsToTranslate.Any())
                    {
                        var translatedTexts = await _translateTextAsync(textsToTranslate);
                        ApplyTranslations(block, linesToTranslateIndices, translatedTexts);
                    }

                    AppendBlockContent(block, translatedContent);
                    UpdateProgress(i + batchSize, lines.Length);
                }

                FinalizeTranslation(outputFilePath, translatedContent);
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        private (List<string> texts, List<int> indices) ProcessBlock(string[] block)
        {
            var textsToTranslate = new List<string>();
            var linesToTranslateIndices = new List<int>();

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
                }
            }
            return (textsToTranslate, linesToTranslateIndices);
        }

        private void ApplyTranslations(string[] block, List<int> indices, List<string> translations)
        {
            for (int j = 0; j < indices.Count; j++)
            {
                int lineIndex = indices[j];
                var originalLine = block[lineIndex];
                var splitParts = originalLine.Split(new[] { "0000,0000,0000,," }, StringSplitOptions.None);

                if (j < translations.Count)
                {
                    block[lineIndex] = $"{splitParts[0]}0000,0000,0000,,{translations[j]}";
                }
            }
        }

        private void AppendBlockContent(string[] block, StringBuilder content)
        {
            foreach (var line in block)
            {
                content.AppendLine(line);
            }
        }

        private void UpdateProgress(int current, int total)
        {
            int progress = Math.Min((int)(current / (float)total * 100), 100);
            _progressHandler?.Invoke(progress);
        }

        private void FinalizeTranslation(string outputPath, StringBuilder content)
        {
            File.WriteAllText(outputPath, content.ToString());
            _outputHandler?.Invoke(content.ToString());
        }
    }
}
