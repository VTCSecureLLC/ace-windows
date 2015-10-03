using System;
using System.Runtime.InteropServices;
using System.Text;
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

        #endregion

        #region Methods

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_logs(IntPtr file);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_disable_logs();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_new(IntPtr vtable, string config_path, string factory_config,
            IntPtr userdata);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_destroy(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_create_proxy_config(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_auth_info_new(string username, string userid, string passwd,
            string ha1, string realm, string domain);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_add_auth_info(IntPtr lc, IntPtr info);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_proxy_config_set_identity(IntPtr obj, string identity);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_proxy_config_set_server_addr(IntPtr cfg, string server_addr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_proxy_config_enable_register(IntPtr obj, bool val);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_address_destroy(IntPtr u);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_add_proxy_config(IntPtr lc, IntPtr cfg);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_default_proxy_config(IntPtr lc, IntPtr config);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_iterate(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_create_default_call_parameters(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_params_enable_video(IntPtr cp, bool enabled);

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
        public static extern IntPtr linphone_call_get_remote_address_as_string(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_accept_call_with_params(IntPtr lc, IntPtr call, IntPtr callparams);

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
        public static extern void linphone_core_use_files(IntPtr lc, bool yesno);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_get_state(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_ref(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_unref(IntPtr call);

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
 * @deprecated Use #linphone_core_enable_mic instead.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_mute_mic(IntPtr lc, bool muted);

/**
 * Get mic state.
 * @deprecated Use #linphone_core_mic_enabled instead
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_is_mic_muted(IntPtr lc);

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
 * Enables video globally.
 *
 * This function does not have any effect during calls. It just indicates LinphoneCore to
 * initiate future calls with video or not. The two boolean parameters indicate in which
 * direction video is enabled. Setting both to false disables video entirely.
 *
 * @param lc The LinphoneCore object
 * @param vcap_enabled indicates whether video capture is enabled
 * @param display_enabled indicates whether video display should be shown
 * @ingroup media_parameters
 * @deprecated Use #linphone_core_enable_video_capture and #linphone_core_enable_video_display instead.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_video(IntPtr lc, bool vcap_enabled, bool display_enabled);

/**
 * Returns TRUE if video is enabled, FALSE otherwise.
 * @ingroup media_parameters
 * @deprecated Use #linphone_core_video_capture_enabled and #linphone_core_video_display_enabled instead.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_core_video_enabled(IntPtr lc);

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

        #endregion

        [DllImport("libmsopenh264.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libmsopenh264_init();

    }
}
