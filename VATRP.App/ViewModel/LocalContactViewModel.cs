using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Enums;
using VATRP.Core.Events;
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
        private int _videoMailCount;

        public LocalContactViewModel()
        {
            _videoMailCount = 0;
            _registrationState = LinphoneRegistrationState.LinphoneRegistrationCleared;
        }

        public LocalContactViewModel(IContactsService contactSvc)
        {
            this._contactService = contactSvc;
            this._contactService.LoggedInContactUpdated += OnLocalContactChanged;
        }

        private void OnLocalContactChanged(object sender, VATRP.Core.Events.ContactEventArgs e)
        {
            if (ServiceManager.Instance.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke((Action)(() => this.OnLocalContactChanged(sender, e)));
                return;
            }
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

        public int VideoMailCount
        {
            get { return _videoMailCount; }
            set
            {
                _videoMailCount = value;
                OnPropertyChanged("VideoMailCount");
            }
        }
    }
}

