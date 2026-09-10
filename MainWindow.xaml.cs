using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using System.IO;

namespace Expie
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

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


            CreateDirectories();


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



        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        public static uint GetIdleTime()
        {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);

            if (GetLastInputInfo(ref lastInputInfo))
            {
                uint idleTime = (uint)Environment.TickCount - lastInputInfo.dwTime;
                return idleTime;
            }
            return 0;
        }


        private void CreateDirectories()
        {
            Directory.CreateDirectory(Path.Combine(_baseDir, "Gifs", "afk_end"));
            Directory.CreateDirectory(Path.Combine(_baseDir, "Gifs", "afk_loop"));
            Directory.CreateDirectory(Path.Combine(_baseDir, "Gifs", "afk_start"));
            Directory.CreateDirectory(Path.Combine(_baseDir, "Gifs", "appear"));
            Directory.CreateDirectory(Path.Combine(_baseDir, "Gifs", "dissapear"));
            Directory.CreateDirectory(Path.Combine(_baseDir, "Gifs", "interact"));
            Directory.CreateDirectory(Path.Combine(_baseDir, "Gifs", "random"));
        }



        const int AppearChance = 100;
        const int MinStealthTime = 2;
        const int RandomEventReverseChance = 1000;
        const int MinutesToAFK = 1;
        Rect workscreen = new Rect(0, -1, 100, 100);

        Random _rand = new Random(DateTime.Now.Microsecond);
        private DispatcherTimer _timer;
        const int _ticksPerSecond = 5;
        DateTime _stealthStartTime = DateTime.MinValue;
        bool _afk = false;
        string _baseDir = AppDomain.CurrentDomain.BaseDirectory;
        void TimerTick(object? sender, EventArgs? e)
        {
            if (AnimImage.Visibility == Visibility.Hidden)
            {
                if (_stealthStartTime.AddSeconds(MinStealthTime) <= DateTime.Now && _rand.Next(0, AppearChance) == 0)
                {
                    AnimImage.Visibility = Visibility.Visible;
                    AppearEvent();
                }
            }
            else if (!_afk)
            {
                if (GetIdleTime() > MinutesToAFK * 60 * 1000)
                {
                    AFKStartEvent();
                }
                else if (_rand.Next(0, RandomEventReverseChance) == 0)
                {
                    RandomEvent();
                }
            }
            else if (GetIdleTime() <= MinutesToAFK * 60 * 1000)
            {
                AFKEndEvent();
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
            _stealthStartTime = DateTime.Now;
        }

        private async void AppearEvent()
        {

        }

        private async void AFKStartEvent()
        {
            _afk = true;
        }

        private async void AFKEndEvent()
        {
            _afk = false;
        }

        private async void RandomEvent()
        {
            // HideEvent();
            // Random event realisation
        }
    }
}