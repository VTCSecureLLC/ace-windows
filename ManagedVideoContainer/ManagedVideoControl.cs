using System;
using System.Windows.Forms.Integration;

namespace VATRP.Linphone.VideoWrapper
{
    public class ManagedVideoControl : WindowsFormsHost
    {
        public ManagedVideoControl()
        {
            Child = new VideoControlWrapper();
        }

        public Renderable RenderContent
        {
            get { return ((VideoControlWrapper)Child).RenderContent; }
            set { ((VideoControlWrapper)Child).RenderContent = value; }
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
