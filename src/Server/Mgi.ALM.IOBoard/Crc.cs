namespace Mgi.ALM.IOBoard
{
    public class Crc
    {
        static ushort[] crcTable = new ushort[256];
        static ushort POLYNOMIAL = 0x8005;
        static ushort INITIAL_REMAINDER = 0x0000;
        static ushort FINAL_XOR_VALUE = 0x0000;
        static ushort WIDTH = (8 * sizeof(ushort));
        static ushort TOPBIT = (ushort)(1 << (WIDTH - 1));

        static bool init = false;

        /// <summary>
        /// Reorder the bits of a binary sequence, by reflecting them about the middle position
        /// </summary>
        /// <remarks>
        /// No checking is done that nBits <= 32
        /// </remarks>
        /// <param name="data"></param>
        /// <param name="nBits"></param>
        /// <returns>The reflection of the original data</returns>
        static ulong reflect(ulong data, byte nBits)
        {
            ulong reflection = 0x00000000;
            byte bit;

            // Reflect the data about the center bit.
            for (bit = 0; bit < nBits; ++bit)
            {
                // If the LSB bit is set, set the reflection of it.
                if ((data & 0x01) != 0)
                {
                    reflection |= ((ulong)1 << ((nBits - 1) - bit));
                }

                data = (data >> 1);
            }

            return reflection;
        }

        /// <summary>
        /// Compute the CRC of a given message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="nBytes"></param>
        /// <returns>The CRC of the message</returns>
        public static ushort crcSlow(byte[] message, int nBytes)
        {
            ushort remainder = INITIAL_REMAINDER;
            int byteIndex;
            byte bit;

            // Perform modulo-2 division, a byteIndex at a time.
            for (byteIndex = 0; byteIndex < nBytes; ++byteIndex)
            {
                // Bring the next byteIndex into the remainder.
                remainder ^= (ushort)((byte)reflect(message[byteIndex], 8) << (WIDTH - 8));

                // Perform modulo-2 division, a bit at a time.
                for (bit = 8; bit > 0; --bit)
                {
                    /*
                     * Try to divide the current data bit.
                     */
                    if ((remainder & TOPBIT) != 0)
                    {
                        remainder = (ushort)((remainder << 1) ^ POLYNOMIAL);
                    }
                    else
                    {
                        remainder = (ushort)(remainder << 1);
                    }
                }
            }

            // The final remainder is the CRC result.
            return (ushort)((ushort)reflect(remainder, (byte)WIDTH) ^ FINAL_XOR_VALUE);

        }

        /// <summary>
        /// Populate the partial CRC lookup table.
        /// </summary>
        /// <remarks>
        /// This function must be rerun any time the CRC standard is changed. 
        /// If desired, it can be run "offline" and the table results stored in a ROM.
        /// </remarks>
        public static void crcInit()
        {
            ushort remainder;
            int dividend;
            byte bit;

            if (init == true)
                return;

            // Compute the remainder of each possible dividend.
            for (dividend = 0; dividend < 256; ++dividend)
            {
                // Start with the dividend followed by zeros.
                remainder = (ushort)(dividend << (WIDTH - 8));

                // Perform modulo-2 division, a bit at a time.
                for (bit = 8; bit > 0; --bit)
                {
                    // Try to divide the current data bit.
                    if ((remainder & TOPBIT) != 0)
                    {
                        remainder = (ushort)((ushort)(remainder << 1) ^ POLYNOMIAL);
                    }
                    else
                    {
                        remainder = (ushort)(remainder << 1);
                    }
                }

                // Store the result into the table.
                crcTable[dividend] = remainder;
            }

            init = true;
        }

        /// <summary>
        /// Compute the CRC of a given message.
        /// </summary>
        /// <remarks>
        /// crcInit() must be called first.
        /// </remarks>
        /// <param name="message"></param>
        /// <param name="nBytes"></param>
        /// <returns>The CRC of the message.</returns>
        public static ushort crcFast(byte[] message, int nBytes)
        {
            ushort remainder = INITIAL_REMAINDER;
            byte data;
            int byteIndex;

            // Divide the message by the polynomial, a byteIndex at a time.
            for (byteIndex = 0; byteIndex < nBytes; ++byteIndex)
            {
                data = (byte)(reflect(message[byteIndex], 8) ^ (ulong)(remainder >> (WIDTH - 8)));
                remainder = (ushort)(crcTable[data] ^ (remainder << 8));
            }

            // The final remainder is the CRC.
            return (ushort)(reflect(remainder, (byte)WIDTH) ^ FINAL_XOR_VALUE);
        }
    }
}
