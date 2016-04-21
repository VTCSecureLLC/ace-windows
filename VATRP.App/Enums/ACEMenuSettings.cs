using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.vtcsecure.ace.windows.Enums
{
    public enum ACEMenuSettingsUpdateType
    {
        // settings calls to update the ui/services
        // Registration/Account
        Logout,
        ClearSettings,
        RunWizard,
        UserNameChanged,
        RegistrationChanged,
        CardDavConfigChanged,
        VideoPolicyChanged,
        // Preferences
        NetworkSettingsChanged,
        AdvancedSettingsChanged,
        ShowSelfViewChanged,
        // menu telling settings to update display if needed
        // View
        ShowSelfViewMenu,
        // Audio
        MuteMicrophoneMenu,
        MuteSpeakerMenu
    }
}
