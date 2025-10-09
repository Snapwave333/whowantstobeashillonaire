using System;
using System.Windows.Forms;

namespace ShillionaireInstaller
{
    public partial class ProgressForm : Form
    {
        public ProgressForm()
        {
            InitializeComponent();
            Brand.ApplyFormBranding(this);
        }

        public void SetStatus(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => lblStatus.Text = text));
            }
            else
            {
                lblStatus.Text = text;
            }
        }

        public void SetProgress(int percent)
        {
            percent = Math.Max(0, Math.Min(100, percent));
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => progressBar.Value = percent));
            }
            else
            {
                progressBar.Value = percent;
            }
        }
    }
}