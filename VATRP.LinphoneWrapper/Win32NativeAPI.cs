using System;
using System.Runtime.InteropServices;

namespace VATRP.LinphoneWrapper
{
    public static class Win32NativeAPI
    {
        [DllImport("winmm.dll", EntryPoint = "PlaySound", SetLastError = true)]
        internal static extern bool PlaySound(byte[] ptrToSound, UIntPtr hmod, SoundFlags fdwSound);
        
        [DllImport("winmm.dll", EntryPoint = "PlaySound", SetLastError = true)]
        internal static extern bool PlaySound(IntPtr ptrToSound, UIntPtr hmod, SoundFlags fdwSound);

        [Flags]
        internal enum SoundFlags
        {
            SND_SYNC = 0x0000, // play synchronously (default)
            SND_ASYNC = 0x0001, // play asynchronously
            SND_NODEFAULT = 0x0002, // silence (!default) if sound not found
            SND_MEMORY = 0x0004, // pszSound points to a memory file
            SND_LOOP = 0x0008, // loop the sound until next sndPlaySound
            SND_NOSTOP = 0x0010, // don't stop any currently playing sound
            SND_NOWAIT = 0x00002000, // don't wait if the driver is busy
            SND_ALIAS = 0x00010000, // name is a registry alias
            SND_ALIAS_ID = 0x00110000, // alias is a predefined id
            SND_FILENAME = 0x00020000, // name is file name
            SND_PURGE = 0x0040 // purge non-static events for task
        }

        [DllImport("user32.dll")]
        public static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hWnd);
    }
}
