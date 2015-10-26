using System.ComponentModel;

namespace VATRP.LinphoneWrapper.Enums
{
    public enum LinphoneIceState
    {
        LinphoneIceStateNotActivated, /**< ICE has not been activated for this call or stream*/
        LinphoneIceStateFailed, /**< ICE processing has failed */
         LinphoneIceStateInProgress, /**< ICE process is in progress */
        LinphoneIceStateHostConnection,
        /**< ICE has established a direct connection to the remote host */
        LinphoneIceStateReflexiveConnection,
        /**< ICE has established a connection to the remote host through one or several NATs */
        LinphoneIceStateRelayConnection
        /**< ICE has established a connection through a relay */
    }
}