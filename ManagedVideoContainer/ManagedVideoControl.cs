using System;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Input;

namespace VATRP.Linphone.VideoWrapper
{
    public class ManagedVideoControl : WindowsFormsHost
    {
        private Point lastMouseCoordinate;

        public ManagedVideoControl()
        {
            lastMouseCoordinate = new Point(0,0);
            Child = new VideoControlWrapper();
            Child.MouseEnter += (sender, args) =>
            {
                var e = new MouseEventArgs(Mouse.PrimaryDevice, (int) DateTime.Now.Ticks)
                {
                    RoutedEvent = Mouse.MouseEnterEvent
                };
                RaiseEvent(e);
            };

            Child.MouseLeave += delegate(object sender, EventArgs args)
            {
                MouseEventArgs e = new MouseEventArgs(Mouse.PrimaryDevice, (int)DateTime.Now.Ticks)
                {
                    RoutedEvent = Mouse.MouseLeaveEvent
                };
                RaiseEvent(e);
            };

            Child.MouseMove += (sender, args) =>
            {
                if (lastMouseCoordinate.X == args.X && lastMouseCoordinate.Y == args.Y)
                    return;
                lastMouseCoordinate.X = args.X;
                lastMouseCoordinate.Y = args.Y;

                MouseEventArgs e = new MouseEventArgs(Mouse.PrimaryDevice, (int)DateTime.Now.Ticks)
                {
                    RoutedEvent = Mouse.MouseMoveEvent,
                };
                RaiseEvent(e);
            };
        }

        public Renderable RenderContent
        {
            get { return ((VideoControlWrapper)Child).RenderContent; }
            set { ((VideoControlWrapper)Child).RenderContent = value; }
        }

        public bool DrawCameraImage
        {
            get { return ((VideoControlWrapper)Child).DrawCameraImage; }
            set
            {
                ((VideoControlWrapper)Child).DrawCameraImage = value; 
                ((VideoControlWrapper)Child).Refresh();
            }
        }

        public IntPtr GetVideoControlPtr
        {
            get
            {
                return Child.Handle;
            }
        }
    }
}
