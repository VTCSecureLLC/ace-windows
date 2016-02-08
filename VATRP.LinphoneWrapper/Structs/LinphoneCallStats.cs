using System;
using System.Runtime.InteropServices;

namespace VATRP.LinphoneWrapper.Structs
{
    public struct LinphoneCallStats
    {
        public float round_trip_delay; 
        public float sender_interarrival_jitter;
        public float receiver_interarrival_jitter;
        public int ice_state; 
        public int upnp_state;
        public float download_bandwidth; //Download bandwidth measurement of received stream, expressed in kbit/s, including IP/UDP/RTP headers
        public float upload_bandwidth; //Download bandwidth measurement of sent stream, expressed in kbit/s, including IP/UDP/RTP headers
        public float receiver_loss_rate; 
        public float sender_loss_rate;
        public ulong total_late_packets;
        public float rtcp_download_bandwidth; //RTCP download bandwidth measurement of received stream, expressed in kbit/s, including IP/UDP/RTP headers
        public float rtcp_upload_bandwidth; //RTCP download bandwidth measurement of sent stream, expressed in kbit/s, including IP/UDP/RTP headers
        public RtpStats rtp_stats; // RTP stats
    }
}