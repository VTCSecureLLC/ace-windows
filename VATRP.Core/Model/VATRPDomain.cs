using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VATRP.Core.Model
{
    // will be parsed as a json resource
    // eg:
    //[
    // {"name": "ACETest Registrar", "domain": "acetest-registrar.vatrp.net", "icon": "http://cdn.vatrp.net/acetest.png", "icon2x": "http://cdn.vatrp.net/acetest2x.png" },
    // {"name": "BC1", "domain": "bc1.vatrp.net", "icon": "http://cdn.vatrp.net/belledonne.png", "icon2x": "http://cdn.vatrp.net/belledonne2x.png" },
    // {"name": "Star", "domain": "caag.vatrp.net", "icon": "http://cdn.vatrp.net/caag.png", "icon2x": "http://cdn.vatrp.net/caag2x.png" },
    // {"name": "Convo", "domain": "convo.vatrp.net", "icon": "http://cdn.vatrp.net/convo.png", "icon2x": "http://cdn.vatrp.net/convo2x.png" },
    // {"name": "Global", "domain": "global.vatrp.net", "icon": "http://cdn.vatrp.net/global.png", "icon2x": "http://cdn.vatrp.net/global2x.png" },
    // {"name": "Purple", "domain": "purple.vatrp.net", "icon": "http://cdn.vatrp.net/purple.png", "icon2x": "http://cdn.vatrp.net/purple2x.png" },
    // {"name": "Sorenson", "domain": "sorenson.vatrp.net", "icon": "http://cdn.vatrp.net/sorenson.png", "icon2x": "http://cdn.vatrp.net/sorenson2.png" },
    // {"name": "ZVRS", "domain": "zvrs.vatrp.net", "icon": "http://cdn.vatrp.net/csdvrs.png", "icon2x": "http://cdn.vatrp.net/csdvrs2x.png" }
    //]
    public class VATRPDomain
    {
        public string name { get; set; }
        public string domain { get; set; }
        public string icon { get; set; }
        public string icon2x { get; set; }

        public VATRPDomain()
        {
        }
    }
}
