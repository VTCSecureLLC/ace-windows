namespace VATRP.Core.Events
{
    public class VATRPCallEventArgs : VATRPEventArgs
    {
        public readonly HistoryEventTypes historyEventType;
        public VATRPCallEventArgs(HistoryEventTypes nType)
        {
            historyEventType = nType;
        }
    }
}