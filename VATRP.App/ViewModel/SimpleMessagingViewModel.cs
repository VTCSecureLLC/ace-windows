using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Threading;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Extensions;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;


namespace com.vtcsecure.ace.windows.ViewModel
{
    public class SimpleMessagingViewModel : MessagingViewModel
    {

        #region Members

        private string _receiverAddress;
        private string _contactSearchCriteria;
        private ICollectionView contactsListView;
        
        #endregion

        public SimpleMessagingViewModel()
        {
            Init();
        }

        public SimpleMessagingViewModel(IChatService chatMng, IContactsService contactsMng)
            : base(chatMng, contactsMng)
        {
            Init();
        }

        private void Init()
        {
            _contactSearchCriteria = string.Empty;
            _receiverAddress = string.Empty;
            LoadContacts();
            this.ContactsListView = CollectionViewSource.GetDefaultView(this.Contacts);
            this.ContactsListView.Filter = new Predicate<object>(this.FilterContactsList);
        }


        #region Methods

        internal void SendMessage(string message)
        {
            if (!message.NotBlank() || string.IsNullOrEmpty( ReceiverAddress))
                return;

            _chatsManager.ComposeAndSendMessage(Chat, message);
            MessageText = string.Empty;
        }

        public bool FilterContactsList(object item)
        {
            var contactVM = item as ContactViewModel;
            if (contactVM != null)
                return contactVM.Contact != null && contactVM.Contact.Fullname.Contains(ContactSearchCriteria);
            return true;
        }
		
   
        #endregion

        #region Properties

        public string ReceiverAddress
        {
            get { return _receiverAddress; }
            set
            {
                _receiverAddress = value;
                OnPropertyChanged("ReceiverAddress");
                OnPropertyChanged("ShowReceiverHint");
            }
        }

        public bool ShowReceiverHint
        {
            get { return !ReceiverAddress.NotBlank(); }
        }


        public bool ShowSearchHint
        {
            get { return !ContactSearchCriteria.NotBlank(); }
        }


        public string ContactSearchCriteria
        {
            get { return _contactSearchCriteria; }
            set
            {
                if (_contactSearchCriteria != value)
                {
                    _contactSearchCriteria = value;
                    OnPropertyChanged("ContactSearchCriteria");
                    OnPropertyChanged("ShowSearchHint");
                    ContactsListView.Refresh();
                }
            }
        }

       
        public ICollectionView ContactsListView
        {
            get { return this.contactsListView; }
            private set
            {
                if (value == this.contactsListView)
                {
                    return;
                }

                this.contactsListView = value;
                OnPropertyChanged("ContactsListView");
            }
        }

       
        #endregion


        internal bool CheckReceiverContact()
        {
            var receiver = string.Empty;
            if (ReceiverAddress != null)
            {
                receiver = ReceiverAddress.Trim();
            }

            if (Contact != null && receiver == Contact.Contact.RegistrationName)
                return true;

            VATRPContact contact = _chatsManager.FindContact(new ContactID(receiver, IntPtr.Zero));
            if (contact == null)
            {
                string un, host, dn;
                int port;
                if (!VATRPCall.ParseSipAddress(receiver, out un,
                    out host, out port))
                    un = "";

                if (!un.NotBlank())
                    return false;

                if (string.IsNullOrEmpty(host))
                    host = App.CurrentAccount.ProxyHostname;
                var contactAddress = string.Format("{0}@{1}", un, host);
                var contactID = new ContactID(contactAddress, IntPtr.Zero);

                contact = new VATRPContact(contactID)
                {
                    DisplayName = un,
                    Fullname = un,
                    SipUsername = un,
                    RegistrationName = contactAddress
                };
                _contactsManager.AddContact(contact, "");
            }

            SetActiveChatContact(contact, IntPtr.Zero);
            if ( ReceiverAddress != contact.RegistrationName )
                ReceiverAddress = contact.RegistrationName;

            return true;
        }
    }
}