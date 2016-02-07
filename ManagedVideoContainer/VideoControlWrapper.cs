using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using VATRP.Linphone.VideoWrapper.Properties;
using Size = System.Drawing.Size;

namespace VATRP.Linphone.VideoWrapper
{
    class VideoControlWrapper : Control
    {
        private Renderable renderable;
        private System.Drawing.Bitmap _cameraBitmap;
        public VideoControlWrapper()
        {
            ClientSize = new System.Drawing.Size(1, 1);

            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(@"pack://application:,,,/VATRP.Linphone.VideoWrapper;component/Resources/camera_mute.png", UriKind.Absolute);
            src.EndInit();
            _cameraBitmap = BitmapSourceToBitmap(src);
        }

        /// <summary>
        /// For static content, use traditional windows paint approach to signaling rendering
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            RenderFrame(e);
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

        internal bool DrawCameraImage { get; set; }
		
        void RepaintContent(object sender, EventArgs e)
        {
            OnPaint(null);
        }

        private void RenderFrame(PaintEventArgs e)
        {
            if (DrawCameraImage)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.Black), 0, 0, ClientSize.Width, ClientSize.Height);

                int imageSize = 300;
                Rectangle srcRect = new Rectangle(0, 0, _cameraBitmap.Width, _cameraBitmap.Height);
                Rectangle destRect = new Rectangle((ClientSize.Width - imageSize) / 2, (ClientSize.Height - imageSize) / 2,
                    imageSize, imageSize);

                if (_cameraBitmap != null)
                    e.Graphics.DrawImage(_cameraBitmap,destRect, srcRect
                        , GraphicsUnit.Pixel);
            }
        }

        private System.Drawing.Bitmap BitmapSourceToBitmap(BitmapSource srcImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(srcImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
    }
}
