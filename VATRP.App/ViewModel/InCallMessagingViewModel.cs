using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Extensions;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class InCallMessagingViewModel : MessagingViewModel
    {

        #region Members

        private ObservableCollection<string> _textSendModes;
        private string _selectedTextSendMode;
        private string _sendButtonTitle;
        private bool _isSendingModeRtt = true;
        private DateTime _conversationStartTime;
        #endregion

        #region Events

        public event EventHandler<EventArgs> RttReceived;
        
        #endregion

        public InCallMessagingViewModel() 
        {
            Init();
        }

        public InCallMessagingViewModel(IChatService chatMng, IContactsService contactsMng)
            : base(chatMng, contactsMng) 
        {
            Init();
            _chatsManager.RttReceived += OnRttReceived;
        }

        private void Init()
        {
            _isRunning = true;
            TextSendModes.Add("Real Time Text");
            TextSendModes.Add("SIP Simple");
            _inputProcessorThread = new Thread(ProcessInputCharacters) { IsBackground = true };
            _inputProcessorThread.Start();
            //SelectedTextSendMode = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
            //    Configuration.ConfEntry.TEXT_SEND_MODE, "Real Time Text");
            _sendButtonTitle = "Send";
        }

        private void OnRttReceived(object sender, EventArgs e)
        {
            if (RttReceived != null)
            {
                RttReceived(sender, EventArgs.Empty);
            }
        }

        #region Methods
    
        internal void SendMessage(char key, bool isIncomplete)
        {
            if (!ServiceManager.Instance.IsRttAvailable)
                return;
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
                    var message = string.Format("{0}", key);
                    if (!message.NotBlank())
                        return;

                    _chatsManager.ComposeAndSendMessage(ServiceManager.Instance.ActiveCallPtr, Chat, key, isIncomplete);

                    if (!isIncomplete || key == '\r')
                    {
                        MessageText = string.Empty;
                    }
                });
        }

        internal void SendMessage(string message)
        {
            Dispatcher dispatcher = null;
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
                    if (!message.NotBlank())
                        return;

                    Debug.WriteLine("Sending message: Count - " + message.Length + " \r" + message);
                    _chatsManager.ComposeAndSendMessage(Chat, message);
                });
        }
		
        public void CreateRttConversation(string remoteUsername, IntPtr callPtr)
        {
            string un, host;
            int port;
            VATRPCall.ParseSipAddress(remoteUsername, out un, out host, out port);
            var contactAddress = string.Format("{0}@{1}", un, host.NotBlank() ? host : App.CurrentAccount.ProxyHostname);
            var contactID = new ContactID(contactAddress, IntPtr.Zero);

            VATRPContact contact =
                ServiceManager.Instance.ContactService.FindContact(contactID);
            if (contact == null)
            {
                contact = new VATRPContact(contactID)
                {
                    Fullname = un,
                    DisplayName = un,
                    SipUsername = un,
                    RegistrationName = contactAddress
                };
                ServiceManager.Instance.ContactService.AddContact(contact, string.Empty);
            }
            SetActiveChatContact(contact, callPtr);
#if false
            if (Chat != null)
                Chat.InsertRttWrapupMarkers(callPtr);
#endif
        }

        public void ClearRTTConversation(IntPtr callPtr)
        {
            _messageText = string.Empty;
            _contactViewModel = null;

            if (Chat != null)
                Chat.ClearRttMarkers(callPtr);

            if (MessagesListView != null)
                MessagesListView.Refresh();
            OnPropertyChanged("Chat");
        }

        protected override bool FilterMessages(object obj)
        {
            var message = obj as VATRPChatMessage;

            if (message != null)
                return ( message.MessageTime >= ConversationStartTime && ( message.IsRTTMessage || message.IsRTTMarker));

            return false;
        }

        protected override void ProcessInputCharacters(object obj)
        {
            var sb = new StringBuilder();
            int wait_time = 5;
            bool readyToDeque = true;
            while (_isRunning)
            {
                regulator.WaitOne(wait_time);

                wait_time = 1;

                if (!readyToDeque)
                    continue;

                lock (_inputTypingQueue)
                {
                    if (_inputTypingQueue.Count != 0)
                    {
                        sb.Append(_inputTypingQueue.Dequeue());
                    }
                    else
                    {
                        wait_time = Int32.MaxValue;
                        readyToDeque = true;
                        continue;
                    }
                }

                readyToDeque = false;
                for (int i = 0; i < sb.Length; i++)
                {
                    SendMessage(sb[i], sb[i] != '\r');
                }

                sb.Remove(0, sb.Length);
                readyToDeque = true;
            }
        }

        #endregion

        #region Properties

        public DateTime ConversationStartTime
        {
            get { return _conversationStartTime; }
            set
            {
                _conversationStartTime = value; 
                if (MessagesListView != null)
                    MessagesListView.Refresh();
            }
        }
        
        public ObservableCollection<string> TextSendModes
        {
            get { return _textSendModes ?? (_textSendModes = new ObservableCollection<string>()); }
        }

        public bool IsSendingModeRTT
        {
            get { return _isSendingModeRtt; }
        }

        public string SelectedTextSendMode
        {
            get { return _selectedTextSendMode; }
            set
            {
                _selectedTextSendMode = value;
                OnPropertyChanged("SelectedTextSendMode");
                //_isSendingModeRtt = _selectedTextSendMode.Equals("Real Time Text");
                //SendButtonTitle = IsSendingModeRTT ? "Send (RTT)" : "Send (SIP)";
            }
        }

        public string SendButtonTitle
        {
            get { return _sendButtonTitle; }
            set
            {
                _sendButtonTitle = value;
                OnPropertyChanged("SendButtonTitle");
            }
        }

        #endregion

    }
}