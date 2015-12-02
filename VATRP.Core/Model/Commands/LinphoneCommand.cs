using System;

namespace VATRP.Core.Model.Commands
{
    internal class LinphoneCommand : ICloneable
    {
        public LinphoneCommandType Command { get; private set; }

        public LinphoneCommand(LinphoneCommandType n)
        {
            Command = n;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public enum LinphoneCommandType
    {
        AcceptCall,
        CreateCall,
        DeclineCall,
        PauseCall,
        ResumeCall,
        TerminateAllCalls,
        TerminateCall,
        StopLinphone
    }
}