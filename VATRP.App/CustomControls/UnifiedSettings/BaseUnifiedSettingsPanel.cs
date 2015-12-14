using com.vtcsecure.ace.windows.Enums;
using com.vtcsecure.ace.windows.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;


namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{
    public delegate void UnifiedSettings_ContentChanging(UnifiedSettingsContentType contentType);
    public delegate void UnifiedSettings_AccountChange(ACEMenuSettingsUpdateType changeType);


    public class BaseUnifiedSettingsPanel : UserControl
    {
        private MainControllerViewModel _parentViewModel;

        public string Title { get; set; }
        // Call When Panel Content needs to change
        public event UnifiedSettings_ContentChanging ContentChanging;
        public event UnifiedSettings_AccountChange AccountChangeRequested;

        // Invoke the Content Changed event
        public virtual void OnContentChanging(UnifiedSettingsContentType contentType)
        {
            if (ContentChanging != null)
            {
                ContentChanging(contentType);
            }
        }

        // Invoke the Account Change Requested event
        public virtual void OnAccountChangeRequested(ACEMenuSettingsUpdateType changeType)
        {
            if (AccountChangeRequested != null)
            {
                AccountChangeRequested(changeType);
            }
        }

        public virtual void SaveData()
        {
          
        }

        public virtual void UpdateForMenuSettingChange(ACEMenuSettingsUpdateType menuSetting)
        {

        }
    }
}
