namespace VATRP.LinphoneWrapper.Enums
{
    /**
 * Indicates for a given media the stream direction
 * */
    public enum LinphoneMediaDirection
    {
        LinphoneMediaDirectionInactive, /** No active media not supported yet*/
        LinphoneMediaDirectionSendOnly, /** Send only mode*/
        LinphoneMediaDirectionRecvOnly, /** recv only mode*/
        LinphoneMediaDirectionSendRecv, /** send receive*/

    };
}