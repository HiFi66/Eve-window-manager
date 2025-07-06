// MainWindow.xaml.cs
using System;
using System.Windows;
using System.Windows.Threading;

namespace EveLayoutManager
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _timer;
        private readonly WindowManager _windowManager;

        public MainWindow()
        {
            InitializeComponent();
            _windowManager = new WindowManager("layouts.json");
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _windowManager.CheckAndRestoreWindows();
        }

        private void SaveLayoutButton_Click(object sender, RoutedEventArgs e)
        {
            _windowManager.SaveLayouts();
            MessageBox.Show("Asettelut tallennettu.", "Tallennus", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}