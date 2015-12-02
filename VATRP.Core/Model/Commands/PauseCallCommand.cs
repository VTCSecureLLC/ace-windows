using System;

namespace VATRP.Core.Model.Commands
{
    internal class PauseCallCommand : CallCommand
    {
        public PauseCallCommand(IntPtr callPtr)
            : base(LinphoneCommandType.PauseCall, callPtr)
        {
            
        }
    }
}