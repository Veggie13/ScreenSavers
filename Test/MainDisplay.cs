using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Linq;

namespace Test
{
    public partial class MainDisplay : Form
    {
        private class MotionPoint
        {
            public static MotionPoint GetRandom(RectangleF boundary, double minVelocity, double maxVelocity)
            {
                double angle = Rand.NextDouble() * 2 * Math.PI;
                double mag = Rand.NextDouble() * (maxVelocity - minVelocity) + minVelocity;

                return new MotionPoint()
                {
                    X = Rand.NextDouble() * boundary.Width + boundary.Left,
                    Y = Rand.NextDouble() * boundary.Height + boundary.Top,
                    VelocityX = Math.Cos(angle) * mag,
                    VelocityY = Math.Sin(angle) * mag
                };
            }

            public double X { get; set; }
            public double Y { get; set; }

            public double VelocityX { get; set; }
            public double VelocityY { get; set; }

            public void Move(double dt)
            {
                X += VelocityX * dt;
                Y += VelocityY * dt;
            }

            public bool Bound(RectangleF boundary)
            {
                bool reflected = false;
                while (!boundary.Contains(AsPointF()))
                {
                    reflected = true;
                    if (X < boundary.Left)
                    {
                        X = 2 * boundary.Left - X;
                        VelocityX *= -1;
                    }
                    if (X > boundary.Right)
                    {
                        X = 2 * boundary.Right - X;
                        VelocityX *= -1;
                    }
                    if (Y < boundary.Top)
                    {
                        Y = 2 * boundary.Top - Y;
                        VelocityY *= -1;
                    }
                    if (Y > boundary.Bottom)
                    {
                        Y = 2 * boundary.Bottom - Y;
                        VelocityY *= -1;
                    }
                }
                return reflected;
            }

            public PointF AsPointF()
            {
                return new PointF((float)X, (float)Y);
            }
        }

        private const double MaxVelocity = 400;
        private const double MinVelocity = 100;
        private const int RefreshRate_ms = 50;
        private const double RefreshRate_s = RefreshRate_ms / 1000.0;

        private bool _preview = false;
        private Timer _timer;
        private static Random Rand = new Random();
        private List<MotionPoint> _shape = new List<MotionPoint>();

        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out Rectangle lpRect);

        public MainDisplay(IntPtr previewWindowHandle)
        {
            InitializeComponent();
            SetParent(this.Handle, previewWindowHandle);
            SetWindowLong(this.Handle, -16, new IntPtr(GetWindowLong(this.Handle, -16) | 0x40000000));

            Rectangle ParentRectangle;
            GetClientRect(previewWindowHandle, out ParentRectangle);
            Size = ParentRectangle.Size;
            Location = new Point(0, 0);

            _preview = true;

            init();
        }


        public MainDisplay(Rectangle bounds)
        {
            InitializeComponent();
            Bounds = bounds;

            init();
        }

        private void init()
        {
            var rect = new Rectangle(Point.Empty, Size);
            _shape.Add(MotionPoint.GetRandom(rect, MinVelocity, MaxVelocity));
            _shape.Add(MotionPoint.GetRandom(rect, MinVelocity, MaxVelocity));
            _shape.Add(MotionPoint.GetRandom(rect, MinVelocity, MaxVelocity));
            _shape.Add(MotionPoint.GetRandom(rect, MinVelocity, MaxVelocity));

            _timer = new Timer()
            {
                Interval = RefreshRate_ms,
                Enabled = !_preview
            };
            _timer.Tick += _timer_Tick;
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            advance();
            Invalidate();
        }

        private void advance()
        {
            foreach (var point in _shape)
            {
                point.Move(RefreshRate_s);
                if (point.Bound(Bounds))
                {
                    double angle = Rand.NextDouble() * 0.5 * Math.PI;
                    double mag = Rand.NextDouble() * (MaxVelocity - MinVelocity) + MinVelocity;

                    point.VelocityX = Math.Cos(angle) * mag * Math.Sign(point.VelocityX);
                    point.VelocityY = Math.Sin(angle) * mag * Math.Sign(point.VelocityY);
                }
            }
        }

        private void MainDisplay_KeyUp(object sender, KeyEventArgs e)
        {
            Finish();
        }

        private void Finish()
        {
            if (!_preview)
            {
                Application.Exit();
            }
        }

        private void draw(Graphics g)
        {
            if (_shape.Any())
            {
                g.DrawPolygon(Pens.Red, _shape.Select(p => p.AsPointF()).ToArray());
            }
        }

        private void MainDisplay_Load(object sender, EventArgs e)
        {
            if (!_preview)
            {
                Cursor.Hide();
                TopMost = true;
            }
        }

        private void MainDisplay_Paint(object sender, PaintEventArgs e)
        {
            draw(e.Graphics);
        }

        private void MainDisplay_MouseClick(object sender, MouseEventArgs e)
        {
        }
    }
}
