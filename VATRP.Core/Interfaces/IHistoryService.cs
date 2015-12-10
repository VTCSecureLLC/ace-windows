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
        
        event EventHandler<VATRPCallEventArgs> OnCallHistoryEvent;
        
        void ClearCallsItems();

        List<VATRPCallEvent> AllCallsEvents { get; }

        #endregion
    }
}
