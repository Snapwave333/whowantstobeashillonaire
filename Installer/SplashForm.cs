using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShillionaireInstaller
{
    public class SplashForm : Form
    {
        private Timer _timer;
        private Bitmap _bmp;

        public SplashForm(int milliseconds = 1500)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Width = 560;
            this.Height = 240;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.DoubleBuffered = true;

            Brand.ApplyFormBranding(this);
            _bmp = Brand.CreateSplashBitmap(this.Width, this.Height);

            _timer = new Timer { Interval = milliseconds };
            _timer.Tick += (s, e) => { _timer.Stop(); this.Close(); };
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _timer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(_bmp, 0, 0, this.Width, this.Height);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Dispose();
                _bmp?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}