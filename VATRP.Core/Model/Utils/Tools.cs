using System;
using System.Collections.Generic;
using System.Windows;

namespace VATRP.Core.Model.Utils
{
    public class Tools
    {
        private static int _messageIdPrefix = 0;

        private static int GetNextPrefixForMessageID()
        {
            if (_messageIdPrefix > 0x12b)
            {
                _messageIdPrefix = 0;
            }
            return _messageIdPrefix++;
        }

        public static string GenerateMessageId()
        {
            int nextPrefixForMessageID = GetNextPrefixForMessageID();
            return (Time.GetTimeTicksUTCString() + nextPrefixForMessageID);
        }

        public static void InsertByIndex<T>(T item, IList<T> itemList, int index)
        {
            if (((item != null) && (itemList != null)) && (index >= 0))
            {
                if (index >= itemList.Count)
                {
                    itemList.Add(item);
                }
                else
                {
                    itemList.Insert(index, item);
                }
            }
        }

        public static void InsertToTop<T>(T item, IList<T> itemList)
        {
            if ((item != null) && (itemList != null))
            {
                InsertByIndex<T>(item, itemList, 0);
            }
        }

        public static string ReplaceNewlineToSpaceSymbols(string text)
        {
            string str = text;
            return str.Replace("\r\n", " ").Replace('\n', ' ');
        }
    }
}

