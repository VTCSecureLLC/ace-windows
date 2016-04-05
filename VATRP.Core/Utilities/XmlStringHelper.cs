using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VATRP.Core.Utilities
{
    public static class XmlStringHelper
    {
         public static string UnescapeXML(this string s)
         {
            if (string.IsNullOrEmpty(s)) 
                return s;
    
            string returnString = s;
            returnString = returnString.Replace("&apos;", "'");
            returnString = returnString.Replace("&quot;", "\"");
            returnString = returnString.Replace("&gt;", ">");
            returnString = returnString.Replace("&lt;", "<");
            returnString = returnString.Replace("&amp;", "&");
    
            return returnString;
        }
    }
}
