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
using TraductorPersonalAi.Traduccion.Ass;
using TraductorPersonalAi.Traduccion.PDF;
using TraductorPersonalAi.Traduccion.SRT;
using TraductorPersonalAi.Python;
namespace TraductorPersonalAi
{
    public partial class Form1 : Form
    {
        private const string HuggingFaceAPIUrl = "https://api-inference.huggingface.co/models/Helsinki-NLP/opus-mt-en-es";
        private const string ApiKey = "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";  // Tu token de Hugging Face
        private string outputFilePath; // ← Añadir esta línea

        private AssTranslator _assTranslator;
        private PdfTranslator _pdfTranslator;
        private SrtTranslator _srtTranslator;
        private readonly PythonTranslationService _pythonService;

        public Form1()
        {
            InitializeComponent();
            _pythonService = InitializePythonService(); // Asignación directa
            InitializeTranslators();
        }

        private PythonTranslationService InitializePythonService()
        {
            const string pythonInterpreter = @"C:\Users\Thecnomax\AppData\Local\Programs\Python\Python312\python.exe";
            const string pythonScript = @"D:\Programacion\Visual Studio\Modelo_AI\ModeloRapido.py";

            return new PythonTranslationService(pythonInterpreter, pythonScript);
        }


        private void InitializeTranslators()
        {
            _assTranslator = new AssTranslator(
     _pythonService.TranslateAsync,
     progress => progressBar.Value = progress,
     content => txtOutput.Text = content
 );

            _pdfTranslator = new PdfTranslator(
                _pythonService.TranslateAsync,
                progress => progressBar.Value = progress
            );

            _srtTranslator = new SrtTranslator(
                _pythonService.TranslateAsync,
                progress => progressBar.Value = progress,
                content => txtOutput.Text = content
            );


            //version con traduciones exacta
           // _assTranslator = new AssTranslator(
           //    TranslateTextAsync,
           //    progress => progressBar.Value = progress,
           //    content => txtOutput.Text = content
           //);

           // _pdfTranslator = new PdfTranslator(
           //     TranslateTextAsync,
           //     progress => progressBar.Value = progress
           // );
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
            
            if (radioAss.Checked)
                saveFileDialog.Filter = "Archivos de subtítulos (*.ass)|*.ass";
            else if (radioSrt.Checked)
                saveFileDialog.Filter = "Archivos de subtítulos (*.srt)|*.srt";
            else
                saveFileDialog.Filter = "Archivos PDF (*.pdf)|*.pdf";
                
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
            else if (radioSrt.Checked)
                openFileDialog.Filter = "Archivos de subtítulos (*.srt)|*.srt";
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
            progressBar.Value = 0;

            //Verificar cuanto tiempo dura la ejeccucion codigo
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //
            try
            {
                if (string.IsNullOrWhiteSpace(inputFilePath))
                {
                    MessageBox.Show("Por favor, selecciona un archivo primero.");
                    return;
                }

                outputFilePath = GetOutputPath(inputFilePath);

                if (radioAss.Checked)
                {
                    await _assTranslator.TranslateAsync(inputFilePath, outputFilePath);
                    ShowNotification("Traducción ASS completada!");
                }
                else if (radioSrt.Checked)
                {
                    await _srtTranslator.TranslateAsync(inputFilePath, outputFilePath);
                    ShowNotification("Traducción SRT completada!");
                }
                else
                {
                    await _pdfTranslator.TranslateAsync(inputFilePath, outputFilePath);
                    ShowNotification("Traducción PDF completada!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                btnTranslate.Enabled = true;
                stopwatch.Stop();
                MessageBox.Show($"Tiempo Ejeccucion: {stopwatch.Elapsed:mm\\:ss} minutos");
            }
        }
        private string GetOutputPath(string inputPath)
        {
            string extension;
            if (radioAss.Checked)
                extension = ".ass";
            else if (radioSrt.Checked)
                extension = ".srt";
            else
                extension = ".pdf";
                
            return Path.Combine(
                Path.GetDirectoryName(inputPath),
                $"{Path.GetFileNameWithoutExtension(inputPath)}_traducido{extension}"
            );
        }

    }
}