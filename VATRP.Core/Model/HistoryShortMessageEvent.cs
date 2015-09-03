using System;

namespace VATRP.Core.Model
{
    public class VATRPMessageEvent : VATRPHistoryEvent, IComparable<VATRPMessageEvent>
    {
        private string content;
        private bool? delivered;
        private string _chatID;
        private bool _seen;
        private string cost; 
        public VATRPMessageEvent()
            :this(null, null, null)
        {
            delivered = null;
        }

        public VATRPMessageEvent(string chatId, string localParty, string remoteParty)
            : base(localParty, remoteParty)
        {
            _chatID = chatId;
            delivered = null;
        }

        // Internal column for the associated chat GUID value
        public string ChatID
        {
            get { return _chatID; }
            set
            {
                _chatID = value;
                NotifyPropertyChanged("ChatID");
            }
        }

      
        public String Content
        {
            get { return this.content; }
            set
            {
                this.content = value;
                NotifyPropertyChanged("Content");
            }
        }        

        public bool? IsDelivered
        {
            get { return this.delivered; }
            set
            {
                if (delivered != value)
                {
                    this.delivered = value;
                    NotifyPropertyChanged("IsDelivered");
                }
            }
        }

        public bool Seen
        {
            get { return this._seen; }
            set
            {
                this._seen = value;
                NotifyPropertyChanged("Seen");
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

        public StatusType Status
        {
            get { return this._status; }
            set
            {
                this._status = value;
                NotifyPropertyChanged("Status");
            }
        }

        public String Cost
        {
            get { return this.cost; }
            set
            {
                this.cost = value;
                NotifyPropertyChanged("Cost");
            }
        }

        #region IComparable implementaion

        public int CompareTo(VATRPMessageEvent other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            return other._date.CompareTo(this._date);
        }

        #endregion
        
        
    }
}
