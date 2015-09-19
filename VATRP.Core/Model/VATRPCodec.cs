using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace VATRP.Core.Model
{
    public class VATRPCodec
    {
        public CodecType Purpose;
        public string CodecName { get; set; }
        public bool Status { get; set; }
        public int Rate { get; set; }
        public int IPBitRate { get; set; }
        public int Priority { get; set; }
        public string Description { get; set; }
        public bool IsUsable { get; set; }
        public int Channels { get; set; }
        public string ReceivingFormat { get; set; }
        public string SendingFormat { get; set; }
        
        public VATRPCodec(string name, bool isEnabled) :
            this()
        {
            CodecName = name;
            Status = isEnabled;
        }

        public VATRPCodec()
        {
            
        }
    }
}
