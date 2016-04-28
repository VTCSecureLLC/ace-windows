using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
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

        #region Members

        protected string _messageText;
        protected VATRPChat _chat;
        protected string _message;
        protected readonly IChatService _chatsManager;
        protected readonly IContactsService _contactsManager;
        protected bool _isMessagesLoaded;
        protected bool _contactsLoaded;
        protected ObservableCollection<ContactViewModel> _contacts;
        protected ContactViewModel _contactViewModel;
        protected ContactViewModel _loggedInContactViewModel;

        protected ICollectionView messagesListView;

        protected Thread _inputProcessorThread;
        protected bool _isRunning;
        protected Queue<string> _inputTypingQueue = new Queue<string>();
        protected ManualResetEvent regulator = new ManualResetEvent(false);

        #endregion

        #region Events
        public event EventHandler<EventArgs> ConversationUpdated;
        #endregion

        public MessagingViewModel()
        {
            _messageText = string.Empty;
        }

        public MessagingViewModel(IChatService chatMng, IContactsService contactsMng):this()
        {
            this._chatsManager = chatMng;
            this._contactsManager = contactsMng;
            this._chatsManager.ConversationUpdated += OnConversationUpdated;
            this._chatsManager.NewConversationCreated += OnNewConversationCreated;
            this._chatsManager.ConversationClosed += OnConversationClosed;
            this._chatsManager.ContactsChanged += OnContactsChanged;
            this._chatsManager.ContactAdded += OnChatContactAdded;
            this._chatsManager.ContactRemoved += OnChatContactRemoved;
            this._contactsManager.LoggedInContactUpdated += OnLoggedContactUpdated;
        }

        #region Events
        private void OnLoggedContactUpdated(object sender, ContactEventArgs e)
        {
            if (ServiceManager.Instance.Dispatcher.Thread != Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new EventHandler<ContactEventArgs>(OnChatContactAdded), sender, new object[] { e });
                return;
            }

            
            if (_loggedInContactViewModel == null || _loggedInContactViewModel.Contact != e.Contact)
            {
                var loggedInContact = _contactsManager.FindContact(e.Contact);
                if (loggedInContact != null)
                {
                    _loggedInContactViewModel = new ContactViewModel(loggedInContact);
                }
            }
        }

        private void OnChatContactRemoved(object sender, ContactRemovedEventArgs e)
        {
            if (ServiceManager.Instance.Dispatcher.Thread != Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new EventHandler<ContactRemovedEventArgs>(OnChatContactRemoved), sender, new object[] { e });
                return;
            }

            var contactVM = FindContactViewModel(e.contactId);
            if (contactVM != null)
            {
                this.Contacts.Remove(contactVM);
                OnPropertyChanged("Contacts");
            }
        }

        private void OnChatContactAdded(object sender, ContactEventArgs e)
        {
            if (ServiceManager.Instance.Dispatcher.Thread != Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new EventHandler<ContactEventArgs>(OnChatContactAdded), sender, new object[] { e });
                return;
            }
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
                if (_contactViewModel != null && _contactViewModel.Contact == e.Conversation.Contact)
                {
                    _contactViewModel = null;
                }

                RemoveContact(contact);

                if (this.Chat == e.Conversation)
                {
                    this._chat = null;
                    MessagesListView = null;
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
                if (MessagesListView != null)
                    MessagesListView.Refresh();
                if (ConversationUpdated != null)
                    ConversationUpdated(this, EventArgs.Empty);
            }
            else
            {
                foreach (var contactVM in this.Contacts)
                {
                    if (contactVM.Contact == e.Conversation.Contact)
                    {
                        contactVM.LastUnreadMessageTime = e.Conversation.LastUnreadMessageTime;
                    }
                }
            }
            RefreshContactsList();
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
                    SetActiveChatContact(this.Contacts[0].Contact, IntPtr.Zero);
            }
        }

        protected void Contact_PropertyChanged(object sender, PropertyChangedEventArgs e)
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

        protected virtual void ChangeUnreadCounter()
        {
            
        }
        protected virtual void RefreshContactsList()
        {

        }

        protected virtual void ProcessInputCharacters(object obj)
        {
            
        }

        #endregion

        #region Methods

        internal void EnqueueInput(string inputString)
        {
            lock (_inputTypingQueue)
            {
                _inputTypingQueue.Enqueue(inputString);
            }
            regulator.Set();
        }

        internal void StopInputProcessor()
        {
            _isRunning = false;
            regulator.Set();
        }

        public void LoadContacts()
        {
            foreach (var contact in _chatsManager.Contacts)
            {
                this.Contacts.Add(new ContactViewModel(contact));
            }
            OnPropertyChanged("Contacts");
        }

        public void SetActiveChatContact(VATRPContact contact, IntPtr callPtr)
        {
            if (contact == null)
                return;

            if (Chat != null && Chat.Contact == contact)
            {
                Chat.Contact.UnreadMsgCount = 0;
                Chat.UnreadMsgCount = 0;
                ChangeUnreadCounter();
                _chatsManager.ActivateChat(Chat);
                return;
            }

            Console.WriteLine("SetActiveChat " + contact.Fullname);

            this._chat = _chatsManager.GetChat(contact);

            var contactVM = FindContactViewModel(contact);
            if (contactVM == null)
            {
                contactVM = new ContactViewModel(contact);
                this.Contacts.Add(contactVM);
            }
            
            if (_contactViewModel != null && _contactViewModel.Contact != contactVM.Contact)
            {
                _contactViewModel.IsSelected = false;
            }

            _contactViewModel = contactVM;

            if (Chat != null)
            {
                Chat.CallPtr = callPtr;
                
                if (Chat.Contact != null)
                {
                    Chat.Contact.UnreadMsgCount = 0;
                    this.Chat.Contact.PropertyChanged += this.Contact_PropertyChanged;
                }

                Chat.CharsCountInBubble = 0;
                Chat.UnreadMsgCount = 0;
                ChangeUnreadCounter();
                _chatsManager.ActivateChat(Chat);
                if (App.CurrentAccount != null)
                {
                    Chat.MessageFont = App.CurrentAccount.RTTFontFamily;
                }
            }

            _contactViewModel.IsSelected = true;
            IsMessagesLoaded = false;

            if ( Messages != null)
            {
                this.MessagesListView = CollectionViewSource.GetDefaultView(this.Messages);
                this.MessagesListView.SortDescriptions.Add(new SortDescription("MessageTime",
                    ListSortDirection.Ascending));
            }
			
            OnPropertyChanged("Chat");
			if (MessagesListView != null)
			     MessagesListView.Refresh();
            if (ConversationUpdated != null)
                ConversationUpdated(this, EventArgs.Empty);
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

        #endregion

        #region Properties

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
            set
            {
                this._chat = value;
                OnPropertyChanged("Chat");
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

        public bool ShowMessageHint
        {
            get { return string.IsNullOrEmpty(MessageText); }
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
       
        public ContactViewModel Contact
        {
            get
            {
                return _contactViewModel;
            }
        }

        public ContactViewModel LoggedContact
        {
            get
            {
                return _loggedInContactViewModel;
            }
        }

        public ICollectionView MessagesListView
        {
            get { return this.messagesListView; }
            protected set
            {
                if (value == this.messagesListView)
                {
                    return;
                }

                this.messagesListView = value;
                OnPropertyChanged("MessagesListView");
            }
        }

        #endregion
        
    }
}