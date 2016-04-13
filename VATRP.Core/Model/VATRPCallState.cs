namespace VATRP.Core.Model
{
    public enum VATRPCallState
    {
        None,
        Trying,
        InProgress,
        Ringing,
        EarlyMedia,
        Connected,
        StreamsRunning,
        RemoteUpdating,
        RemoteUpdated,
        RemotePausing,
        RemotePaused,
        LocalPausing,
        LocalPaused,
        LocalResuming,
        LocalResumed,
        Closed,
        Declined,
        Error
    }
}