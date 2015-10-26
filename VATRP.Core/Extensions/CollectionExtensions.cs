using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace VATRP.Core.Extensions
{
    public static class CollectionExtensions
    {
        public static string AsString(this byte[] bytes)
        {
            StringBuilder builder = null;
            if ((bytes != null) && (bytes.Length > 0))
            {
                builder = new StringBuilder(bytes.Length);
                foreach (byte num in bytes)
                {
                    builder.Append((char) num);
                }
            }
            if ((builder != null) && (builder.Length > 0))
            {
                return builder.ToString();
            }
            return string.Empty;
        }

        public static void InsertByIndex<T>(this IList<T> itemList, T item, int index)
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

        public static void InsertToTop<T>(this IList<T> itemList, T item)
        {
            if ((item != null) && (itemList != null))
            {
                itemList.InsertByIndex<T>(item, 0);
            }
        }

        public static void ReplaceToTop<T>(this ObservableCollection<T> itemList, T element)
        {
            if ((element != null) && (itemList.Count != 0))
            {
                lock (itemList)
                {
                    int index = itemList.IndexOf(element);
                    if (index > 0)
                    {
                        itemList.Move(index, 0);
                    }
                }
            }
        }
    }
}

