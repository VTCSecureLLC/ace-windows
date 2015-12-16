using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.vtcsecure.ace.windows.Enums
{
    public enum ACEMenuSettingsUpdateType
    {
        // Registration/Account
        Logout,
        ClearAccount,
        RunWizard,
        UserNameChanged,
        RegistrationChanged,
        VideoPolicyChanged,
        // Audio
        MuteMicrophoneMenu,
        MuteSpeakerMenu,
        // Preferences
        NetworkSettingsChanged
    }
}
