using System.Windows.Forms;

namespace ShillionaireInstaller
{
    partial class ProgressForm
    {
        private Label lblStatus;
        private ProgressBar progressBar;

        private void InitializeComponent()
        {
            this.Text = "Installing...";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Width = 500;
            this.Height = 140;

            lblStatus = new Label { Left = 12, Top = 16, Width = 460, Text = "Preparing files..." };
            progressBar = new ProgressBar { Left = 12, Top = 44, Width = 460, Height = 24, Minimum = 0, Maximum = 100, Value = 0 };

            this.Controls.Add(lblStatus);
            this.Controls.Add(progressBar);
        }
    }
}