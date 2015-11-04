using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Events;
using VATRP.Core.Extensions;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;


namespace com.vtcsecure.ace.windows.ViewModel
{
    public class MessagingViewModel : ViewModelBase
    {
        private string _receiverAddress;
        private string _messageText;
        private VATRPChat _chat;
        private string _message;
        private readonly IChatService _chatsManager;
        private readonly IContactsService _contactsManager;
        private bool _isMessagesLoaded;
        private bool _contactsLoaded;

        private ObservableCollection<ContactViewModel> _contacts;

        private ContactViewModel _contactViewModel;
        private int _lastSentTextIndex;
        private string _contactSearchCriteria;

        public MessagingViewModel()
        {
            
        }

        public MessagingViewModel(IChatService chatMng, IContactsService contactsMng)
        {
            this._chatsManager = chatMng;
            this._contactsManager = contactsMng;
            this._chatsManager.ConversationUpdated += OnConversationUpdated;
            this._chatsManager.NewConversationCreated += OnNewConversationCreated;
            this._chatsManager.ConversationClosed += OnConversationClosed;
            this._chatsManager.ContactsChanged += OnContactsChanged;
            this._chatsManager.ContactAdded += OnChatContactAdded;
            this._chatsManager.ContactRemoved += OnChatContactRemoved;
            LastInput = string.Empty;
        }

        private void OnChatContactRemoved(object sender, ContactRemovedEventArgs e)
        {
            var contactVM = FindContactViewModel(e.contactId);
            if (contactVM != null)
            {
                this.Contacts.Remove(contactVM);
                OnPropertyChanged("Contacts");
            }
        }

        private void OnChatContactAdded(object sender, ContactEventArgs e)
        {
            var contactVM = FindContactViewModel(e.Contact);
            if (contactVM == null)
            {
                var contact = this._contactsManager.FindContact(e.Contact);
                if (contact != null)
                {
                    this.Contacts.Add(new ContactViewModel(contact));
                    OnPropertyChanged("Contacts");
                }
            }
        }

        private void OnConversationClosed(object sender, ConversationEventArgs e)
        {
            if (ServiceManager.Instance.Dispatcher.Thread != Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new EventHandler<ConversationEventArgs>(OnConversationClosed), sender, new object[] { e });
                return;
            }
            VATRPContact contact = e.Conversation.Contact;
            if (contact != null)
            {
                RemoveContact(contact);

                if (this.Chat == e.Conversation)
                {
                    this._chat = null;
                //    MessagesListView = null;
                }

                if (_contactViewModel.Contact == e.Conversation.Contact)
                {
                    _contactViewModel = null;
                    ReceiverAddress = string.Empty;
                }
                OnPropertyChanged("Chat");
            }
        }

        private void OnNewConversationCreated(object sender, ConversationEventArgs e)
        {
            if (ServiceManager.Instance.Dispatcher.Thread != Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new EventHandler<ConversationEventArgs>(OnNewConversationCreated), sender, new object[] { e });
                return;
            }
        }

        private void OnContactRemoved(object sender, ContactRemovedEventArgs e)
        {
            RemoveContact(e.contactId);
        }

        private void RemoveContact(ContactID contactId)
        {
            foreach (var contactViewModel in this.Contacts)
            {
                if (contactViewModel.Contact == contactId)
                {
                    this.Contacts.Remove(contactViewModel);
                    OnPropertyChanged("Contacts");
                    return;
                }
            }
        }

        private void OnConversationUpdated(object sender, ConversationUpdatedEventArgs e)
        {
            if (ServiceManager.Instance.Dispatcher.Thread != Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new EventHandler<ConversationUpdatedEventArgs>(OnConversationUpdated), sender, new object[] { e });
                return;
            }

            if (_contactViewModel != null && _contactViewModel.Contact == e.Conversation.Contact )
            {
                OnPropertyChanged("Messages");
            }
        }

        private void OnContactsChanged(object sender, EventArgs e)
        {
            if (ServiceManager.Instance.Dispatcher.Thread != Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new EventHandler<EventArgs>(OnContactsChanged), sender, new object[] { e });
                return;
            }

            if (Contacts != null)
            {
                this.Contacts.Clear();

                LoadContacts();
                if (Contacts != null && Contacts.Count > 0)
                    SetActiveChatContact(this.Contacts[0].Contact);
            }
        }

        private void Contact_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string propertyName = e.PropertyName;
            if (propertyName != null)
            {
                if (propertyName != "CustomStatus")
                {
                    if (propertyName != "Status")
                    {
                        return;
                    }
                }
                else
                {
                    OnPropertyChanged("CustomStatusUI");
                    return;
                }
                OnPropertyChanged("CustomStatusUI");
            }
        }

        public void LoadContacts()
        {
            foreach (var contact in _chatsManager.Contacts)
            {
                this.Contacts.Add(new ContactViewModel(contact));
            }
            OnPropertyChanged("Contacts");
        }

        public void SetActiveChatContact(VATRPContact contact)
        {
            if (contact == null)
                return;

            Console.WriteLine("SetActiveChat " + contact.Fullname);

            this._chat = _chatsManager.GetChat(contact);

            var contactVM = FindContactViewModel(contact);
            if (contactVM == null)
            {

                return;
            }
            
            if (_contactViewModel != null && _contactViewModel.Contact != contactVM.Contact)
            {
                _contactViewModel.IsSelected = false;
            }

            _contactViewModel = contactVM;
            

            if (this.Chat != null)
            {
                if (this.Chat.Contact != null)
                {
                    this.Chat.Contact.PropertyChanged += this.Contact_PropertyChanged;
                }
            }

            _contactViewModel.IsSelected = true;
            IsMessagesLoaded = false;

            ReceiverAddress = contact.Fullname;
            OnPropertyChanged("Chat");
            OnPropertyChanged("Messages");
        }

        private ContactViewModel FindContactViewModel(ContactID contact)
        {
            if (contact != null)
            {
                foreach (var contactViewModel in this.Contacts)
                {
                    if (contactViewModel.Contact == contact)
                        return contactViewModel;
                }
            }
            return null;
        }

        internal void SendMessage(char key, bool isIncomplete)
        {
            if (!ServiceManager.Instance.IsRttAvailable)
                return;

            var message = string.Format("{0}", key);
            if (!ReadyToSend(message))
                return;

            _chatsManager.ComposeAndSendMessage(ServiceManager.Instance.ActiveCallPtr, Chat, key, isIncomplete);

            if (!isIncomplete)
            {
                MessageText = string.Empty;
            }
        }

        internal void SendMessage(string message)
        {
            if (!ReadyToSend(message))
                return;

            _chatsManager.ComposeAndSendMessage(Chat, MessageText);
            MessageText = string.Empty;
        }

        private bool ReadyToSend(string message)
        {
            if (!ReceiverAddress.NotBlank() || !message.NotBlank())
                return false;

            VATRPContact contact = _chatsManager.FindContact(new ContactID(this.ReceiverAddress, IntPtr.Zero));
            if (contact == null)
            {
                contact = new VATRPContact(new ContactID(ReceiverAddress, IntPtr.Zero));
                contact.Fullname = ReceiverAddress;
                _contactsManager.AddContact(contact, string.Empty);
            }

            SetActiveChatContact(contact);
            return true;
        }

        public bool IsContactsLoaded
        {
            get { return _contactsLoaded; }
            set
            {
                if (_contactsLoaded != value)
                {
                    _contactsLoaded = value;
                    OnPropertyChanged("ContactsLoaded");
                }
            }
        }

        #region Properties

        public ObservableCollection<VATRPChatMessage> Messages
        {
            get
            {
                if (this.Chat != null)
                    return this.Chat.Messages;
                return null;
            }
        }


        public ObservableCollection<ContactViewModel> Contacts
        {
            get { return _contacts ?? (_contacts = new ObservableCollection<ContactViewModel>()); }
        }

        public VATRPChat Chat
        {
            get
            {
                return this._chat;
            }
        }
        
        public bool IsMessagesLoaded
        {
            get
            {
                return this._isMessagesLoaded;
            }
            private set
            {
                if (value != _isMessagesLoaded)
                {
                    _isMessagesLoaded = value;
                    OnPropertyChanged("IsMessagesLoaded");
                }
            }
        }

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

        public bool ShowMessageHint
        {
            get { return !MessageText.NotBlank(); }
        }

        public bool ShowSearchHint
        {
            get { return !ContactSearchCriteria.NotBlank(); }
        }

        public string MessageText
        {
            get { return _messageText; }
            set
            {
                if (_messageText != value)
                {
                    _messageText = value;
                    OnPropertyChanged("MessageText");
                    OnPropertyChanged("ShowMessageHint");
                }
            }
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
                }
            }
        }

        public ContactViewModel Contact
        {
            get
            {
                return _contactViewModel;
            }
        }

        #endregion


        public string LastInput { get; set; }
    }
}