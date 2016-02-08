using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace VATRP.Linphone.VideoWrapper
{
    public class VATRPTranslucentWindow
    {
        private Window _window;
        private Window _parent;
        private VATRPOverlay _container;
        double windowTopMargin;
        double windowLeftMargin;

        public VATRPTranslucentWindow(VATRPOverlay decorator)
        {
            _container = decorator;
            _window = new Window();
            
            //Make the window itself transparent, with no style.
            _window.Background = Brushes.Transparent;
            _window.AllowsTransparency = true;
            _window.WindowStyle = WindowStyle.None;

            //Hide from taskbar until it becomes a child
            _window.ShowInTaskbar = false;

            //HACK: This window and it's child controls should never have focus, as window styling of an invisible window 
            //will confuse user.
            _window.Focusable = false;
            _window.PreviewMouseDown += OnPreviewMouseDown;
            _window.IsVisibleChanged += OnVisibilityChanged;

        }

        private void OnVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool) e.NewValue != ShowWindow)
            {
                if (ShowWindow)
                    _window.Show();
                else
                {
                    _window.Hide();
                }
            }
        }

        void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_parent != null) 
                _parent.Focus();
        }

        public double WindowLeftMargin
        {
            get { return windowLeftMargin; }
            set { windowLeftMargin = value; }
        }
        public double WindowTopMargin
        {
            get { return windowTopMargin; }
            set { windowTopMargin = value; }
        }

        public int OverlayWidth { get; set; }
        public int OverlayHeight { get; set; }
        public bool ShowWindow { get; set; }

        public Window TransparentWindow
        {
            get
            {
                return _window;
            }
        }
        private void parent_LocationChanged(object sender, EventArgs e)
        {
            UpdateWindow();
        }

        private Window GetParentWindow(DependencyObject o)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(o);
            if (parent == null)
            {
                FrameworkElement fe = o as FrameworkElement;
                if (fe != null)
                {
                    if (fe is Window)
                    {
                        return fe as Window;
                    }
                    else if (fe.Parent != null)
                    {
                        return GetParentWindow(fe.Parent);
                    }
                }
                throw new ApplicationException("A window parent could not be found for " + o.ToString());
            }
            else
            {
                return GetParentWindow(parent);
            }
        }

        internal void Refresh()
        {
            if (_window.Visibility != Visibility.Visible && ShowWindow)
            {
                UpdateWindow();
                _window.Show();
                if (_parent == null)
                {
                    _parent = GetParentWindow(_container);
                    _window.Owner = _parent;
                    _parent.LocationChanged += new EventHandler(parent_LocationChanged);
                }
            }
            else
            {
                if (!ShowWindow && _window.Visibility == Visibility.Visible)
                {
                    _window.Hide();
                    if (_parent != null) 
                        _parent.Activate();
                }
            }
        }

        internal void UpdateWindow()
        {
            Window parent = GetParentWindow(_container);
            FrameworkElement windowContent=((parent.Content) as FrameworkElement);
            if (windowContent != null)
            {
                windowLeftMargin = WindowLeftMargin == 0 ? (parent.ActualWidth - windowContent.ActualWidth)/2 : WindowLeftMargin;
                windowTopMargin = (WindowTopMargin == 0) ? (parent.ActualHeight - windowContent.ActualHeight) : WindowTopMargin;

                _window.Left = parent.Left + windowLeftMargin;
                _window.Top = parent.Top + windowTopMargin;
                _window.Width = OverlayWidth;
                _window.Height = OverlayHeight;
            }

            if (ShowWindow)
                _window.Show();
            else
                _window.Hide();
        }
    }
}
