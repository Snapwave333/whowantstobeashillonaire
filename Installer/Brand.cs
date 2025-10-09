using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ShillionaireInstaller
{
    // This function is smoother than a fresh Windows install
    public static class Brand
    {
        private static Icon? _icon;

        public static Icon GetAppIcon()
        {
            if (_icon != null) return _icon;
            using var bmp = new Bitmap(64, 64);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var lg = new LinearGradientBrush(new Rectangle(0, 0, 64, 64),
                Color.FromArgb(20, 20, 60), Color.FromArgb(0, 128, 255), LinearGradientMode.ForwardDiagonal);
            g.FillEllipse(lg, 0, 0, 64, 64);

            using var pen = new Pen(Color.White, 3);
            g.DrawArc(pen, 8, 8, 48, 48, 210, 180);

            using var font = new Font("Segoe UI", 16, FontStyle.Bold);
            var text = "S"; // Shillionaire, naturally
            var size = g.MeasureString(text, font);
            g.DrawString(text, font, Brushes.White, (64 - size.Width) / 2, (64 - size.Height) / 2 - 4);

            var hicon = bmp.GetHicon();
            _icon = Icon.FromHandle(hicon);
            return _icon;
        }

        public static void ApplyFormBranding(Form form)
        {
            form.Icon = GetAppIcon();
        }

        public static Bitmap CreateSplashBitmap(int width, int height)
        {
            var bmp = new Bitmap(width, height);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var bg = new LinearGradientBrush(new Rectangle(0, 0, width, height),
                Color.FromArgb(12, 12, 32), Color.FromArgb(0, 100, 200), LinearGradientMode.Vertical);
            g.FillRectangle(bg, 0, 0, width, height);

            using var titleFont = new Font("Segoe UI", 28, FontStyle.Bold);
            g.DrawString("Shillionaire Setup", titleFont, Brushes.White, new PointF(24, 24));

            using var accentPen = new Pen(Color.FromArgb(255, 255, 255), 2);
            g.DrawBezier(accentPen,
                new PointF(24, height - 80),
                new PointF(width / 3f, height - 20),
                new PointF(width * 2 / 3f, height - 140),
                new PointF(width - 24, height - 40));

            using var small = new SolidBrush(Color.FromArgb(255, 255, 255));
            g.FillEllipse(small, width - 80, 24, 40, 40);
            return bmp;
        }
    }
}