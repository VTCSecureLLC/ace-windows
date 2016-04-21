using System;
using System.Collections.Generic;
using VATRP.Core.Model;

namespace VATRP.Core.Events
{
    public class CardDavContactEventArgs : EventArgs
    {
        public enum CardDavAction 
        {
            Create,
            Update,
            Delete
        }

        public CardDavAction OpCode { get; private set; }

        public IntPtr FriendListPtr { get; set; }
        public IntPtr NewContactPtr { get; set; }
        public IntPtr ChangedContactPtr { get; set; }

        public CardDavContactEventArgs(CardDavAction code)
        {
            OpCode = code;
            FriendListPtr = IntPtr.Zero;
            NewContactPtr = IntPtr.Zero;
            ChangedContactPtr = IntPtr.Zero;
        }
    }
}

