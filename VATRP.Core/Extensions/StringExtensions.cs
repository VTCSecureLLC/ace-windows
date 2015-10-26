using System;
using System.Diagnostics;

namespace VATRP.Core.Extensions
{
    public static class StringExtensions
    {
        public static DateTime FromMilliseconds(this string str)
        {
            DateTime minValue = DateTime.MinValue;
            try
            {
                if (str.IsValid())
                {
                    long num2 = Convert.ToInt64(str) * 10000;
                    minValue = new DateTime(num2, (DateTimeKind) DateTimeKind.Utc);
                }
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
            return minValue;
        }

        public static bool IsValid(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        public static bool NotBlank(this string str)
        {
            return (!string.IsNullOrEmpty(str) && (str.Length > 0));
        }

        public static bool ToBool(this string str)
        {
            bool flag = false;
            if (str.NotBlank())
            {
                flag = (str.CompareTo("true") == 0) || (str.CompareTo(bool.TrueString) == 0);
            }
            return flag;
        }

        public static byte[] ToByteArray(this string str)
        {
            if (str.NotBlank())
            {
                try
                {
                    byte[] buffer2 = new byte[str.Length];
                    for (int i = 0; i < str.Length; i++)
                    {
                        buffer2[i] = (byte) str[i];
                    }
                    return buffer2;
                }
                catch
                {
                }
            }
            return null;
        }

        public static T ToEnum<T>(this string str)
        {
            Type type = typeof(T);
            if (str.NotBlank() && Enum.IsDefined(type, str))
            {
                return (T) Enum.Parse(type, str, false);
            }
            return default(T);
        }

        public static int ToInt32(this string str)
        {
            int num = 0;
            try
            {
                if (str.NotBlank())
                {
                    num = Convert.ToInt32(str);
                }
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
            return num;
        }

        public static string UppercaseWords(this string str)
        {
            char[] chArray = str.ToLower().ToCharArray();
            if ((chArray.Length >= 1) && char.IsLower(chArray[0]))
            {
                chArray[0] = char.ToUpper(chArray[0]);
            }
            for (int i = 1; i < chArray.Length; i++)
            {
                if ((chArray[i - 1] == ' ') && char.IsLower(chArray[i]))
                {
                    chArray[i] = char.ToUpper(chArray[i]);
                }
            }
            return (string) new string(chArray);
        }
    }
}

