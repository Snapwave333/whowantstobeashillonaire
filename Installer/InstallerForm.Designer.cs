using System.Windows.Forms;

namespace ShillionaireInstaller
{
    partial class InstallerForm
    {
        private Button btnInstall;
        private Button btnBrowse;
        private TextBox txtPath;
        private Label lblPath;
        private CheckBox chkShortcut;
        private CheckBox chkLaunch;
        private CheckBox chkStartMenu;
        private Label lblEula;
        private RichTextBox rtbEula;
        private CheckBox chkAgree;

        private void InitializeComponent()
        {
            this.Text = "Shillionaire Setup";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Width = 640;
            this.Height = 480;

            lblPath = new Label { Left = 12, Top = 18, Width = 600, Text = "Install path:" };
            txtPath = new TextBox { Left = 12, Top = 40, Width = 520 };
            btnBrowse = new Button { Left = 540, Top = 38, Width = 80, Text = "Browse..." };
            btnBrowse.Click += btnBrowse_Click;

            chkShortcut = new CheckBox { Left = 12, Top = 80, Width = 300, Text = "Create desktop shortcut" };
            chkStartMenu = new CheckBox { Left = 320, Top = 80, Width = 300, Text = "Create Start Menu shortcut" };
            chkLaunch = new CheckBox { Left = 12, Top = 105, Width = 300, Text = "Launch app after install", Checked = true };

            lblEula = new Label { Left = 12, Top = 140, Width = 600, Text = "End User License Agreement (EULA):" };
            rtbEula = new RichTextBox { Left = 12, Top = 162, Width = 608, Height = 220, ReadOnly = true, BorderStyle = BorderStyle.FixedSingle };
            chkAgree = new CheckBox { Left = 12, Top = 388, Width = 400, Text = "I have read and agree to the EULA" };
            chkAgree.CheckedChanged += chkAgree_CheckedChanged;

            btnInstall = new Button { Left = 440, Top = 420, Width = 180, Text = "Install", Enabled = false };
            btnInstall.Click += btnInstall_Click;

            this.Controls.Add(lblPath);
            this.Controls.Add(txtPath);
            this.Controls.Add(btnBrowse);
            this.Controls.Add(chkShortcut);
            this.Controls.Add(chkStartMenu);
            this.Controls.Add(chkLaunch);
            this.Controls.Add(lblEula);
            this.Controls.Add(rtbEula);
            this.Controls.Add(chkAgree);
            this.Controls.Add(btnInstall);
        }
    }
}