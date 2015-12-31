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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{
    public delegate void UnifiedSettings_DeviceSelected(VATRPDevice selectedDevice);
    /// <summary>
    /// Interaction logic for UnifiedSettingsCameraCtrl.xaml
    /// </summary>
    public partial class UnifiedSettingsDeviceCtrl : BaseUnifiedSettingsPanel
    {
        public event UnifiedSettings_DeviceSelected OnDeviceSelected;
        public List<VATRPDevice> deviceList;
        public UnifiedSettingsDeviceCtrl()
        {
            InitializeComponent();
        }

        public override void Initialize()
        {
            base.Initialize();
            if (deviceList != null)
            {
                DeviceListView.Items.Clear();
                foreach (VATRPDevice item in deviceList)
                {
                    DeviceListView.Items.Add(item);
                }
            }
        }


        private void DeviceSelected(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = DeviceListView.SelectedItem;
            if (selectedItem is VATRPDevice)
            {
                VATRPDevice device = DeviceListView.SelectedItem as VATRPDevice;
                if (OnDeviceSelected != null)
                {
                    OnDeviceSelected(device);
                }
            }

        }
    }
}
