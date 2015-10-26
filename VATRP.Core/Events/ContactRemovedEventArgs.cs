
using VATRP.Core.Model;

namespace VATRP.Core.Events
{
    public class ContactRemovedEventArgs : ContactBasedEventArgs
    {
        private readonly bool _isUserAction;

        internal ContactRemovedEventArgs(ContactID contact, bool isUserAction) : base(contact)
        {
            this._isUserAction = isUserAction;
        }

        public bool IsUserAction
        {
            get
            {
                return this._isUserAction;
            }
        }
    }
}

