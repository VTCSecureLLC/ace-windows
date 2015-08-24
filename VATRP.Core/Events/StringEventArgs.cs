
namespace VATRP.Core.Events
{
    public class StringEventArgs : MyEventArgs
    {
        private readonly string _value;

        public StringEventArgs(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }
    }
}
