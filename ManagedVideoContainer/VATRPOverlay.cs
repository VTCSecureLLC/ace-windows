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
        private System.Timers.Timer _timerCall;
        private int _duration = 0;
        public VATRPOverlay()
        {
            commandBarWindow = new VATRPTranslucentWindow(this);
            numpadWindow = new VATRPTranslucentWindow(this);
            callInfoWindow = new VATRPTranslucentWindow(this);
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

        #region CalInfo window
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
                textBlock.Text = callState;
        }

        public void StartCallTimer(int duration)
        {
            _duration = duration;
            if (_timerCall != null) 
                _timerCall.Start();
            UpdateCallDuration();
        }

        public void StopCallTimer()
        {
            _duration = 0;
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
                        new EventHandler<ElapsedEventArgs>(OnUpdatecallTimer), sender, new object[] {e});
                    return;
                }
                
                _duration++;
                UpdateCallDuration();
                _timerCall.Start();
            }
        }

        private void UpdateCallDuration()
        {
            var str = string.Empty;

            if (_duration > 3599)
                str = string.Format("{0:D2}:{1:D2}:{2:D2}", _duration/3600, (_duration/60)%60, _duration%60);
            else
                str = string.Format("{0:D2}:{1:D2}", _duration/60, _duration%60);
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

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            commandBarWindow.UpdateWindow();
            numpadWindow.UpdateWindow();
            callInfoWindow.UpdateWindow();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            Refresh();
        }

        private void Refresh()
        {
            commandBarWindow.Refresh();
            numpadWindow.Refresh();
            callInfoWindow.Refresh();
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
