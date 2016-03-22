using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace VATRP.Core.Utilities
{
    public static class DataProtectionHelper
    {
        public static string Protect(string clearText, string optionalEntropy = null, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        {
            if (clearText == null)
            {
                return "";  // if nothing is passed in to encry, just return an empty string.
            }

            byte[] encryptedBytes = GetProtectedBytes(clearText, optionalEntropy, scope);
            return Convert.ToBase64String(encryptedBytes);
        }

        public static byte[] GetProtectedBytes(string clearText, string optionalEntropy = null, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        {
            byte[] clearBytes = Encoding.UTF8.GetBytes(clearText);
            byte[] entropyBytes = string.IsNullOrEmpty(optionalEntropy)
                ? null
                : Encoding.UTF8.GetBytes(optionalEntropy);
            byte[] encryptedBytes = ProtectedData.Protect(clearBytes, entropyBytes, scope);
            return encryptedBytes;
        }

        public static bool WriteProtectedBytesToFile(string fileName, string clearText)
        {
            byte[] encryptedBytes = GetProtectedBytes(clearText);
            try
            {
                // Open file for reading
                System.IO.FileStream fileStream = new System.IO.FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                // Writes a block of bytes to this stream using data from a byte array.
                fileStream.Write(encryptedBytes, 0, encryptedBytes.Length);

                // close file stream
                fileStream.Close();

                return true;
            }
            catch (Exception exception)
            {
                // Error - but this just means that we will not store the password
                Console.WriteLine("Exception caught in process: {0}", exception.ToString());
            }

            // error occured, return false
            return false;
        }


        public static string Unprotect(string encryptedText, string optionalEntropy = null, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        {
            if (encryptedText == null)
                return ""; // if there is no text, then return an empty string
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            string clearString = GetUnprotectedStringFromBytes(encryptedBytes, optionalEntropy, scope);
            return clearString;
        }

        public static string GetUnprotectedStringFromBytes(byte[] encryptedBytes, string optionalEntropy = null, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        {
            byte[] entropyBytes = string.IsNullOrEmpty(optionalEntropy)
                ? null
                : Encoding.UTF8.GetBytes(optionalEntropy);
            byte[] clearBytes = ProtectedData.Unprotect(encryptedBytes, entropyBytes, scope);
            return Encoding.UTF8.GetString(clearBytes);
        }

        public static string ReadUnprotectedBytesFromProtectedFile(string fileName)
        {
            // first read the bytes if the file exists.
            if (!File.Exists(fileName))
                return "";
            try
            {
                byte[] bytes = System.IO.File.ReadAllBytes(fileName);
                if ((bytes != null) && (bytes.Length > 0))
                {
                    return GetUnprotectedStringFromBytes(bytes);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to read the bytes from the file " + fileName);
            }
            return "";
        }
    }
}