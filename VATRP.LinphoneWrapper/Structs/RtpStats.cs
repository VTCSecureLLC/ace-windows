using System;
using System.Runtime.InteropServices;

namespace VATRP.LinphoneWrapper.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct RtpStats
    {
        public UInt64 packet_sent;		/*number of outgoing packets */
        public UInt64 packet_dup_sent;	/*number of outgoing duplicate packets */
        public UInt64 sent;				/* outgoing total bytes (excluding IP header) */
        public UInt64 packet_recv;		/* number of incoming packets */
        public UInt64 packet_dup_recv;	/* number of incoming duplicate packets */
        public UInt64 recv;				/* incoming bytes of payload and delivered in time to the application */
        public UInt64 hw_recv;			/* incoming bytes of payload */
        public UInt64 outoftime;			/* number of incoming packets that were received too late */
        public Int64 cum_packet_loss;	/* cumulative number of incoming packet lost */
        public UInt64 bad;				/* incoming packets that did not appear to be RTP */
        public UInt64 discarded;			/* incoming packets discarded because the queue exceeds its max size */
        public UInt64 sent_rtcp_packets;	/* outgoing RTCP packets counter (only packets that embed a report block are considered) */ 
    }
}