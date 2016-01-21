using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VATRP.Core.Model
{
    public enum VATRPDeviceType
    {
        CAMERA,
        MICROPHONE,
        SPEAKER,
        UNKNOWN
    }
    public class VATRPDevice
    {
        public string deviceId { get; set; }
        public string displayName { get; set; }
        public VATRPDeviceType deviceType { get; set; }

        public VATRPDevice(string deviceId, VATRPDeviceType deviceType)
        {
            this.deviceId = deviceId.Trim();
            string[] parts = deviceId.Split(':');
            if (parts.Count() > 1)
            {
                displayName = parts[1].Trim();
            }
            this.deviceType = deviceType;
        }
    }
}
