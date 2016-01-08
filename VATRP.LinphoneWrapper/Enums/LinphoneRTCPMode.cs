using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VATRP.LinphoneWrapper.Enums
{
    public enum LinphoneRTCPMode
    {
        LinphoneRTCPDefault = -1, // Use default value defined at upper level
        LinphoneRTCPDisabled = 0, // RTCP is disabled
        LinphoneRTCPEnabled = 1 // RTCP is enabled 
    }
}
