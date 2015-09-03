using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace VATRP.Core.Model
{
    public class VATRPContact : IComparable<VATRPContact>, INotifyPropertyChanged, ICloneable
    {
        #region Members
        private string _displayName;
        private string _firstName;
        private string _lastName;
        private string _imageurl;
        private string _contactId;
        #endregion

        #region Methods
        public VATRPContact()
        {
            FavoriteNumbers = new List<PhoneNumber>();
        }

        public static string GetFirstNameKey(VATRPContact contact)
        {
            char key = char.ToLower(contact.DisplayName[0]);

            if (key < 'a' || key > 'z')
            {
                key = '#';
            }

            return key.ToString();
        }
        #endregion

        #region Properties
        public List<PhoneNumber> FavoriteNumbers { get; set; }

        // Think about Clone() when you add new fields

        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(_lastName))
                    return _firstName;

                return string.Format("{0} {1}", _firstName, _lastName);
            }
        }

        public string DisplayName
        {
            get
            {
                return _displayName;
            }

            set
            {
                this._displayName = value;
                this.OnPropertyChanged("DisplayName");
            }
        }

        public string FirstName
        {
            get
            {
                return _firstName;
            }

            set
            {
                this._firstName = value;
                this.OnPropertyChanged("FirstName");
            }
        }

        public string LastName
        {
            get
            {
                return _lastName;
            }

            set
            {
                this._lastName = value;
                this.OnPropertyChanged("LastName");
            }
        }  
        public string ImageUrl
        {
            get { return _imageurl; }
            set
            {
                _imageurl = value;
                this.OnPropertyChanged("ImageURL");
            }
        }
        #endregion

        #region ICloneable

        public object Clone()
        {
            var clone = new VATRPContact
            {
                _contactId = _contactId,
                _displayName = _displayName,
                _imageurl = _imageurl,
                _firstName = _firstName,
                _lastName = _lastName
            };
            clone.FavoriteNumbers.AddRange(this.FavoriteNumbers);
            return clone;
        }

        #endregion

        #region IComparable

        public int CompareTo(VATRPContact other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            return string.Compare(this.DisplayName, other.DisplayName);
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected void OnPropertyChanged(String propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
