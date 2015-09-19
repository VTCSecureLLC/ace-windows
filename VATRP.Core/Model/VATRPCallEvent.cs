using System;

namespace VATRP.Core.Model
{
    public class VATRPCallEvent : VATRPHistoryEvent, IComparable<VATRPCallEvent>
    {
        private DateTime startTime;
        private DateTime endTime;
        private int _duration = 0;
        private string _codecs = string.Empty;

        public VATRPCallEvent() 
            : this(null, null)
        {
            
        }

        public VATRPCallEvent(string localParty, string remoteParty)
            : base (localParty, remoteParty)
        {
            this.startTime = base._date;
            this.endTime = base._date;
        }

        public string Codec
        {
            get { return _codecs; }
            set { _codecs = value; }
        }

        public DateTime StartTime
        {
            get { return this.startTime; }
            set
            {
                this.startTime = value;
                NotifyPropertyChanged("StartTime");
            }
        }

        public DateTime EndTime
        {
            get { return this.endTime; }
            set
            {
                this.endTime = value;
                NotifyPropertyChanged("EndTime");
                _duration = (this.endTime - this.startTime).Seconds;
                NotifyPropertyChanged("Duration");
            }
        }

        public string LocalParty
        {
            get { return this._localParty; }
            set
            {
                this._localParty = value;
                NotifyPropertyChanged("LocalParty");
            }
        }

        public string RemoteParty
        {
            get { return this._remoteParty; }
            set
            {
                this._remoteParty = value;
                NotifyPropertyChanged("RemoteParty");
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

        public int Duration
        {
            get
            {
                return _duration;
            }
        }

        public int CompareTo(VATRPCallEvent other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            return other.StartTime.CompareTo(this.StartTime);
        }
       
    }
}
