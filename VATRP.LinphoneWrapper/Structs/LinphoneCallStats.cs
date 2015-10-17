using System;
using System.Runtime.InteropServices;

namespace VATRP.LinphoneWrapper.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct LinphoneCallStats
    {
        public int type; // Can be either LINPHONE_CALL_STATS_AUDIO or LINPHONE_CALL_STATS_VIDEO
        public JitterStats jitter_stats; //jitter buffer statistics, see oRTP documentation for details 
        public IntPtr received_rtcp; //Last RTCP packet received, as a mblk_t structure. See oRTP documentation for details how to extract information from it
        public IntPtr sent_rtcp;//Last RTCP packet sent, as a mblk_t structure. See oRTP documentation for details how to extract information from it
        public float round_trip_delay; //Round trip propagation time in seconds if known, -1 if unknown.
        public int ice_state; // State of ICE processing. 
        public int upnp_state; // State of uPnP processing. 
        public float download_bandwidth; //Download bandwidth measurement of received stream, expressed in kbit/s, including IP/UDP/RTP headers
        public float upload_bandwidth; //Download bandwidth measurement of sent stream, expressed in kbit/s, including IP/UDP/RTP headers
        public float local_late_rate; //percentage of packet received too late over last second
        public float local_loss_rate; //percentage of lost packet over last second
        public int updated; // Tell which RTCP packet has been updated (received_rtcp or sent_rtcp). Can be either LINPHONE_CALL_STATS_RECEIVED_RTCP_UPDATE or LINPHONE_CALL_STATS_SENT_RTCP_UPDATE 
        public float rtcp_download_bandwidth; //RTCP download bandwidth measurement of received stream, expressed in kbit/s, including IP/UDP/RTP headers
        public float rtcp_upload_bandwidth; //RTCP download bandwidth measurement of sent stream, expressed in kbit/s, including IP/UDP/RTP headers
        public RtpStats rtp_stats; // RTP stats
    }
}