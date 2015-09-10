using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VATRP.App.Model;
using VATRP.App.Services;

namespace VATRP.App.Views
{
    /// <summary>
    /// Interaction logic for SelfView.xaml
    /// </summary>
    public partial class SelfView 
    {
        public SelfView() : base(VATRPWindowType.SELF_VIEW)
        {
            InitializeComponent();
        }

        private void OnVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SwitchPreviewPanel(Convert.ToBoolean(e.NewValue));
        }

        private void SwitchPreviewPanel(bool bOn)
        {
            var _linphone = ServiceManager.Instance.LinphoneSipService;
            if (_linphone == null) 
                return;
            if (!bOn)
            {
                _linphone.SetVideoPreviewWindowHandle(IntPtr.Zero, true);
            }
            else
            {
                var source = (HwndSource)HwndSource.FromVisual(this);
                if (source != null)
                {
                    IntPtr hWnd = source.Handle;
                    if (hWnd != IntPtr.Zero)
                    {
                        _linphone.SetVideoPreviewWindowHandle(hWnd);
                    }

                }
            }
        }
    }
}
