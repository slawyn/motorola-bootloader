using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bootloader
{
    class Helper
    {
        private static readonly ushort[] arrusCrcTable = {
                    0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50A5, 0x60C6, 0x70E7, 0x8108, 0x9129, 0xA14A, 0xB16B, 0xC18C, 0xD1AD, 0xE1CE, 0xF1EF,
                    0x1231, 0x0210, 0x3273, 0x2252, 0x52B5, 0x4294, 0x72F7, 0x62D6, 0x9339, 0x8318, 0xB37B, 0xA35A, 0xD3BD, 0xC39C, 0xF3FF, 0xE3DE,
                    0x2462, 0x3443, 0x0420, 0x1401, 0x64E6, 0x74C7, 0x44A4, 0x5485, 0xA56A, 0xB54B, 0x8528, 0x9509, 0xE5EE, 0xF5CF, 0xC5AC, 0xD58D,
                    0x3653, 0x2672, 0x1611, 0x0630, 0x76D7, 0x66F6, 0x5695, 0x46B4, 0xB75B, 0xA77A, 0x9719, 0x8738, 0xF7DF, 0xE7FE, 0xD79D, 0xC7BC,
                    0x48C4, 0x58E5, 0x6886, 0x78A7, 0x0840, 0x1861, 0x2802, 0x3823, 0xC9CC, 0xD9ED, 0xE98E, 0xF9AF, 0x8948, 0x9969, 0xA90A, 0xB92B,
                    0x5AF5, 0x4AD4, 0x7AB7, 0x6A96, 0x1A71, 0x0A50, 0x3A33, 0x2A12, 0xDBFD, 0xCBDC, 0xFBBF, 0xEB9E, 0x9B79, 0x8B58, 0xBB3B, 0xAB1A,
                    0x6CA6, 0x7C87, 0x4CE4, 0x5CC5, 0x2C22, 0x3C03, 0x0C60, 0x1C41, 0xEDAE, 0xFD8F, 0xCDEC, 0xDDCD, 0xAD2A, 0xBD0B, 0x8D68, 0x9D49,
                    0x7E97, 0x6EB6, 0x5ED5, 0x4EF4, 0x3E13, 0x2E32, 0x1E51, 0x0E70, 0xFF9F, 0xEFBE, 0xDFDD, 0xCFFC, 0xBF1B, 0xAF3A, 0x9F59, 0x8F78,
                    0x9188, 0x81A9, 0xB1CA, 0xA1EB, 0xD10C, 0xC12D, 0xF14E, 0xE16F, 0x1080, 0x00A1, 0x30C2, 0x20E3, 0x5004, 0x4025, 0x7046, 0x6067,
                    0x83B9, 0x9398, 0xA3FB, 0xB3DA, 0xC33D, 0xD31C, 0xE37F, 0xF35E, 0x02B1, 0x1290, 0x22F3, 0x32D2, 0x4235, 0x5214, 0x6277, 0x7256,
                    0xB5EA, 0xA5CB, 0x95A8, 0x8589, 0xF56E, 0xE54F, 0xD52C, 0xC50D, 0x34E2, 0x24C3, 0x14A0, 0x0481, 0x7466, 0x6447, 0x5424, 0x4405,
                    0xA7DB, 0xB7FA, 0x8799, 0x97B8, 0xE75F, 0xF77E, 0xC71D, 0xD73C, 0x26D3, 0x36F2, 0x0691, 0x16B0, 0x6657, 0x7676, 0x4615, 0x5634,
                    0xD94C, 0xC96D, 0xF90E, 0xE92F, 0x99C8, 0x89E9, 0xB98A, 0xA9AB, 0x5844, 0x4865, 0x7806, 0x6827, 0x18C0, 0x08E1, 0x3882, 0x28A3,
                    0xCB7D, 0xDB5C, 0xEB3F, 0xFB1E, 0x8BF9, 0x9BD8, 0xABBB, 0xBB9A, 0x4A75, 0x5A54, 0x6A37, 0x7A16, 0x0AF1, 0x1AD0, 0x2AB3, 0x3A92,
                    0xFD2E, 0xED0F, 0xDD6C, 0xCD4D, 0xBDAA, 0xAD8B, 0x9DE8, 0x8DC9, 0x7C26, 0x6C07, 0x5C64, 0x4C45, 0x3CA2, 0x2C83, 0x1CE0, 0x0CC1,
                    0xEF1F, 0xFF3E, 0xCF5D, 0xDF7C, 0xAF9B, 0xBFBA, 0x8FD9, 0x9FF8, 0x6E17, 0x7E36, 0x4E55, 0x5E74, 0x2E93, 0x3EB2, 0x0ED1, 0x1EF0};
        private static readonly char[] arrHexCharacters = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };



        public static byte[] ConvertHexStringToByteArray(string sText)
        {
            if (sText.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", sText));
            }

            byte[] arrbData = new byte[sText.Length / 2];
            for (int iIndex = 0; iIndex < arrbData.Length; iIndex++)
            {
                string sByteValue = sText.Substring(iIndex * 2, 2);
                arrbData[iIndex] = byte.Parse(sByteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return arrbData;
        }


        public static string ConvertByteArrayToHexString(byte[] arrbBuffer, int iOffset, int iCount)
        {
            if (iCount >= 0)
            {
                StringBuilder clsHex = new StringBuilder(iCount * 2);
                for (int i = 0; i < (iCount); i++)
                {
                    byte b = arrbBuffer[iOffset + i];
                    int iHigh = ((b & 0xf0) >> 4);
                    int iLow = (b & 0x0f);
                    clsHex.Append(arrHexCharacters[iHigh]);
                    clsHex.Append(arrHexCharacters[iLow]);
                    clsHex.Append(" ");
                }
                return clsHex.ToString();

            }
            return "";
        }

        public static string ConvertByteArrayToAsciiString(byte[] arrbBuffer, int iOffset, int iCount)
        {
            StringBuilder clsBuffer = new StringBuilder(iCount);

            for (int i = 0; i < iCount; i++)
            {
                clsBuffer.Append((char)arrbBuffer[iOffset + i]);
            }
            return clsBuffer.ToString();
        }

        public static int PrepareEepromBufferPIC18F46K22(byte[] arrbBuffer, long lAddress, byte[] arrbData, long lOffset) {

            int iChecksum = 0;
            int iIndex;

            arrbBuffer[0] = (byte)(lAddress >> 16);
            arrbBuffer[1] = (byte)(lAddress >> 8);
            arrbBuffer[2] = (byte)(lAddress);

      
            arrbBuffer[3] = (byte)4;
            arrbBuffer[4] = (byte)(1 + 1);

            // Offset
            if (lOffset < arrbData.Length)
            {
                arrbBuffer[5] = arrbData[lOffset];
            }
            else {
                arrbBuffer[5] = 0xFF;

            }


            for (iIndex = 0; iIndex < 6; ++iIndex)
            {
                iChecksum += arrbBuffer[iIndex];
            }

            arrbBuffer[6] = (byte)(0x00FF & (-iChecksum));
            return 2 + 5;

        }

        public static int PrepareConfigurationBufferPIC18F46K22(byte[] arrbBuffer, long lAddress, byte bConfig)
        {

            int iChecksum = 0;
            int iIndex;

            arrbBuffer[0] = (byte)(lAddress >> 16);
            arrbBuffer[1] = (byte)(lAddress >> 8);
            arrbBuffer[2] = (byte)(lAddress);

            arrbBuffer[3] = (byte)8;
            arrbBuffer[4] = (byte)(1 + 1);
            arrbBuffer[5] = bConfig;
  
            for (iIndex = 0; iIndex < 6; ++iIndex)
            {
                iChecksum += arrbBuffer[iIndex];
            }

            arrbBuffer[6] = (byte)(0x00FF & (-iChecksum));
            return 2 + 5;

        }




        public static int PrepareFlashBufferPIC18F46K22(byte[] arrbBuffer, long lAddress, byte[] arrbData)
        {
            int iChecksum = 0;
            int iIndex;

            arrbBuffer[0] = (byte)(lAddress >> 16);
            arrbBuffer[1] = (byte)(lAddress >> 8);
            arrbBuffer[2] = (byte)(lAddress);


            arrbBuffer[3] = (byte)(2);
            arrbBuffer[4] = (byte)(arrbData.Length + 1);


            // Fill Array with Data, Calculate part of the checksum
            for (iIndex = 0; iIndex < arrbData.Length; ++iIndex)
            {
                arrbBuffer[5 + iIndex] = arrbData[iIndex];
                iChecksum += arrbData[iIndex];
            }

            for (iIndex = 0; iIndex < 5; ++iIndex)
            {
                iChecksum += arrbBuffer[iIndex];
            }

            arrbBuffer[5 + arrbData.Length] = (byte)(0x00FF & (-iChecksum));
            return arrbData.Length + 5 + 1;
        }

 

        public static int PrepareEraseBufferPIC18F46K22(byte[] arrbBuffer, long lAddress)
        {

            int iChecksum = 0;
            int iIndex;

            arrbBuffer[0] = (byte)(lAddress >> 16);
            arrbBuffer[1] = (byte)(lAddress >> 8);
            arrbBuffer[2] = (byte)(lAddress);

            arrbBuffer[3] = (byte)(1);
            arrbBuffer[4] = (byte)(1);

            for (iIndex = 0; iIndex < 5; ++iIndex)
            {
                iChecksum += arrbBuffer[iIndex];
            }
            arrbBuffer[5] = (byte)(0x00FF & (-iChecksum));

            return 6;

        }

        private static byte[] CrcXmodemCcitt(byte[] arrbBytes, int iOffset, int iEnd)
        {

            const ushort usInitialValue = 0x00;
            ushort usCrc = usInitialValue;
         
            for (int i = iOffset; i < iEnd; ++i)
            {
                usCrc = (ushort)((usCrc << 8) ^ arrusCrcTable[((usCrc >> 8) ^ (0xff & arrbBytes[i]))]);
            }

            return BitConverter.GetBytes(usCrc);
        }

        public static void Memset(byte[] arrbBuffer, byte bData) {
            for (long lIndex = 0; lIndex < arrbBuffer.Length; ++lIndex)
            {
                arrbBuffer[lIndex] = bData;
            }
        }

        public static int PrepareErasingBufferMC9S08AC60(byte[] arrbBuffer) {
            arrbBuffer[0] = 2;
            arrbBuffer[1] = 0xBF;
            return 2;
        }

        public static int PrepareBlankCheckBufferMC9S08AC60(byte[] arrbBuffer)
        {
            arrbBuffer[0] = 2;
            arrbBuffer[1] = 0xBE;
            return 2;
        }

        public static int PrepareUnsecuringBufferMC9S08AC60(byte[] arrbBuffer)
        {
            arrbBuffer[0] = 2;
            arrbBuffer[1] = 0xBD;
            return 2;
        }

        public static int PrepareProgrammingBufferMC9S08AC60(byte[] arrbBuffer, long lAddress, int iLength, byte[] arrbData)
        {


            arrbBuffer[0] = (byte)(iLength + 5);
            arrbBuffer[1] = 0xCF;
            arrbBuffer[2] = (byte)(lAddress >> 8);
            arrbBuffer[3] = (byte)(lAddress);
            arrbBuffer[4] = (byte)(iLength);

            for (int iIndex = 0; iIndex < iLength; ++iIndex)
            {
                arrbBuffer[5 + iIndex] = arrbData[iIndex];
            }

            return iLength + 5;
        }

        public static int PrepareConnectMC9S08AC60(byte[] arrbBuffer) {
            arrbBuffer[0] = (byte)'B';
            arrbBuffer[1] = (byte)'L';
            arrbBuffer[2] = 7;
            arrbBuffer[3] = (byte)'B';
            arrbBuffer[4] = (byte)'O';
            arrbBuffer[5] = (byte)'O';
            arrbBuffer[6] = (byte)'T';
            arrbBuffer[7] = (byte)'M';
            arrbBuffer[8] = (byte)'E';
            arrbBuffer[9] = (byte)'!';

            byte[] arrbCrc16 = CrcXmodemCcitt(arrbBuffer, 3, 10);
            arrbBuffer[10] = arrbCrc16[1];
            arrbBuffer[11] = arrbCrc16[0];
            return 12;
        }

        public static int PrepareEchoMC9S08AC60(byte[] arrbBuffer)
        {
            arrbBuffer[0] = 6;
            arrbBuffer[1] = 0xFE;
            arrbBuffer[2] = (byte)'E';
            arrbBuffer[3] = (byte)'C';
            arrbBuffer[4] = (byte)'H';
            arrbBuffer[5] = (byte)'O';
            return 6;
        }
    }
}
