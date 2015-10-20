namespace VATRP.LinphoneWrapper.Enums
{
    public enum LinphoneChatMessageState
    {
        LinphoneChatMessageStateIdle, /**< Initial state */
        LinphoneChatMessageStateInProgress, /**< Delivery in progress */
        LinphoneChatMessageStateDelivered, /**< Message successfully delivered and acknowledged by remote end point */
        LinphoneChatMessageStateNotDelivered, /**< Message was not delivered */
        LinphoneChatMessageStateFileTransferError, /**< Message was received(and acknowledged) but cannot get file from server */
        LinphoneChatMessageStateFileTransferDone /**< File transfer has been completed successfully. */
    }
}