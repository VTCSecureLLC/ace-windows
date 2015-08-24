using System;

namespace VATRP.Core.Model
{
	public class Configuration
	{
		public enum ConfSection
		{
			GENERAL,
			VIDEO,
			SELF_VIEW,
			MESSAGE_VIEW,
			DIALPAD,
			CONTACT,
			REGSTATUS,
			MENUBAR,
			LINPHONE
		}

		public enum ConfEntry
		{
			WINDOW_STATE, WINDOW_LEFT, WINDOW_TOP, WINDOW_RIGHT, WINDOW_BOTTOM,
            ACTIVE_WINDOW

		}
	}
}
