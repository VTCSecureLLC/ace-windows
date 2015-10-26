using System;
using System.Runtime.InteropServices;

namespace VATRP.LinphoneWrapper.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct RtpStats
    {
        UInt64 packet_sent;		/*number of outgoing packets */
        UInt64 packet_dup_sent;	/*number of outgoing duplicate packets */
        UInt64 sent;				/* outgoing total bytes (excluding IP header) */
        UInt64 packet_recv;		/* number of incoming packets */
        UInt64 packet_dup_recv;	/* number of incoming duplicate packets */
        UInt64 recv;				/* incoming bytes of payload and delivered in time to the application */
        UInt64 hw_recv;			/* incoming bytes of payload */
        UInt64 outoftime;			/* number of incoming packets that were received too late */
        Int64 cum_packet_loss;	/* cumulative number of incoming packet lost */
        UInt64 bad;				/* incoming packets that did not appear to be RTP */
        UInt64 discarded;			/* incoming packets discarded because the queue exceeds its max size */
        UInt64 sent_rtcp_packets;	/* outgoing RTCP packets counter (only packets that embed a report block are considered) */ 
    }
}