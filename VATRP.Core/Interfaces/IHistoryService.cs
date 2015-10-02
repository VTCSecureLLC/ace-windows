using System;
using System.Collections.Generic;
using VATRP.Core.Events;
using VATRP.Core.Model;

namespace VATRP.Core.Interfaces
{
    public interface IHistoryService : IVATRPservice
    {
        #region Calls
        bool IsLoadingCalls { get; }
        
        int AddCallEvent(VATRPCallEvent callEvent);

        event EventHandler<VATRPCallEventArgs> OnCallHistoryEvent;

        int DeleteCallEvent(VATRPCallEvent callEvent);

        void ClearCallsItems();

        List<VATRPCallEvent> AllCallsEvents { get; }
        void LoadCallEvents();

        #endregion
    }
}
