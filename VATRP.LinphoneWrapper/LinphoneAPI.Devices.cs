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
        public static extern IntPtr linphone_core_get_ms_factory(IntPtr lc);

        [DllImport(MSBase_DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_factory_get_snd_card_manager(IntPtr f);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_sound_devices(IntPtr lc);

        [DllImport(MSBase_DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_snd_card_manager_get_list(IntPtr m);

        [DllImport(MSBase_DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_snd_card_get_string_id(IntPtr obj);

        [DllImport(MSBase_DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_snd_card_get_name(IntPtr obj);

        [DllImport(MSBase_DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_snd_card_get_driver_type(IntPtr obj);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_reload_sound_devices(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_sound_device_can_capture(IntPtr lc, byte[] device);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_sound_device_can_playback(IntPtr lc, byte[] device);

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
        public static extern int linphone_core_set_ringer_device(IntPtr lc, byte[] devid);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_set_playback_device(IntPtr lc, byte[] devid);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_set_capture_device(IntPtr lc, byte[] devid);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_stop_ringing(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_ring(IntPtr lc, byte[] path);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_ring(IntPtr lc);

        [DllImport(MSVoip_DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ms_static_image_set_default_image(string path);

        [DllImport(MSBase_DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void video_stream_change_camera(IntPtr stream, IntPtr cam);

        [DllImport(MSBase_DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_video_device(IntPtr call);
    }
}
