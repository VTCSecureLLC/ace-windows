using System.ComponentModel;

namespace VATRP.Core.Model
{
    public class ChatID : ContactID
    {
        public ChatID(ChatID chatID) : base(chatID.ID, chatID.NativePtr)
        {
            this.DialogID = chatID.DialogID;
        }

        public ChatID(ContactID contactId, string dialogId) : base(contactId.ID, contactId.NativePtr)
        {
            this.DialogID = dialogId;
        }

        public virtual bool Equals(ChatID other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }
            if (object.ReferenceEquals(other, this))
            {
                return true;
            }
            if (!string.IsNullOrEmpty( this.DialogID) && !string.IsNullOrEmpty( other.DialogID))
            {
                return (this.DialogID == other.DialogID);
            }
            return (string.IsNullOrEmpty(this.DialogID) && string.IsNullOrEmpty(other.DialogID) && base.Equals((ContactID)other));
        }

        public override bool Equals(object obj)
        {
            return ((obj is ChatID) && this.Equals(obj as ChatID));
        }

        public override int GetHashCode()
        {
            return (base.GetHashCode() ^ this.DialogID.GetHashCode());
        }

        public static bool operator ==(ChatID first, ChatID second)
        {
            if (object.ReferenceEquals(first, null))
            {
                return object.ReferenceEquals(first, second);
            }
            return first.Equals(second);
        }

        public static bool operator !=(ChatID first, ChatID second)
        {
            return !(first == second);
        }

        public string DialogID { get; protected set; }

    }
}

