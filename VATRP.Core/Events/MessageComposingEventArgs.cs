using System;

namespace VATRP.Core.Events
{
    public class MessageComposingEventArgs : TextMessageEventArgs
    {
        public MessageComposingEventArgs(IntPtr callPtr, uint rttCode):base(string.Empty)
        {
            this.ChatRoomPtr = callPtr;
            this.RTTCode = rttCode;
        }

        public uint RTTCode { get; private set; }
        public IntPtr ChatRoomPtr { get; private set; }
    }
}

