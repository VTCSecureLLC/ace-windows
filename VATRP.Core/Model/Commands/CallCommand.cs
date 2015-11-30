using System;

namespace VATRP.Core.Model.Commands
{
    internal class CallCommand : LinphoneCommand
    {
        private readonly IntPtr _callPtr;

        protected CallCommand(LinphoneCommandType cmdType, IntPtr callPtr)
            : base(cmdType)
        {
            _callPtr = callPtr;
        }

        public IntPtr CallPtr
        {
            get { return _callPtr; }
        }
    }
}