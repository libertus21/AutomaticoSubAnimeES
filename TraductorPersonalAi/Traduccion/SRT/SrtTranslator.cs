using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraductorPersonalAi.Traduccion.SRT
{
    public class SrtTranslator
    {
        private readonly Func<List<string>, Task<List<string>>> _translateTextAsync;
        private Action<int> _progressHandler;
        private Action<string> _outputHandler;

        public SrtTranslator(Func<List<string>, Task<List<string>>> translateTextAsync,
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
                // Leer todo el archivo y localizar todas las líneas de texto reales
                string[] lines = File.ReadAllLines(inputFilePath);
                var textLineIndices = new List<int>();
                var textsToTranslate = new List<string>();

                for (int i = 0; i < lines.Length; i++)
                {
                    var trimmed = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(trimmed) || IsNumber(trimmed) || IsTimeLine(trimmed))
                    {
                        continue;
                    }
                    textLineIndices.Add(i);
                    textsToTranslate.Add(lines[i]); // mantener formato original de la línea
                }

                if (textsToTranslate.Any())
                {
                    var translatedTexts = await _translateTextAsync(textsToTranslate);

                    int applied = 0;
                    int total = textLineIndices.Count;
                    for (int k = 0; k < total; k++)
                    {
                        int idx = textLineIndices[k];
                        string original = lines[idx];
                        string translated = k < translatedTexts.Count ? translatedTexts[k] : string.Empty;

                        // Evitar romper SRT: forzar una sola línea sustituyendo saltos por espacio
                        if (!string.IsNullOrEmpty(translated))
                        {
                            translated = translated.Replace("\r", " ").Replace("\n", " ");
                        }

                        // Si la traducción viene vacía, conservar la original para no desalinear
                        lines[idx] = string.IsNullOrWhiteSpace(translated) ? original : translated;

                        applied++;
                        UpdateProgress(applied, total);
                    }
                }

                // Escribir salida final
                var sb = new StringBuilder();
                foreach (var line in lines)
                {
                    sb.AppendLine(line);
                }
                FinalizeTranslation(outputFilePath, sb);
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
                var line = block[j].Trim();
                
                // En SRT, las líneas de texto están después de las líneas de tiempo
                // El formato es: número, tiempo, texto, línea vacía
                if (!string.IsNullOrWhiteSpace(line) && 
                    !IsNumber(line) && 
                    !IsTimeLine(line))
                {
                    textsToTranslate.Add(line);
                    linesToTranslateIndices.Add(j);
                }
            }
            return (textsToTranslate, linesToTranslateIndices);
        }

        private bool IsNumber(string line)
        {
            return int.TryParse(line, out _);
        }

        private bool IsTimeLine(string line)
        {
            // Las líneas de tiempo en SRT tienen el formato: 00:00:00,000 --> 00:00:00,000
            return line.Contains(" --> ") && line.Contains(":");
        }

        private void ApplyTranslations(string[] block, List<int> indices, List<string> translations)
        {
            for (int j = 0; j < indices.Count; j++)
            {
                int lineIndex = indices[j];
                if (j < translations.Count)
                {
                    block[lineIndex] = translations[j];
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