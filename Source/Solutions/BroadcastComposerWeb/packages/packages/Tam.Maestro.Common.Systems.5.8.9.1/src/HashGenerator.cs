namespace Tam.Maestro.Common
{
    public static class HashGenerator
    {
        public static string ComputeHash(byte[] fileData)
        {
            System.Security.Cryptography.SHA1CryptoServiceProvider sha1Provider = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            byte[] encryptedData = sha1Provider.ComputeHash(fileData);
            return System.BitConverter.ToString(encryptedData);
        }
    }
}
