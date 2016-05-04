using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using VATRP.Core.Enums;
using VATRP.Core.Extensions;
using VATRP.Core.Model.Utils;
using VATRP.LinphoneWrapper.Enums;

namespace VATRP.Core.Model
{
    public class VATRPChatMessage : INotifyPropertyChanged, IComparable<VATRPChatMessage>, ICloneable
    {
        protected VATRPChat _chatID;
        protected string _content;
        protected bool _isSeparator;
        protected string _fileName;
        protected bool _isHeadingMessage;
        protected bool _isIncompleteMessage;
        protected LinphoneChatMessageState _status;
        protected DateTime _time;
        private MessageDirection _direction;
        private bool _isRead;
        private bool _isRTTMessage;
        private bool _isRTTStartMarker;
        private bool _isRTTEndMarker;
        private bool _isRTTMarker;
        private bool _isDeclineMessage;

        public static string DECLINE_PREFIX = "@@info@@ ";

        public VATRPChatMessage()
        {
            ID = Tools.GenerateMessageId();
            ContentType = MessageContentType.Text;
            this.MessageTime = DateTime.Now;
            _isIncompleteMessage = false;
            NativePtr = IntPtr.Zero;
            _isSeparator = false;
            _isRTTStartMarker = false;
            _isRTTEndMarker = false;
            _isRTTMarker = false;
            _isRTTMessage = false;
            _isDeclineMessage = false;
            RttCallPtr = IntPtr.Zero;
        }

        public VATRPChatMessage(MessageContentType contentType) :this()
        {
            this.ContentType = contentType;

        }

        public int CompareTo(VATRPChatMessage other)
        {
            if (other == null)
            {
                return -1;
            }
            return this.MessageTime.CompareTo(other.MessageTime);
        }

        public VATRPChat Chat
        {
            get
            {
                return this._chatID;
            }
            set
            {
                this._chatID = value;
                this.OnPropertyChanged("Chat");
            }
        }

        public string Content
        {
            get
            {
                return this._content;
            }
            set
            {
                if (_content != value)
                {
                    _content = value;
                    this.OnPropertyChanged("Content");
                }
            }
        }

        public bool ShowInList
        {
            get
            {
                return (this.Direction == MessageDirection.Incoming || !this.IsIncompleteMessage ) && !IsSeparator &&
                       !IsRTTStartMarker && !IsRTTEndMarker;
            }
        }

        public IntPtr NativePtr { get; set; }

        public MessageContentType ContentType { get; set; }
		
        public IntPtr RttCallPtr { get; set; }

        public MessageDirection Direction
        {
            get { return _direction; }
            set
            {
                _direction = value;
                OnPropertyChanged("Direction");
                OnPropertyChanged("HasDeliveryStatus");
            }
        }

        public bool IsRead
        {
            get { return _isRead; }
            set
            {
                _isRead = value; 
                OnPropertyChanged("IsRead");
            }
        }

        public string ID { get; set; }
        
        public string FileName
        {
            get
            {
                return this._fileName;
            }
            set
            {
                this._fileName = value;
                this.OnPropertyChanged("FileName");
            }
        }
        
        public bool IsHeadingMessage
        {
            get
            {
                return ((this.ContentType == MessageContentType.Info) && this._isHeadingMessage);
            }
            set
            {
                if (this.ContentType == MessageContentType.Info)
                {
                    this._isHeadingMessage = value;
                }
            }
        }
        
        public bool IsIncompleteMessage
        {
            get
            {
                return this._isIncompleteMessage;
            }
            set
            {
                this._isIncompleteMessage = value;
                OnPropertyChanged("ShowInList");
            }
        }

        public bool IsRTTMessage
        {
            get
            {
                return this._isRTTMessage;
            }
            set
            {
                this._isRTTMessage = value;
                OnPropertyChanged("IsRTTMessage");
            }
        }

        public bool IsRTTStartMarker
        {
            get
            {
                return this._isRTTStartMarker;
            }
            set
            {
                this._isRTTStartMarker = value;
                OnPropertyChanged("IsRTTStartMarker");
            }
        }

        public bool IsRTTEndMarker
        {
            get
            {
                return this._isRTTEndMarker;
            }
            set
            {
                this._isRTTEndMarker = value;
                OnPropertyChanged("IsRTTEndMarker");
            }
        }

        public bool IsRTTMarker
        {
            get
            {
                return this._isRTTMarker || _isRTTMessage;
            }
            set
            {
                this._isRTTMarker = value;
                OnPropertyChanged("IsRTTMarker");
            }
        }

        public bool IsDeclineMessage
        {
            get
            {
                return this._isDeclineMessage;
            }
            set
            {
                this._isDeclineMessage = value;
                OnPropertyChanged("IsDeclineMessage");
            }
        }

        public bool ShowRTTMarker
        {
            get
            {
                return IsRTTMarker && IsRTTMessage && Content.NotBlank();
            }
        }
        public LinphoneChatMessageState Status
        {
            get
            {
                return this._status;
            }
            set
            {
                this._status = value;
                this.OnPropertyChanged("Status");
            }
        }

        public bool IsSeparator
        {
            get
            {
                return this._isSeparator;
            }
            set
            {
                this._isSeparator = value;
                this.OnPropertyChanged("IsSeparator");
            }
        }

        public bool HasDeliveryStatus
        {
            get
            {
                return (this.Direction == MessageDirection.Outgoing) && !IsRTTMessage ;
            }
        }

        public DateTime MessageTime
        {
            get
            {
                return this._time;
            }
            set
            {
                this._time = value;
                this.OnPropertyChanged("MessageTime");
            }
        }

        public long UtcTimestamp { get; set; }

        #region INotifyPropertyChanged Interface

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region ICloneable
        
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        
        #endregion

    }
}

