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
        /**
 * Creates a CardDAV context for all related operations
 * @param lfl LinphoneFriendList object
 * @return LinphoneCardDavContext object if vCard support is enabled and server URL is available, NULL otherwise
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_carddav_context_new(IntPtr lfl);


/**
 * Deletes a LinphoneCardDavContext object
 * @param cdc LinphoneCardDavContext object
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_carddav_context_destroy(IntPtr cdc);

/**
 * Starts a synchronization with the remote server to update local friends with server changes
 * @param cdc LinphoneCardDavContext object
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_carddav_synchronize(IntPtr cdc);

        /**
 * Acquire a reference to the friend list.
 * @param[in] list LinphoneFriendList object.
 * @return The same LinphoneFriendList object.
**/
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_list_ref(IntPtr list);

/**
 * Release reference to the friend list.
 * @param[in] list LinphoneFriendList object.
**/
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_friend_list_unref(IntPtr list);

/**
 * Sends a LinphoneFriend to the CardDAV server for update or creation
 * @param cdc LinphoneCardDavContext object
 * @param lf a LinphoneFriend object to update/create on the server
 */
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_carddav_put_vcard(IntPtr cdc, IntPtr lf);

        /**
 * Sets a user pointer to the LinphoneCardDAVContext object
 * @param cdc LinphoneCardDavContext object
 * @param ud The user data pointer
 */
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_carddav_set_user_data(IntPtr cdc, IntPtr ud);

/**
 * Gets the user pointer set in the LinphoneCardDAVContext object
 * @param cdc LinphoneCardDavContext object
 * @return The user data pointer if set, NULL otherwise
 */
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_carddav_get_user_data(IntPtr cdc);

/**
 * Deletes a LinphoneFriend on the CardDAV server 
 * @param cdc LinphoneCardDavContext object
 * @param lf a LinphoneFriend object to delete on the server
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_carddav_delete_vcard(IntPtr cdc, IntPtr lf);

/**
 * Set the synchronization done callback.
 * @param cdc LinphoneCardDavContext object
 * @param cb The synchronization done callback to be used.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_carddav_set_synchronization_done_callback(IntPtr cdc, IntPtr cb);

/**
 * Set the new contact callback.
 * @param cdc LinphoneCardDavContext object
 * @param cb The new contact callback to be used.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_carddav_set_new_contact_callback(IntPtr cdc, IntPtr cb);

/**
 * Set the updated contact callback.
 * @param cdc LinphoneCardDavContext object
 * @param cb The updated contact callback to be used.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_carddav_set_updated_contact_callback(IntPtr cdc, IntPtr cb);

/**
 * Set the removed contact callback.
 * @param cdc LinphoneCardDavContext object
 * @param cb The removed contact callback to be used.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_carddav_set_removed_contact_callback(IntPtr cdc, IntPtr cb);

    }
}
