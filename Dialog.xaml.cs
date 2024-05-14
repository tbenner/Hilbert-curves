using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Hilbert
{
    public partial class Dialog : Window, INotifyPropertyChanged
    {
        #region Fields
        int _recDepth = 4;
        int _maxLength = 1000;
        ListBoxItem _brushColor;
        ListBoxItem _backColor;
        #endregion

        #region Constructors
        public Dialog() {
            InitializeComponent();
            fillComboBoxWithBrushes(cbBrushes);
            fillComboBoxWithBrushes(cbBackBrushes);
            BrushColor = Brushes.Black;
            BackColor = Brushes.Wheat;
            DataContext = this;
        }
        #endregion

        #region Properties
        // Recursion depth of the Hilbert curve
        public int RecursionDepth {
            get => _recDepth;
            set {
                _recDepth = value;
                OnPropertyChanged("RecursionDepth");
            }
        }

        // Maximum size of the Hilbert curve on both axes
        public int MaxLength {
            get => _maxLength;
            set {
                _maxLength = value;
                OnPropertyChanged("MaxLength");
            }
        }

        // ListBoxItem which holds the color of the lines
        public ListBoxItem ColorItem {
            get => _brushColor;
            set {
                _brushColor = value;
                BrushColor = value.Background as SolidColorBrush;
                OnPropertyChanged("ColorItem");
            }
        }

        // ListBoxItem which holds the canvas background color
        public ListBoxItem BackColorItem {
            get => _backColor;
            set {
                _backColor = value;
                BackColor = value.Background as SolidColorBrush;
                OnPropertyChanged("BackColorItem");
            }
        }

        // Contains the Hilbert line color from a ListBoxItem
        public SolidColorBrush BrushColor { get; set; }
        // Contains the Canvas background color from a ListBoxItem
        // TODO: Could probably create an IValueConverter to do this
        public SolidColorBrush BackColor { get; set; }
        #endregion

        #region INotifyPropertyChanged interface methods
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string info = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        #endregion

        #region Event Handlers
        private void btnOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
            //SolidColorBrush scb = BrushColor.Background as SolidColorBrush;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            this.Close();
        }
        #endregion

        #region Methods
        public void fillComboBoxWithBrushes(ComboBox brushesBox) {
            // Get all properties of the Brushes class
            var brushProperties = typeof(Brushes).GetProperties();

            // Loop through each property
            foreach (var brushProperty in brushProperties) {
                // Create a new ListBoxItem
                ListBoxItem item = new ListBoxItem();

                // Set the content to the name of the brush
                item.Content = brushProperty.Name;

                // Set the background to the brush
                item.Background = (Brush)brushProperty.GetValue(null);

                // Add the ListBoxItem to the ListBox
                brushesBox.Items.Add(item);
            }
        }
        #endregion
    }
}
