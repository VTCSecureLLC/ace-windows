using System;

namespace VATRP.Core.Events
{
    public class MessageComposingEventArgs : TextMessageEventArgs
    {
        public MessageComposingEventArgs(string remote, IntPtr callPtr, uint rttCode):base(remote)
        {
            this.CallPtr = callPtr;
            this.RTTCode = rttCode;
        }

        public uint RTTCode { get; private set; }
        public IntPtr CallPtr { get; private set; }
    }
}

