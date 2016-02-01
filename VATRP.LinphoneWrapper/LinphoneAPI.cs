﻿using System;
using System.Runtime.InteropServices;
using VATRP.LinphoneWrapper.Enums;
using VATRP.LinphoneWrapper.Structs;

namespace VATRP.LinphoneWrapper
{
    public static class LinphoneAPI
    {
        public const string DllName = "linphone.dll";

        #region Constants

        public const int LC_SIP_TRANSPORT_RANDOM = -1; // Randomly chose a sip port for this transport
        public const int LC_SIP_TRANSPORT_DISABLED = 0; // Disable a sip transport

        public const int LINPHONE_FIND_PAYLOAD_IGNORE_RATE = -1;
        // Wildcard value used by #linphone_core_find_payload_type to ignore rate in search algorithm

        public const int LINPHONE_FIND_PAYLOAD_IGNORE_CHANNELS = -1;
        // Wildcard value used by #linphone_core_find_payload_type to ignore channel in search algorithm

        public static int LINPHONE_CALL_STATS_AUDIO = 0;
        public static int LINPHONE_CALL_STATS_VIDEO = 1;
        public static int LINPHONE_CALL_STATS_TEXT = 2;

        #endregion

        #region Methods

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_version();

        public static string linphone_core_get_version_asString()
        {
            IntPtr ptr = linphone_core_get_version();
            // assume returned string is utf-8 encoded
            return PtrToStringUtf8(ptr);
        }

        public static string PtrToStringUtf8(IntPtr ptr) // aPtr is nul-terminated
        {
            if (ptr == IntPtr.Zero)
                return "";
            int len = 0;
            while (System.Runtime.InteropServices.Marshal.ReadByte(ptr, len) != 0)
                len++;
            if (len == 0)
                return "";
            byte[] array = new byte[len];
            System.Runtime.InteropServices.Marshal.Copy(ptr, array, 0, len);
            return System.Text.Encoding.UTF8.GetString(array);
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_logs(IntPtr file);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_disable_logs();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_log_level_mask(OrtpLogLevel loglevel);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_new(IntPtr vtable, string config_path, string factory_config,
            IntPtr userdata);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_destroy(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_create_proxy_config(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_clear_proxy_config(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_default_proxy_config(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_proxy_config_normalize_sip_uri(IntPtr proxy, string username);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_set_primary_contact(IntPtr proxy, string contact_params);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_auth_info_new(string username, string userid, string passwd,
            string ha1, string realm, string domain);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_remove_auth_info(IntPtr lc, IntPtr auth_info);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_add_auth_info(IntPtr lc, IntPtr info);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_clear_all_auth_info(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_proxy_config_set_identity(IntPtr obj, string identity);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_proxy_config_set_server_addr(IntPtr cfg, string server_addr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_proxy_config_enable_register(IntPtr obj, bool val);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_proxy_config_set_avpf_mode(IntPtr cfg, LinphoneAVPFMode mode);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_proxy_config_set_avpf_rr_interval(IntPtr cfg, byte interval);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_address_destroy(IntPtr u);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_add_proxy_config(IntPtr lc, IntPtr cfg);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_default_proxy_config(IntPtr lc, IntPtr config);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_iterate(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_keep_alive(IntPtr lc, bool enable);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_keep_alive_enabled(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_create_default_call_parameters(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_params_add_custom_header(IntPtr cp, string header_name, string header_value);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_params_enable_video(IntPtr cp, bool enabled);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_params_set_audio_bandwidth_limit(IntPtr cp, int kbit);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_call_params_video_enabled(IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_params_set_video_direction(IntPtr cp, LinphoneMediaDirection dir);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_in_call(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_current_call(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_params_enable_early_media_sending(IntPtr cp, bool enabled);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_invite(IntPtr lc, string url);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_invite_address(IntPtr lc, IntPtr addr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_invite_with_params(IntPtr lc, string url, IntPtr param);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_params_destroy(IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_update_call(IntPtr lc, IntPtr call, IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_accept_call_update(IntPtr lc, IntPtr call, IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_create_call_params(IntPtr lc, IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_terminate_call(IntPtr lc, IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_terminate_all_calls(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_default_proxy(IntPtr lc, ref IntPtr config);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_proxy_config_is_registered(IntPtr config);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_proxy_config_edit(IntPtr config);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_proxy_config_done(IntPtr config);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_call_get_current_quality(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_call_get_average_quality(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_remote_address_as_string(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_set_media_encryption(IntPtr lc, LinphoneMediaEncryption menc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern LinphoneMediaEncryption linphone_core_get_media_encryption(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_is_media_encryption_mandatory(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_accept_call_with_params(IntPtr lc, IntPtr call, IntPtr callparams);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_decline_call(IntPtr lc, IntPtr call, LinphoneReason reason);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_redirect_call(IntPtr lc, IntPtr call, string redirect_uri);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_pause_call(IntPtr lc, IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]

        public static extern int linphone_core_pause_all_calls(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_resume_call(IntPtr lc, IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_defer_call_update(IntPtr lc, IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_start_recording(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_stop_recording(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_params_set_record_file(IntPtr callparams, string filename);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_params_get_record_file(IntPtr callparams);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_set_sip_transports(IntPtr lc, IntPtr tr_config);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_user_agent(IntPtr lc, string ua_name, string version);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_play_file(IntPtr lc, string file);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_record_file(IntPtr lc, string file);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_play_dtmf(IntPtr lc, char dtmf, int duration_ms);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_stop_dtmf(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_use_files(IntPtr lc, bool yesno);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_get_state(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_ref(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_unref(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]

        public static extern IntPtr linphone_call_get_chat_room(IntPtr call);

        /* sound functions */
/* returns a null terminated static array of string describing the sound devices */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_sound_devices(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_reload_sound_devices(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_sound_device_can_capture(IntPtr lc, string device);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_sound_device_can_playback(IntPtr lc, string device);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_ring_level(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_play_level(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_rec_level(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_ring_level(IntPtr lc, int level);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_play_level(IntPtr lc, int level);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]

        public static extern void linphone_core_set_mic_gain_db(IntPtr lc, float level);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_core_get_mic_gain_db(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_playback_gain_db(IntPtr lc, float level);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_core_get_playback_gain_db(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]

        public static extern void linphone_core_set_rec_level(IntPtr lc, int level);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_ringer_device(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_playback_device(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_capture_device(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_set_ringer_device(IntPtr lc, string devid);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_set_playback_device(IntPtr lc, string devid);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_set_capture_device(IntPtr lc, string devid);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_stop_ringing(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_ring(IntPtr lc, string path);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_ring(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_verify_server_certificates(IntPtr lc, bool yesno);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_verify_server_cn(IntPtr lc, bool yesno);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_root_ca(IntPtr lc, string path);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_root_ca(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_ringback(IntPtr lc, string path);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_ringback(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_download_bandwidth(IntPtr lc, int bw);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_upload_bandwidth(IntPtr lc, int bw);

/**
 * Specify a ring back tone to be played to far end during incoming calls.
 * @param[in] lc #LinphoneCore object
 * @param[in] ring The path to the ring back tone to be played.
 * @ingroup media_parameters
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_remote_ringback_tone(IntPtr lc, string ring);

/**
 * Get the ring back tone played to far end during incoming calls.
 * @param[in] lc #LinphoneCore object
 * @ingroup media_parameters
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_remote_ringback_tone(IntPtr lc);

/**
 * Enable or disable the ring play during an incoming early media call.
 * @param[in] lc #LinphoneCore object
 * @param[in] enable A boolean value telling whether to enable ringing during an incoming early media call.
 * @ingroup media_parameters
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_ring_during_incoming_early_media(IntPtr lc, bool enable);

/**
 * Tells whether the ring play is enabled during an incoming early media call.
 * @param[in] lc #LinphoneCore object
 * @ingroup media_paramaters
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_get_ring_during_incoming_early_media(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_play_local(IntPtr lc, string audiofile);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_echo_cancellation(IntPtr lc, bool val);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_echo_cancellation_enabled(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]

        public static extern void linphone_core_enable_echo_limiter(IntPtr lc, bool val);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_echo_limiter_enabled(IntPtr lc);

/**
 * Enable or disable the microphone.
 * @param[in] lc #LinphoneCore object
 * @param[in] enable TRUE to enable the microphone, FALSE to disable it.
 * @ingroup media_parameters
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_mic(IntPtr lc, bool enable);

/**
 * Tells whether the microphone is enabled.
 * @param[in] lc #LinphoneCore object
 * @return TRUE if the microphone is enabled, FALSE if disabled.
 * @ingroup media_parameters
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_mic_enabled(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_is_rtp_muted(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_get_rtp_no_xmit_on_audio_mute(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_rtp_no_xmit_on_audio_mute(IntPtr lc, bool val);

        /* video support */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_video_supported(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_video_preset(IntPtr lc, string preset);

/**
 * Enable or disable video capture.
 *
 * This function does not have any effect during calls. It just indicates the #LinphoneCore to
 * initiate future calls with video capture or not.
 * @param[in] lc #LinphoneCore object.
 * @param[in] enable TRUE to enable video capture, FALSE to disable it.
 * @ingroup media_parameters
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_video_capture(IntPtr lc, bool enable);

/**
 * Enable or disable video display.
 *
 * This function does not have any effect during calls. It just indicates the #LinphoneCore to
 * initiate future calls with video display or not.
 * @param[in] lc #LinphoneCore object.
 * @param[in] enable TRUE to enable video display, FALSE to disable it.
 * @ingroup media_parameters
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_video_display(IntPtr lc, bool enable);


/**
 * Enable or disable video source reuse when switching from preview to actual video call.
 *
 * This source reuse is useful when you always display the preview, even before calls are initiated.
 * By keeping the video source for the transition to a real video call, you will smooth out the
 * source close/reopen cycle.
 *
 * This function does not have any effect durfing calls. It just indicates the #LinphoneCore to
 * initiate future calls with video source reuse or not.
 * Also, at the end of a video call, the source will be closed whatsoever for now.
 * @param[in] lc #LinphoneCore object
 * @param[in] enable TRUE to enable video source reuse. FALSE to disable it for subsequent calls.
 * @ingroup media_parameters
 *
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_video_source_reuse(IntPtr lc, bool enable);

/**
 * Tells whether video capture is enabled.
 * @param[in] lc #LinphoneCore object.
 * @return TRUE if video capture is enabled, FALSE if disabled.
 * @ingroup media_parameters
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_video_capture_enabled(IntPtr lc);

/**
 * Tells whether video display is enabled.
 * @param[in] lc #LinphoneCore object.
 * @return TRUE if video display is enabled, FALSE if disabled.
 * @ingroup media_parameters
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_video_display_enabled(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_video_policy(IntPtr lc, IntPtr policy);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_video_policy(IntPtr lc);

/**
 * Returns the zero terminated table of supported video resolutions.
 *
 * @ingroup media_parameters
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_supported_video_sizes(IntPtr lc);

/**
 * Sets the preferred video size.
 *
 * @ingroup media_parameters
 * This applies only to the stream that is captured and sent to the remote party,
 * since we accept all standard video size on the receive path.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_preferred_video_size(IntPtr lc, IntPtr vsize);

/**
 * Sets the video size for the captured (preview) video.
 * This method is for advanced usage where a video capture must be set independently of the size of the stream actually sent through the call.
 * This allows for example to have the preview window with HD resolution even if due to bandwidth constraint the sent video size is small.
 * Using this feature increases the CPU consumption, since a rescaling will be done internally.
 * @ingroup media_parameters
 * @param lc the linphone core
 * @param vsize the video resolution choosed for capuring and previewing. It can be (0,0) to not request any specific preview size and let the core optimize the processing.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_preview_video_size(IntPtr lc, IntPtr vsize);

/**
 * Sets the preview video size by its name. See linphone_core_set_preview_video_size() for more information about this feature.
 *
 * @ingroup media_parameters
 * Video resolution names are: qcif, svga, cif, vga, 4cif, svga ...
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_preview_video_size_by_name(IntPtr lc, string name);

/**
 * Returns video size for the captured video if it was previously set by linphone_core_set_preview_video_size(), otherwise returns a 0,0 size.
 * @see linphone_core_set_preview_video_size()
 * @ingroup media_parameters
 * @param lc the core
 * @return a MSVideoSize
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern MSVideoSize linphone_core_get_preview_video_size(IntPtr lc);

/**
 * Returns the effective video size for the captured video as provided by the camera.
 * When preview is disabled or not yet started, this function returns a zeroed video size.
 * @see linphone_core_set_preview_video_size()
 * @ingroup media_parameters
 * @param lc the core
 * @return a MSVideoSize
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern MSVideoSize linphone_core_get_current_preview_video_size(IntPtr lc);

/**
 * Returns the current preferred video size for sending.
 *
 * @ingroup media_parameters
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern MSVideoSize linphone_core_get_preferred_video_size(IntPtr lc);

/**
 * Get the name of the current preferred video size for sending.
 * @param[in] lc #LinphoneCore object.
 * @return A string containing the name of the current preferred video size (to be freed with ms_free()).
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_preferred_video_size_name(IntPtr lc);

/**
 * Sets the preferred video size by its name.
 *
 * @ingroup media_parameters
 * This is identical to linphone_core_set_preferred_video_size() except
 * that it takes the name of the video resolution as input.
 * Video resolution names are: qcif, svga, cif, vga, 4cif, svga ...
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_preferred_video_size_by_name(IntPtr lc, string name);

/**
 * Set the preferred frame rate for video.
 * Based on the available bandwidth constraints and network conditions, the video encoder
 * remains free to lower the framerate. There is no warranty that the preferred frame rate be the actual framerate.
 * used during a call. Default value is 0, which means "use encoder's default fps value".
 * @ingroup media_parameters
 * @param lc the LinphoneCore
 * @param fps the target frame rate in number of frames per seconds.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_preferred_framerate(IntPtr lc, float fps);

/**
 * Returns the preferred video framerate, previously set by linphone_core_set_preferred_framerate().
 * @ingroup media_parameters
 * @param lc the linphone core
 * @return frame rate in number of frames per seconds.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_core_get_preferred_framerate(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_video_preview(IntPtr lc, bool val);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_video_preview_enabled(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]

        public static extern void linphone_core_enable_self_view(IntPtr lc, bool val);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_self_view_enabled(IntPtr lc);


/**
 * Update detection of camera devices.
 *
 * Use this function when the application is notified of USB plug events, so that
 * list of available hardwares for video capture is updated.
 * @param[in] lc #LinphoneCore object.
 * @ingroup media_parameters
 **/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_reload_video_devices(IntPtr lc);


        /* returns a null terminated static array of string describing the webcams */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_video_devices(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_set_video_device(IntPtr lc, string id);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_video_device(IntPtr lc);

/* Set and get static picture to be used when "Static picture" is the video device */
/**
 * Set the path to the image file to stream when "Static picture" is set as the video device.
 * @param[in] lc #LinphoneCore object.
 * @param[in] path The path to the image file to use.
 * @ingroup media_parameters
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_set_static_picture(IntPtr lc, string path);

/**
 * Get the path to the image file streamed when "Static picture" is set as the video device.
 * @param[in] lc #LinphoneCore object.
 * @return The path to the image file streamed when "Static picture" is set as the video device.
 * @ingroup media_parameters
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_static_picture(IntPtr lc);

/**
 * Set the frame rate for static picture.
 * @param[in] lc #LinphoneCore object.
 * @param[in] fps The new frame rate to use for static picture.
 * @ingroup media_parameters
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_set_static_picture_fps(IntPtr lc, float fps);

/**
 * Get the frame rate for static picture
 * @param[in] lc #LinphoneCore object.
 * @return The frame rate used for static picture.
 * @ingroup media_parameters
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_core_get_static_picture_fps(IntPtr lc);

/*function to be used for eventually setting window decorations (icons, title...)*/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_native_video_window_id(IntPtr lc);

/**
 * @ingroup media_parameters
 * Set the native video window id where the video is to be displayed.
 * For MacOS, Linux, Windows: if not set or LINPHONE_VIDEO_DISPLAY_AUTO the core will create its own window, unless the special id LINPHONE_VIDEO_DISPLAY_NONE is given.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_native_video_window_id(IntPtr lc, long id);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_native_preview_window_id(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_native_preview_window_id(IntPtr lc, long id);

/**
 * Tells the core to use a separate window for local camera preview video, instead of
 * inserting local view within the remote video window.
 * @param[in] lc #LinphoneCore object.
 * @param[in] yesno TRUE to use a separate window, FALSE to insert the preview in the remote video window.
 * @ingroup media_parameters
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_use_preview_window(IntPtr lc, bool yesno);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_device_rotation(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_device_rotation(IntPtr lc, int rotation);

/**
 * Get the camera sensor rotation.
 *
 * This is needed on some mobile platforms to get the number of degrees the camera sensor
 * is rotated relative to the screen.
 *
 * @param lc The linphone core related to the operation
 * @return The camera sensor rotation in degrees (0 to 360) or -1 if it could not be retrieved
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_camera_sensor_rotation(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]

        public static extern bool linphone_call_asked_to_autoanswer(IntPtr call);

/**
 * Get the remote address of the current call.
 * @param[in] lc LinphoneCore object.
 * @return The remote address of the current call or NULL if there is no current call.
 * @ingroup call_control
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_current_call_remote_address(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_remote_address(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_get_dir(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_call_log(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_refer_to(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_call_has_transfer_pending(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_transferer_call(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_transfer_target_call(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_replaced_call(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_get_duration(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_current_params(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_params_copy(IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_remote_params(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_enable_camera(IntPtr lc, bool enabled);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_call_camera_enabled(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_take_video_snapshot(IntPtr call, string file);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_take_preview_snapshot(IntPtr call, string file);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_get_reason(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_video_jittcomp(IntPtr lc, int milliseconds);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_audio_port(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_get_audio_port_range(IntPtr lc, ref int min_port, ref int max_port);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_video_port(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_get_video_port_range(IntPtr lc, ref int min_port, ref int max_port);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_nortp_timeout(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_audio_port(IntPtr lc, int port);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_audio_port_range(IntPtr lc, int min_port, int max_port);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_video_port(IntPtr lc, int port);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_video_port_range(IntPtr lc, int min_port, int max_port);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_nortp_timeout(IntPtr lc, int port);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_use_info_for_dtmf(IntPtr lc, bool use_info);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_get_use_info_for_dtmf(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_use_rfc2833_for_dtmf(IntPtr lc, bool use_rfc2833);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_get_use_rfc2833_for_dtmf(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_sip_port(IntPtr lc, int port);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_sip_port(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_sip_transports(IntPtr lc, IntPtr transports);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_get_sip_transports_used(IntPtr lc, IntPtr tr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_sip_transport_supported(IntPtr lc, LinphoneTransportType tp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_inc_timeout(IntPtr lc, int seconds);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_inc_timeout(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_in_call_timeout(IntPtr lc, int seconds);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_in_call_timeout(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_delayed_timeout(IntPtr lc, int seconds);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_delayed_timeout(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_stun_server(IntPtr lc, string server);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_stun_server(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_firewall_policy(IntPtr lc, LinphoneFirewallPolicy pol);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_firewall_policy(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_avpf_mode(IntPtr lc, LinphoneAVPFMode mode);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_avpf_mode(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_avpf_rr_interval(IntPtr lc, int interval);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_avpf_rr_interval(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_adaptive_rate_control(IntPtr lc, bool enabled);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_adaptive_rate_control_enabled(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_adaptive_rate_algorithm(IntPtr lc, string algorithm);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_adaptive_rate_algorithm(IntPtr lc);

        /**
 * Returns the list of available audio codecs.
 * @param[in] lc The LinphoneCore object
 * @return \mslist{PayloadType}
 *
 * This list is unmodifiable. The ->data field of the MSList points a PayloadType
 * structure holding the codec information.
 * It is possible to make copy of the list with ms_list_copy() in order to modify it
 * (such as the order of codecs).

**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_audio_codecs(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_set_audio_codecs(IntPtr lc, IntPtr codecs);

/**
 * Returns the list of available video codecs.
 * @param[in] lc The LinphoneCore object
 * @return \mslist{PayloadType}
 *
 * This list is unmodifiable. The ->data field of the MSList points a PayloadType
 * structure holding the codec information.
 * It is possible to make copy of the list with ms_list_copy() in order to modify it
 * (such as the order of codecs).

**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_video_codecs(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_set_video_codecs(IntPtr lc, IntPtr codecs);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_generic_confort_noise(IntPtr lc, bool enabled);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_generic_confort_noise_enabled(IntPtr lc);

/**
 * Tells whether the specified payload type is enabled.
 * @param[in] lc #LinphoneCore object.
 * @param[in] pt The #LinphonePayloadType we want to know is enabled or not.
 * @return TRUE if the payload type is enabled, FALSE if disabled.

 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_payload_type_enabled(IntPtr lc, IntPtr pt);

/**
 * Tells whether the specified payload type represents a variable bitrate codec.
 * @param[in] lc #LinphoneCore object.
 * @param[in] pt The #LinphonePayloadType we want to know
 * @return TRUE if the payload type represents a VBR codec, FALSE if disabled.

 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_payload_type_is_vbr(IntPtr lc, IntPtr pt);

/**
 * Set an explicit bitrate (IP bitrate, not codec bitrate) for a given codec, in kbit/s.
 * @param[in] lc the #LinphoneCore object
 * @param[in] pt the #LinphonePayloadType to modify.
 * @param[in] bitrate the IP bitrate in kbit/s.

**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_payload_type_bitrate(IntPtr lc, IntPtr pt, int bitrate);

/**
 * Get the bitrate explicitely set with linphone_core_set_payload_type_bitrate().
 * @param[in] lc the #LinphoneCore object
 * @param[in] pt the #LinphonePayloadType to modify.
 * @return bitrate the IP bitrate in kbit/s, or -1 if an error occurred.

**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_payload_type_bitrate(IntPtr lc, IntPtr pt);

/**
 * Enable or disable the use of the specified payload type.
 * @param[in] lc #LinphoneCore object.
 * @param[in] pt The #LinphonePayloadType to enable or disable. It can be retrieved using #linphone_core_find_payload_type
 * @param[in] enable TRUE to enable the payload type, FALSE to disable it.
 * @return 0 if successful, any other value otherwise.

 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_enable_payload_type(IntPtr lc, IntPtr pt, bool enable);


/**
 * Get payload type from mime type and clock rate

 * This function searches in audio and video codecs for the given payload type name and clockrate.
 * @param lc #LinphoneCore object
 * @param type payload mime type (I.E SPEEX, PCMU, VP8)
 * @param rate can be #LINPHONE_FIND_PAYLOAD_IGNORE_RATE
 * @param channels  number of channels, can be #LINPHONE_FIND_PAYLOAD_IGNORE_CHANNELS
 * @return Returns NULL if not found.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_find_payload_type(IntPtr lc, string type, int rate, int channels);

/**

 * Returns the payload type number assigned for this codec.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_payload_type_number(IntPtr lc, IntPtr pt);

/**
 * Force a number for a payload type. The LinphoneCore does payload type number assignment automatically. THis function is to be used mainly for tests, in order
 * to override the automatic assignment mechanism.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_payload_type_number(IntPtr lc, IntPtr pt, int number);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_payload_type_description(IntPtr lc, IntPtr pt);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_check_payload_type_usability(IntPtr lc, IntPtr pt);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_send_dtmf(IntPtr lc, char dtmf);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_send_dtmfs(IntPtr call, string dtmfs);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_cancel_dtmfs(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_create_address(IntPtr lc, string address);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_proxy_config_set_route(IntPtr proxy, string route);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_proxy_config_set_expires(IntPtr proxy, int expires);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_address_set_transport(IntPtr u, int transport);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_address_set_port(IntPtr u, int port);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_address_as_string_uri_only(IntPtr u);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_address_as_string(IntPtr u);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_refresh_registers(IntPtr lc);

        #region Call Info

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_params_get_media_encryption(IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_params_get_privacy(IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_call_params_get_received_framerate(IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern MSVideoSizeDef linphone_call_params_get_received_video_size(IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_params_get_rtp_profile(IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_call_params_get_sent_framerate(IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern MSVideoSizeDef linphone_call_params_get_sent_video_size(IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_params_get_used_audio_codec(IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_params_get_used_video_codec(IntPtr cp);

        #endregion

        #region Call Statistics

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_audio_stats(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_video_stats(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_call_stats_get_sender_loss_rate(IntPtr stats);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_call_stats_get_receiver_loss_rate(IntPtr stats);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_call_stats_get_sender_interarrival_jitter(IntPtr stats, IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_call_stats_get_receiver_interarrival_jitter(IntPtr stats, IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_stats_get_rtp_stats(IntPtr stats);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt64 linphone_call_stats_get_late_packets_cumulative_number(IntPtr stats, IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_call_stats_get_download_bandwidth(IntPtr stats);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_call_stats_get_upload_bandwidth(IntPtr stats);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_stats_get_ice_state(IntPtr stats);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_stats_get_upnp_state(IntPtr stats);

        #endregion

        #region RTT

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_params_enable_realtime_text(IntPtr cp, bool yesno);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_call_params_realtime_text_enabled(IntPtr cp);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_chat_database_path(IntPtr lc, string path);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_chat_room(IntPtr lc, IntPtr addr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_chat_room_from_uri(IntPtr lc, string to);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_delete_chat_room(IntPtr lc, IntPtr cr);

/**
 * Unconditionally disable incoming chat messages.
 * @param lc the core
 * @param deny_reason the deny reason (#LinphoneReasonNone has no effect).
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_disable_chat(IntPtr lc, LinphoneReason deny_reason);

/**
 * Enable reception of incoming chat messages.
 * By default it is enabled but it can be disabled with linphone_core_disable_chat().
 * @param lc the core
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_chat(IntPtr lc);

/**
 * Returns whether chat is enabled.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_chat_enabled(IntPtr lc);

/**
 * Create a message attached to a dedicated chat room;
 * @param cr the chat room.
 * @param message text message, NULL if absent.
 * @return a new #LinphoneChatMessage
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_room_create_message(IntPtr cr, string message);

/**
 * Create a message attached to a dedicated chat room;
 * @param cr the chat room.
 * @param message text message, NULL if absent.
 * @param external_body_url the URL given in external body or NULL.
 * @param state the LinphoneChatMessage.State of the message.
 * @param time the time_t at which the message has been received/sent.
 * @param is_read TRUE if the message should be flagged as read, FALSE otherwise.
 * @param is_incoming TRUE if the message has been received, FALSE otherwise.
 * @return a new #LinphoneChatMessage
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_room_create_message_2(IntPtr cr, string message,
            string external_body_url, LinphoneChatMessageState state, uint time, bool is_read, bool is_incoming);

/**
 * Acquire a reference to the chat room.
 * @param[in] cr The chat room.
 * @return The same chat room.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_room_ref(IntPtr cr);

/**
 * Release reference to the chat room.
 * @param[in] cr The chat room.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_room_unref(IntPtr cr);

/**
 * Retrieve the user pointer associated with the chat room.
 * @param[in] cr The chat room.
 * @return The user pointer associated with the chat room.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_room_get_user_data(IntPtr cr);

/**
 * Assign a user pointer to the chat room.
 * @param[in] cr The chat room.
 * @param[in] ud The user pointer to associate with the chat room.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_room_set_user_data(IntPtr cr, IntPtr ud);

        /**
 * Create a message attached to a dedicated chat room with a particular content.
 * Use #linphone_chat_room_send_message to initiate the transfer
 * @param cr the chat room.
 * @param initial_content #LinphoneContent initial content. #LinphoneCoreVTable.file_transfer_send is invoked later to notify file transfer progress and collect next chunk of the message if #LinphoneContent.data is NULL.
 * @return a new #LinphoneChatMessage
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_room_create_file_transfer_message(IntPtr cr, IntPtr initial_content);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_room_get_peer_address(IntPtr cr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_room_send_message(IntPtr cr, string msg);

/**
 * Send a message to peer member of this chat room.
 * @param[in] cr LinphoneChatRoom object
 * @param[in] msg LinphoneChatMessage object
 * The state of the message sending will be notified via the callbacks defined in the LinphoneChatMessageCbs object that can be obtained
 * by calling linphone_chat_message_get_callbacks().
 * The LinphoneChatMessage reference is transfered to the function and thus doesn't need to be unref'd by the application.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_room_send_chat_message(IntPtr cr, IntPtr msg);

/**
 * Mark all messages of the conversation as read
 * @param[in] cr The #LinphoneChatRoom object corresponding to the conversation.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_room_mark_as_read(IntPtr cr);

/**
 * Delete a message from the chat room history.
 * @param[in] cr The #LinphoneChatRoom object corresponding to the conversation.
 * @param[in] msg The #LinphoneChatMessage object to remove.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_room_delete_message(IntPtr cr, IntPtr msg);

/**
 * Delete all messages from the history
 * @param[in] cr The #LinphoneChatRoom object corresponding to the conversation.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_room_delete_history(IntPtr cr);

/**
 * Gets the number of messages in a chat room.
 * @param[in] cr The #LinphoneChatRoom object corresponding to the conversation for which size has to be computed
 * @return the number of messages.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_chat_room_get_history_size(IntPtr cr);

/**
 * Gets nb_message most recent messages from cr chat room, sorted from oldest to most recent.
 * @param[in] cr The #LinphoneChatRoom object corresponding to the conversation for which messages should be retrieved
 * @param[in] nb_message Number of message to retrieve. 0 means everything.
 * @return \mslist{LinphoneChatMessage}
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_room_get_history(IntPtr cr, int nb_message);

/**
 * Gets the partial list of messages in the given range, sorted from oldest to most recent.
 * @param[in] cr The #LinphoneChatRoom object corresponding to the conversation for which messages should be retrieved
 * @param[in] begin The first message of the range to be retrieved. History most recent message has index 0.
 * @param[in] end The last message of the range to be retrieved. History oldest message has index of history size - 1 (use #linphone_chat_room_get_history_size to retrieve history size)
 * @return \mslist{LinphoneChatMessage}
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_room_get_history_range(IntPtr cr, int begin, int end);

/**
 * Notifies the destination of the chat message being composed that the user is typing a new message.
 * @param[in] cr The #LinphoneChatRoom object corresponding to the conversation for which a new message is being typed.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_room_compose(IntPtr cr);

/**
 * Tells whether the remote is currently composing a message.
 * @param[in] cr The "LinphoneChatRoom object corresponding to the conversation.
 * @return TRUE if the remote is currently composing a message, FALSE otherwise.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_chat_room_is_remote_composing(IntPtr cr);

/**
 * Gets the number of unread messages in the chatroom.
 * @param[in] cr The "LinphoneChatRoom object corresponding to the conversation.
 * @return the number of unread messages.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_chat_room_get_unread_messages_count(IntPtr cr);

/**
 * Returns back pointer to LinphoneCore object.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_room_get_core(IntPtr cr);

/**
 * When realtime text is enabled #linphone_call_params_realtime_text_enabled, #LinphoneCoreIsComposingReceivedCb is call everytime a char is received from peer.
 * At the end of remote typing a regular #LinphoneChatMessage is received with committed data from #LinphoneCoreMessageReceivedCb.
 * @param[in] msg LinphoneChatMessage
 * @returns  RFC 4103/T.140 char
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint linphone_chat_room_get_char(IntPtr cr);

/**
 * Returns an list of chat rooms
 * @param[in] lc #LinphoneCore object
 * @return \mslist{LinphoneChatRoom}
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_chat_rooms(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint linphone_chat_message_store(IntPtr msg);

/**
 * Returns a #LinphoneChatMessageState as a string.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_state_to_string(LinphoneChatMessageState state);

/**
 * Get the state of the message
 *@param message #LinphoneChatMessage obj
 *@return #LinphoneChatMessageState
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern LinphoneChatMessageState linphone_chat_message_get_state(IntPtr message);

/**
 * Duplicate a LinphoneChatMessage
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_clone(IntPtr message);

/**
 * Acquire a reference to the chat message.
 * @param msg the chat message
 * @return the same chat message
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_ref(IntPtr msg);

/**
 * Release reference to the chat message.
 * @param msg the chat message.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_unref(IntPtr msg);

/**
 * Destroys a LinphoneChatMessage.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_destroy(IntPtr msg);

/**
 * Set origin of the message
 * @param[in] message #LinphoneChatMessage obj
 * @param[in] from #LinphoneAddress origin of this message (copied)
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_set_from_address(IntPtr message, IntPtr addr);

/**
 * Get origin of the message
 * @param[in] message #LinphoneChatMessage obj
 * @return #LinphoneAddress
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_get_from_address(IntPtr message);

/**
 * Set destination of the message
 * @param[in] message #LinphoneChatMessage obj
 * @param[in] to #LinphoneAddress destination of this message (copied)
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_set_to_address(IntPtr message, IntPtr addr);

/**
 * Get destination of the message
 * @param[in] message #LinphoneChatMessage obj
 * @return #LinphoneAddress
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_get_to_address(IntPtr message);

/**
 * Linphone message can carry external body as defined by rfc2017
 * @param message #LinphoneChatMessage
 * @return external body url or NULL if not present.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern string linphone_chat_message_get_external_body_url(IntPtr message);

/**
 * Linphone message can carry external body as defined by rfc2017
 *
 * @param message a LinphoneChatMessage
 * @param url ex: access-type=URL; URL="http://www.foo.com/file"
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_set_external_body_url(IntPtr message, string url);

/**
 * Get the file_transfer_information (used by call backs to recover informations during a rcs file transfer)
 *
 * @param message #LinphoneChatMessage
 * @return a pointer to the LinphoneContent structure or NULL if not present.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_get_file_transfer_information(IntPtr message);

/**
 * Start the download of the file referenced in a LinphoneChatMessage from remote server.
 * @param[in] message LinphoneChatMessage object.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_download_file(IntPtr message);

/**
 * Cancel an ongoing file transfer attached to this message.(upload or download)
 * @param msg	#LinphoneChatMessage
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_cancel_file_transfer(IntPtr msg);

/**
 * Linphone message has an app-specific field that can store a text. The application might want
 * to use it for keeping data over restarts, like thumbnail image path.
 * @param message #LinphoneChatMessage
 * @return the application-specific data or NULL if none has been stored.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern string linphone_chat_message_get_appdata(IntPtr message);

/**
 * Linphone message has an app-specific field that can store a text. The application might want
 * to use it for keeping data over restarts, like thumbnail image path.
 *
 * Invoking this function will attempt to update the message storage to reflect the change if it is
 * enabled.
 *
 * @param message #LinphoneChatMessage
 * @param data the data to store into the message
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_set_appdata(IntPtr message, string data);

/**
 * Get text part of this message
 * @return text or NULL if no text.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_get_text(IntPtr message);

/**
 * Get the time the message was sent.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint linphone_chat_message_get_time(IntPtr message);

/**
 * Returns the chatroom this message belongs to.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_get_chat_room(IntPtr msg);

/**
 * get peer address \link linphone_core_get_chat_room() associated to \endlink this #LinphoneChatRoom
 * @param cr #LinphoneChatRoom object
 * @return #LinphoneAddress peer address
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_get_peer_address(IntPtr msg);

/**
 * Returns the origin address of a message if it was a outgoing message, or the destination address if it was an incoming message.
 *@param message #LinphoneChatMessage obj
 *@return #LinphoneAddress
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_get_local_address(IntPtr message);

/**
 * Add custom headers to the message.
 * @param message the message
 * @param header_name name of the header_name
 * @param header_value header value
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_add_custom_header(IntPtr message, string header_name,
            string header_value);

/**
 * Retrieve a custom header value given its name.
 * @param message the message
 * @param header_name header name searched
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern string linphone_chat_message_get_custom_header(IntPtr message, string header_name);

/**
 * Returns TRUE if the message has been read, otherwise returns FALSE.
 * @param message the message
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_chat_message_is_read(IntPtr message);

/**
 * Returns TRUE if the message has been sent, returns FALSE if the message has been received.
 * @param message the message
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_chat_message_is_outgoing(IntPtr message);

/**
 * Returns the id used to identify this message in the storage database
 * @param message the message
 * @return the id
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint linphone_chat_message_get_storage_id(IntPtr message);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern LinphoneReason linphone_chat_message_get_reason(IntPtr msg);

/**
 * Get full details about delivery error of a chat message.
 * @param msg a LinphoneChatMessage
 * @return a LinphoneErrorInfo describing the details.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_get_error_info(IntPtr msg);

/**
 * Set the path to the file to read from or write to during the file transfer.
 * @param[in] msg LinphoneChatMessage object
 * @param[in] filepath The path to the file to use for the file transfer.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_set_file_transfer_filepath(IntPtr msg, string filepath);

/**
 * Get the path to the file to read from or write to during the file transfer.
 * @param[in] msg LinphoneChatMessage object
 * @return The path to the file to use for the file transfer.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern string linphone_chat_message_get_file_transfer_filepath(IntPtr msg);

/**
 * Fulfill a chat message char by char. Message linked to a Real Time Text Call send char in realtime following RFC 4103/T.140
 * To commit a message, use #linphone_chat_room_send_message
 * @param[in] msg LinphoneChatMessage
 * @param[in] character T.140 char
 * @returns 0 if succeed.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_chat_message_put_char(IntPtr msg, uint charater);

/**
 * get Curent Call associated to this chatroom if any
 * To commit a message, use #linphone_chat_room_send_message
 * @param[in] room LinphoneChatRomm
 * @returns LinphoneCall or NULL.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_room_get_call(IntPtr room);


/**
 * Get the LinphoneChatMessageCbs object associated with the LinphoneChatMessage.
 * @param[in] msg LinphoneChatMessage object
 * @return The LinphoneChatMessageCbs object associated with the LinphoneChatMessage.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_get_callbacks(IntPtr msg);

/**
 * Acquire a reference to the LinphoneChatMessageCbs object.
 * @param[in] cbs LinphoneChatMessageCbs object.
 * @return The same LinphoneChatMessageCbs object.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_cbs_ref(IntPtr cbs);

/**
 * Release reference to the LinphoneChatMessageCbs object.
 * @param[in] cbs LinphoneChatMessageCbs object.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_cbs_unref(IntPtr cbs);

/**
 * Assign a user pointer to the LinphoneChatMessageCbs object.
 * @param[in] cbs LinphoneChatMessageCbs object.
 * @param[in] ud The user pointer to associate with the LinphoneChatMessageCbs object.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_cbs_set_user_data(IntPtr cbs, IntPtr ud);

/**
 * Get the message state changed callback.
 * @param[in] cbs LinphoneChatMessageCbs object.
 * @return The current message state changed callback.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_cbs_get_msg_state_changed(IntPtr cbs);

/**
 * Set the message state changed callback.
 * @param[in] cbs LinphoneChatMessageCbs object.
 * @param[in] cb The message state changed callback to be used.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_cbs_set_msg_state_changed(IntPtr cbs, IntPtr cb);

/**
 * Get the file transfer receive callback.
 * @param[in] cbs LinphoneChatMessageCbs object.
 * @return The current file transfer receive callback.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_cbs_get_file_transfer_recv(IntPtr cbs);

/**
 * Set the file transfer receive callback.
 * @param[in] cbs LinphoneChatMessageCbs object.
 * @param[in] cb The file transfer receive callback to be used.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_cbs_set_file_transfer_recv(IntPtr cbs, IntPtr cb);

/**
 * Get the file transfer send callback.
 * @param[in] cbs LinphoneChatMessageCbs object.
 * @return The current file transfer send callback.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_cbs_get_file_transfer_send(IntPtr cbs);

/**
 * Set the file transfer send callback.
 * @param[in] cbs LinphoneChatMessageCbs object.
 * @param[in] cb The file transfer send callback to be used.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_cbs_set_file_transfer_send(IntPtr cbs, IntPtr cb);

/**
 * Get the file transfer progress indication callback.
 * @param[in] cbs LinphoneChatMessageCbs object.
 * @return The current file transfer progress indication callback.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_cbs_get_file_transfer_progress_indication(IntPtr cbs);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_cbs_set_file_transfer_progress_indication(IntPtr cbs, IntPtr cb);

        #endregion


        #region Contacts

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
        public static extern void linphone_friend_edit(IntPtr fr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_friend_done(IntPtr fr);

        #endregion


        #region Call history

/**
 * Get the list of call logs (past calls).
 * @param[in] lc LinphoneCore object
 * @return \mslist{LinphoneCallLog}
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_call_logs(IntPtr lc);

/**
 * Get the list of call logs (past calls) that matches the given #LinphoneAddress.
 * At the contrary of linphone_core_get_call_logs, it is your responsability to unref the logs and free this list once you are done using it.
 * @param[in] lc LinphoneCore object
 * @return \mslist{LinphoneCallLog}
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_call_history_for_address(IntPtr lc, IntPtr addr);

/**
 * Erase the call log.
 * @param[in] lc LinphoneCore object
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_clear_call_logs(IntPtr lc);

/**
 * Get the number of missed calls.
 * Once checked, this counter can be reset with linphone_core_reset_missed_calls_count().
 * @param[in] lc #LinphoneCore object.
 * @return The number of missed calls.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_missed_calls_count(IntPtr lc);

/**
 * Reset the counter of missed calls.
 * @param[in] lc #LinphoneCore object.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_reset_missed_calls_count(IntPtr lc);

/**
 * Remove a specific call log from call history list.
 * This function destroys the call log object. It must not be accessed anymore by the application after calling this function.
 * @param[in] lc #LinphoneCore object
 * @param[in] call_log #LinphoneCallLog object to remove.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_remove_call_log(IntPtr lc, IntPtr call_log);

/**
 * Sets the database filename where call logs will be stored.
 * If the file does not exist, it will be created.
 * @ingroup initializing
 * @param lc the linphone core
 * @param path filesystem path
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_call_logs_database_path(IntPtr lc, string path);

/**
 * Migrates the call logs from the linphonerc to the database if not done yet
 * @ingroup initializing
 * @param lc the linphone core
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_migrate_logs_from_rc_to_db(IntPtr lc);


        #endregion

        #region CallLog

        /**
 * Get the call ID used by the call.
 * @param[in] cl LinphoneCallLog object
 * @return The call ID used by the call as a string.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_log_get_call_id(IntPtr cl);

/**
 * Get the direction of the call.
 * @param[in] cl LinphoneCallLog object
 * @return The direction of the call.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern LinphoneCallDir linphone_call_log_get_dir(IntPtr cl);

/**
 * Get the duration of the call since connected.
 * @param[in] cl LinphoneCallLog object
 * @return The duration of the call in seconds.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_log_get_duration(IntPtr cl);

/**
 * Get the origin address (ie from) of the call.
 * @param[in] cl LinphoneCallLog object
 * @return The origin address (ie from) of the call.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_log_get_from_address(IntPtr cl);

/**
 * Get the RTP statistics computed locally regarding the call.
 * @param[in] cl LinphoneCallLog object
 * @return The RTP statistics that have been computed locally for the call.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_log_get_local_stats(IntPtr cl);

/**
 * Get the overall quality indication of the call.
 * @param[in] cl LinphoneCallLog object
 * @return The overall quality indication of the call.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_call_log_get_quality(IntPtr cl);

/**
 * Get the persistent reference key associated to the call log.
 *
 * The reference key can be for example an id to an external database.
 * It is stored in the config file, thus can survive to process exits/restarts.
 *
 * @param[in] cl LinphoneCallLog object
 * @return The reference key string that has been associated to the call log, or NULL if none has been associated.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_log_get_ref_key(IntPtr cl);

/**
 * Get the remote address (that is from or to depending on call direction).
 * @param[in] cl LinphoneCallLog object
 * @return The remote address of the call.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_log_get_remote_address(IntPtr cl);

/**
 * Get the RTP statistics computed by the remote end and sent back via RTCP.
 * @note Not implemented yet.
 * @param[in] cl LinphoneCallLog object
 * @return The RTP statistics that have been computed by the remote end for the call.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_log_get_remote_stats(IntPtr cl);

/**
 * Get the start date of the call.
 * @param[in] cl LinphoneCallLog object
 * @return The date of the beginning of the call.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint linphone_call_log_get_start_date(IntPtr cl);

/**
 * Get the status of the call.
 * @param[in] cl LinphoneCallLog object
 * @return The status of the call.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern LinphoneCallStatus linphone_call_log_get_status(IntPtr cl);

/**
 * Get the destination address (ie to) of the call.
 * @param[in] cl LinphoneCallLog object
 * @return The destination address (ie to) of the call.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_log_get_to_address(IntPtr cl);

/**
 * Associate a persistent reference key to the call log.
 *
 * The reference key can be for example an id to an external database.
 * It is stored in the config file, thus can survive to process exits/restarts.
 *
 * @param[in] cl LinphoneCallLog object
 * @param[in] refkey The reference key string to associate to the call log.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_log_set_ref_key(IntPtr cl, string refkey);

/**
 * Tell whether video was enabled at the end of the call or not.
 * @param[in] cl LinphoneCallLog object
 * @return A boolean value telling whether video was enabled at the end of the call.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_call_log_video_enabled(IntPtr cl);

        #endregion

        #region LinphoneAddress

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_address_get_display_name(IntPtr u);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_address_get_username(IntPtr u);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_address_get_domain(IntPtr u);

        #endregion

        #region Security

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_zrtp_secrets_file(IntPtr lc, string file);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_zrtp_secrets_file(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_user_certificates_path(IntPtr lc, string path);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_user_certificates_path(IntPtr lc);

        #endregion

        #endregion

        [DllImport("libmsopenh264.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libmsopenh264_init();

        [DllImport("ortp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ortp_free(IntPtr p);

        #region Contacts

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
        public static extern int linphone_core_import_friends_from_vcard4_file(IntPtr lc, string vcard_file);

/**
 * Creates and export LinphoneFriend objects from LinphoneCore to a file using vCard 4 format
 * @param[in] lc the LinphoneCore object
 * @param[in] vcard_file the path to a file that will contain the vCards
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_export_friends_as_vcard4_file(IntPtr lc, string vcard_file);

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

        #endregion

        #region Configuration

        /**void linphone_proxy_config_set_dial_prefix (LinphoneProxyConfig* cfg, const char* prefix)
         * Sets a dialing prefix to be automatically prepended when inviting a number with linphone_core_invite(); This dialing prefix shall usually be the country code of the country where the user is living.
         * */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_proxy_config_set_dial_prefix(IntPtr cfg, string route);

        /** void linphone_proxy_config_set_dial_escape_plus	(LinphoneProxyConfig * 	cfg, bool_t val)
         * Sets whether liblinphone should replace "+" by international calling prefix in dialed numbers (passed to linphone_core_invite ).
         * */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_proxy_config_set_dial_escape_plus(IntPtr cfg, bool enable);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_config(IntPtr linphoneCore);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lp_config_set_int(IntPtr lpconfig, string section, string key, int value);

        #endregion

        #region MS LIST

/** Inserts a new element containing data to the end of a given list
 * @param list list where data should be added. If NULL, a new list will be created.
 * @param data data to insert into the list
 * @return first element of the list
**/
        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_list_append(IntPtr list, IntPtr data);

/** Inserts given element to the end of a given list
 * @param list list where data should be added. If NULL, a new list will be created.
 * @param new_elem element to append
 * @return first element of the list
**/
        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_list_append_link(IntPtr list, IntPtr new_elem);

/** Inserts a new element containing data to the start of a given list
 * @param list list where data should be added. If NULL, a new list will be created.
 * @param data data to insert into the list
 * @return first element of the list - the one which was just created.
**/
        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_list_prepend(IntPtr list, IntPtr data);

/** Frees all elements of a given list
 * Note that data contained in each element will not be freed. If you need to clean
 * them, consider using @ms_list_free_with_data
 * @param list object to free.
 * @return NULL
**/
        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_list_free(IntPtr list);

/** Concatenates second list to the end of first list
 * @param first First list
 * @param second Second list to append at the end of first list.
 * @return first element of the merged list
**/
        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_list_concat(IntPtr first, IntPtr second);

/** Finds and remove the first element containing the given data. Nothing is done if element is not found.
 * @param list List in which data must be removed
 * @param data Data to remove
 * @return first element of the modified list
**/
        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_list_remove(IntPtr list, IntPtr data);

/** Returns size of a given list
 * @param list List to measure
 * @return Size of list
**/
        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ms_list_size(IntPtr list);

/** Finds and remove given element in list.
 * @param list List in which element must be removed
 * @param element element to remove
 * @return first element of the modified list
**/
        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_list_remove_link(IntPtr list, IntPtr elem);

/** Finds first element containing data in the given list.
 * @param list List in which element must be found
 * @param data data to find
 * @return element containing data, or NULL if not found
**/
        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_list_find(IntPtr list, IntPtr data);

/** Returns the nth element data of the list
 * @param list List object
 * @param index data index which must be returned.
 * @return Element at the given index. NULL if index is invalid (negative or greater or equal to ms_list_size).
**/
        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_list_nth_data(IntPtr list, int index);

/** Returns the index of the given element
 * @param list List object
 * @param elem Element to search for.
 * @return Index of the given element. -1 if not found.
**/
        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ms_list_position(IntPtr list, IntPtr elem);

/** Returns the index of the first element containing data
 * @param list List object
 * @param data Data to search for.
 * @return Index of the element containing data. -1 if not found.
**/
        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ms_list_index(IntPtr list, IntPtr data);

/** Inserts a new element containing data before the given element
 * @param list list where data should be added. If NULL, a new list will be created.
 * @param before element parent to the one we will insert.
 * @param data data to insert into the list
 * @return first element of the modified list.
**/
        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_list_insert(IntPtr list, IntPtr before, IntPtr data);

/** Copies a list in another one, duplicating elements but not data
 * @param list list to copy
 * @return Newly created list
**/
        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_list_copy(IntPtr list);

        #endregion

        #region Content

        /**
 * Get the mime type of the content data.
 * @param[in] content LinphoneContent object.
 * @return The mime type of the content data, for example "application".
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_content_get_type(IntPtr content);

/**
 * Set the mime type of the content data.
 * @param[in] content LinphoneContent object.
 * @param[in] type The mime type of the content data, for example "application".
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_content_set_type(IntPtr content, IntPtr type);

/**
 * Get the mime subtype of the content data.
 * @param[in] content LinphoneContent object.
 * @return The mime subtype of the content data, for example "html".
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_content_get_subtype(IntPtr content);

/**
 * Set the mime subtype of the content data.
 * @param[in] content LinphoneContent object.
 * @param[in] subtype The mime subtype of the content data, for example "html".
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_content_set_subtype(IntPtr content, IntPtr subtype);

/**
 * Get the content data buffer, usually a string.
 * @param[in] content LinphoneContent object.
 * @return The content data buffer.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_content_get_buffer(IntPtr content);

/**
 * Set the content data buffer, usually a string.
 * @param[in] content LinphoneContent object.
 * @param[in] buffer The content data buffer.
 * @param[in] size The size of the content data buffer.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_content_set_buffer(IntPtr content, IntPtr buffer, uint size);

/**
 * Get the string content data buffer.
 * @param[in] content LinphoneContent object
 * @return The string content data buffer.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_content_get_string_buffer(IntPtr content);

/**
 * Set the string content data buffer.
 * @param[in] content LinphoneContent object.
 * @param[in] buffer The string content data buffer.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_content_set_string_buffer(IntPtr content, IntPtr buffer);

/**
 * Get the content data buffer size, excluding null character despite null character is always set for convenience.
 * @param[in] content LinphoneContent object.
 * @return The content data buffer size.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint linphone_content_get_size(IntPtr content);

/**
 * Set the content data size, excluding null character despite null character is always set for convenience.
 * @param[in] content LinphoneContent object
 * @param[in] size The content data buffer size.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_content_set_size(IntPtr content, uint size);

/**
 * Get the encoding of the data buffer, for example "gzip".
 * @param[in] content LinphoneContent object.
 * @return The encoding of the data buffer.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_content_get_encoding(IntPtr content);

/**
 * Set the encoding of the data buffer, for example "gzip".
 * @param[in] content LinphoneContent object.
 * @param[in] encoding The encoding of the data buffer.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_content_set_encoding(IntPtr content, IntPtr encoding);

/**
 * Get the name associated with a RCS file transfer message. It is used to store the original filename of the file to be downloaded from server.
 * @param[in] content LinphoneContent object.
 * @return The name of the content.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_content_get_name(IntPtr content);

/**
 * Set the name associated with a RCS file transfer message. It is used to store the original filename of the file to be downloaded from server.
 * @param[in] content LinphoneContent object.
 * @param[in] name The name of the content.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_content_set_name(IntPtr content, IntPtr name);

        #endregion
    }
}
