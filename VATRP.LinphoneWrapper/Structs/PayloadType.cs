using System;
using System.Runtime.InteropServices;

namespace VATRP.LinphoneWrapper.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct PayloadType
    {
        public int type; //  one of PAYLOAD_* macros
        public int clock_rate; //  rtp clock rate
        public char bits_per_sample; //  in case of continuous audio data 
        public string zero_pattern;
        public int pattern_length;
        //  other useful information for the application
        public int normal_bitrate; // in bit/s 
        public string mime_type; // actually the submime, ex: pcm, pcma, gsm
        public int channels; //  number of channels of audio 
        public string recv_fmtp; //  various format parameters for the incoming stream 
        public string send_fmtp; //  various format parameters for the outgoing stream 
        public PayloadTypeAvpfParams avpf ; //  AVPF parameters 
        public int flags;
        public IntPtr user_data;
    }
}