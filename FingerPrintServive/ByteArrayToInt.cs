using System;

namespace FingerPrintService
{
    /// <summary>
    /// Byte[] ? int konverter, little/big endian támogatással.
    /// Tökéletes natív SDK-khoz (ZKTeco, USB, hálózati protokollok).
    /// </summary>
    public static class ByteIntConverter
    {
        /// <summary>
        /// Byte[] › int (alapértelmezés: little endian)
        /// </summary>
        public static int ToInt(byte[] bytes, bool bigEndian = false)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            if (bytes.Length != 4)
                throw new ArgumentException("A bemenetnek pontosan 4 byte hosszúnak kell lennie.", nameof(bytes));

            byte[] tmp = (byte[])bytes.Clone();

            if (bigEndian)
                Array.Reverse(tmp);         // big › little

            return BitConverter.ToInt32(tmp, 0);
        }

        /// <summary>
        /// int › byte[] (alapértelmezés: little endian)
        /// </summary>
        public static byte[] FromInt(int value, bool bigEndian = false)
        {
            byte[] bytes = BitConverter.GetBytes(value);

            if (bigEndian)
                Array.Reverse(bytes);       // little › big

            return bytes;
        }
    }
}