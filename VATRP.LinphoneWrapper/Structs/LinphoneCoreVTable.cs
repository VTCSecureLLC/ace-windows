using System;
using System.Runtime.InteropServices;

namespace VATRP.LinphoneWrapper.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct LinphoneCoreVTable
    {
        public IntPtr global_state_changed; //<Notifies global state changes
        public IntPtr registration_state_changed; // Notifies registration state changes
        public IntPtr call_state_changed; // Notifies call state changes
        public IntPtr notify_presence_received; // Notify received presence events
        public IntPtr new_subscription_requested; // Notify about pending presence subscription request
        public IntPtr auth_info_requested; // Ask the application some authentication information
        public IntPtr call_log_updated; // Notifies that call log list has been updated
        public IntPtr message_received; // A message is received, can be text or external body
        public IntPtr is_composing_received; // An is-composing notification has been received
        public IntPtr dtmf_received; // A dtmf has been received received
        public IntPtr refer_received; // An out of call refer was received
        public IntPtr call_encryption_changed; // Notifies on change in the encryption of call streams
        public IntPtr transfer_state_changed; // Notifies when a transfer is in progress
        public IntPtr buddy_info_updated; // A LinphoneFriend's BuddyInfo has changed
        public IntPtr call_stats_updated; // Notifies on refreshing of call's statistics.
        public IntPtr info_received; // Notifies an incoming informational message received.
        public IntPtr subscription_state_changed; // Notifies subscription state change
        public IntPtr notify_received; // Notifies a an event notification, see linphone_core_subscribe()
        public IntPtr publish_state_changed; // Notifies publish state change (only from #LinphoneEvent api)
        public IntPtr configuring_status; // Notifies configuring status changes
        public IntPtr display_status; // @deprecated Callback that notifies various events with human readable text.
        public IntPtr display_message; // @deprecated Callback to display a message to the user
        public IntPtr display_warning; // @deprecated Callback to display a warning to the user
        public IntPtr display_url; // @deprecated
        public IntPtr show; // @deprecated Notifies the application that it should show up
        public IntPtr text_received; // @deprecated, use #message_received instead <br> A text message has been received
        public IntPtr file_transfer_recv; // @deprecated Callback to store file received attached to a #LinphoneChatMessage 
        public IntPtr file_transfer_send; // @deprecated Callback to collect file chunk to be sent for a #LinphoneChatMessage 
        public IntPtr file_transfer_progress_indication; // @deprecated Callback to indicate file transfer progress 
        public IntPtr network_reachable; // Callback to report IP network status (I.E up/down )
        public IntPtr log_collection_upload_state_changed; // Callback to upload collected logs 
        public IntPtr log_collection_upload_progress_indication; // Callback to indicate log collection upload progress 
        public IntPtr user_data; //User data associated with the above callbacks 

    };
}