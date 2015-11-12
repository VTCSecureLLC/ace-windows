using System;

namespace VATRP.Core.Model
{
	public class Configuration
	{
        public enum ConfSection
        {
            GENERAL,
            MAIN_WINDOW,
            CALL_WINDOW,
            MESSAGE_WINDOW,
            DIALPAD_WINDOW,
            CONTACT_WINDOW,
            DOCK_WINDOW,
            HISTORY_WINDOW,
            SELF_WINDOW,
            SETTINGS_WINDOW,
            MENUBAR,
            LINPHONE,
            ACCOUNT,
            REMOTE_VIDEO_VIEW,
            KEYPAD_WINDOW,
            CALLINFO_WINDOW
        }

        public enum ConfEntry
        {
            WINDOW_STATE, WINDOW_LEFT, WINDOW_TOP, WINDOW_WIDTH, WINDOW_HEIGHT,
            ACTIVE_WINDOW,
            PASSWD,
            LOGIN,
            SERVER_ADDRESS,
            SERVER_PORT,
            USERNAME,
            DISPLAYNAME,
            LINPHONE_USERAGENT,
            REQUEST_LINK,
            ACCOUNT_IN_USE,
            AUTO_ANSWER,
            AUTO_ANSWER_AFTER,
            AVPF_ON,
            DTMF_SIP_INFO,
            USE_RTT,
            ENABLE_ADAPTIVE_RATE_CTRL,
            CURRENT_PROVIDER
        }

        public static string LINPHONE_SIP_SERVER = "bc1.vatrp.net";
		public static ushort LINPHONE_SIP_PORT = 5060;
		public static string DISPLAY_NAME = "John Doe";
		public static string LINPHONE_USERAGENT = "VATRP";
        public static string DEFAULT_REQUEST = @"https://crm.videoremoteassistance.com/users/sign_in";
	}
}
