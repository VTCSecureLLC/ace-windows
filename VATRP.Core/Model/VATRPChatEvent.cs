
using System;

namespace VATRP.Core.Model
{
    public class VATRPChatEvent : VATRPHistoryEvent, IComparable<VATRPChatEvent>
    {
        private string _chatId;
        public VATRPChatEvent()
            : this(null, null)
        {

        }

        public VATRPChatEvent(string sender, string receiver)
            : base(sender, receiver)
        {
            
        }

        public string ChatId
        {
            get { return _chatId; }
            set
            {
                _chatId = value;
                NotifyPropertyChanged("ChatId");
            }
        }


        public string Sender
        {
            get { return this._remoteParty; }
            set
            {
                this._remoteParty = value;
                NotifyPropertyChanged("Sender");
            }
        }

        public DateTime Date
        {
            get { return this._date; }
            set
            {
                this._date = value;
                NotifyPropertyChanged("Date");
            }
        }

        public int CompareTo(VATRPChatEvent other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            return other._date.CompareTo(this._date);
        }
    }
}
