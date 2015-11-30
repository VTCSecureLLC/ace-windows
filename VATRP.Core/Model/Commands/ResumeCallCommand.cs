using System;

namespace VATRP.Core.Model.Commands
{
    internal class ResumeCallCommand : CallCommand
    {
        public ResumeCallCommand(IntPtr callPtr)
            : base(LinphoneCommandType.ResumeCall, callPtr)
        {
            
        }
    }
}