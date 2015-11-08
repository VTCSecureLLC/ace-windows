using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using com.vtcsecure.ace.windows.CustomControls;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Services;
using VATRP.LinphoneWrapper.Enums;
using System.Timers;


namespace com.vtcsecure.ace.windows.Views
{
    /// <summary>
    /// Interaction logic for CallView.xaml
    /// </summary>
    public partial class CallView
    {
        public CallOverlayView AttachedControlsView { get; set; }
        
        private System.Timers.Timer dimmerTimer;
        private bool _showAttachedWindow;
        private bool _mouseIsOn;
        private Rect windowRect;
        private System.Windows.Threading.DispatcherTimer cursorTimer;

        public CallView()
            : base(VATRPWindowType.REMOTE_VIDEO_VIEW)
        {
            InitializeComponent();
            InitializeCursorMonitoring();
            AttachedControlsView = null;
            dimmerTimer = new System.Timers.Timer { AutoReset = false, Interval = 500 };
            dimmerTimer.Elapsed += OnDimmerTimerElapsed;

        }
        private void OnDimmerTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Dispatcher.Thread != Thread.CurrentThread)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new EventHandler<ElapsedEventArgs>(OnDimmerTimerElapsed), sender, new object[] { e });
                return;
            }
            
            if (_mouseIsOn)
            {
                _showAttachedWindow = true;
                ShowAttachedView(true);
                ArrangeAttachedView();
            }
            else
            {
                ShowAttachedView(false);
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            base.Window_Unloaded(sender, e);
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            Resize(MSVideoSize.MS_VIDEO_SIZE_CIF_W,
                MSVideoSize.MS_VIDEO_SIZE_CIF_H);

            ArrangeAttachedView();
        }

        private void ArrangeAttachedView()
        {

            if (AttachedControlsView == null || this.ActualWidth == 0 || this.ActualHeight == 0)
                return;

            windowRect.X = this.Left;
            windowRect.Y = this.Top;
            windowRect.Width = this.ActualWidth;
            windowRect.Height = this.ActualHeight;

            AttachedControlsView.Left = this.Left + (this.ActualWidth - AttachedControlsView.Width)/2;
            AttachedControlsView.Top = this.Top + this.ActualHeight - AttachedControlsView.ActualHeight - 20;
        }

        internal void Resize(VATRP.LinphoneWrapper.Enums.MSVideoSize width,
            VATRP.LinphoneWrapper.Enums.MSVideoSize height)
        {
            this.Width = Convert.ToInt32(width);
            this.Height = Convert.ToInt32(height);

        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ArrangeAttachedView();
            base.Window_SizeChanged(sender, e);
        }

        private void OnLocationChanged(object sender, EventArgs e)
        {
            ArrangeAttachedView();
            base.Window_LocationChanged(sender, e);
        }

        private void OnStateChanged(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Normal:
                    break;
                case WindowState.Minimized:
                    ShowAttachedView(false);
                    break;
                case WindowState.Maximized:
                    ArrangeAttachedView();
                    break;
            }

            base.Window_StateChanged(sender, e);
        }

        private void ShowAttachedView(bool bShow)
        {
            if (AttachedControlsView != null )
            {
                if (bShow )
                {
                    if (!AttachedControlsView.EndCallRequested && _showAttachedWindow)
                    {
                        AttachedControlsView.Show();
                    }
                }
                else
                    AttachedControlsView.Hide();
            }
        }

        private void SwitchVideoPanel(bool bOn)
        {
            var _linphone = ServiceManager.Instance.LinphoneService;
            if (_linphone != null)
            {
                if (!bOn)
                {
                    _linphone.SetVideoCallWindowHandle(IntPtr.Zero, true);
                }
                else
                {
                    var source = GetWindow(this);
                    if (source != null)
                    {
                        var wih = new WindowInteropHelper(source);
                        IntPtr hWnd = wih.EnsureHandle();
                        if (hWnd != IntPtr.Zero)
                        {
                            _linphone.SetVideoCallWindowHandle(hWnd);
                        }

                    }
                }
            }
        }

        private void OnVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool bShow = Convert.ToBoolean(e.NewValue);
            SwitchVideoPanel(bShow);
        }

        private void VATRPWindow_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsActivated && !AttachedControlsView.EndCallRequested)
            {
                Activate();
            }
        }

        private void VATRPWindow_Deactivated(object sender, EventArgs e)
        {

        }

        private void OnWindowActivated(object sender, EventArgs e)
        {
            base.Window_Activated(sender, e);
            AttachedControlsView.Activate();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            dimmerTimer.Stop();
            dimmerTimer = null;
            cursorTimer.Stop();
            cursorTimer = null;
        }

        private void InitializeCursorMonitoring()
        {
            cursorTimer = new System.Windows.Threading.DispatcherTimer();
            cursorTimer.Tick += TimerOnTick;
            cursorTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            cursorTimer.Start();
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            var point = Mouse.GetPosition(this);
            var x = windowRect.Left + point.X;
            var y = point.Y + windowRect.Top;
            if (x > windowRect.Left && y > windowRect.Top &&
                x < windowRect.Right && y < windowRect.Bottom)
            {
                _mouseIsOn = true;
                if (dimmerTimer != null)
                {
                    if (AttachedControlsView != null && !AttachedControlsView.IsVisible && !dimmerTimer.Enabled)
                    {
                        dimmerTimer.Start();
                    }
                }
            }
            else
            {
                _mouseIsOn = false;
                // mouse is moving outside view, start dimm timer to hide controls panel
                if (dimmerTimer != null)
                {
                    if (AttachedControlsView != null && AttachedControlsView.IsVisible  )
                    {
                        if (!dimmerTimer.Enabled)
                        {
                            dimmerTimer.Start();
                        }
                    }
                }
            }
        }
    }
}
