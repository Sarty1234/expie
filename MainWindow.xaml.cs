using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Expie
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Rect workscreen = new Rect(0, -1, 100, 100);

        public MainWindow()
        {
            InitializeComponent();

            this.WindowStartupLocation = WindowStartupLocation.Manual;
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WM_NCHITTEST = 0x0084;

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

            // Combines ToolWindow (hide from Alt+Tab) and Transparent (click-through)
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TOOLWINDOW | WM_NCHITTEST);


            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(1000 / _ticksPerSecond);
            _timer.Tick += TimerTick;
            _timer.Start();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Width = workscreen.Width;
            Height = workscreen.Height;
            AnimImage.Width = workscreen.Width;
            AnimImage.Height = workscreen.Height;

            if (workscreen.X >= 0)
            {
                Left = workscreen.X;
            } else
            {
                // SystemParameters.PrimaryScreenWidth
                Left = SystemParameters.WorkArea.Width + workscreen.X - workscreen.Width;
            }
            if (workscreen.Y >= 0)
            {
                Top = workscreen.Y;
            }
            else
            {
                Top = SystemParameters.WorkArea.Height + workscreen.Y - workscreen.Height;
            }
        }



        const int StealthDurationTime = 4;
        const int RandomEventReverseChance = 50;

        DateTime _CuriosityStart = DateTime.MinValue;
        Random _rand = new Random(DateTime.Now.Microsecond);
        private DispatcherTimer _timer;
        const int _ticksPerSecond = 5;
        void TimerTick(object? sender, EventArgs? e)
        {
            if (AnimImage.IsMouseOver)
            {
                _CuriosityStart = DateTime.Now;
            } 

            if (AnimImage.Visibility == Visibility.Hidden)
            {
                if (_CuriosityStart.AddSeconds(StealthDurationTime) <= DateTime.Now)
                {
                    AnimImage.Visibility = Visibility.Visible;
                    AppearEvent();
                }
            }
            else if (_rand.Next(0, RandomEventReverseChance) == 0)
            {
                RandomEvent();
            }
        }

        private void AnimImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            InteractEvent();
        }

        private void AnimImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            HideEvent();
        }




        private async void InteractEvent()
        {

        }
        
        private async void HideEvent()
        {
            // play hiding gif here
            await Task.Delay(100);


            AnimImage.Visibility = Visibility.Hidden;
            _CuriosityStart = DateTime.Now;
        }

        private async void AppearEvent()
        {

        }

        private async void AFKStartEvent()
        {

        }

        private async void AFKEndEvent()
        {

        }

        private async void RandomEvent()
        {
            HideEvent();
            // Random event realisation
        }
    }
}