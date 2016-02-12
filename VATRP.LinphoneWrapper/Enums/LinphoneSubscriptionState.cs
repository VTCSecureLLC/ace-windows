namespace VATRP.LinphoneWrapper.Enums
{
    public enum LinphoneSubscriptionState
    {
        LinphoneSubscriptionNone, /**< Initial state, should not be used.**/
        LinphoneSubscriptionOutgoingProgress, /**<An outgoing subcription was sent*/
        LinphoneSubscriptionIncomingReceived, /**<An incoming subcription is received*/
        LinphoneSubscriptionPending, /**<Subscription is pending, waiting for user approval*/
        LinphoneSubscriptionActive, /**<Subscription is accepted.*/
        LinphoneSubscriptionTerminated, /**<Subscription is terminated normally*/
        LinphoneSubscriptionError, /**<Subscription encountered an error, indicated by linphone_event_get_reason()*/
        LinphoneSubscriptionExpiring, /**<Subscription is about to expire, only sent if [sip]->refresh_generic_subscribe property is set to 0.*/

    }
}