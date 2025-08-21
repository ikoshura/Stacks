using System.Windows;

namespace Stacks
{
    public partial class App : Application
    {
        private MainWindow? _mainWindow;
        private FanView? _fanView;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _mainWindow = new MainWindow();
            _fanView = new FanView();

            _mainWindow.Show();

            // --- PERBAIKAN UTAMA: Dengarkan event dari objek _mainWindow, bukan kelas MainWindow ---
            _mainWindow.WidgetClicked += OnWidgetClicked;
        }

        private void OnWidgetClicked()
        {
            if (_fanView == null || _mainWindow == null) return;

            if (_fanView.IsVisible)
            {
                _fanView.Hide();
            }
            else
            {
                Point widgetPosition = _mainWindow.PointToScreen(new Point(0, 0));
                _fanView.ShowAt(widgetPosition);
            }
        }
    }
}