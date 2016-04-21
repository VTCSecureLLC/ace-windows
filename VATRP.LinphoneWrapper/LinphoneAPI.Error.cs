using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VATRP.LinphoneWrapper.Enums;

namespace VATRP.LinphoneWrapper
{
    public static partial class LinphoneAPI
    {
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_error_info_get_reason(IntPtr ei);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_error_info_get_phrase(IntPtr ei);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_error_info_get_details(IntPtr ei);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_error_info_get_protocol_code(IntPtr ei);
    }
}
