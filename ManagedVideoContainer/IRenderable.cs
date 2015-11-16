using System.Windows;

namespace VATRP.Linphone.VideoWrapper
{
    public abstract class Renderable : DependencyObject
    {
        public abstract void Create();

        public abstract void Render();
        
        public void Repaint()
        {
            if (OnPaint != null)
            {
                OnPaint.Invoke(this, null);
            }
        }

        internal System.EventHandler OnPaint;
    }
}
