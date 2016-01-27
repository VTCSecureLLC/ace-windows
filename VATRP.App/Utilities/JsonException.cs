using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.vtcsecure.ace.windows.Utilities
{
    public enum JsonExceptionType
    {
        DESERIALIZATION_FAILED,
        CONNECTION_FAILED,
        UNKNOWN
    }

    public class JsonException : Exception
    {
        public JsonExceptionType jsonExceptionType { get; set; }

        public JsonException(JsonExceptionType exceptionType)
        {
            jsonExceptionType = exceptionType;
        }

        public JsonException(JsonExceptionType jsonExceptionType, string message)
            : base(message)
        {
        }

        public JsonException(JsonExceptionType jsonExceptionType, string message, Exception inner)
            : base(message, inner)
        {
        }

    }
}
