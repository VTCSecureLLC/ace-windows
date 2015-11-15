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
        private string _phoneNumber;
        private string _displayName;
        private ImageSource _avatar;
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
            _phoneNumber = string.Empty;
            _displayName = string.Empty;
            _avatar = null;
        }

        public HistoryCallEventViewModel(VATRPCallEvent callEvent, VATRPContact contact)
        {
            this._callEvent = callEvent;
            this._contact = contact;
            this._backColor = callEvent.Status == VATRPHistoryEvent.StatusType.Missed ? new SolidColorBrush(Color.FromArgb(255, 0xFE, 0xCD, 0xCD)) : new SolidColorBrush(Color.FromArgb(255, 0xE9, 0xEF, 0xE9));

            var vatrpContact = this._contact;
            if (vatrpContact != null)
            {
                vatrpContact.PropertyChanged += OnContactPropertyChanged;
                if (_contact != null && _contact.Fullname.NotBlank())
                    DisplayName = _contact.Fullname;
            }
            if (_callEvent != null && _callEvent.RemoteParty.NotBlank())
                PhoneNumber = _callEvent.RemoteParty;
            LoadContactAvatar();
        }

        private void OnContactPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // reserved
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

        public string PhoneNumber
        {
            get { return _phoneNumber; }

            set
            {
                _phoneNumber = value;
                OnPropertyChanged("PhoneNumber");
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
    }
}

