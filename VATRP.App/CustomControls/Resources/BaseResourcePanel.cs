using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace com.vtcsecure.ace.windows.CustomControls.Resources
{
    public delegate void Resources_ContentChanging(ResourcesType contentType);
    public delegate void Resources_CallResource(ResourceInfo resourceInfo);

    public class BaseResourcePanel : UserControl
    {
        public event Resources_ContentChanging ContentChanging;
        public event Resources_CallResource CallResourceRequested;

        public string Title { get; set; }

        // Invoke the Content Changed event
        public virtual void OnContentChanging(ResourcesType contentType)
        {
            if (ContentChanging != null)
            {
                ContentChanging(contentType);
            }
        }

        public virtual void OnCallResourceRequested(ResourceInfo resourceInfo)
        {
            if (CallResourceRequested != null)
            {
                CallResourceRequested(resourceInfo);
            }
        }

    }
}
