using System.Runtime.InteropServices;

namespace VATRP.LinphoneWrapper.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct PayloadTypeAvpfParams
    {
        public char features; // A bitmask of PAYLOAD_TYPE_AVPF_* macros.
        public short trr_interval; // The interval in milliseconds between regular RTCP packets. 
    }
}