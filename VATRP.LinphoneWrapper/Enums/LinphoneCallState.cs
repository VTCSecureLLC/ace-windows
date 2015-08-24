namespace VATRP.LinphoneWrapper.Enums
{
    public enum LinphoneCallState
    {
        LinphoneCallIdle, // Initial call state
        LinphoneCallIncomingReceived, // This is a new incoming call
        LinphoneCallOutgoingInit, // An outgoing call is started
        LinphoneCallOutgoingProgress, // An outgoing call is in progress
        LinphoneCallOutgoingRinging, // An outgoing call is ringing at remote end
        LinphoneCallOutgoingEarlyMedia, // An outgoing call is proposed early media
        LinphoneCallConnected, // <Connected, the call is answered
        LinphoneCallStreamsRunning, // The media streams are established and running
        LinphoneCallPausing, // The call is pausing at the initiative of local end
        LinphoneCallPaused, // The call is paused, remote end has accepted the pause
        LinphoneCallResuming, // The call is being resumed by local end
        LinphoneCallRefered, // <The call is being transfered to another party, resulting in a new outgoing call to follow immediately
        LinphoneCallError, // The call encountered an error
        LinphoneCallEnd, // The call ended normally
        LinphoneCallPausedByRemote, // The call is paused by remote end
        LinphoneCallUpdatedByRemote, // The call's parameters change is requested by remote end, used for example when video is added by remote
        LinphoneCallIncomingEarlyMedia, // We are proposing early media to an incoming call
        LinphoneCallUpdating, // A call update has been initiated by us
        LinphoneCallReleased, // The call object is no more retained by the core
        LinphoneCallEarlyUpdatedByRemote, // The call is updated by remote while not yet answered (early dialog SIP UPDATE received).
        LinphoneCallEarlyUpdating // We are updating the call while not yet answered (early dialog SIP UPDATE sent)
    };
}