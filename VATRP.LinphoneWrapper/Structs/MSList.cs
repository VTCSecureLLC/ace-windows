using System;
using System.Runtime.InteropServices;

namespace VATRP.LinphoneWrapper.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct MSList
    {
        public IntPtr next;
        public IntPtr prev;
        public IntPtr data;
    }
}