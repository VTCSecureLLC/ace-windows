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
        private VATRPCallState callState = VATRPCallState.None;
        private readonly IntPtr nativeCallPtr = IntPtr.Zero;
        private string _from;
        private string _to;
        private DateTime callStartTime;
        private DateTime callEstablishTime;
        public System.DateTime CallEstablishTime
        {
            get { return callEstablishTime; }
            set { callEstablishTime = value; }
        }
        public System.DateTime CallStartTime
        {
            get { return callStartTime; }
            set { callStartTime = value; }
        }
        public System.IntPtr NativeCallPtr
        {
            get { return nativeCallPtr; }
        }
        public VATRPCall(IntPtr callPtr)
        {
            nativeCallPtr = callPtr;
        }
        public VATRPCallState CallState
        {
            get { return callState; }
            set { callState = value; }
        }

        public LinphoneCallDir CallDirection
        {
            get { return callDirection; }
            set { callDirection = value; }
        }

        public string From
        {
            get { return _from; }
            set { _from = value; }
        }

        public string To
        {
            get { return _to; }
            set { _to = value; }
        }
    }
}
