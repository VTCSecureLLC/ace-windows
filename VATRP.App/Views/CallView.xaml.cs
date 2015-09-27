using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using VATRP.App.CustomControls;
using VATRP.App.Model;
using VATRP.App.Services;
using VATRP.Core.Services;
using VATRP.LinphoneWrapper.Enums;

namespace VATRP.App.Views
{
    /// <summary>
    /// Interaction logic for CallView.xaml
    /// </summary>
    public partial class CallView
    {
        public CallView()
            : base(VATRPWindowType.CALL_VIEW)
        {
            InitializeComponent();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            base.Window_Unloaded(sender, e);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Resize(MSVideoSize.MS_VIDEO_SIZE_CIF_W,
                                MSVideoSize.MS_VIDEO_SIZE_CIF_H);
        }

        internal void Resize(LinphoneWrapper.Enums.MSVideoSize width, LinphoneWrapper.Enums.MSVideoSize height)
        {
            this.Width = Convert.ToInt32(width);
            this.Height = Convert.ToInt32(height);

        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            base.Window_SizeChanged(sender, e);
        }

        private void SwitchVideoPanel(bool bOn)
        {
            var _linphone = ServiceManager.Instance.LinphoneSipService;
            if (_linphone != null)
            {
                if (!bOn)
                {
                    _linphone.SetVideoCallWindowHandle(IntPtr.Zero, true);
                }
                else
                {
                    var source = GetWindow(this);
                    if (source != null)
                    {
                        var wih = new WindowInteropHelper(source);
                        IntPtr hWnd = wih.EnsureHandle();
                        if (hWnd != IntPtr.Zero)
                        {
                            _linphone.SetVideoCallWindowHandle(hWnd);
                        }

                    }
                }
            }
        }

        private void OnVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SwitchVideoPanel(Convert.ToBoolean(e.NewValue));
        }
    }
}
