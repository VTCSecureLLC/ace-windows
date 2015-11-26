using System;
using System.Windows;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class ContactEditViewModel : ViewModelBase
    {
        private string _title;
        private bool _isAddMode;
        private string _infoTitle;
        private string _contactName;
        private string _contactSipAddress;

        public ContactEditViewModel(bool addMode)
        {
            _isAddMode = addMode;

            Title = _isAddMode ? "Add new contact" : "Edit contact";
            InfoTitle = "Contact information";
        }

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        public string InfoTitle
        {
            get { return _infoTitle; }
            set
            {
                _infoTitle = value; 
                OnPropertyChanged("InfoTitle");
            }
        }

        public string ContactName
        {
            get { return _contactName; }
            set
            {
                _contactName = value;
                OnPropertyChanged("ContactName");
            }
        }

        public string ContactSipAddress
        {
            get { return _contactSipAddress; }
            set
            {
                _contactSipAddress = value;
                OnPropertyChanged("ContactSipAddress");
            }
        }

        public bool IsAddMode
        {
            get { return _isAddMode; }
        }

        internal bool ValidateName()
        {
            return !string.IsNullOrWhiteSpace(ContactName);
        }

        internal bool ValidateAddress()
        {
            if (string.IsNullOrWhiteSpace(ContactSipAddress))
                return false;
            return ValidateEmailInput(ContactSipAddress);
        }

        private bool ValidateEmailInput(string input)
        {
            // Create a new Regex based on the specified regular expression.
            string regex = @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(regex);

            return r.IsMatch(input);
        }
    }
}