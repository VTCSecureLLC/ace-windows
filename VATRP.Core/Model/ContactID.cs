using System;
using System.ComponentModel;

namespace VATRP.Core.Model
{
    public class ContactID : IEquatable<ContactID>, INotifyPropertyChanged
    {
        public string ID { get; set; }

        public IntPtr NativePtr { get; set; }
        public ContactID()
        {
            this.ID = string.Empty;
            this.NativePtr = IntPtr.Zero;
        }

        public ContactID(ContactID contactID)
        {
            if (contactID != null)
            {
                this.ID = contactID.ID;
                this.NativePtr = contactID.NativePtr;
            }
        }

        public ContactID(string contactID, IntPtr nativePtr)
        {
            this.ID = contactID;
            this.NativePtr = nativePtr;
        }

        public virtual bool Equals(ContactID other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }
            return (object.ReferenceEquals(other, this) || ((this.ID == other.ID) && (this.NativePtr == other.NativePtr)));
        }

        public override bool Equals(object obj)
        {
            return ((obj is ContactID) && this.Equals(obj as ContactID));
        }

        public override int GetHashCode()
        {
            return (base.GetHashCode() ^ this.ID.GetHashCode());
        }

        public static bool operator ==(ContactID first, ContactID second)
        {
            if (object.ReferenceEquals(first, null))
            {
                return object.ReferenceEquals(first, second);
            }
            return first.Equals(second);
        }

        public static bool operator !=(ContactID first, ContactID second) 
        {
            return !(first == second);
        }

        public override string ToString()
        {
            return (base.ToString() + ";" + this.ID);
        }

        #region NotifyPropertyChanged Interface

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}

