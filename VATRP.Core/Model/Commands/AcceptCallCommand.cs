using System;

namespace VATRP.Core.Model.Commands
{
    internal class AcceptCallCommand : CallCommand
    {
        private readonly IntPtr _callParamsPtr;

        public AcceptCallCommand(IntPtr callPtr, IntPtr callParamsPtr)
            : base(LinphoneCommandType.AcceptCall, callPtr)
        {
            _callParamsPtr = callParamsPtr;
        }

        public IntPtr CallParamsPtr
        {
            get { return _callParamsPtr; }
        }
    }
}