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
    public class VATRPOverlay : Decorator
    {
        private VATRPTranscluentWindow commandBarWindow;
        private VATRPTranscluentWindow numpadWindow;

        public VATRPOverlay()
        {
            commandBarWindow = new VATRPTranscluentWindow(this);
            numpadWindow = new VATRPTranscluentWindow(this);
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

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            commandBarWindow.UpdateWindow();
            numpadWindow.UpdateWindow();
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
        }
    }
}
