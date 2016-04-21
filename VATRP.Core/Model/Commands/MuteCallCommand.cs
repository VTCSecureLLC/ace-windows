using System;

namespace VATRP.Core.Model.Commands
{
    internal class MuteCallCommand : CallCommand
    {
        public readonly bool MuteOn;
        public MuteCallCommand(IntPtr callPtr, bool mute)
            : base(LinphoneCommandType.MuteCall, callPtr)
        {
            MuteOn = mute;
        }
    }
}