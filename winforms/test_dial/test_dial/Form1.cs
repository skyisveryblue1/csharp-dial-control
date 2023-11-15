using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Permissions;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        GradientArcBorder[] arcs = new GradientArcBorder[3];
        Font font;
        Brush brush;
        Pen pen;
        Rectangle rect;
        float percent = 80;
        int radius = 100;
        int borderWidth = 40;
        public Form1()
        {
            InitializeComponent();

            this.DoubleBuffered = true;

            float degreeRate = (float)(Math.PI / 180);

            Color cFirst = Color.FromArgb(0xFF, 0x00, 0x30, 0x90);
            Color cSecond = Color.FromArgb(0xFF, 0xC9, 0, 0);
            Color cThird = Color.FromArgb(0xFF, 0xFF, 0, 0);
            Color cFourth = Color.FromArgb(0xFF, 0xD7, 0xEF, 0x1C);

            double limitAngle = -90 + 360 * percent / 100;
            arcs[0] = new GradientArcBorder(new PointF(260, 240), radius, borderWidth, degreeRate * -90, degreeRate * 30, degreeRate * limitAngle, cFirst, cSecond);
            arcs[1] = new GradientArcBorder(new PointF(260, 240), radius, borderWidth, degreeRate * 30, degreeRate * 90, degreeRate * limitAngle, cSecond, cThird);
            arcs[2] = new GradientArcBorder(new PointF(260, 240), radius, borderWidth, degreeRate * 90, degreeRate * 270, degreeRate * limitAngle, cThird, cFourth, 2880);

            this.BackColor = Color.FromArgb(0xFF, 0x00, 0x20, 0x43);

            // Set the font and color for the text
            font = new Font("Arial", 50);
            rect = new Rectangle(160, 140, 200, 200);
            pen = new Pen(Color.FromArgb(0x20, 0x00, 0xFF, 0xFF), borderWidth);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            e.Graphics.DrawEllipse(pen, 140, 120, 240, 240);

            DrawArcByGradient(e.Graphics, arcs);

            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            // Draw the text in the center of the region
            e.Graphics.DrawString("" + percent, font, Brushes.White, rect, format);
        }

        class GradientArcBorder
        {
            public PointF centre;
            public int innerRadius;
            public int width;
            public double startAngle;
            public double endAngle;
            public double limitAngle;
            public Color startColor;
            public Color endColor;
            public int steps;

            public GradientArcBorder(PointF centre, int innerRadius, int width, double startAngle, double endAngle,
                double limitAngle, Color startColor, Color endColor, int steps = 1440)
            {
                this.centre = centre;
                this.innerRadius = innerRadius;
                this.width = width;
                this.startAngle = startAngle;
                this.endAngle = endAngle;  
                this.limitAngle = limitAngle;
                this.startColor = startColor;
                this.endColor = endColor;
                this.steps = steps;
            }
        }

        void DrawArcByGradient(Graphics g, GradientArcBorder[] arcs)
        {
            foreach (GradientArcBorder item in arcs)
            {
                PointF centre = item.centre;
                int innerRadius = item.innerRadius;
                int width = item.width;
                double startAngle = item.startAngle;
                double endAngle = item.endAngle;
                double limitAngle = item.limitAngle;
                Color startColor = item.startColor;
                Color endColor = item.endColor;
                int steps = item.steps;

                if (startAngle > endAngle)
                    return;

                if (limitAngle < startAngle)
                    return;

                if (limitAngle > endAngle)
                    limitAngle = endAngle;

                int outerRadius = innerRadius + width;

                double rStart = startColor.R;
                double gStart = startColor.G;
                double bStart = startColor.B;

                double rEnd = endColor.R;
                double gEnd = endColor.G;
                double bEnd = endColor.B;

                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (SolidBrush br = new SolidBrush(startColor))
                {
                    double aInc = (endAngle - startAngle) / steps;
                    double rStep = (rEnd - rStart) / steps;
                    double gStep = (gEnd - gStart) / steps;
                    double bStep = (bEnd - bStart) / steps;

                    for (int i = 0; i < steps; i++)
                    {
                        double t1 = startAngle + i * aInc;
                        double t2 = t1 + aInc;
                        if (t2 > limitAngle)
                            break;

                        PointF p1 = new PointF((float)(outerRadius * Math.Cos(t1) + centre.X), (float)(outerRadius * Math.Sin(t1) + centre.Y));
                        PointF p2 = new PointF((float)(innerRadius * Math.Cos(t1) + centre.X), (float)(innerRadius * Math.Sin(t1) + centre.Y));
                        PointF p3 = new PointF((float)(innerRadius * Math.Cos(t2) + centre.X), (float)(innerRadius * Math.Sin(t2) + centre.Y));
                        PointF p4 = new PointF((float)(outerRadius * Math.Cos(t2) + centre.X), (float)(outerRadius * Math.Sin(t2) + centre.Y));

                        PointF[] pts = { p1, p2, p3, p4 };
                        br.Color = Color.FromArgb(255, (byte)(rStart + i * rStep), (byte)(gStart + i * gStep), (byte)(bStart + i * bStep));

                        g.FillPolygon(br, pts);
                    }
                }
            }
        }

    }
}