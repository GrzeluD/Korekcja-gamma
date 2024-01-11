namespace Korekcja_gamma
{
    partial class Gamma
    {
        /// <summary>
        /// Wymagana zmienna projektanta.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Wyczyść wszystkie używane zasoby.
        /// </summary>
        /// <param name="disposing">prawda, jeżeli zarządzane zasoby powinny zostać zlikwidowane; Fałsz w przeciwnym wypadku.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kod generowany przez Projektanta formularzy systemu Windows

        /// <summary>
        /// Metoda wymagana do obsługi projektanta — nie należy modyfikować
        /// jej zawartości w edytorze kodu.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Gamma));
            this.loadImageButton = new System.Windows.Forms.Button();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.transparencyTrackBar = new System.Windows.Forms.TrackBar();
            this.processImageButton = new System.Windows.Forms.Button();
            this.transparencyLabel = new System.Windows.Forms.Label();
            this.dropDown = new System.Windows.Forms.ComboBox();
            this.threadsLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.transparencyTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // loadImageButton
            // 
            resources.ApplyResources(this.loadImageButton, "loadImageButton");
            this.loadImageButton.Name = "loadImageButton";
            this.loadImageButton.UseVisualStyleBackColor = true;
            this.loadImageButton.Click += new System.EventHandler(this.loadImageButton_Click);
            // 
            // pictureBox
            // 
            resources.ApplyResources(this.pictureBox, "pictureBox");
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.TabStop = false;
            // 
            // transparencyTrackBar
            // 
            resources.ApplyResources(this.transparencyTrackBar, "transparencyTrackBar");
            this.transparencyTrackBar.Maximum = 100;
            this.transparencyTrackBar.Minimum = -100;
            this.transparencyTrackBar.Name = "transparencyTrackBar";
            this.transparencyTrackBar.Scroll += new System.EventHandler(this.transparencyTrackBar_Scroll);
            // 
            // processImageButton
            // 
            resources.ApplyResources(this.processImageButton, "processImageButton");
            this.processImageButton.Name = "processImageButton";
            this.processImageButton.UseVisualStyleBackColor = true;
            this.processImageButton.Click += new System.EventHandler(this.processImageButton_Click);
            // 
            // transparencyLabel
            // 
            resources.ApplyResources(this.transparencyLabel, "transparencyLabel");
            this.transparencyLabel.Name = "transparencyLabel";
            // 
            // dropDown
            // 
            this.dropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dropDown.FormattingEnabled = true;
            resources.ApplyResources(this.dropDown, "dropDown");
            this.dropDown.Name = "dropDown";
            this.dropDown.Sorted = true;
            // 
            // threadsLabel
            // 
            resources.ApplyResources(this.threadsLabel, "threadsLabel");
            this.threadsLabel.Name = "threadsLabel";
            // 
            // Gamma
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.threadsLabel);
            this.Controls.Add(this.dropDown);
            this.Controls.Add(this.transparencyLabel);
            this.Controls.Add(this.processImageButton);
            this.Controls.Add(this.transparencyTrackBar);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.loadImageButton);
            this.Name = "Gamma";
            this.Load += new System.EventHandler(this.Gamma_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.transparencyTrackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button loadImageButton;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.TrackBar transparencyTrackBar;
        private System.Windows.Forms.Button processImageButton;
        private System.Windows.Forms.Label transparencyLabel;
        private System.Windows.Forms.ComboBox dropDown;
        private System.Windows.Forms.Label threadsLabel;
    }
}

