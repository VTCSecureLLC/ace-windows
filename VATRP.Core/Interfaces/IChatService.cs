using System;
using System.Collections.ObjectModel;
using VATRP.Core.Events;
using VATRP.Core.Model;

namespace VATRP.Core.Interfaces
{
    public interface IChatService : IVATRPservice
    {
        VATRPChat GetChat(VATRPContact contact);
        VATRPContact FindContact(ContactID contactID);
        ObservableCollection<VATRPContact> Contacts { get; }

        void OnUnreadMsgUpdated();

        bool ComposeAndSendMessage(IntPtr callPtr, VATRPChat chat, char key, bool inCompleteMessage);
		
        bool ComposeAndSendMessage(IntPtr callPtr, VATRPChat chat, string text, bool inCompleteMessage);

        event EventHandler<ConversationEventArgs> ConversationClosed;

        event EventHandler<ConversationEventArgs> ConversationStateChanged;

        event EventHandler<ConversationEventArgs> ConversationUnReadStateChanged;

        event EventHandler<ConversationUpdatedEventArgs> ConversationUpdated;

        event EventHandler<EventArgs> UnreadMsgUpdated;

        event EventHandler<EventArgs> ContactsChanged;

        event EventHandler<ConversationEventArgs> NewConversationCreated;

    }
}
