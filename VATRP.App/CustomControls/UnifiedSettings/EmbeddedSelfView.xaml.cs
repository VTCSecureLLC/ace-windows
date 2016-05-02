using com.vtcsecure.ace.windows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VATRP.Linphone.VideoWrapper;

namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{
    /// <summary>
    /// Interaction logic for EmbeddedSelfView.xaml
    /// </summary>
    public partial class EmbeddedSelfView : UserControl
    {
        public double VideoWidth
        {
            get
            {
                var managedVideoControl = this.ctrlVideo;
                if (managedVideoControl != null)
                    return managedVideoControl.Width;
                return 0;
            }

            set
            {
                var managedVideoControl = this.ctrlVideo;
                if (managedVideoControl != null)
                    managedVideoControl.Width = value;
            }
        }

        public double VideoHeight {
            get
            {
                var managedVideoControl = this.ctrlVideo;
                if (managedVideoControl != null)
                    return managedVideoControl.Height;
                return 0;
            }

            set
            {
                var managedVideoControl = this.ctrlVideo;
                if (managedVideoControl != null) 
                    managedVideoControl.Height = value;
            }
        }

        private string currentCameraID = String.Empty;
        public EmbeddedSelfView()
        {
            InitializeComponent();
        }

        public void Initialize(string cameraID)
        {
            if (string.IsNullOrEmpty(cameraID) || currentCameraID == cameraID)
                return;
            currentCameraID = cameraID;
            SwitchPreviewPanel(false);
            ServiceManager.Instance.LinphoneService.SetCamera(cameraID);
            SwitchPreviewPanel(true);
        }

        public void Uninitialize()
        {
            SwitchPreviewPanel(false);
            currentCameraID = string.Empty;
        }

        private void SwitchPreviewPanel(bool bOn)
        {
            var _linphone = ServiceManager.Instance.LinphoneService;
            if (_linphone == null) 
                return;
            if (!bOn)
            {
                _linphone.SetVideoPreviewWindowHandle(IntPtr.Zero, true);
            }
            else
            {
                _linphone.SetPreviewVideoSizeByName("cif");
                ServiceManager.Instance.LinphoneService.SetVideoPreviewWindowHandle(ctrlVideo.GetVideoControlPtr);
                ctrlVideo.Visibility = System.Windows.Visibility.Visible;
            }
        }

    }
}
