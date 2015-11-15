using System;
using System.ComponentModel;
using System.Windows.Media;
using VATRP.Core.Enums;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using VATRP.LinphoneWrapper.Enums;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class LocalContactViewModel : ViewModelBase
    {
        private VATRPContact _contact;
        private IContactsService _contactService;
        private LinphoneRegistrationState _registrationState;

        public LocalContactViewModel()
        {
            _registrationState = LinphoneRegistrationState.LinphoneRegistrationCleared;
        }

        public LocalContactViewModel(IContactsService contactSvc)
        {
            this._contactService = contactSvc;
            this._contactService.LoggedInContactUpdated += OnLocalContactChanged;
        }

        private void OnLocalContactChanged(object sender, VATRP.Core.Events.ContactEventArgs e)
        {
            Contact = this._contactService.FindContact(e.Contact);
        }

        public VATRPContact Contact
        {
            get { return this._contact; }
            set
            {
                this._contact = value; 
                OnPropertyChanged("Contact");
            }
        }

        public LinphoneRegistrationState RegistrationState
        {
            get { return _registrationState; }
            set
            {
                _registrationState = value;
                OnPropertyChanged("RegistrationState");
            }
        }
    }
}

