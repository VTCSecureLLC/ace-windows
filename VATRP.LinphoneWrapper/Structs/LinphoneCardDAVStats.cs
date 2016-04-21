using System.Runtime.InteropServices;

namespace VATRP.LinphoneWrapper.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct LinphoneCardDAVStats
    {
        public int sync_done_count;
        public int new_contact_count;
        public int removed_contact_count;
        public int updated_contact_count;
    }
}