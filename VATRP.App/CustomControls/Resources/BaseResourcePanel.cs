using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace com.vtcsecure.ace.windows.CustomControls.Resources
{
    public delegate void Resources_ContentChanging(ResourcesType contentType);

    public class BaseResourcePanel : UserControl
    {
        public event Resources_ContentChanging ContentChanging;

        public string Title { get; set; }

        // Invoke the Content Changed event
        public virtual void OnContentChanging(ResourcesType contentType)
        {
            if (ContentChanging != null)
            {
                ContentChanging(contentType);
            }
        }

    }
}
