using System.Windows;

namespace MHMacro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow = this;
        }

        public void SetStateText(string text)
        {
            stateText.Text = text;
        }
    }
}