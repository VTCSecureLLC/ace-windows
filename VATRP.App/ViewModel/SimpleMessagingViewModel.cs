using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Data;
using System.Windows.Threading;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Events;
using VATRP.Core.Extensions;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using System.Text;
using VATRP.Core.Enums;


namespace com.vtcsecure.ace.windows.ViewModel
{
    public class SimpleMessagingViewModel : MessagingViewModel
    {

        #region Members

        private string _receiverAddress;
        private string _contactSearchCriteria;
        private ICollectionView contactsListView;
        
        #endregion

        #region Events

        public event EventHandler UnreadMessagesCountChanged;
        public event EventHandler<DeclineMessageArgs> DeclineMessageReceived;
        #endregion

        public SimpleMessagingViewModel()
        {
            Init();
        }

        public SimpleMessagingViewModel(IChatService chatMng, IContactsService contactsMng)
            : base(chatMng, contactsMng)
        {
            Init();
            _chatsManager.ConversationUnReadStateChanged += OnUnreadStateChanged;
            _chatsManager.ConversationDeclineMessageReceived += OnDeclineMessageReceived;
        }

        private void OnDeclineMessageReceived(object sender, DeclineMessageArgs args)
        {
            if (ServiceManager.Instance.Dispatcher.Thread != Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new EventHandler<DeclineMessageArgs>(OnDeclineMessageReceived) , sender, new object[] { args });
                return;
            }

            var newArgs = new DeclineMessageArgs(args.MessageHeader, args.DeclineMessage) {Sender = args.Sender};
            if (DeclineMessageReceived != null) 
                DeclineMessageReceived(sender, newArgs);
        }

        private void OnUnreadStateChanged(object sender, VATRP.Core.Events.ConversationEventArgs e)
        {
            if (ServiceManager.Instance.Dispatcher.Thread != Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new EventHandler<VATRP.Core.Events.ConversationEventArgs>(OnUnreadStateChanged), sender, new object[] { e });
                return;
            }

            ChangeUnreadCounter();
        }

        private void Init()
        {
            _isRunning = true;
            _inputProcessorThread = new Thread(ProcessInputCharacters) { IsBackground = true };
            _inputProcessorThread.Start();

            _contactSearchCriteria = string.Empty;
            _receiverAddress = string.Empty;
            LoadContacts();
            this.ContactsListView = new CollectionViewSource { Source = this.Contacts }.View;  
            this.ContactsListView.SortDescriptions.Add(new SortDescription("LastUnreadMessageTime", ListSortDirection.Descending));
            this.ContactsListView.SortDescriptions.Add(new SortDescription("ContactUI", ListSortDirection.Ascending));
            this.ContactsListView.Filter = new Predicate<object>(this.FilterContactsList);
        }


        #region Methods

        protected override bool FilterMessages(object obj)
        {
            var message = obj as VATRPChatMessage;

            if (message != null)
            {
                if (message.Direction == MessageDirection.Incoming)
                    return !message.IsIncompleteMessage && !message.IsRTTStartMarker && !message.IsRTTEndMarker;
                return !message.IsRTTMarker;
            }

            return false;
        }

        protected override void ChangeUnreadCounter()
        {
            if (UnreadMessagesCountChanged != null)
                UnreadMessagesCountChanged(this, EventArgs.Empty);
        }

        protected override void RefreshContactsList()
        {
            if (ContactsListView != null)
                ContactsListView.Refresh();
            OnPropertyChanged("ContactsListView");
        }

        protected override void ProcessInputCharacters(object obj)
        {
            var sendBuffer = new StringBuilder();
            var wait_time = Int32.MaxValue;

            while (_isRunning)
            {
                regulator.WaitOne(wait_time);

                lock (_inputTypingQueue)
                {
                    if (_inputTypingQueue.Count != 0)
                    {
                        sendBuffer.Append(_inputTypingQueue.Dequeue());
                    }
                    else
                    {
                        wait_time = Int32.MaxValue;
                        continue;
                    }
                }

                SendMessage(sendBuffer.ToString());

                sendBuffer.Remove(0, sendBuffer.Length);
                lock (_inputTypingQueue)
                {
                    wait_time = _inputTypingQueue.Count == 0 ? Int32.MaxValue : 1;
                }
            }
        }

        internal bool SendSimpleMessage(string message)
        {
            if (!message.NotBlank() || (Chat == null && string.IsNullOrEmpty(ReceiverAddress)))
                return false;
            EnqueueInput(message);
            MessageText = string.Empty;
            return true;
        }

        private void SendMessage(string message)
        {
            Dispatcher dispatcher;
            try
            {
                dispatcher = ServiceManager.Instance.Dispatcher;
            }
            catch (NullReferenceException)
            {
                return;
            }

            if (dispatcher != null)
                dispatcher.BeginInvoke((Action) delegate()
                {
                    _chatsManager.ComposeAndSendMessage(Chat, message);
                });
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

        internal void ShowUnreadMessageInfo(bool updUnreadCounter)
        {
            if (Chat == null)
                return;
            Chat.UpdateUnreadCounter = updUnreadCounter;
        }
    }
}