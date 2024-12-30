using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestSharp;
using Newtonsoft.Json;

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
            var client = new RestClient(HuggingFaceAPIUrl);
            var request = new RestRequest();
            request.AddHeader("Authorization", $"Bearer {ApiKey}");
            request.AddJsonBody(new { inputs = text });

            // Usar RestRequest.Post() en lugar de Method.POST
            request.Method = Method.Post;

            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                dynamic result = JsonConvert.DeserializeObject(response.Content);
                return result[0].translation_text;
            }
            else
            {
                throw new Exception("Error al traducir el texto.");
            }
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

                // Procesar el archivo en bloques de 30 líneas
                for (int i = 0; i < lines.Length; i += 30)
                {
                    var block = new string[30];
                    Array.Copy(lines, i, block, 0, Math.Min(30, lines.Length - i));

                    foreach (var line in block)
                    {
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
