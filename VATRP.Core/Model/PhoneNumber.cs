
namespace VATRP.Core.Model
{
    public class PhoneNumber
    {
        private PhoneType _phType;

        public PhoneType NumberKind
        {
            get { return _phType; }
            set { _phType = value; }
        }

        public string Number { get; set; }

        public string TrimmedNumber { get; set; }

        public bool IsFavorite { get; set; }

        public PhoneNumber()
        {
            IsFavorite = false;
        }

        public PhoneNumber(string number, PhoneType phType)
            : this()
        {
            Number = number;
            _phType = phType;

        }

        public static string RemoveSymbols(string inpNumber)
        {
            if (string.IsNullOrEmpty(inpNumber))
                return string.Empty;
            const string delimiters = "-() +";
            var outNumber = inpNumber.ToLower();

            return outNumber.Trim(delimiters.ToCharArray());
        }

        public override string ToString()
        {
            return _phType.ToString();
        }

        public void Modify(string number)
        {
            Number = number;
            TrimmedNumber = RemoveSymbols(number);
        }
    }
}
