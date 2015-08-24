using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VATRP.LinphoneWrapper.Enums;

namespace VATRP.Core.Model
{
    public class VATRPCall
    {
        private LinphoneCallDir callDirection = LinphoneCallDir.LinphoneCallIncoming;

        public LinphoneCallDir GetCallDirection()
        {
            return this.callDirection;
        }
        
    }
}
