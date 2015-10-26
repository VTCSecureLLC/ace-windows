using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using VATRP.Core.Enums;
using VATRP.Core.Extensions;
using VATRP.Core.Model.Utils;

namespace VATRP.Core.Model
{
    public class VATRPChat : ChatID, IComparable<VATRPChat>
    {
        private string _injectedText;
        private bool _is_messagesLoaded;
        private string _lastMessage;
        private ObservableCollection<VATRPChatMessage> _messages;
        private string _name;
        private ObservableCollection<VATRPChatMessage> _temp_messages;
        private string _typing_text;
        private List<string> _typingIdList;
        private uint _unreadMsgCount;
        public int ChatUniqueID;

        public VATRPChat(VATRPContact contact, string dialogId ) : base(contact, dialogId)
        {
            this._typing_text = string.Empty;
            this._injectedText = string.Empty;
            this._is_messagesLoaded = true;
            this._lastMessage = string.Empty;
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

        internal void AddMessage(VATRPChatMessage msg)
        {
            if (this.IsMessagesLoaded)
            {
                if (this.Messages == null)
                {
                    return;
                }
                this.Messages.Add(msg);
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

        internal void ClearTyping()
        {
            if (this._typingIdList != null)
            {
                this._typingIdList.Clear();
                this.TypingText = string.Empty;
            }
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

        internal VATRPChatMessage FindMessage(string msgID)
        {
            if (!string.IsNullOrEmpty( msgID))
            {
                for (int i = this._messages.Count - 1; i >= 0; i--)
                {
                    if (this._messages[i].ID == msgID)
                    {
                        return this._messages[i];
                    }
                }
            }
            return null;
        }

        internal string GetContactsSequenceString(bool useGap = false)
        {
            if ((this.Contacts == null) || (this.Contacts.Count == 0))
            {
                return string.Empty;
            }
            int length = 20;
            int num2 = 100;
            StringBuilder builder = new StringBuilder();
            using (IEnumerator<VATRPContact> enumerator = this.Contacts.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    string str = enumerator.Current.Fullname;
                    if (str.Length > length)
                    {
                        builder.Append(str.Substring(0, length) + "...");
                    }
                    else
                    {
                        builder.Append(str);
                    }
                    builder.Append(";");
                    if (builder.Length > num2)
                    {
                        builder.Append("...");
                        return builder.ToString();
                    }
                    if (useGap)
                    {
                        builder.Append(" ");
                    }
                }
            }
            return builder.ToString();
        }

        internal void InsertMessageByTimestamp(VATRPChatMessage msg)
        {
            if (this.IsMessagesLoaded)
            {
                if (this.Messages == null)
                {
                    return;
                }
                InsertMessageByTimestamp(msg, this.Messages);
            }
            else
            {
                if (this.TempMessages == null)
                {
                    return;
                }
                InsertMessageByTimestamp(msg, this.TempMessages);
            }
            this.UpdateLastMessage();
        }

        private static void InsertMessageByTimestamp(VATRPChatMessage message, ObservableCollection<VATRPChatMessage> messageList)
        {
            if ((message != null) && (messageList != null) && (message.Content != null))
            {
                bool addToList = false;
                for (int i = 0; i < messageList.Count; i++)
                {
                    var msg = messageList[i];
                    if ((msg != null) && !msg.IsHeadingMessage)
                    {
                        long utcTimeStamp = 0L;
                        long localTimeToUtc = 0L;
                        try
                        {
                            if (msg.UtcTimestamp != 0)
                            {
                                localTimeToUtc = msg.UtcTimestamp;
                            }
                            else
                            {
                                DateTime time = msg.MessageTime;
                                if (msg.MessageTime == DateTime.MinValue)
                                {
                                    continue;
                                }
                                localTimeToUtc = long.Parse(Time.ConvertLocalTimeToUtcTime(msg.MessageTime));
                            }
                        }
                        catch (Exception exception)
                        {
                            Debug.WriteLine("Chat.InsertMessageByTimestamp: Error: " + exception.Message);
                            continue;
                        }

                        if (utcTimeStamp <= localTimeToUtc)
                        {
                            Tools.InsertByIndex(msg, messageList, i);
                            addToList = true;
                            break;
                        }
                    }
                }
                if (!addToList)
                {
                    Console.WriteLine("Adding new message: " + message);
                    messageList.Add(message);
                }
            }
        }

        private void InsertMessageToTop(VATRPChatMessage msg)
        {
            if (this.IsMessagesLoaded)
            {
                if (this.Messages == null)
                {
                    return;
                }
                InsertMessageToTop(msg, this.Messages);
            }
            else
            {
                if (this.TempMessages == null)
                {
                    return;
                }
                InsertMessageToTop(msg, this.TempMessages);
            }
            this.UpdateLastMessage();
        }

        private static void InsertMessageToTop(VATRPChatMessage msg, ObservableCollection<VATRPChatMessage> messageList)
        {
            if ((msg != null) && (messageList != null))
            {
                Tools.InsertToTop(msg, messageList);
            }
        }

        internal bool IsExistsTypingContacts()
        {
            if (this._typingIdList == null)
            {
                return false;
            }
            return (this._typingIdList.Count > 0);
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


        internal void UpdateLastMessage()
        {
            if ((this._messages != null) && (this._messages.Count != 0))
            {
                this.LastMessage = this._messages[this._messages.Count - 1].Content;
                this.LastMessageDirection = this._messages[this._messages.Count - 1].Direction;
            }
        }

        internal VATRPChatMessage SearchIncompleteMessage()
        {
            for (int i = 0; i < this.Messages.Count; i++)
            {
                var msg = this.Messages[i];
                if ((msg != null) && msg.IsIncompleteMessage)
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

        public MessageDirection LastMessageDirection { get; private set; }

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
                return this._unreadMsgCount;
            }
            set
            {
                this._unreadMsgCount = value;
                base.OnPropertyChanged("UnreadMsgCount");
                base.OnPropertyChanged("HasUnreadMsg");
            }
        }

    }
}

