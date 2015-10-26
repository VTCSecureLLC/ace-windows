using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Reflection;

namespace VATRP.Core.Model
{
    public sealed class VATRPUserGroup : INotifyPropertyChanged, IEquatable<VATRPUserGroup>, IComparable, IComparable<VATRPUserGroup>
    {
        private string _name;
        private int _id;
        private int _id_forUI;
        private string _interests;
        private bool _isFavorite;
        private string _description;

        public VATRPUserGroup()
        {
            this._id = -1;
            this._id_forUI = -1;
        }

        public VATRPUserGroup(int groupID)
        {
            this._id = groupID;
            this._id_forUI = -1;
        }

        public int ID
        {
            get
            {
                return this._id;
            }
            set
            {
                this._id = value;
                OnPropertyChanged("ID");
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
                OnPropertyChanged("Name");
            }
        }

        public string Description
        {
            get
            {
                return this._description;
            }
            set
            {
                this._description = value;
                OnPropertyChanged("Description");
            }
        }

        public bool IsFavorite
        {
            get
            {
                return this._isFavorite;
            }
            set
            {
                if (this._isFavorite != value)
                {
                    this._isFavorite = value;
                    OnPropertyChanged("IsFavorite");
                }
            }
        }

        public string Interests
        {
            get { return _interests; }
            set
            {
                _interests = value;
                OnPropertyChanged("Interests");
            }
        }

        public int CompareTo(VATRPUserGroup other)
        {
            return ID.CompareTo(other.ID);
        }

        public bool Equals(VATRPUserGroup other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }
            return (object.ReferenceEquals(this, other) || (ID == other.ID));
        }

        public override bool Equals(object obj)
        {
            return ((obj is VATRPUserGroup) && this.Equals(obj as VATRPUserGroup));
        }


        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }


        public static bool operator ==(VATRPUserGroup one, VATRPUserGroup two)
        {
            return (object.ReferenceEquals(one, two) || one.Equals(two));
        }

        public static bool operator !=(VATRPUserGroup one, VATRPUserGroup two)
        {
            return !(one == two);
        }

        int IComparable.CompareTo(object obj)
        {
            if (!(obj is VATRPUserGroup))
            {
                throw new ArgumentException("Argument is not a GroupElement", "obj");
            }
            return this.CompareTo((VATRPUserGroup)obj);
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        
        public void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

    }
}

