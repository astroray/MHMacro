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
            Loaded                                  += OnLoaded;
            ((App) Application.Current).StateChange += OnStateChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow = this;
        }

        private void OnStateChanged(bool _enabled)
        {
            var text = _enabled ? "활성" : "비활성";
            stateText.Text = $"현재상태 : {text}";
        }
    }
}