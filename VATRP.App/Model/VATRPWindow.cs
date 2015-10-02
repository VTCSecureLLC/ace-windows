using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using VATRP.App.Services;
using VATRP.Core.Model;

namespace VATRP.App.Model
{
    public class VATRPWindow : Window
    {
        private readonly System.Timers.Timer moveDetectionTimer;
        public bool DestroyOnClosing { get; set; }
        public  VATRPWindowType WindowType { get; private set; }
        
        protected VATRPWindow(VATRPWindowType wndType)
        {
            IsActivated = false;
            WindowType = wndType;
            DestroyOnClosing = false;
            moveDetectionTimer = new System.Timers.Timer {AutoReset = false, Interval = 1000};
            moveDetectionTimer.Elapsed += OnMoveTimerElapsed;
        }
        private void OnMoveTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new EventHandler<System.Timers.ElapsedEventArgs>(this.OnMoveTimerElapsed), sender, new object[] { e });
                return;
            }

            SaveWindowLocation();
        }

        public bool IsActivated { get; private set; }

        protected void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (moveDetectionTimer.Enabled)
                moveDetectionTimer.Stop();
            moveDetectionTimer.Start();
        }

        protected void Window_Initialized(object sender, EventArgs e)
        {
            Configuration.ConfSection section = ConfSectionFromWindow(WindowType);

            var ws = WindowState.Normal;
            try
            {
                var wsText = ServiceManager.Instance.ConfigurationService.Get(section,
                Configuration.ConfEntry.WINDOW_STATE, "Normal");

                ws = (WindowState) Enum.Parse(typeof (WindowState), wsText, true);
            }
            catch (Exception ex)
            {
                ws = WindowState.Normal;
            }

            this.WindowState = ws;

            try
            {
                this.Left = Convert.ToInt32(ServiceManager.Instance.ConfigurationService.Get(section,
                    Configuration.ConfEntry.WINDOW_LEFT, GetWindowDefaultCoordinates(WindowType).X.ToString()));
            }
            catch (FormatException)
            {
                this.Left = GetWindowDefaultCoordinates(WindowType).X;
            }
            catch (Exception ex)
            {
                
            }

            try
            {
                this.Top = Convert.ToInt32(ServiceManager.Instance.ConfigurationService.Get(section,
                    Configuration.ConfEntry.WINDOW_TOP, GetWindowDefaultCoordinates(WindowType).Y.ToString()));
            }
            catch (FormatException)
            {
                this.Top = GetWindowDefaultCoordinates(WindowType).Y;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            
            try
            {
                this.Width = Convert.ToInt32(ServiceManager.Instance.ConfigurationService.Get(section,
                    Configuration.ConfEntry.WINDOW_WIDTH, "300"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            try
            {
                this.Height = Convert.ToInt32(ServiceManager.Instance.ConfigurationService.Get(section,
                    Configuration.ConfEntry.WINDOW_HEIGHT, "400"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }


        protected void Window_Activated(object sender, EventArgs e)
        {
            IsActivated = true;
        }

        protected void Window_Closing(object sender, CancelEventArgs e)
        {
            if (DestroyOnClosing)
                return;
            if (!App.AllowDestroyWindows)
            {
                switch (WindowType)
                {
                    case VATRPWindowType.CALL_VIEW:
                    case VATRPWindowType.REMOTE_VIDEO_VIEW:
                    break;
                    default:
                        Hide();
                        break;
                }
                e.Cancel = true;
            }
        }

        protected void Window_StateChanged(object sender, EventArgs e)
        {
            Configuration.ConfSection section = ConfSectionFromWindow(WindowType);
            ServiceManager.Instance.ConfigurationService.Set(section,
                Configuration.ConfEntry.WINDOW_STATE, this.WindowState.ToString());

        }

        protected void Window_LocationChanged(object sender, EventArgs e)
        {
            // save location
            if (moveDetectionTimer.Enabled)
                moveDetectionTimer.Stop();
            moveDetectionTimer.Start();
        }

        protected void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (moveDetectionTimer.Enabled)
            {
                moveDetectionTimer.Stop();
            }
            SaveWindowLocation();
        }

        private void SaveWindowLocation()
        {
            Configuration.ConfSection section = ConfSectionFromWindow(WindowType);
            ServiceManager.Instance.ConfigurationService.Set(section,
                Configuration.ConfEntry.WINDOW_LEFT, this.Left.ToString());
            ServiceManager.Instance.ConfigurationService.Set(section,
                Configuration.ConfEntry.WINDOW_TOP, this.Top.ToString());
            ServiceManager.Instance.ConfigurationService.Set(section,
                Configuration.ConfEntry.WINDOW_WIDTH, this.ActualWidth.ToString());
            ServiceManager.Instance.ConfigurationService.Set(section,
                Configuration.ConfEntry.WINDOW_HEIGHT, this.ActualHeight.ToString());
        }

        private static Configuration.ConfSection ConfSectionFromWindow(VATRPWindowType wndType)
        {
            switch (wndType)
            {
                case VATRPWindowType.CALL_VIEW:
                    return Configuration.ConfSection.CALL_WINDOW;
                case VATRPWindowType.CONTACT_VIEW:
                    return Configuration.ConfSection.CONTACT_WINDOW;
                case VATRPWindowType.DIALPAD_VIEW:
                    return Configuration.ConfSection.DIALPAD_WINDOW;
                case VATRPWindowType.MAIN_VIEW:
                    return Configuration.ConfSection.MAIN_WINDOW;
                case VATRPWindowType.MESSAGE_VIEW:
                    return Configuration.ConfSection.MESSAGE_WINDOW;
                case VATRPWindowType.RECENTS_VIEW:
                    return Configuration.ConfSection.HISTORY_WINDOW;
                case VATRPWindowType.SELF_VIEW:
                    return Configuration.ConfSection.SELF_WINDOW;
                case VATRPWindowType.SETTINGS_VIEW:
                    return Configuration.ConfSection.SETTINGS_WINDOW;
                case VATRPWindowType.REMOTE_VIDEO_VIEW:
                    return Configuration.ConfSection.REMOTE_VIDEO_VIEW;
                default:
                    throw new ArgumentOutOfRangeException("wndType");
            }
        }

        private static Point GetWindowDefaultCoordinates(VATRPWindowType wndType)
        {
            switch (wndType)
            {
                case VATRPWindowType.CALL_VIEW:
                    return new Point() {X = 100, Y = 100};
                case VATRPWindowType.CONTACT_VIEW:
                    return new Point() { X = 300, Y = 100 };
                case VATRPWindowType.DIALPAD_VIEW:
                    return new Point() { X = 100, Y = 100 };
                case VATRPWindowType.MAIN_VIEW:
                    return new Point() { X = 300, Y = 400 };
                case VATRPWindowType.MESSAGE_VIEW:
                    return new Point() { X = 300, Y = 100 };
                case VATRPWindowType.RECENTS_VIEW:
                    return new Point() { X = 500, Y = 100 };
                case VATRPWindowType.SELF_VIEW:
                    return new Point() { X = 100, Y = 400 };
                case VATRPWindowType.SETTINGS_VIEW:
                    return new Point() { X = 100, Y = 400 };
                case VATRPWindowType.REMOTE_VIDEO_VIEW:
                    return new Point() { X = 100, Y = 400 };
                default:
                    throw new ArgumentOutOfRangeException("wndType");
            }
        }
    }
}
