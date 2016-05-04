using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using VATRP.Core.Enums;
using VATRP.Core.Events;
using VATRP.Core.Extensions;
using VATRP.Core.Model.Utils;
using VATRP.LinphoneWrapper;
using VATRP.LinphoneWrapper.Structs;

namespace VATRP.Core.Model
{
    public class VATRPChat : ChatID, IComparable<VATRPChat>
    {
        private bool _is_messagesLoaded;
        private string _lastMessage;
        private ObservableCollection<VATRPChatMessage> _messages;
        private string _name;
        private ObservableCollection<VATRPChatMessage> _temp_messages;
        private string _typing_text;
        private List<string> _typingIdList;
        private uint _unreadMsgCount;
        private string _messageFont;
        public int ChatUniqueID;

        public VATRPChat() : this(null, string.Empty, false)
        {
            
        }
        public VATRPChat(VATRPContact contact, string dialogId, bool isRtt) : base(contact, isRtt, dialogId)
        {
            this._typing_text = string.Empty;
            this._is_messagesLoaded = true;
            this._lastMessage = string.Empty;
            this._messageFont = "Segoe UI";
            if (contact != null)
            {
                this.Messages = new ObservableCollection<VATRPChatMessage>();
                this._temp_messages = new ObservableCollection<VATRPChatMessage>();
                this.Contacts = new ObservableCollection<VATRPContact>();
                this._is_messagesLoaded = true;
                this.Contacts.Add(contact);
                base.ID = contact.ID;
                this.Name = contact.Fullname;
                this.ShowNotInCL_Notification = false;
                this.IsEmptychat = false;
            }
        }

        internal bool AddContact(VATRPContact contact)
        {
            if (contact == null )
            {
                return false;
            }

            VATRPContact existingContact = this.FindContact(contact);
            if (existingContact != null)
            {
                existingContact.Status = contact.Status;
                return false;
            }

            lock (this.Contacts)
            {
                this.Contacts.Add(contact);
            }
            return true;
        }

        internal void AddMessage(VATRPChatMessage msg, bool isRtt)
        {
            if (this.IsMessagesLoaded)
            {
                if (this.Messages == null)
                {
                    return;
                }
                    
                if (!isRtt)
                {
                    this.Messages.Add(msg);
                }
                else
                {
                    // RTT message should be inserted before RTT end marker
                    if (this.Messages.Count > 0)
                    {
                        if (Messages[Messages.Count - 1].IsRTTEndMarker)
                        {
                            Tools.InsertByIndex(msg, this.Messages, this._messages.Count - 2);
                        }
                        else
                        {
                            this.Messages.Add(msg);
                        }
                    }
                }
            }
            else if (this.TempMessages != null)
            {
                this.TempMessages.Add(msg);
            }
        }

        internal void DeleteMessage(VATRPChatMessage msg)
        {
            if (this.IsMessagesLoaded)
            {
                if (this.Messages == null)
                {
                    return;
                }
                this.Messages.Remove(msg);
            }
            else if (this.TempMessages != null)
            {
                this.TempMessages.Remove(msg);
            }
        }

        public void ClearRttMarkers(IntPtr callPtr)
        {
            bool continueSearch = true;
            while (continueSearch)
            {
                continueSearch = false;
                for (int i = 0; i < Messages.Count; i++)
                {
                    Messages[i].IsIncompleteMessage = false;
                    if (Messages[i].IsRTTMarker)
                    {
                        Messages[i].IsRTTMarker = false;
                    }
                    else if (Messages[i].IsRTTStartMarker || Messages[i].IsRTTEndMarker)
                    {
                        Messages.RemoveAt(i);
                        continueSearch = true;
                        break;
                    }
                }
            }
        }

        public void InsertRttWrapupMarkers(IntPtr callPtr)
        {
            var startRttMessage = new VATRPChatMessage(MessageContentType.Text)
            {
                IsRTTMarker = false,
                IsRTTStartMarker = true,
                IsIncompleteMessage = false,
                IsRTTMessage = false,
                MessageTime = DateTime.Now
            };

            Messages.Add(startRttMessage);

            var endRttMessage = new VATRPChatMessage(MessageContentType.Text)
            {
                IsRTTMarker = false,
                IsRTTEndMarker = true,
                IsIncompleteMessage = false,
                IsRTTMessage = false,
                MessageTime = DateTime.Now.AddHours(3)
            };

            Messages.Add(endRttMessage);
        }

        public int CompareTo(VATRPChat other)
        {
           
            return 0;
        }

        internal bool DeleteContact(VATRPContact contact)
        {
            if (contact == null)
            {
                return false;
            }
            return this.DeleteContact(new ContactID(contact));
        }

        internal bool DeleteContact(ContactID contactID)
        {
            bool flag = false;
            if (contactID != null)
            {
                lock (this.Contacts)
                {
                    for (int i = 0; i < this.Contacts.Count; i++)
                    {
                        if (this.Contacts[i].ID == contactID.ID)
                        {
                            flag = true;
                            this.Contacts.RemoveAt(i);
                            break;
                        }
                    }
                    
                }
            }
            return flag;
        }

        public override bool Equals(ContactID other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }
            return (object.ReferenceEquals(other, this) || base.Equals(other) );
        }

        private VATRPContact FindContact(VATRPContact _ce)
        {
            VATRPContact contact = null;
            if (_ce != null)
            {
                
                lock (this.Contacts)
                {
                    for (int i = 0; i < this.Contacts.Count; i++)
                    {
                        if (this.Contacts[i].ID == _ce.ID)
                        {
                            return this.Contacts[i];
                        }
                    }
                }
            }
            return contact;
        }

        internal VATRPChatMessage FindMessage(IntPtr msgID)
        {
            for (int i = this._messages.Count - 1; i >= 0; i--)
            {
                if (this._messages[i].NativePtr == msgID)
                {
                    return this._messages[i];
                }
            }
            return null;
        }

        internal void SetTyping(ContactID contactID)
        {
            if (contactID == null)
            {
                return;
            }
            if (this._typingIdList == null)
            {
                this._typingIdList = new List<string>();
            }
            bool flag = false;
            using (List<string>.Enumerator enumerator = this._typingIdList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current == contactID.ID)
                    {
                        flag = true;
                        break;
                    }
                }
            }

            if (!flag)
            {
                this._typingIdList.Add(contactID.ID);
            }
            try
            {
                if (this._typingIdList.Count == 1)
                {
                    foreach (VATRPContact contact in this.Contacts)
                    {
                        if (contact.ID == contactID.ID)
                        {
                            // TODO Send VATRPContact typing message, One contact typing
                        }
                    }
                }
                else if (this._typingIdList.Count > 1)
                {
                    // TODO Send VATRPContact typing message, several contacts typing
                }
                else
                {
                    this.TypingText = string.Empty;
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Chat.UnSetTyping: Error: " + exception.Message);
            }
        }

        internal void UnSetTyping(ContactID contactID)
        {
            if (contactID == null)
            {
                return;
            }
            if (this._typingIdList == null)
            {
                return;
            }
            bool flag = false;
            using (List<string>.Enumerator enumerator = this._typingIdList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current == contactID.ID)
                    {
                        flag = true;
                        break;
                    }
                }
            }

            if (flag)
            {
                this._typingIdList.Remove(contactID.ID);
            }
            try
            {
                if (this._typingIdList.Count == 1)
                {
                    foreach (VATRPContact contact in this.Contacts)
                    {
                        if (contact.ID == this._typingIdList[0])
                        {
                            // TODO; Send User unset typing
                        }
                    }
                }
                else if (this._typingIdList.Count > 1)
                {
                    // TODO; Send User unset typing
                }
                else
                {
                    this.TypingText = string.Empty;
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine( "Chat.UnSetTyping: Error: " + exception.Message);
            }
        }


        internal void UpdateLastMessage(bool updateRttMessage)
        {
            if ((this._messages != null) && (this._messages.Count != 0))
            {
                int offset = !updateRttMessage ? 1 : 2;
                this.LastMessage = this._messages[this._messages.Count - offset].Content;
                LastMessageTime = this._messages[this._messages.Count - offset].MessageTime;

                if (this.LastMessageDate.Date != this._messages[this._messages.Count - offset].MessageTime.Date)
                {
                    this.LastMessageDate = this._messages[this._messages.Count - offset].MessageTime.Date;
                    // Add date separator here
                    var chatMsg = new VATRPChatMessage(MessageContentType.Info)
                    {
                        MessageTime = new DateTime(this.LastMessageDate.Year, this.LastMessageDate.Month, this.LastMessageDate.Day),
                        IsSeparator = true,
                        IsRTTMarker = updateRttMessage,
                        IsIncompleteMessage = false
                    };
                    Tools.InsertByIndex(chatMsg, this.Messages, this._messages.Count - offset);
                }
                else
                {
                    this.LastMessageDate = this._messages[this._messages.Count - offset].MessageTime.Date;
                }
            }
        }

        internal VATRPChatMessage SearchIncompleteMessage(MessageDirection msgDirection)
        {
            for (int i = 0; i < this.Messages.Count; i++)
            {
                var msg = this.Messages[i];
                if ((msg != null) && msg.IsIncompleteMessage && msg.Direction == msgDirection)
                {
                    return msg;
                }
            }
            return null;
        }

        public VATRPContact Contact
        {
            get
            {
                if (this.Contacts.Count >= 1)
                {
                    return this.Contacts[0];
                }
                return null;
            }
        }

        public VATRPContact LoggedContact
        {
            get
            {
                if (this.Contacts.Count > 1)
                {
                    return this.Contacts[1];
                }
                return null;
            }
        }

        public ObservableCollection<VATRPContact> Contacts { get; set; }

        
        public string DialogId
        {
            get
            {
                return base.DialogID;
            }
            set
            {
                base.DialogID = value;
            }
        }

        public bool HasUnreadMsg
        {
            get
            {
                return (this.UnreadMsgCount > 0);
            }
        }

      
        public bool IsEmptychat { get; set; }

        public bool IsMessagesLoaded
        {
            get
            {
                return true;
            }
            set
            {
                if (!value && Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                this._is_messagesLoaded = value;
                base.OnPropertyChanged("IsMessagesLoaded");
            }
        }

        
        public string LastMessage
        {
            get
            {
                return this._lastMessage;
            }
            private set
            {
                if (value != null)
                {
                    if (value.Length < 45)
                    {
                        this._lastMessage = value;
                    }
                    else
                    {
                        this._lastMessage = value.Substring(0, 43) + "...";
                    }
                }
                else
                {
                    this._lastMessage = value;
                }
                base.OnPropertyChanged("LastMessage");
            }
        }

        public DateTime LastMessageDate { get; private set; }
        public DateTime LastMessageTime { get; private set; }

        public DateTime LastUnreadMessageTime
        {
            get
            {
                if (HasUnreadMsg)
                    return LastMessageTime;
                return DateTime.MinValue;
            }
        }

        public ObservableCollection<VATRPChatMessage> Messages
        {
            get
            {
                if (this._messages == null)
                {
                    this._messages = new ObservableCollection<VATRPChatMessage>();
                }
                return this._messages;
            }
            set
            {
                this._messages = value;
                base.OnPropertyChanged("Messages");
            }
        }

        public string MessageFont
        {
            get { return _messageFont; }
            set
            {
                _messageFont = value;
                base.OnPropertyChanged("MessageFont");
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
                base.OnPropertyChanged("Name");
            }
        }

        public bool ShowNotInCL_Notification { get; set; }

        public IntPtr CallPtr { get; set; }

        public ObservableCollection<VATRPChatMessage> TempMessages
        {
            get
            {
                return this._temp_messages;
            }
            set
            {
                this._temp_messages = value;
                base.OnPropertyChanged("TempMessages");
            }
        }

        public string TypingText
        {
            get
            {
                return this._typing_text;
            }
            set
            {
                this._typing_text = value;
                base.OnPropertyChanged("TypingText");
            }
        }

        
        public uint UnreadMsgCount
        {
            get
            {
                if (IsRttChat)
                    return 0; 
                return this._unreadMsgCount;
            }
            set
            {
                if (!IsSelected || UpdateUnreadCounter)
                {
                    this._unreadMsgCount = value;
                    base.OnPropertyChanged("UnreadMsgCount");
                    base.OnPropertyChanged("HasUnreadMsg");
                    base.OnPropertyChanged("LastUnreadMessageTime");
                }
            }
        }
        
        internal bool CheckMessage(VATRPChatMessage other)
        {
            if (other == null)
                return false;
            for (int i = 0; i < Messages.Count; i++)
            {
                var msg = Messages[i];
                if ((msg != null) && 
                    (msg.MessageTime == other.MessageTime && 
                    msg.Direction == other.Direction ))
                {
                    return false;
                }
            }
            return true;
        }

        public int CharsCountInBubble { get; set; }

        public bool IsSelected { get; set; }

        public bool UpdateUnreadCounter { get; set; }
    }
}

