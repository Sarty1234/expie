using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfAnimatedGif;

namespace Expie
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int AppearChance = 300;
        const int MinStealthTime = 60;
        const int RandomEventReverseChance = 1500;
        const int MinutesToAFK = 2;
        Rect workscreen = new Rect(0, -1, 100, 100);

        Random _rand = new Random(DateTime.Now.Microsecond);
        private DispatcherTimer _timer;
        const int _ticksPerSecond = 5;
        DateTime _stealthStartTime = DateTime.MinValue;
        bool _afk = false;
        string _baseDir = AppDomain.CurrentDomain.BaseDirectory;
        private bool _isPlaying = false;
        private RoutedEventHandler _currentCompletionHandler;
        string[] afk_endAnims, afk_loopAnims, loopAnims, dissapearAnims, interactAnims, randomAnims;




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
            LoadAnimations();


            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(1000 / _ticksPerSecond);
            _timer.Tick += TimerTick;
            _timer.Start();

            AppearEvent();
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
            Directory.CreateDirectory(Path.Combine(_baseDir, "Gifs", "loop"));
            Directory.CreateDirectory(Path.Combine(_baseDir, "Gifs", "dissapear"));
            Directory.CreateDirectory(Path.Combine(_baseDir, "Gifs", "interact"));
            Directory.CreateDirectory(Path.Combine(_baseDir, "Gifs", "random"));
        }




        private void LoadAnimations()
        {
            afk_endAnims = Directory.GetFiles(Path.Combine(_baseDir, "Gifs", "afk_end"), "*.gif");
            afk_loopAnims = Directory.GetFiles(Path.Combine(_baseDir, "Gifs", "afk_loop"), "*.gif");
            loopAnims = Directory.GetFiles(Path.Combine(_baseDir, "Gifs", "loop"), "*.gif");
            dissapearAnims = Directory.GetFiles(Path.Combine(_baseDir, "Gifs", "dissapear"), "*.gif");
            interactAnims = Directory.GetFiles(Path.Combine(_baseDir, "Gifs", "interact"), "*.gif");
            randomAnims = Directory.GetFiles(Path.Combine(_baseDir, "Gifs", "random"), "*.gif");
        }

        private void ClearAnimImage()
        {
            if (_currentCompletionHandler != null)
            {
                ImageBehavior.RemoveAnimationCompletedHandler(AnimImage, _currentCompletionHandler);
                _currentCompletionHandler = null;
            }

            var controller = ImageBehavior.GetAnimationController(AnimImage);
            if (controller != null)
            {
                controller.Dispose();
            }

            ImageBehavior.SetAnimatedSource(AnimImage, null);
            AnimImage.Source = null;
        }

        public static BitmapImage LoadOptimizedBitmap(string filePath)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(Path.GetFullPath(filePath), UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }


        private Task PlayAndWaitAsync(Image image)
        {
            var tcs = new TaskCompletionSource<bool>();

            _currentCompletionHandler = (sender, e) =>
            {
                ImageBehavior.RemoveAnimationCompletedHandler(image, _currentCompletionHandler);
                _currentCompletionHandler = null;
                tcs.TrySetResult(true);
            };

            ImageBehavior.AddAnimationCompletedHandler(image, _currentCompletionHandler);

            return tcs.Task;
        }


        private async Task PlayTransientAnimationAsync(string[] animFiles)
        {
            if (animFiles == null || animFiles.Length == 0) return;

            _isPlaying = true;
            try
            {
                ClearAnimImage();
                ImageBehavior.SetRepeatBehavior(AnimImage, new RepeatBehavior(1));
                ImageBehavior.SetAnimatedSource(AnimImage, LoadOptimizedBitmap(animFiles[_rand.Next(animFiles.Length)]));

                await PlayAndWaitAsync(AnimImage);
            }
            finally
            {
                _isPlaying = false;
            }

            AppearEvent();
        }




        void TimerTick(object? sender, EventArgs? e)
        {
            if (_isPlaying) return;

            if (AnimImage.Visibility == Visibility.Hidden)
            {
                if (_stealthStartTime.AddSeconds(MinStealthTime) <= DateTime.Now && _rand.Next(0, AppearChance) == 0)
                {
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
                    _ = PlayTransientAnimationAsync(randomAnims);
                }
            }
            else if (GetIdleTime() <= MinutesToAFK * 60 * 1000)
            {
                _afk = false;
                _ = PlayTransientAnimationAsync(afk_endAnims);
            }
            
        }

        private void AnimImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isPlaying) return;

            _ = PlayTransientAnimationAsync(interactAnims);
        }

        private async void AnimImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isPlaying) return;

            _isPlaying = true;
            try
            {
                if (dissapearAnims.Length > 0)
                {
                    ClearAnimImage();
                    ImageBehavior.SetRepeatBehavior(AnimImage, new RepeatBehavior(1));
                    ImageBehavior.SetAnimatedSource(AnimImage, LoadOptimizedBitmap(dissapearAnims[_rand.Next(dissapearAnims.Length)]));
                    await PlayAndWaitAsync(AnimImage);
                }

                ClearAnimImage();
                AnimImage.Visibility = Visibility.Hidden;
                _stealthStartTime = DateTime.Now;
            }
            finally
            {
                _isPlaying = false;
            }
        }



        private void AppearEvent()
        {
            AnimImage.Visibility = Visibility.Visible;

            if (loopAnims.Length > 0)
            {
                ClearAnimImage();

                ImageBehavior.SetRepeatBehavior(AnimImage, RepeatBehavior.Forever);
                ImageBehavior.SetAnimatedSource(AnimImage, LoadOptimizedBitmap(loopAnims[_rand.Next(0, loopAnims.Length)]));
            }
        }

        private void AFKStartEvent()
        {
            _afk = true;

            if (afk_loopAnims.Length > 0)
            {
                ClearAnimImage();

                ImageBehavior.SetRepeatBehavior(AnimImage, RepeatBehavior.Forever);
                ImageBehavior.SetAnimatedSource(AnimImage, LoadOptimizedBitmap(afk_loopAnims[_rand.Next(0, afk_loopAnims.Length)]));
            }
        }
    }
}