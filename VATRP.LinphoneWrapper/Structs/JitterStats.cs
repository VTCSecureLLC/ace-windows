using System.Runtime.InteropServices;

namespace VATRP.LinphoneWrapper.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct JitterStats
    {
        public uint jitter; // interarrival jitter at last emitted sender report 
        public uint max_jitter; /* biggest interarrival jitter (value in stream clock unit) */
        public uint sum_jitter; /* sum of all interarrival jitter (value in stream clock unit) */
        public uint max_jitter_ts; /* date (in ms since Epoch) of the biggest interarrival jitter */
        public float jitter_buffer_size_ms; /* mean jitter buffer size in milliseconds.*/
    }
}