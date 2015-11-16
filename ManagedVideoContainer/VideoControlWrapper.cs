using System;
using System.Drawing;
using System.Windows.Forms;

namespace VATRP.Linphone.VideoWrapper
{
    class VideoControlWrapper : Control
    {
        public VideoControlWrapper()
        {
            ClientSize = new System.Drawing.Size(1, 1);
        }

        /// <summary>
        /// For static content, use traditional windows paint approach to signaling rendering
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            RenderFrame();
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Size s = new Size();
            s.Width = Width;
            s.Height = Height;
            ClientSize = s;
        }

        internal Renderable RenderContent
        {
            get { return renderable; }
            set
            {
                renderable = value;
                RenderContent.OnPaint += new EventHandler(RepaintContent);
            }
        }

        void RepaintContent(object sender, EventArgs e)
        {
            OnPaint(null);
        }

        private void RenderFrame()
        {
            // draw additional info
        }

        private Renderable renderable;
    }
}
