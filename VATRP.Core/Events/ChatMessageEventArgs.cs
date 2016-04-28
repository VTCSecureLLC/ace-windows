using System;
using System.Collections.Generic;
using VATRP.Core.Model;

namespace VATRP.Core.Events
{
    public class ChatMessageEventArgs : TextMessageEventArgs, ICloneable
    {
        private List<IntPtr> _chatPtrList;
        private VATRPChatMessage _chatMessage;

        public ChatMessageEventArgs(string remote, IntPtr chatPtr, List<IntPtr> callChatPtrList, VATRPChatMessage chatMessage)
            : base(remote)
        {
            _chatPtrList = new List<IntPtr>();
            _chatPtrList.AddRange(callChatPtrList);
            this.ChatPtr = chatPtr;
            _chatMessage = new VATRPChatMessage(chatMessage.ContentType);
            _chatMessage = chatMessage;
        }

        public List<IntPtr> CallsChatPtrList
        {
            get { return _chatPtrList; }
            private set { _chatPtrList = value; }
        }

        public IntPtr ChatPtr { get; private set; }

        public VATRPChatMessage ChatMessage
        {
            get { return _chatMessage; }
            private set { _chatMessage = value; }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}

