using System;
using System.ComponentModel;
using System.Windows.Media;
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

        }

        public ContactViewModel(VATRPContact contact)
        {
            this._viewModelID = ++IdIncremental;
            this._contact = contact;

            this.backColor = contact.Status != UserStatus.Offline ? new SolidColorBrush(Color.FromArgb(255, 115, 215, 120)) : new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
            this._contact.PropertyChanged += _contact_PropertyChanged;
        }


        private void _contact_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Status")
            {
                OnPropertyChanged("StatusUI");
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

        public string StatusUI
        {
            get
            {
                return this.Contact.Status.ToString();
            }
        }

        public int ViewModelID
        {
            get { return this._viewModelID; }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsOnline
        {
            get { return _contact != null && _contact.Status != UserStatus.Offline; }
        }
    }
}

