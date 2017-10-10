namespace Plus.Utilities
{
    using System;

    public static class Converter
    {
        public static string BytesToHexString(byte[] bytes)
        {
            var hexstring = BitConverter.ToString(bytes);
            return hexstring.Replace("-", "");
        }

        public static byte[] HexStringToBytes(string hexstring)
        {
            var NumberChars = hexstring.Length;
            var bytes = new byte[NumberChars / 2];
            for (var i = 0; i < NumberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexstring.Substring(i, 2), 16);
            }

            return bytes;
        }
    }
}