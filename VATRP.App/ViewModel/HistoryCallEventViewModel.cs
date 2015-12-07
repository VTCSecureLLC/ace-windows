using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VATRP.Core.Enums;
using VATRP.Core.Extensions;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class HistoryCallEventViewModel : ViewModelBase, IComparable<HistoryCallEventViewModel>
    {
        private VATRPCallEvent _callEvent;
        private VATRPContact _contact;
        private SolidColorBrush _backColor;
        private string _displayName;
        private ImageSource _avatar;
        private ImageSource _callStateIndicator;
        private int _statusWidth;
        private string _callDate;
        private string _duration;

        public SolidColorBrush CallEventStateBrush
        {
            get { return _backColor; }
            set
            {
                _backColor = value;
                OnPropertyChanged("CallEventStateBrush");
            }
        }

        public HistoryCallEventViewModel()
        {
            _displayName = string.Empty;
            _avatar = null;
            _callStateIndicator = null;
            _statusWidth = 16;
        }

        public HistoryCallEventViewModel(VATRPCallEvent callEvent, VATRPContact contact):this()
        {
            this._callEvent = callEvent;
            this._contact = contact;
            this._backColor = callEvent.Status == VATRPHistoryEvent.StatusType.Missed ? new SolidColorBrush(Color.FromArgb(255, 0xFE, 0xCD, 0xCD)) : new SolidColorBrush(Color.FromArgb(255, 0xE9, 0xEF, 0xE9));

            var vatrpContact = this._contact;
            DisplayName = callEvent.DisplayName;

            if (vatrpContact != null)
            {
                vatrpContact.PropertyChanged += OnContactPropertyChanged;
                if (_contact != null && _contact.Fullname.NotBlank())
                    DisplayName = _contact.Fullname;
            }
            
            LoadContactAvatar();
            LoadCallStateIndicator();

            DateTime callTime =
                VATRP.Core.Model.Utils.Time.ConvertUtcTimeToLocalTime(
                    VATRP.Core.Model.Utils.Time.ConvertDateTimeToLong(callEvent.StartTime)/1000);
            string dateFormat = "d/MM, HH:mm";
            var diffTime = DateTime.Now - callTime;
            if (diffTime.Days == 0)
                dateFormat = "HH:mm";
            else if (diffTime.Days < 8)
                dateFormat = "ddd, HH:mm";
            else if (diffTime.Days > 365)
                dateFormat = "d/MM/yyyy, HH:mm";

            CallDate = callTime.ToString(dateFormat);
        }

        private void OnContactPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string propertyName = e.PropertyName ?? "";
            if (propertyName == "DisplayName")
            {
                this.DisplayName = this.Contact.DisplayName;
                OnPropertyChanged("DisplayName");
            }
        }

        public int CompareTo(HistoryCallEventViewModel other)
        {
            if (other == null)
            {
                return -1;
            }

            return (this.CallEvent).CompareTo(other.CallEvent);
        }

        private void LoadContactAvatar()
        {
            if (_contact != null && _contact.Avatar.NotBlank())
            {
                try
                {
                    byte[] data = File.ReadAllBytes(_contact.Avatar);
                    var source = new BitmapImage();
                    source.BeginInit();
                    source.StreamSource = new MemoryStream(data);
                    source.EndInit();

                    Avatar = source;
                    // use public setter
                }
                catch (Exception ex)
                {
                    LoadCommonAvatar();
                }
            }
            else
            {
                LoadCommonAvatar();
            }
        }

        private void LoadCallStateIndicator()
        {
            try
            {
                var uriString = "pack://application:,,,/ACE;component/Resources/incoming.png";
                if (_callEvent != null)
                    switch (_callEvent.Status)
                    {
                        case VATRPHistoryEvent.StatusType.Outgoing:
                            uriString = "pack://application:,,,/ACE;component/Resources/outgoing.png";
                            break;
                        case VATRPHistoryEvent.StatusType.Incoming:
                            break;
                        case VATRPHistoryEvent.StatusType.Missed:
                            uriString = "pack://application:,,,/ACE;component/Resources/missed.png";
                            _statusWidth = 26;
                            break;
                    }

                CallStatusIndicator = new BitmapImage(new Uri(uriString));
                
            }
            catch (Exception ex)
            {

            }
        }

        private void LoadCommonAvatar()
        {
            var avatarUri = "pack://application:,,,/ACE;component/Resources/male.png";
            if (_contact != null && _contact.Gender.ToLower() == "female")
                avatarUri = "pack://application:,,,/ACE;component/Resources/female.png";
            try
            {
                Avatar = new BitmapImage(new Uri(avatarUri));
                // use public setter
            }
            catch (Exception ex)
            {

            }
        }

        public DateTime SortDate
        {
            get
            {
                var vatrpCallEvent = this._callEvent;
                if (vatrpCallEvent != null) 
                    return vatrpCallEvent.StartTime;
                return DateTime.Now;
            }
        }
        public VATRPCallEvent CallEvent
        {
            get { return this._callEvent; }
            set
            {
                this._callEvent = value;
                OnPropertyChanged("CallEvent");
            }
        }

        public VATRPContact Contact
        {
            get { return this._contact; }
            set { this._contact = value; }
        }

        public string DisplayName
        {
            get { return _displayName; }

            set
            {
                _displayName = value;
                OnPropertyChanged("DisplayName");
            }
        }

        public ImageSource Avatar
        {
            get { return _avatar; }
            set
            {
                _avatar = value;
                OnPropertyChanged("Avatar");
            }
        }

        public ImageSource CallStatusIndicator
        {
            get { return _callStateIndicator; }
            set
            {
                _callStateIndicator = value;
                OnPropertyChanged("CallStatusIndicator");
            }
        }

        public int StatusImageWidth
        {
            get { return _statusWidth; }
            set
            {
                _statusWidth = value;
                OnPropertyChanged("StatusImageWidth");
            }
        }

        public string CallDate
        {
            get { return _callDate; }
            set
            {
                _callDate = value; 
                OnPropertyChanged("CallDate");
            }
        }

        public string Duration
        {
            get
            {
                if (CallEvent == null || CallEvent.Duration == -1)
                {
                    return "0m 0s";
                }
                int hours = 0, minutes = 0, seconds = 0;
                if (CallEvent.Duration > 0)
                {
                    seconds = CallEvent.Duration % 60;
                    minutes = CallEvent.Duration / 60;
                    hours = CallEvent.Duration / 3600;
                }

                if (hours > 0)
                {
                    return string.Format("{0}h {1:00}m {2:00}s", hours, minutes, seconds);
                }

                if (minutes > 0)
                {
                    return string.Format("{0}m {1:00}s", minutes, seconds);
                }

                return string.Format("0m {0}s", seconds);
            }
        }

    }
}

