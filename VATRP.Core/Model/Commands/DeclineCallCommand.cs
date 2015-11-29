using System;
using VATRP.LinphoneWrapper.Enums;

namespace VATRP.Core.Model.Commands
{
    internal class DeclineCallCommand : CallCommand
    {
        private readonly LinphoneReason _reason;

        public DeclineCallCommand(IntPtr callPtr, LinphoneReason reason)
            : base(LinphoneCommandType.DeclineCall, callPtr)
        {
            _reason = reason;
        }

        public LinphoneReason Reason
        {
            get { return _reason; }
        }
    }
}