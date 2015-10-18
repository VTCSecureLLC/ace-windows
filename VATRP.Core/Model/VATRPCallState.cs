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
        RemoteUpdated,
        RemotePaused,
        Closed,
        Error
    }
}