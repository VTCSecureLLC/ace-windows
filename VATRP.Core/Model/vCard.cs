using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using VATRP.Core.Extensions;
using EWSoftware.PDI.Parser;

namespace VATRP.Core.Model
{

    public class vCard
    {
        #region Properties

        public string FormattedName { get; set; }

        public string Surname { get; set; }

        public string GivenName { get; set; }

        public string MiddleName { get; set; }

        public string Prefix { get; set; }

        public string Suffix { get; set; }

        public string Title { get; set; }

        public string ProdId { get; set; }
        public DateTime Rev { get; set; }
        public string IMPP { get; set; }
        public string TEL { get; set; }
        public string URI { get; set; }
        #endregion

        public vCard()
        {
            FormattedName = string.Empty;
            Surname = string.Empty;
            GivenName = string.Empty;
            MiddleName = string.Empty;
            Prefix = string.Empty;
            Suffix = string.Empty;
            Title = string.Empty;
            Rev = DateTime.Now;
            ProdId = string.Empty;
            IMPP = string.Empty;
            TEL = string.Empty;
            URI = string.Empty;
        }
    }

    public class vCardReader
    {
        #region Members

        private static readonly ILog LOG = LogManager.GetLogger(typeof (vCardReader));

        public List<vCard> vCards = new List<vCard>();
        #endregion

        public vCardReader(TextReader stream)
        {
            var lines = new StringBuilder();
            try
            {
                if (stream != null)
                    lines.Append(stream.ReadToEnd());
            }
            catch (Exception ex)
            {
                LOG.Error("Failed to read stream");
                return;
            }

            ParseAll(lines);
        }

        public vCardReader(string filePath)
        {
            if (!File.Exists(filePath))
            {
                LOG.Error("File does not exists. " + filePath);
                return;
            }

            var lines = new StringBuilder();
            try
            {
                lines.Append(File.ReadAllText(filePath));

            }
            catch (Exception ex)
            {
                LOG.Error("Failed to read text file: " + filePath);
                return;
            }

            ParseAll(lines);
        }

        public vCardReader(StringBuilder lines)
        {
            ParseAll(lines);
        }

        private void ParseAll(StringBuilder lines)
        {
            int posStart = lines.ToString().IndexOf("BEGIN:VCARD");
            int posPosEnd = lines.ToString().IndexOf("END:VCARD");
            while (posStart != -1 && posPosEnd != -1 && posPosEnd > posStart)
            {
                int endOfVcard = posPosEnd + "END:VCARD".Length + 1;
                var vCardText = lines.ToString(posStart, endOfVcard);
                Parse(vCardText);
                lines.Remove(0, endOfVcard+1);
                posStart = lines.ToString().IndexOf("BEGIN:VCARD");
                posPosEnd = lines.ToString().IndexOf("END:VCARD");
            }
        }

        private void Parse(string lines)
        {           

            vCard card = new vCard();
            RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline |
                                   RegexOptions.IgnorePatternWhitespace;

            Regex regex;
            Match m;
            MatchCollection mc;

            regex = new Regex(@"(?<strElement>(FN))   (:(?<strFN>[^\n\r]*))", options);
            m = regex.Match(lines);
            if (m.Success)
                card.FormattedName = m.Groups["strFN"].Value;

            // N
            regex =
                new Regex(
                    @"(\n(?<strElement>(N)))   (:(?<strSurname>([^;]*))) (;(?<strGivenName>([^;]*)))  (;(?<strMidName>([^;]*))) (;(?<strPrefix>([^;]*))) (;(?<strSuffix>[^\n\r]*))",
                    options);
            m = regex.Match(lines);
            if (m.Success)
            {
                card.Surname = m.Groups["strSurname"].Value;
                card.GivenName = m.Groups["strGivenName"].Value;
                card.MiddleName = m.Groups["strMidName"].Value;
                card.Prefix = m.Groups["strPrefix"].Value;
                card.Suffix = m.Groups["strSuffix"].Value;
            }

            // IMPP
            regex = new Regex(@"(?<strElement>(IMPP))((:|;TYPE=.*sip:)(?<strIMPP>[^\n\r]*))", options);
            m = regex.Match(lines);
            if (m.Success)
                card.IMPP = m.Groups["strIMPP"].Value;

            regex = new Regex(@"(?<strElement>(TEL))(:|;TYPE=.*sip:)(?<strTEL>[^\n\r]*)", options);
            m = regex.Match(lines);
            if (m.Success)
                card.TEL = m.Groups["strTEL"].Value;

            regex = new Regex(@"(?<strElement>(URI))   (:(?<strURI>[^\n\r]*))", options);
            m = regex.Match(lines);
            if (m.Success)
                card.URI = m.Groups["strURI"].Value;

            // PRODID
            regex = new Regex(@"(?<strElement>(PRODID))   (:(?<strPRODID>[^\n\r]*))", options);
            m = regex.Match(lines);
            if (m.Success)
                card.ProdId = m.Groups["strPRODID"].Value;

            // REV
            regex = new Regex(@"(?<strElement>(REV))   (:(?<strREV>[^\n\r]*))", options);
            m = regex.Match(lines);
            if (m.Success)
            {
                string[] expectedFormats = { "yyyyMMddHHmmss", "yyyy-MM-ddTHHmmssZ", "yyyyMMddTHHmmssZ" };
                card.Rev = DateTime.ParseExact(m.Groups["strREV"].Value, expectedFormats, null, System.Globalization.DateTimeStyles.AllowWhiteSpaces);
            }

            if (card.FormattedName.NotBlank())
                vCards.Add(card);
        }
    }

    public class vCardWriter
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(vCardWriter));
        public vCardWriter()
        {
            
        }

        public void WriteCards(string filePath, List<vCard> vCards)
        {
            var output = new StringBuilder();
            if (vCards != null)
            {
                foreach (var card in vCards)
                {
                    Write(ref output, card);
                }
            }

            try
            {
                var stream = File.CreateText(filePath);
                stream.Write(output.ToString());
                stream.Close();
            }
            catch (Exception ex)
            {
                LOG.Error("Failed to write vCard file: " + filePath);
            }
        }

        private void Write(ref StringBuilder output, vCard card)
        {
            if (card == null)
                return;

            output.Append("BEGIN:VCARD" + Environment.NewLine + "VERSION:3.0" + Environment.NewLine);
            output.Append("N:" + card.Surname + ";" + card.GivenName + ";" + card.MiddleName + ";" + 
                card.Prefix + ";" + card.Suffix + Environment.NewLine);
            output.Append("FN:" + card.FormattedName + Environment.NewLine);
            if (card.Title.NotBlank())
                output.Append("TITLE:" + card.Title + Environment.NewLine);
            output.Append("END:VCARD" + Environment.NewLine);
        }
    }
}