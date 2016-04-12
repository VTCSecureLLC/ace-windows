using System;
using VATRP.LinphoneWrapper.Enums;

namespace VATRP.Core.Model.Commands
{
    internal class SendChatMessageCommand : CallCommand
    {
        public IntPtr ChatPtr { get; private set; }
        public SendChatMessageCommand(IntPtr msgPtr, IntPtr chatPtr)
            : base(LinphoneCommandType.SendChatMessage, msgPtr)
        {
            ChatPtr = chatPtr;
        }

    }
}