using System;
using System.IO;
using System.Windows.Forms;

namespace ShillionaireInstaller
{
    public partial class InstallerForm : Form
    {
        public string InstallDirectory => txtPath.Text.Trim();
        public bool CreateDesktopShortcut => chkShortcut.Checked;
        public bool LaunchAfterInstall => chkLaunch.Checked;
        public bool CreateStartMenuShortcut => chkStartMenu.Checked;

        public InstallerForm()
        {
            InitializeComponent();
            Brand.ApplyFormBranding(this);
            var companyName = "Shillionaire Studios";
            var productName = "Who Wants to Be a Shillionaire";
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            txtPath.Text = Path.Combine(localAppData, companyName, productName);

            // Load a basic EULA text. Keep it local to avoid internet dependency.
            rtbEula.Text = "By installing, you agree to use this software at your own discretion. No warranties implied. Have fun and be kind.\n\nKey points:\n- Personal use permitted.\n- Do not distribute paid copies without consent.\n- Respect API usage limits and provider terms.";
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog();
            fbd.Description = "Choose installation directory";
            fbd.ShowNewFolderButton = true;
            if (fbd.ShowDialog(this) == DialogResult.OK)
            {
                txtPath.Text = fbd.SelectedPath;
            }
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPath.Text))
            {
                MessageBox.Show(this, "Please select a valid installation path.", "Installer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!chkAgree.Checked)
            {
                MessageBox.Show(this, "You must agree to the EULA to proceed.", "Installer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void chkAgree_CheckedChanged(object sender, EventArgs e)
        {
            btnInstall.Enabled = chkAgree.Checked;
        }
    }
}