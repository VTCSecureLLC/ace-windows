using System;
using VATRP.Core.Model;

namespace VATRP.Core.Events
{
    public class ConversationEventArgs : EventArgs
    {
        public ConversationEventArgs(VATRPChat chat)
        {
            this.Conversation = chat;
        }

        public VATRPChat Conversation { get; private set; }
    }
}

