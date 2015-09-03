using System;
using System.ComponentModel;

namespace VATRP.Core.Model
{
    public abstract class VATRPHistoryEvent : IComparable<VATRPHistoryEvent>, INotifyPropertyChanged
    {
        protected string _remoteParty;
        protected string _localParty;
        protected bool seen;
        protected StatusType _status;
        protected DateTime _date;
        protected string _displayName;
        protected VATRPContact _contact;

        [Flags]
        public enum StatusType
        {
            Outgoing = 0x01 << 0,
            Incoming = 0x01 << 1,
            Missed = 0x01 << 2,
            Failed = 0x01 << 3,
            Rejected = 0x01 << 4,
            Sent = 0x01 << 5,
            Received = 0x01 << 6,
            All = Outgoing | Incoming | Missed | Failed | Rejected | Sent | Received
        }

        protected VATRPHistoryEvent()
        {
            this._status = StatusType.Missed;
            this._date = DateTime.Now;
        }

        protected VATRPHistoryEvent(string localParty, string remoteParty)
        {
            this._localParty = localParty;
            this._remoteParty = remoteParty;
        }

        public VATRPContact Contact
        {
            get { return this._contact; }
            set
            {
                this._contact = value;
                NotifyPropertyChanged("Contact");
            }
        }

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Contact.DisplayName))
                    return this.Contact.DisplayName;
                return _remoteParty;
            }
            set
            {
                this._displayName = value;
                NotifyPropertyChanged("DisplayName");
            }
        }


        public int CompareTo(VATRPHistoryEvent other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            return other._date.CompareTo(this._date);
        }



        public bool ContainsFilteredData(string searchData)
        {
            if (DisplayName != null && DisplayName.Contains(searchData))
                return true;

            if (_remoteParty != null && _remoteParty.Contains(searchData))
                return true;

            return false;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify that a property changed
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
