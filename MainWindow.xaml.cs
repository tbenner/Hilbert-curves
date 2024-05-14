using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Point = System.Windows.Point;
using WPFPath = System.Windows.Shapes.Path;

/***********************************************************************************
 * This program draws a hilbert curve to the size and recursion depth you specify. *
 * I used the article I found below to base the Hilbert algorithm on.              *
 * https://www.fundza.com/algorithmic/space_filling/hilbert/basics/                *
 * I had originally used CoPilot to generate a WPF program for generating a        *
 * Hilbert curve, but the code did not work, but was close. I replaced it's        *
 * Hilbert code with the above articles and modified some of the WPF code.         *
 * The menu and dialog box was added by me.                                        *
 *                                                                                 *
 * Program features:                                                               *
 * > Generates the Hilbert curve in a variety of sizes and recursion depths        *
 * > Has a dialog for entering information for the Hilbert curve generation        *
 * > Can save the generated Hilbert curve data to a text file                      *
 * > Can save the Hilbert image to a PNG file                                      *
 *                                                                                 *
 * Written by: Tim Benner                                                          *
 * Date      : May 12, 2024                                                        *
 ***********************************************************************************/
namespace Hilbert
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string WINDOW_TITLE = "Hilbert curve";

        #region Fields
        PathFigure _figure;         // Used for displaying the hilbert curve
        List<Point> _points;        // Contains the generated points from the Hilbert curve
        Dialog _parmsDlg;           // Points to the Hilbert parameters dialog
        #endregion

        #region Constructors
        public MainWindow() {
            InitializeComponent();
        }
        #endregion

        #region Event Handlers
        // Selecting File|Exit does...what else? :-)
        private void fileExit_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        // Selecting File|New resets the program
        private void fileNew_Click(object sender, RoutedEventArgs e) {
            initializeHilbert();
        }

        // Selecting File|SavePoints saves the Hilbert point data to a text file
        private void fileSavePoints_Click(object sender, RoutedEventArgs e) {
            if (_points != null && _points.Count > 0) {
                SaveFileDialog sfd = new SaveFileDialog();
                if (sfd.ShowDialog() == true) {
                    string ext = System.IO.Path.GetExtension(sfd.FileName).Trim();
                    if (ext == String.Empty)
                        sfd.FileName = System.IO.Path.ChangeExtension(sfd.FileName, ".txt");

                    using (TextWriter tw = new StreamWriter(sfd.FileName)) {
                        foreach (Point pnt in _points) {
                            tw.WriteLine(pnt);
                        }
                    }
                }
            } else MessageBox.Show("No data to save");
        }

        // Selecting File|SaveImage saves the current image as a PNG file
        private void fileSaveImage_Click(object sender, RoutedEventArgs e) {
            if (_points != null && _points.Count > 0) {
                SaveFileDialog sfd = new SaveFileDialog();
                if (sfd.ShowDialog() == true) {
                    string ext = System.IO.Path.GetExtension(sfd.FileName).Trim();
                    if (ext == String.Empty)
                        sfd.FileName = System.IO.Path.ChangeExtension(sfd.FileName, ".png");

                    RenderTargetBitmap bitmap = new RenderTargetBitmap((int)canvas.Width, (int)canvas.Height, 96d, 96d, PixelFormats.Pbgra32);
                    bitmap.Render(canvas);

                    using (FileStream stream = new FileStream(sfd.FileName, FileMode.Create)) {
                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmap));
                        encoder.Save(stream);
                    }
                }
            } else MessageBox.Show("No image to save");
        }

        // Selecting Hilbert|Run will display a dialog and then draw the Hilbert curve
        private void hilbertRun_Click(object sender, RoutedEventArgs e) {
            initializeHilbert();
            _parmsDlg = new Dialog();
            if (_parmsDlg.ShowDialog() == true) {
                Vector XVector = new Vector(_parmsDlg.MaxLength, 0);    // Sets the max X axis size
                Vector YVector = new Vector(0, _parmsDlg.MaxLength);    // Sets the max Y axis size
                this.Background = _parmsDlg.BackColor;
                DrawHilbertCurve(XVector, YVector);
            }
        }
        #endregion

        #region Methods
        // Initializes the code for drawing the Hilbert curve
        private void DrawHilbertCurve(Vector XVector, Vector YVector) {
            PathGeometry lineGeometry = new PathGeometry();

            Point initialPoint = new Point(0, 0);   // Starting point
            setCanvasSize(XVector, YVector);
            // Set the starting point the first line segment will draw from
            Point startPoint = calculateStartingPoint(XVector, YVector, _parmsDlg.RecursionDepth);
            _figure = new PathFigure { StartPoint = startPoint };
            updateTitle(_parmsDlg.RecursionDepth);
            hilbert(lineGeometry, initialPoint.X, initialPoint.Y, XVector.X, XVector.Y, YVector.X, YVector.Y, _parmsDlg.RecursionDepth);
            // Do 2D WPF graphics stuff to display the curve on the canvas
            lineGeometry.Figures.Add(_figure);
            WPFPath path = new WPFPath { Stroke = _parmsDlg.BrushColor, StrokeThickness = 1, Data = lineGeometry };
            canvas.Children.Add(path);
        }

        /// <summary>
        /// Recursively calculates the Hilbert curve to the specified recursion depth
        /// </summary>
        /// <param name="geometry">The list of line segments to display</param>
        /// <param name="x0">Starting X point</param>
        /// <param name="y0">Starting Y point</param>
        /// <param name="xi">X component of first vector</param>
        /// <param name="xj">Y component of first vector</param>
        /// <param name="yi">X component of second vector</param>
        /// <param name="yj">Y component of second vector</param>
        /// <param name="n">Recursion depth</param>
        void hilbert(PathGeometry geometry, double x0, double y0, double xi, double xj, double yi, double yj, int n) {
            if (n <= 0) {
                double X = x0 + (xi + yi)/2;
                double Y = y0 + (xj + yj)/2;
                // Create a line segment on the Hilbert curve and add it to the Segments collection
                LineSegment lineSegment = new LineSegment(new Point(X, Y), true);
                _figure.Segments.Add(lineSegment);

                // Save the generated point to a list
                _points.Add(new Point(X, Y));
            } else {
                hilbert(geometry, x0, y0, yi/2, yj/2, xi/2, xj/2, n - 1);
                hilbert(geometry, x0 + xi/2, y0 + xj/2, xi/2, xj/2, yi/2, yj/2, n - 1);
                hilbert(geometry, x0 + xi/2 + yi/2, y0 + xj/2 + yj/2, xi/2, xj/2, yi/2, yj/2, n - 1);
                hilbert(geometry, x0 + xi/2 + yi, y0 + xj/2 + yj, -yi/2, -yj/2, -xi/2, -xj/2, n - 1);
            }
        }

        // Sets some important variables to their default values
        void initializeHilbert() {
            _points = new List<Point>();
            canvas.Children.Clear();
            canvas.Width = 0;
            canvas.Height = 0;
        }

        /// <summary>
        /// Calculates the start point for the Hilbert curve.
        /// </summary>
        /// <param name="vectorX">Vector on the X axis</param>
        /// <param name="vectorY">Vector on the Y axis</param>
        /// <param name="recDepth">Depth of the recursion</param>
        /// <returns>Starting point of Hilbert curve</returns>
        Point calculateStartingPoint(Vector vectorX, Vector vectorY, int recDepth) {
            double vectorXMax = Math.Max(vectorX.X, vectorX.Y);
            double vectorYMax = Math.Max(vectorY.X, vectorY.Y);
            double startX = vectorXMax / (Math.Pow(2, recDepth) * 2);
            double startY = vectorYMax / (Math.Pow(2, recDepth) * 2);
            return new Point(startX, startY);
        }

        /// <summary>
        /// Updates the window's title with the recursion depth
        /// </summary>
        /// <param name="depth">Depth of the recursion</param>
        void updateTitle(int depth) {
            this.Title = String.Format("{0} - recursion depth = {1}", WINDOW_TITLE, depth);
        }

        /// <summary>
        /// Will set the size of the canvas.
        /// This ensures the scrollbars will display if needed.
        /// </summary>
        /// <param name="vectorX">Vector on the X axis</param>
        /// <param name="vectorY">Vector on the Y axis</param>
        void setCanvasSize(Vector vectorX, Vector vectorY) {
            double vectorXMax = Math.Max(vectorX.X, vectorX.Y);
            double vectorYMax = Math.Max(vectorY.X, vectorY.Y);
            canvas.Width = vectorXMax;
            canvas.Height = vectorYMax;
        }
        #endregion

    }
}