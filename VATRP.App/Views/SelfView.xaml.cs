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
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;

namespace com.vtcsecure.ace.windows.Views
{
    /// <summary>
    /// Interaction logic for SelfView.xaml
    /// </summary>
    public partial class SelfView 
    {
        public bool ResetNativePreviewHandle { get; set; }
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
            var _linphone = ServiceManager.Instance.LinphoneService;
            if (_linphone == null) 
                return;
            if (!bOn)
            {
                if (ResetNativePreviewHandle)
                    _linphone.SetVideoPreviewWindowHandle(IntPtr.Zero, true);
            }
            else
            {
                _linphone.SetPreviewVideoSizeByName("cif");
                var source = GetWindow(this);
                if (source != null)
                {
                    var wih = new WindowInteropHelper(source);

                    IntPtr hWnd = wih.EnsureHandle();
                    if (hWnd != IntPtr.Zero)
                    {
                        _linphone.SetVideoPreviewWindowHandle(hWnd);
                    }
                }
                ResetNativePreviewHandle = true;
            }
        }
    }
}
