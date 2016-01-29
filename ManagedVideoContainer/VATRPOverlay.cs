using System;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace VATRP.Linphone.VideoWrapper
{
    public class VATRPOverlay : Decorator
    {
        private VATRPTranslucentWindow commandBarWindow;
        private VATRPTranslucentWindow numpadWindow;
        private VATRPTranslucentWindow callInfoWindow;
        private VATRPTranslucentWindow callsSwitchWindow;
        private VATRPTranslucentWindow newCallAcceptWindow;
        private VATRPTranslucentWindow onHoldWindow;

        private System.Timers.Timer _timerCall;
        private int _foregroundCallDuration = 0;
        private int _backgroundCallDuration = 0;

        public VATRPOverlay()
        {
            commandBarWindow = new VATRPTranslucentWindow(this);
            numpadWindow = new VATRPTranslucentWindow(this);
            callInfoWindow = new VATRPTranslucentWindow(this);
            callsSwitchWindow = new VATRPTranslucentWindow(this);
            newCallAcceptWindow = new VATRPTranslucentWindow(this);
            onHoldWindow = new VATRPTranslucentWindow(this);

            _timerCall = new System.Timers.Timer
            {
                Interval = 1000,
                AutoReset = true
            };
            _timerCall.Elapsed += OnUpdatecallTimer;
        }

        #region Command Bar window
        public double CommandWindowLeftMargin
        {
            get
            {
                if (commandBarWindow != null)
                    return commandBarWindow.WindowLeftMargin;
                return 0;
            }
            set
            {
                if (commandBarWindow != null)
                    commandBarWindow.WindowLeftMargin = value;
            }
        }
        public double CommandWindowTopMargin
        {
            get
            {
                if (commandBarWindow != null)
                    return commandBarWindow.WindowTopMargin;
                return 0;
            }
            set
            {
                if (commandBarWindow != null)
                    commandBarWindow.WindowTopMargin = value;
            }
        }

        public int CommandOverlayWidth
        {
            get
            {
                if (commandBarWindow != null)
                    return commandBarWindow.OverlayWidth;
                return 0;
            }
            set
            {
                if (commandBarWindow != null)
                    commandBarWindow.OverlayWidth = value;
            }
        }

        public int CommandOverlayHeight
        {
            get
            {
                if (commandBarWindow != null)
                    return commandBarWindow.OverlayHeight;
                return 0;
            }
            set
            {
                if (commandBarWindow != null)
                    commandBarWindow.OverlayHeight = value;
            }
        }

        public void ShowCommandBar(bool bshow)
        {
            commandBarWindow.ShowWindow = bshow;
            commandBarWindow.Refresh();
            if (bshow)
            commandBarWindow.UpdateWindow();
        }

        public object OverlayCommandbarChild
        {
            get
            {
                if (commandBarWindow != null && commandBarWindow.TransparentWindow != null)
                {
                    return commandBarWindow.TransparentWindow.Content;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (commandBarWindow != null && commandBarWindow.TransparentWindow != null)
                {
                    commandBarWindow.TransparentWindow.Content = value;
                }
            }
        }

        #endregion

        #region Numpad window
        public double NumpadWindowLeftMargin
        {
            get
            {
                if (numpadWindow != null)
                    return numpadWindow.WindowLeftMargin;
                return 0;
            }
            set
            {
                if (numpadWindow != null)
                    numpadWindow.WindowLeftMargin = value;
            }
        }
        public double NumpadWindowTopMargin
        {
            get
            {
                if (numpadWindow != null)
                    return numpadWindow.WindowTopMargin;
                return 0;
            }
            set
            {
                if (numpadWindow != null)
                    numpadWindow.WindowTopMargin = value;
            }
        }

        public int NumpadOverlayWidth
        {
            get
            {
                if (numpadWindow != null)
                    return numpadWindow.OverlayWidth;
                return 0;
            }
            set
            {
                if (numpadWindow != null)
                    numpadWindow.OverlayWidth = value;
            }
        }

        public int NumpadOverlayHeight
        {
            get
            {
                if (numpadWindow != null)
                    return numpadWindow.OverlayHeight;
                return 0;
            }
            set
            {
                if (numpadWindow != null)
                    numpadWindow.OverlayHeight = value;
            }
        }

        public void ShowNumpadWindow(bool bshow)
        {
            numpadWindow.ShowWindow = bshow;
            numpadWindow.Refresh();
            if (bshow)
                numpadWindow.UpdateWindow();
        }
        
        public object OverlayNumpadChild
        {
            get
            {
                if (numpadWindow != null && numpadWindow.TransparentWindow != null)
                {
                    return numpadWindow.TransparentWindow.Content;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (numpadWindow != null && numpadWindow.TransparentWindow != null)
                {
                    numpadWindow.TransparentWindow.Content = value;
                }
            }
        }

        #endregion

        #region CallInfo window
        public double CallInfoWindowLeftMargin
        {
            get
            {
                if (callInfoWindow != null)
                    return callInfoWindow.WindowLeftMargin;
                return 0;
            }
            set
            {
                if (callInfoWindow != null)
                    callInfoWindow.WindowLeftMargin = value;
            }
        }
        public double CallInfoWindowTopMargin
        {
            get
            {
                if (callInfoWindow != null)
                    return callInfoWindow.WindowTopMargin;
                return 0;
            }
            set
            {
                if (callInfoWindow != null)
                    callInfoWindow.WindowTopMargin = value;
            }
        }

        public int CallInfoOverlayWidth
        {
            get
            {
                if (callInfoWindow != null)
                    return callInfoWindow.OverlayWidth;
                return 0;
            }
            set
            {
                if (callInfoWindow != null)
                    callInfoWindow.OverlayWidth = value;
            }
        }

        public int CallInfoOverlayHeight
        {
            get
            {
                if (callInfoWindow != null)
                    return callInfoWindow.OverlayHeight;
                return 0;
            }
            set
            {
                if (callInfoWindow != null)
                    callInfoWindow.OverlayHeight = value;
            }
        }
        public int ForegroundCallDuration
        {
            get { return _foregroundCallDuration; }
            set
            {
                _foregroundCallDuration = value; 
            }
        }

        public void SetCallerInfo(string callerInfo)
        {
            var textBlock =
                FindChild<TextBlock>(callInfoWindow.TransparentWindow, "CallerInfoLabel");
            if (textBlock != null)
                textBlock.Text = callerInfo;
        }

        public void SetCallState(string callState)
        {
            var textBlock =
                FindChild<TextBlock>(callInfoWindow.TransparentWindow, "CallStateLabel");
            if (textBlock != null)
            {
                textBlock.Text = callState;
                ShowOnHoldWindow(callState.Equals("On Hold"));
            }
        }

        public void StartCallTimer(int duration)
        {
            _foregroundCallDuration = duration;
            if (_timerCall != null && !_timerCall.Enabled)
                _timerCall.Start();
            UpdateCallDuration();
        }

        public void StopCallTimer()
        {
            _foregroundCallDuration = 0;
            if (_timerCall != null && _timerCall.Enabled)
                _timerCall.Stop();
        }

        private void OnUpdatecallTimer(object sender, ElapsedEventArgs e)
        {
            if (callInfoWindow.TransparentWindow.Dispatcher != null)
            {
                if (callInfoWindow.TransparentWindow.Dispatcher.Thread != Thread.CurrentThread)
                {
                    callInfoWindow.TransparentWindow.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        new EventHandler<ElapsedEventArgs>(OnUpdatecallTimer), sender, new object[] { e });
                    return;
                }

                _foregroundCallDuration++;
                UpdateCallDuration();

                if (callsSwitchWindow.ShowWindow)
                {
                    _backgroundCallDuration++;
                    UpdatebackgroundCallDuration();
                }
                _timerCall.Start();
            }
        }

        private void UpdateCallDuration()
        {
            var str = string.Empty;

            if (_foregroundCallDuration > 3599)
                str = string.Format("{0:D2}:{1:D2}:{2:D2}", _foregroundCallDuration / 3600, (_foregroundCallDuration / 60) % 60, _foregroundCallDuration % 60);
            else
                str = string.Format("{0:D2}:{1:D2}", _foregroundCallDuration / 60, _foregroundCallDuration % 60);
            var textBlock =
                FindChild<TextBlock>(callInfoWindow.TransparentWindow, "CallDurationLabel");
            if (textBlock != null)
            {
                textBlock.Text = str;
            }
        }
        
        public void ShowCallInfoWindow(bool bshow)
        {
            callInfoWindow.ShowWindow = bshow;
            callInfoWindow.Refresh();
            if (bshow)
                callInfoWindow.UpdateWindow();
        }

        public object OverlayCallInfoChild
        {
            get
            {
                if (callInfoWindow != null && callInfoWindow.TransparentWindow != null)
                {
                    return callInfoWindow.TransparentWindow.Content;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (callInfoWindow != null && callInfoWindow.TransparentWindow != null)
                {
                    callInfoWindow.TransparentWindow.Content = value;
                }
            }
        }

        #endregion

        #region onHoldWindow
        public double OnHoldWindowLeftMargin
        {
            get
            {
                if (onHoldWindow != null)
                    return onHoldWindow.WindowLeftMargin;
                return 0;
            }
            set
            {
                if (onHoldWindow != null)
                    onHoldWindow.WindowLeftMargin = value;
            }
        }
        public double OnHoldWindowTopMargin
        {
            get
            {
                if (onHoldWindow != null)
                    return onHoldWindow.WindowTopMargin;
                return 0;
            }
            set
            {
                if (onHoldWindow != null)
                    onHoldWindow.WindowTopMargin = value;
            }
        }

        public int OnHoldOverlayWidth
        {
            get
            {
                if (onHoldWindow != null)
                    return onHoldWindow.OverlayWidth;
                return 0;
            }
            set
            {
                if (onHoldWindow != null)
                    onHoldWindow.OverlayWidth = value;
            }
        }

        public int OnHoldOverlayHeight
        {
            get
            {
                if (onHoldWindow != null)
                    return onHoldWindow.OverlayHeight;
                return 0;
            }
            set
            {
                if (onHoldWindow != null)
                    onHoldWindow.OverlayHeight = value;
            }
        }

        public void ShowOnHoldWindow(bool bshow)
        {
            onHoldWindow.ShowWindow = bshow;
            onHoldWindow.Refresh();
            if (bshow)
            {
                onHoldWindow.UpdateWindow();
            }
        }

        public object OverlayOnHoldChild
        {
            get
            {
                if (onHoldWindow != null && onHoldWindow.TransparentWindow != null)
                {
                    return onHoldWindow.TransparentWindow.Content;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (onHoldWindow != null && onHoldWindow.TransparentWindow != null)
                {
                    onHoldWindow.TransparentWindow.Content = value;
                }
            }
        }

        #endregion

        #region CallsSwitch window
        public double CallsSwitchWindowLeftMargin
        {
            get
            {
                if (callsSwitchWindow != null)
                    return callsSwitchWindow.WindowLeftMargin;
                return 0;
            }
            set
            {
                if (callsSwitchWindow != null)
                    callsSwitchWindow.WindowLeftMargin = value;
            }
        }
        public double CallsSwitchWindowTopMargin
        {
            get
            {
                if (callsSwitchWindow != null)
                    return callsSwitchWindow.WindowTopMargin;
                return 0;
            }
            set
            {
                if (callsSwitchWindow != null)
                    callsSwitchWindow.WindowTopMargin = value;
            }
        }

        public int CallsSwitchOverlayWidth
        {
            get
            {
                if (callsSwitchWindow != null)
                    return callsSwitchWindow.OverlayWidth;
                return 0;
            }
            set
            {
                if (callsSwitchWindow != null)
                    callsSwitchWindow.OverlayWidth = value;
            }
        }

        public int CallsSwitchOverlayHeight
        {
            get
            {
                if (callsSwitchWindow != null)
                    return callsSwitchWindow.OverlayHeight;
                return 0;
            }
            set
            {
                if (callsSwitchWindow != null)
                    callsSwitchWindow.OverlayHeight = value;
            }
        }
        public int BackgroundCallDuration
        {
            get { return _backgroundCallDuration; }
            set
            {
                _backgroundCallDuration = value;
            }
        }

        private void UpdatebackgroundCallDuration()
        {
            var textBlock =
                FindChild<TextBlock>(callsSwitchWindow.TransparentWindow, "PausedCallDurationLabel");
            if (textBlock != null)
            {
                var str = string.Empty;

                if (_backgroundCallDuration > 3599)
                    str = string.Format("{0:D2}:{1:D2}:{2:D2}", _backgroundCallDuration / 3600, (_backgroundCallDuration / 60) % 60, _backgroundCallDuration % 60);
                else
                    str = string.Format("{0:D2}:{1:D2}", _backgroundCallDuration / 60, _backgroundCallDuration % 60);
                textBlock.Text = str;
            }
        }

        public void SetPausedCallerInfo(string callerInfo)
        {
            var textBlock =
                FindChild<TextBlock>(callsSwitchWindow.TransparentWindow, "PausedCallerInfoLabel");
            if (textBlock != null)
                textBlock.Text = callerInfo;
        }

        public void SetPausedCallState(string callState)
        {
            var textBlock =
                FindChild<TextBlock>(callsSwitchWindow.TransparentWindow, "PausedCallStateLabel");
            if (textBlock != null)
                textBlock.Text = callState;
        }

        public void StartPausedCallTimer(int duration)
        {
            _backgroundCallDuration = duration;
            UpdatebackgroundCallDuration();
        }

        public void StopPausedCallTimer()
        {
            _backgroundCallDuration = 0;
        }

        public void ShowCallsSwitchWindow(bool bshow)
        {
            callsSwitchWindow.ShowWindow = bshow;
            callsSwitchWindow.Refresh();
            if (bshow)
                callsSwitchWindow.UpdateWindow();
        }

        public object OverlayCallsSwitchChild
        {
            get
            {
                if (callsSwitchWindow != null && callsSwitchWindow.TransparentWindow != null)
                {
                    return callsSwitchWindow.TransparentWindow.Content;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (callsSwitchWindow != null && callsSwitchWindow.TransparentWindow != null)
                {
                    callsSwitchWindow.TransparentWindow.Content = value;
                }
            }
        }

        #endregion

        #region New Call window
        public double NewCallAcceptWindowLeftMargin
        {
            get
            {
                if (newCallAcceptWindow != null)
                    return newCallAcceptWindow.WindowLeftMargin;
                return 0;
            }
            set
            {
                if (newCallAcceptWindow != null)
                    newCallAcceptWindow.WindowLeftMargin = value;
            }
        }
        public double NewCallAcceptWindowTopMargin
        {
            get
            {
                if (newCallAcceptWindow != null)
                    return newCallAcceptWindow.WindowTopMargin;
                return 0;
            }
            set
            {
                if (newCallAcceptWindow != null)
                    newCallAcceptWindow.WindowTopMargin = value;
            }
        }

        public int NewCallAcceptOverlayWidth
        {
            get
            {
                if (newCallAcceptWindow != null)
                    return newCallAcceptWindow.OverlayWidth;
                return 0;
            }
            set
            {
                if (newCallAcceptWindow != null)
                    newCallAcceptWindow.OverlayWidth = value;
            }
        }

        public int NewCallAcceptOverlayHeight
        {
            get
            {
                if (newCallAcceptWindow != null)
                    return newCallAcceptWindow.OverlayHeight;
                return 0;
            }
            set
            {
                if (newCallAcceptWindow != null)
                    newCallAcceptWindow.OverlayHeight = value;
            }
        }

        public void SetNewCallerInfo(string callerInfo)
        {
            var textBlock =
                FindChild<TextBlock>(newCallAcceptWindow.TransparentWindow, "NewCallerInfoLabel");
            if (textBlock != null)
                textBlock.Text = callerInfo;
        }

        public void ShowNewCallAcceptWindow(bool bshow)
        {
            newCallAcceptWindow.ShowWindow = bshow;
            newCallAcceptWindow.Refresh();
            if (bshow)
                newCallAcceptWindow.UpdateWindow();
        }

        public object OverlayNewCallAcceptChild
        {
            get
            {
                if (newCallAcceptWindow != null && newCallAcceptWindow.TransparentWindow != null)
                {
                    return newCallAcceptWindow.TransparentWindow.Content;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (newCallAcceptWindow != null && newCallAcceptWindow.TransparentWindow != null)
                {
                    newCallAcceptWindow.TransparentWindow.Content = value;
                }
            }
        }

        public VATRPTranslucentWindow CommandBarWindow
        {
            get { return commandBarWindow; }
        }

        public VATRPTranslucentWindow NumpadWindow
        {
            get { return numpadWindow; }
        }

        public VATRPTranslucentWindow CallInfoWindow
        {
            get { return callInfoWindow; }
        }

        public VATRPTranslucentWindow CallsSwitchWindow
        {
            get { return callsSwitchWindow; }
        }

        public VATRPTranslucentWindow NewCallAcceptWindow
        {
            get { return newCallAcceptWindow; }
        }

        public VATRPTranslucentWindow OnHoldWindow
        {
            get { return onHoldWindow; }
        }
        #endregion

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            commandBarWindow.UpdateWindow();
            numpadWindow.UpdateWindow();
            callInfoWindow.UpdateWindow();
            callsSwitchWindow.UpdateWindow();
            newCallAcceptWindow.UpdateWindow();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            Refresh();
        }

        public void Refresh()
        {
            commandBarWindow.Refresh();
            numpadWindow.Refresh();
            callInfoWindow.Refresh();
            callsSwitchWindow.Refresh();
            newCallAcceptWindow.Refresh();
        }

        private static T FindChild<T>(DependencyObject parent, string childName)
   where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }
    }
}
