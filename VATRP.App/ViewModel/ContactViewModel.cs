using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VATRP.Core.Enums;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class ContactViewModel : IComparable<ContactViewModel>, INotifyPropertyChanged
    {
        private VATRPContact _contact;
        private int _viewModelID;
        private int IdIncremental;
        private bool _isSelected;
        private SolidColorBrush backColor;
        private ImageSource _avatar;

        public SolidColorBrush ContactStateBrush
        {
            get { return backColor; }
            set
            {
                backColor = value;
                OnPropertyChanged("ContactStateBrush");
            }
        }
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public ContactViewModel()
        {
            _avatar = null;
        }

        public ContactViewModel(VATRPContact contact)
        {
            this._viewModelID = ++IdIncremental;
            this._contact = contact;

            this.backColor = contact.Status != UserStatus.Offline ? new SolidColorBrush(Color.FromArgb(255, 115, 215, 120)) : new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
            this._contact.PropertyChanged += _contact_PropertyChanged;
            LoadContactAvatar();
        }

        private void LoadContactAvatar()
        {
            if (_contact != null && !string.IsNullOrWhiteSpace(_contact.Avatar))
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

        private void _contact_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ContactName_ForUI")
            {
                OnPropertyChanged("ContactUI");
            }
            else if (e.PropertyName == "Avatar")
            {
                LoadContactAvatar();
            }
        }
        
        public int CompareTo(ContactViewModel other)
        {
            if (other == null)
            {
                return -1;
            }

            return ((int) this.ViewModelID).CompareTo(other.ViewModelID);
        }
        
        public VATRPContact Contact
        {
            get { return this._contact; }
            set { this._contact = value; }
        }

        public string ContactUI
        {
            get
            {
                var vatrpContact = this.Contact;
                if (vatrpContact != null) 
                    return vatrpContact.ContactName_ForUI;
                return string.Empty;
            }
        }

        public int ViewModelID
        {
            get { return this._viewModelID; }
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

