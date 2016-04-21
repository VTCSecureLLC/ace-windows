using System;
using System.Collections.Generic;
using VATRP.Core.Model;
using VATRP.LinphoneWrapper.Enums;

namespace VATRP.Core.Events
{
    public class CardDavSyncEventArgs : EventArgs
    {
        public LinphoneFriendListSyncStatus SyncStatus { get; private set; }
        public CardDavSyncEventArgs(LinphoneFriendListSyncStatus status)
        {
            SyncStatus = status;
        }
    }
}

