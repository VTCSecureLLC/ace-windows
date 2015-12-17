using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{
    public enum UnifiedSettingsContentType
    {
        // Main Panel
        MainContent,
        // Top section of main
        GeneralContent,
        AudioVideoContent,
        ThemeContent,
        TextContent,
        SummaryContent,

        // Preferences
        AudioSettingsContent,
        VideoSettingsContent,
        CallSettingsContent,
        NetworkSettingsContent,
        AdvancedSettingsContent
    }
}
