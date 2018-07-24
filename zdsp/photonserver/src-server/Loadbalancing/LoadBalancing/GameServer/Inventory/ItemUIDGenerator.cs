using System;
using System.Text;

namespace Zealot.Server.Inventory
{
    /// <summary>
    /// helper class to generate item UID
    /// <remarks>
    /// this algorithm assumes that a player will not generate more than 8100 UIDs in a second, else there will be repetition
    /// </remarks>
    /// </summary>
    public class ItemUIDGenerator
    {
        private string peerID;
        private int UID_SERIAL = 0;
	    
        private const byte ASCII_MAX = 90;
        private const byte ASCII_Start = 33;	//char "!"
        private const int MAX_UID_SERIAL = 8100;
        private const byte CONNID_BITS = 20;

	    //number of ascii chars to fit 31bits for datetime
        private const int INT_ASCII_CHAR = 5;

        public ItemUIDGenerator(int serverid, int connectionid)
        {
            int maxconn = (1 << CONNID_BITS) - 1;
            if (connectionid > maxconn)
                throw new ArgumentOutOfRangeException("connectionid", "connectionid is greater than 2^20");

            peerID = GetEncodedPeerId(serverid, connectionid);
        }

        /// <summary>
        /// Generates a 12 char ascii encoded string
        /// </summary>
        public string GenerateItemUID()
        {
            StringBuilder sb = new StringBuilder();
            
            AppendEncodedDateTime(sb);
            AppendEncodedSerial(sb);
            sb.Append(peerID);

            return sb.ToString();
        }
        
        /// <summary>
        /// Decodes and returns multiple item generation param from item UID
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="serverID">returns server ID from UID</param>
        /// /// <param name="serverID">returns peer connection ID from UID</param>
        /// <param name="serialNum">returns generation serial number from UID</param>
        /// <param name="datetime">returns datetime from UID</param>
        /// <returns></returns>
        public static void DecodeItemUID(string uid, out int serverID, out int connectionID, out int serialNum, out DateTime datetime)
        {
            string dtascii = uid.Substring(0,5);
            string serialchars = uid.Substring(5, 2);
            string peerIDchars = uid.Substring(7, 5);

            datetime = DecodeDTAscii(dtascii);
            serialNum = DecodeAscii(serialchars);

            int numPeerID = DecodeAscii(peerIDchars);
            int connIDMask = (1 << CONNID_BITS) - 1;
            
            serverID = numPeerID >> CONNID_BITS;
            connectionID = numPeerID & connIDMask;
        }

        #region Encoding helpers
        private void AppendEncodedSerial(StringBuilder sb)
        {
            sb.Append(GenerateAscii(UID_SERIAL / ASCII_MAX));
            sb.Append(GenerateAscii(UID_SERIAL % ASCII_MAX));

            UID_SERIAL++;
            if (UID_SERIAL > MAX_UID_SERIAL)
                UID_SERIAL = 0;
        }

        private void AppendEncodedDateTime(StringBuilder sb)
        {
            //31 bits
		    //datetime = yyyyymmmmdddddhhhhhmmmmmmssssss
		
		    DateTime now = DateTime.Now;	//0-60
		    int mins = now.Minute << 6;	    //0-60
            int hours = now.Hour << 12;	    //0-23
            int day = now.Day << 17;		//1-31
            int month = now.Month << 22;	//1-12
            int year = (now.Year - 2000) << 26;	//1-99, -2000 is faster than %100

		    int nDateTime = year + month + day + mins + hours + now.Second;

            IntToAscii(nDateTime, sb);
        }

        private string GetEncodedPeerId(int serverID, int connectionID)
        {
            StringBuilder sb = new StringBuilder();

            int encodedPeerID = (serverID << CONNID_BITS) + connectionID;
            IntToAscii(encodedPeerID, sb);

            return sb.ToString();
        }
        #endregion

        #region Ascii Helpers
        private static char GenerateAscii(int number)
        {
            if (number < ASCII_MAX)
            {
                char c = (char)(number + ASCII_Start);
                return c;
            }
            else
            {
                throw new ArgumentOutOfRangeException("number", "number is greater than ASCII range");
            }
        }

        private static void IntToAscii(int number, StringBuilder sb)
        {
            //encode ASCII
            for (int i = INT_ASCII_CHAR - 1; i > 0; i--)
            {
                int divider = (int)Math.Pow(ASCII_MAX, i);
                int charint = number / divider;

                number = number - divider * charint;

                sb.Append(GenerateAscii(charint));
            }

            sb.Append(GenerateAscii(number));
        }

        private static int DecodeAscii(string ascii)
        {
            int len = ascii.Length;
	        int res = 0;
	        for( int i = 0 ; i < len ; i++ )
	        {
		        char c = ascii[i];
                int multiple = (int)Math.Pow(ASCII_MAX, (len - i - 1));
		        res += multiple*((int)c - ASCII_Start);
	        }
	
	        return res;
        }

        private static DateTime DecodeDTAscii(string ascii)
        {
            int number = DecodeAscii(ascii);
		
		    int year = (number >> 26) & 31;
		    int month = (number >> 22) & 15;
		    int day = (number >> 17) & 31;
		    int hour = (number >> 12) & 31;
		    int min = (number >> 6) & 63;
		    int secs = number & 63;

            return new DateTime(year, month, day, hour, min, secs);
        }
        #endregion
    }
}
