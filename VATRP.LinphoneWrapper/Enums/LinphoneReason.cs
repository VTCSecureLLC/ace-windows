namespace VATRP.LinphoneWrapper.Enums
{
    public enum LinphoneReason
    {
        LinphoneReasonNone,
        LinphoneReasonNoResponse, /**<No response received from remote*/
        LinphoneReasonForbidden, /**<Authentication failed due to bad credentials or resource forbidden*/
        LinphoneReasonDeclined, /**<The call has been declined*/
        LinphoneReasonNotFound, /**<Destination of the call was not found.*/
        LinphoneReasonNotAnswered, /**<The call was not answered in time (request timeout)*/
        LinphoneReasonBusy, /**<Phone line was busy */
        LinphoneReasonUnsupportedContent, /**<Unsupported content */
        LinphoneReasonIOError, /**<Transport error: connection failures, disconnections etc...*/
        LinphoneReasonDoNotDisturb, /**<Do not disturb reason*/
        LinphoneReasonUnauthorized, /**<Operation is unauthorized because missing credential*/
        LinphoneReasonNotAcceptable, /**<Operation like call update rejected by peer*/
        LinphoneReasonNoMatch, /**<Operation could not be executed by server or remote client because it didn't have any context for it*/
        LinphoneReasonMovedPermanently, /**<Resource moved permanently*/
        LinphoneReasonGone, /**<Resource no longer exists*/
        LinphoneReasonTemporarilyUnavailable, /**<Temporarily unavailable*/
        LinphoneReasonAddressIncomplete, /**<Address incomplete*/
        LinphoneReasonNotImplemented, /**<Not implemented*/
        LinphoneReasonBadGateway, /**<Bad gateway*/
        LinphoneReasonServerTimeout, /**<Server timeout*/
        LinphoneReasonUnknown /**Unknown reason*/
    }
}