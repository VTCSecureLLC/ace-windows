using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VATRP.LinphoneWrapper.Enums
{
    public enum LinphoneRTCPMode
    {
        LinphoneAVPFDefault = -1, // Use default value defined at upper level
        LinphoneAVPFDisabled = 0, // RTCP is disabled
        LinphoneAVPFEnabled = 1 // RTCP is enabled 
    }
}
