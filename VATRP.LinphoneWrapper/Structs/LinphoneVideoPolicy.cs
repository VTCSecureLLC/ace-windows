using System.Runtime.InteropServices;

namespace VATRP.LinphoneWrapper.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct LinphoneVideoPolicy
    {
        public bool automatically_initiate; /**<Whether video shall be automatically proposed for outgoing calls.*/
        public bool automatically_accept; /**<Whether video shall be automatically accepted for incoming calls*/
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public bool[] unused;
    }
}