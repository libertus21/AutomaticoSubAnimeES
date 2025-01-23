namespace TraductorPersonalAi
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.RadioButton radioAss;
        private System.Windows.Forms.RadioButton radioPdf;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        private void InitializeComponent()
        {

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtPrompt = new System.Windows.Forms.TextBox();
            this.btnTranslate = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.lblFilePath = new System.Windows.Forms.Label();
            this.lblPrompt = new System.Windows.Forms.Label();
            this.lblOutput = new System.Windows.Forms.Label();
            this.panelInput = new System.Windows.Forms.Panel();
            this.panelOutput = new System.Windows.Forms.Panel();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.panelInput.SuspendLayout();
            this.panelOutput.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.SuspendLayout();

            this.radioAss = new System.Windows.Forms.RadioButton();
            this.radioPdf = new System.Windows.Forms.RadioButton();

            // Dentro de panelInput
            this.panelInput.Controls.Add(this.radioAss);
            this.panelInput.Controls.Add(this.radioPdf);

            // 
            // radioAss
            // 
            this.radioAss.AutoSize = true;
            this.radioAss.Checked = true;
            this.radioAss.Location = new System.Drawing.Point(530, 60);
            this.radioAss.Name = "radioAss";
            this.radioAss.Size = new System.Drawing.Size(50, 23);
            this.radioAss.TabIndex = 4;
            this.radioAss.TabStop = true;
            this.radioAss.Text = ".ASS";
            this.radioAss.UseVisualStyleBackColor = true;
            // 
            // radioPdf
            // 
            this.radioPdf.AutoSize = true;
            this.radioPdf.Location = new System.Drawing.Point(530, 85);
            this.radioPdf.Name = "radioPdf";
            this.radioPdf.Size = new System.Drawing.Size(51, 23);
            this.radioPdf.TabIndex = 5;
            this.radioPdf.Text = "PDF";
            this.radioPdf.UseVisualStyleBackColor = true;

            // 
            // txtFilePath
            // 
            this.txtFilePath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.txtFilePath.Location = new System.Drawing.Point(120, 20);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.ReadOnly = true;
            this.txtFilePath.Size = new System.Drawing.Size(400, 25);
            this.txtFilePath.TabIndex = 1;
            // 
            // btnBrowse
            // 
            this.btnBrowse.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(130)))), ((int)(((byte)(180)))));
            this.btnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowse.ForeColor = System.Drawing.Color.White;
            this.btnBrowse.Location = new System.Drawing.Point(530, 20);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(100, 25);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Text = "Seleccionar";
            this.btnBrowse.UseVisualStyleBackColor = false;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtPrompt
            // 
            this.txtPrompt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.txtPrompt.Location = new System.Drawing.Point(120, 60);
            this.txtPrompt.Name = "txtPrompt";
            this.txtPrompt.Size = new System.Drawing.Size(510, 25);
            this.txtPrompt.TabIndex = 3;
            // 
            // btnTranslate
            // 
            this.btnTranslate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(179)))), ((int)(((byte)(113)))));
            this.btnTranslate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTranslate.ForeColor = System.Drawing.Color.White;
            this.btnTranslate.Location = new System.Drawing.Point(20, 10);
            this.btnTranslate.Name = "btnTranslate";
            this.btnTranslate.Size = new System.Drawing.Size(150, 40);
            this.btnTranslate.TabIndex = 3;
            this.btnTranslate.Text = "Traducir";
            this.btnTranslate.UseVisualStyleBackColor = false;
            this.btnTranslate.Click += new System.EventHandler(this.btnTranslate_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(190, 20);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(300, 20);
            this.progressBar.TabIndex = 4;
            // 
            // txtOutput
            // 
            this.txtOutput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.txtOutput.Location = new System.Drawing.Point(20, 40);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutput.Size = new System.Drawing.Size(620, 160);
            this.txtOutput.TabIndex = 1;
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(105)))), ((int)(((byte)(225)))));
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(510, 10);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(130, 40);
            this.btnSave.TabIndex = 6;
            this.btnSave.Text = "Guardar";
            this.btnSave.UseVisualStyleBackColor = false;
            // 
            // lblFilePath
            // 
            this.lblFilePath.Location = new System.Drawing.Point(20, 20);
            this.lblFilePath.Name = "lblFilePath";
            this.lblFilePath.Size = new System.Drawing.Size(100, 25);
            this.lblFilePath.TabIndex = 0;
            this.lblFilePath.Text = "Archivo:";
            // 
            // lblPrompt
            // 
            this.lblPrompt.Location = new System.Drawing.Point(20, 60);
            this.lblPrompt.Name = "lblPrompt";
            this.lblPrompt.Size = new System.Drawing.Size(100, 25);
            this.lblPrompt.TabIndex = 2;
            this.lblPrompt.Text = "Texto Manual:";
            // 
            // lblOutput
            // 
            this.lblOutput.Location = new System.Drawing.Point(20, 10);
            this.lblOutput.Name = "lblOutput";
            this.lblOutput.Size = new System.Drawing.Size(100, 25);
            this.lblOutput.TabIndex = 0;
            this.lblOutput.Text = "Resultado:";
            // 
            // panelInput
            // 
            this.panelInput.BackColor = System.Drawing.Color.White;
            this.panelInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelInput.Controls.Add(this.lblFilePath);
            this.panelInput.Controls.Add(this.txtFilePath);
            this.panelInput.Controls.Add(this.btnBrowse);
            this.panelInput.Controls.Add(this.lblPrompt);
            this.panelInput.Controls.Add(this.txtPrompt);
            this.panelInput.Location = new System.Drawing.Point(20, 20);
            this.panelInput.Name = "panelInput";
            this.panelInput.Size = new System.Drawing.Size(660, 120);
            this.panelInput.TabIndex = 0;
            // 
            // panelOutput
            // 
            this.panelOutput.BackColor = System.Drawing.Color.White;
            this.panelOutput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelOutput.Controls.Add(this.lblOutput);
            this.panelOutput.Controls.Add(this.txtOutput);
            this.panelOutput.Location = new System.Drawing.Point(20, 150);
            this.panelOutput.Name = "panelOutput";
            this.panelOutput.Size = new System.Drawing.Size(660, 230);
            this.panelOutput.TabIndex = 1;
            // 
            // panelButtons
            // 
            this.panelButtons.BackColor = System.Drawing.Color.Transparent;
            this.panelButtons.Controls.Add(this.btnTranslate);
            this.panelButtons.Controls.Add(this.progressBar);
            this.panelButtons.Controls.Add(this.btnSave);
            this.panelButtons.Location = new System.Drawing.Point(20, 400);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(660, 60);
            this.panelButtons.TabIndex = 2;
            // 
            // Form1
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))));
            this.ClientSize = new System.Drawing.Size(700, 500);
            this.Controls.Add(this.panelInput);
            this.Controls.Add(this.panelOutput);
            this.Controls.Add(this.panelButtons);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Traductor Personal AI";
            this.panelInput.ResumeLayout(false);
            this.panelInput.PerformLayout();
            this.panelOutput.ResumeLayout(false);
            this.panelOutput.PerformLayout();
            this.panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtPrompt;
        private System.Windows.Forms.Button btnTranslate;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblFilePath;
        private System.Windows.Forms.Label lblPrompt;
        private System.Windows.Forms.Label lblOutput;
        private System.Windows.Forms.Panel panelInput;
        private System.Windows.Forms.Panel panelOutput;
        private System.Windows.Forms.Panel panelButtons;
    }
}
