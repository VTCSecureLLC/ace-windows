using VATRP.Core.Model;

namespace VATRP.Core.Events
{
    public class ConversationUpdatedEventArgs : ConversationEventArgs
    {
        public ConversationUpdatedEventArgs(VATRPChat chat) : base(chat)
        {
            this.AllowToChangeUnreadMessageCounter = true;
        }

        public bool AllowToChangeUnreadMessageCounter { get; set; }
    }
}

