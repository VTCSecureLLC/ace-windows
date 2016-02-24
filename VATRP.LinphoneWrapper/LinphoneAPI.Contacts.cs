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
 * Returns the vCard object associated to this friend, if any
 * @param[in] fr LinphoneFriend object
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_get_vcard(IntPtr fr);

        /**
         * Binds a vCard object to a friend
         * @param[in] fr LinphoneFriend object
         * @param[in] vcard The vCard object to bind
         */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_friend_set_vcard(IntPtr fr, IntPtr vcard);

        /**
         * Creates a vCard object associated to this friend if there isn't one yet and if the full name is available, either by the parameter or the one in the friend's SIP URI
         * @param[in] fr LinphoneFriend object
         * @param[in] name The full name of the friend or NULL to use the one from the friend's SIP URI
         * @return true if the vCard has been created, false if it wasn't possible (for exemple if name and the friend's SIP URI are null or if the friend's SIP URI doesn't have a display name), or if there is already one vcard
         */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_friend_create_vcard(IntPtr fr, string name);

        /**
         * Contructor same as linphone_friend_new() + linphone_friend_set_address()
         * @param vcard a vCard object
         * @return a new #LinphoneFriend with \link linphone_friend_get_vcard() vCard initialized \endlink
         */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_new_from_vcard(IntPtr vcard);

        /**
         * Creates and adds LinphoneFriend objects to LinphoneCore from a file that contains the vCard(s) to parse
         * @param[in] lc the LinphoneCore object
         * @param[in] vcard_file the path to a file that contains the vCard(s) to parse
         * @return the amount of linphone friends created
         */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_friend_list_import_friends_from_vcard4_file(IntPtr friendsList, string vcard_file);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_friend_list_export_friends_as_vcard4_file(IntPtr friendsList, string vcard_file);

        /**
         * Sets the database filename where friends will be stored.
         * If the file does not exist, it will be created.
         * @ingroup initializing
         * @param lc the linphone core
         * @param path filesystem path
        **/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_friends_database_path(IntPtr lc, string path);

        /**
         * Migrates the friends from the linphonerc to the database if not done yet
         * @ingroup initializing
         * @param lc the linphone core
        **/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_migrate_friends_from_rc_to_db(IntPtr lc);

        /**
 * Set the display name for this friend
 * @param lf #LinphoneFriend object
 * @param name 
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_friend_set_name(IntPtr lf, string name);

        /**
 * Create a default LinphoneFriend.
 * @param[in] lc #LinphoneCore object
 * @return The created #LinphoneFriend object
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_create_friend(IntPtr lc);

        /**
         * Create a LinphoneFriend from the given address.
         * @param[in] lc #LinphoneCore object
         * @param[in] address A string containing the address to create the LinphoneFriend from
         * @return The created #LinphoneFriend object
         */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_create_friend_with_address(IntPtr lc, string address);

        /**
 * Add a friend to the current buddy list, if \link linphone_friend_enable_subscribes() subscription attribute \endlink is set, a SIP SUBSCRIBE message is sent.
 * @param lc #LinphoneCore object
 * @param fr #LinphoneFriend to add
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_add_friend(IntPtr lc, IntPtr fr);

        /**
         * remove a friend from the buddy list
         * @param lc #LinphoneCore object
         * @param fr #LinphoneFriend to add
         */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_remove_friend(IntPtr lc, IntPtr fr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_default_friend_list(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_friend_list(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_get_address(IntPtr lf);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_set_address(IntPtr fr, IntPtr address);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_new_with_address(string addr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_friend_enable_subscribes(IntPtr fr, bool val);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_friend_set_inc_subscribe_policy(IntPtr fr, int val);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_friend_edit(IntPtr fr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_friend_done(IntPtr fr);

    }
}
