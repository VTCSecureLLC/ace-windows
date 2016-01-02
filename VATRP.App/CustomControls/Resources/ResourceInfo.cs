using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.vtcsecure.ace.windows.CustomControls.Resources
{
   // eg 
   // [ {"name":"SBA ASL Line", "address":"+18554404960"},
   // {"name":"FCC ASL Line", "address":"+18444322275"}
   // ]
    public class ResourceInfo
    {
        public string name { get; set; }
        public string address { get; set; }

        public ResourceInfo()
        {

        }
    }

}
