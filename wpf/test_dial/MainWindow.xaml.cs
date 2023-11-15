using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace test_dial
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DialupControl dialup = new DialupControl();

            dialup.Radius = 120;
            dialup.BorderWidth = 40;
            dialup.Percent = 80;
            dialup.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x20, 0x43));
            dialup.RailwayColor = Color.FromArgb(0x20, 0x00, 0xFF, 0xFF);
            dialup.FontSize = 80;

            Color cFirst = Color.FromArgb(0xFF, 0x00, 0x30, 0x90);
            Color cSecond = Color.FromArgb(0xFF, 0xC9, 0, 0);
            Color cThird = Color.FromArgb(0xFF, 0xFF, 0, 0);
            Color cFourth = Color.FromArgb(0xFF, 0xD7, 0xEF, 0x1C);

            dialup.AddArc(-90, 30, cFirst, cSecond);
            dialup.AddArc(30, 90, cSecond, cThird);
            dialup.AddArc(90, 270, cThird, cFourth, 2880);

            // Add the dialup control to the visual tree
            Container.Children.Add(dialup);
        }

        public class DialupControl : Control
        {
            private bool _isMouseDown;
            private Point _previousMousePosition;
            private double _mouseMoveThreshold = 20;

            const float degreeRate = (float)(Math.PI / 180);

            public double FontSize
            {
                get { return base.FontSize; }
                set
                {
                    base.FontSize = value;
                    UpdateText();
                }
            }

            public FontFamily FontFamily
            {
                get { return base.FontFamily; }
                set
                {
                    base.FontFamily = value;
                    UpdateText();
                }
            }

            public Brush Foreground
            {
                get { return base.Foreground; }
                set
                {
                    base.Foreground = value;
                    UpdateText();
                }
            }

            FormattedText _formattedText;

            Point _centre;
            public Point Centre
            {
                get { return _centre; }
                set { _centre = value; }
            }

            double _limitAngle;
            float _percent;
            public float Percent
            {
                get { return _percent; }
                set
                {
                    _percent = value;
                    _limitAngle = (-90 + 360 * _percent / 100) * degreeRate;
                    UpdateText();
                }
            }

            float _interval;
            public float Interval
            {
                get { return _interval; }
                set { _interval = value; }
            }

            int _radius;
            public int Radius
            {
                get { return _radius; }
                set { _radius = value; }
            }

            int _borderWidth;
            public int BorderWidth
            {
                get { return _borderWidth; }
                set
                {
                    _borderWidth = value;
                    _railwayPen.Thickness = _borderWidth;
                }
            }

            private Color[] _gradientColors;
            public Color[] GradientColors
            {
                get { return _gradientColors; }
                set { _gradientColors = value; }
            }

            private Pen _railwayPen;
            private Color _railwayColor;
            public Color RailwayColor
            {
                get { return _railwayColor; }
                set
                {
                    _railwayColor = value;
                    _railwayPen.Brush = new SolidColorBrush(_railwayColor);
                }
            }

            ArrayList _arcs;

            public DialupControl()
            {
                _arcs = new ArrayList();

                _radius = 100;
                _borderWidth = 40;

                _percent = 80;
                _interval = 1;

                base.FontSize = 50;
                base.FontFamily = new FontFamily("Arial");
                base.Foreground = new SolidColorBrush(Colors.White);

                _railwayPen = new Pen(new SolidColorBrush(Colors.White), _borderWidth);

                AddHandler(MouseDownEvent, new MouseButtonEventHandler(OnMouseDown));
                AddHandler(MouseUpEvent, new MouseButtonEventHandler(OnMouseUp));
                AddHandler(MouseMoveEvent, new MouseEventHandler(OnMouseMove));
                AddHandler(MouseWheelEvent, new MouseWheelEventHandler(OnMouseWheel));
            }

            [Obsolete]
            private void UpdateText()
            {
                _formattedText = new FormattedText("" + _percent, CultureInfo.CurrentCulture,
                   FlowDirection.LeftToRight, new Typeface(this.FontFamily.Source), this.FontSize, Foreground);
                InvalidateVisual();
            }

            public void AddArc(double startAngle, double endAngle, Color startColor, Color endColor, int steps = 1440)
            {
                _arcs.Add(new GradientArcBorder(degreeRate * startAngle, degreeRate * endAngle, startColor, endColor, steps));
            }

            #region Events

            private void OnMouseWheel(object sender, MouseWheelEventArgs e)
            {
                float d = e.Delta / 120; // Mouse wheel 1 click (120 delta) = 1 step
                Percent += d * _interval;
                if (Percent >= 100)
                    Percent = 100;
                else if (Percent <= 0)
                    Percent = 0;
            }

            private void OnMouseDown(object sender, MouseButtonEventArgs e)
            {
                _isMouseDown = true;
                _previousMousePosition = e.GetPosition((DialupControl)sender);
            }

            private void OnMouseMove(object sender, MouseEventArgs e)
            {
                if (_isMouseDown)
                {
                    Point newMousePosition = e.GetPosition((DialupControl)sender);
                    double dY = (_previousMousePosition.Y - newMousePosition.Y);
                    if (Math.Abs(dY) > _mouseMoveThreshold)
                    {
                        Percent += Math.Sign(dY) * _interval;
                        _previousMousePosition = newMousePosition;
                        if (Percent >= 100)
                            Percent = 100;
                        else if (Percent <= 0)
                            Percent = 0;
                    }
                }
            }

            protected void OnMouseUp(object sender, MouseButtonEventArgs e)
            {
                _isMouseDown = false;
                (sender as DialupControl).ReleaseMouseCapture();
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                // Call the base OnRender method to draw the standard control elements
                base.OnRender(drawingContext);

                _centre.X = ActualWidth / 2;
                _centre.Y = ActualHeight / 2;

                // Clear the background
                drawingContext.DrawRectangle(Background, null, new Rect(0, 0, ActualWidth, ActualHeight));

                // Draw railway
                drawingContext.DrawEllipse(Brushes.Transparent, _railwayPen, _centre, _radius + _borderWidth / 2, _radius + _borderWidth / 2);

                // Draw gradient arc
                DrawArcByGradient(drawingContext);

                // Draw percent text
                Point textOrigin = new Point(((ActualWidth - _formattedText.Width) / 2),
                               ((ActualHeight - _formattedText.Height) / 2));
                drawingContext.DrawText(_formattedText, textOrigin);
            }
            #endregion

            void DrawPolygon(DrawingContext g, StreamGeometry streamGeometry, SolidColorBrush br, Point[] pts)
            {
                using (StreamGeometryContext geometryContext = streamGeometry.Open())
                {
                    geometryContext.BeginFigure(pts[0], true, true);
                    geometryContext.PolyLineTo(pts, true, true);
                }

                g.DrawGeometry(br, null, streamGeometry);
            }

            void DrawArcByGradient(DrawingContext g)
            {
                foreach (GradientArcBorder arc in _arcs)
                {
                    double limitAngle = _limitAngle;
                    double startAngle = arc.startAngle;
                    double endAngle = arc.endAngle;
                    Color startColor = arc.startColor;
                    Color endColor = arc.endColor;
                    int steps = arc.steps;

                    if (startAngle > endAngle)
                        return;

                    if (limitAngle < startAngle)
                        return;

                    if (limitAngle > endAngle)
                        limitAngle = endAngle;

                    int outerRadius = _radius + _borderWidth;

                    double rStart = startColor.R;
                    double gStart = startColor.G;
                    double bStart = startColor.B;

                    double rEnd = endColor.R;
                    double gEnd = endColor.G;
                    double bEnd = endColor.B;


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

                        Point p1 = new Point(outerRadius * Math.Cos(t1) + _centre.X, outerRadius * Math.Sin(t1) + _centre.Y);
                        Point p2 = new Point(_radius * Math.Cos(t1) + _centre.X, _radius * Math.Sin(t1) + _centre.Y);
                        Point p3 = new Point(_radius * Math.Cos(t2) + _centre.X, _radius * Math.Sin(t2) + _centre.Y);
                        Point p4 = new Point(outerRadius * Math.Cos(t2) + _centre.X, outerRadius * Math.Sin(t2) + _centre.Y);

                        Point[] pts = { p1, p2, p3, p4 };
                        SolidColorBrush br = new SolidColorBrush(
                            Color.FromArgb(255, (byte)(rStart + i * rStep), (byte)(gStart + i * gStep), (byte)(bStart + i * bStep)));
                        StreamGeometry streamGeometry = new StreamGeometry();
                        DrawPolygon(g, streamGeometry, br, pts);
                    }
                }
            }

            class GradientArcBorder
            {
                public double startAngle;
                public double endAngle;
                public Color startColor;
                public Color endColor;
                public int steps;

                public GradientArcBorder(double startAngle, double endAngle, Color startColor, Color endColor, int steps = 1440)
                {
                    this.startAngle = startAngle;
                    this.endAngle = endAngle;
                    this.startColor = startColor;
                    this.endColor = endColor;
                    this.steps = steps;
                }
            }
        }
    }
}
