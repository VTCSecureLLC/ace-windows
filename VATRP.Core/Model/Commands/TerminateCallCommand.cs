using System;

namespace VATRP.Core.Model.Commands
{
    internal class TerminateCallCommand : CallCommand
    {
        public TerminateCallCommand(IntPtr callPtr) : base(LinphoneCommandType.TerminateCall, callPtr)
        {
            
        }
    }
}