using System;
using VATRP.Core.Enums;
using VATRP.Core.Model;

namespace VATRP.Core.Events
{
    public class TypingInChatEventArgs : EventArgs
    {
        public TypingInChatEventArgs(VATRPChat chat, TypingAction action)
        {
            this.Chat = chat;
            this.Action = action;
        }

        public TypingAction Action { get; private set; }

        public VATRPChat Chat { get; private set; }
    }
}

