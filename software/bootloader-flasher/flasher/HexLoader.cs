using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bootloader
{
    class HexLoader
    {

        List<Processor> clsProcessors;
        private Processor clsCurrentProcessor;
        private int iInternalIndex;

        public HexLoader() {
            clsProcessors = new List<Processor>();
            clsProcessors.Add(new Processor(0,0,0,0));
            clsProcessors.Add(new Processor(0x00F00000, 0x10000, 0x00300000, 64));
            vReset(0);
        }

        public async Task LoadHex(string strFilename, int iProcessorNumber) {

            await ReadTextAsync(strFilename, iProcessorNumber);

        }

        public void vReset(int iProcessorNumber) {
            iInternalIndex = 0;
            clsCurrentProcessor = clsProcessors.ElementAt(iProcessorNumber);
        }

   

        public bool FileLoaded(int iProcessorNumber) {
            return clsProcessors.ElementAt(iProcessorNumber).GetTotalEntries() > 0;
        }

        public int GetEntriesCountFlash(int iProcessorNumber)
        {
            return clsProcessors.ElementAt(iProcessorNumber).clsFlash.Count;
        }

        public int GetEntriesCountEeprom(int iProcessorNumber)
        {
            return clsProcessors.ElementAt(iProcessorNumber).clsFlash.Count;
        }

        public Processor GetProcessor(int iProcessorNumber)
        {
            return clsProcessors.ElementAt(iProcessorNumber);
        }

        public int GetEntriesCountConfig(int iProcessorNumber)
        {
            return clsProcessors.ElementAt(iProcessorNumber).clsConfig.Count;
        }

        public HexLine PeekNextLine(int iMemoryType) {
            HexLine hexline = null;
            try
            {
                switch (iMemoryType)
                {
                    case 0:
                        hexline = clsCurrentProcessor.clsFlash.ElementAt(iInternalIndex);
                        break;
                    case 1:
                        hexline = clsCurrentProcessor.clsEeprom.ElementAt(iInternalIndex);
                        break;
                    case 2:
                        hexline = clsCurrentProcessor.clsConfig.ElementAt(iInternalIndex);
                        break;
                }
            }
            catch (Exception ex) { 
            
            }


            return hexline;
        }

        public HexLine GetNextLine(int iMemoryType) {
            HexLine hexline = PeekNextLine(iMemoryType);
            ++iInternalIndex;
            return hexline;
        }

        // Optimize if loading file can be partitioned into await tasks
        async Task ReadTextAsync(string filePath, int iProcessorNumber)
        {
            try
            {
                clsCurrentProcessor = clsProcessors.ElementAt(iProcessorNumber);
                clsCurrentProcessor.Undefine();


                // Open the text file using a stream reader.
                using (var sr = new StreamReader(filePath))
                {
                    string strLine;
                    long  lAddress = 0;
                    long lOffset = 0;
                    while ((strLine = sr.ReadLine()) != null)
                    {
                        // Valid Line
                        if (strLine[0] == ':')
                        {
                            byte[] bLine = Helper.ConvertHexStringToByteArray(strLine.Substring(1, strLine.Length-1));
                            int iLength = bLine[0];

                            // Is Data: Need to optimize this method using Span and Memory to avoid allocating memory twice
                            if (iLength > 0 && bLine[3] == 0x00)
                            {
                                lAddress = ((long)bLine[1] & 0x0000FF) << 8 | (long)(bLine[2] & 0x0000FF) + lOffset;
                                byte[] bArray = new byte[iLength];
                                for (int i = 0; i < iLength; ++i) {
                                    bArray[i] = bLine[4 + i];
                                }

                                clsCurrentProcessor.AddHexLine(new HexLine(lAddress, bArray));

                            }
                            // Add offset
                            else if(iLength > 0 && bLine[3] == 0x04) {
                                lOffset = ((long)bLine[4] << 24) | ((long)bLine[5] << 16);
                            }
                            // Ignore everything else
                            else
                            {

                            }
                        }
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
    
          
        }
    }

    public class Processor {
        public List<HexLine> clsFlash;
        public List<HexLine> clsConfig;
        public List<HexLine> clsEeprom;

        private long lAddressEepromStart;
        private long lAddressEepromEnd;
        private long lAddressConfigStart;
        private long lAddressConfigEnd;

        public Processor(long lAddressEeprom, long lEepromSize,  long lAddressConfig, long lConfigSize)
        {
            clsFlash = new List<HexLine>();
            clsEeprom = new List<HexLine>();
            clsConfig = new List<HexLine>();

            if (lAddressEeprom > 0)
            {
                this.lAddressEepromStart = lAddressEeprom;
                this.lAddressEepromEnd = lAddressEeprom + lEepromSize;
         
            }

            if (lAddressConfig > 0)
            {
                this.lAddressConfigStart = lAddressConfig;
                this.lAddressConfigEnd = lAddressConfig + lConfigSize;
    
            }
        }

        public void Undefine() {
            clsFlash.Clear();
            clsEeprom.Clear();
            clsConfig.Clear();
        }

        public long GetTotalEntries() {
            return clsFlash.Count + clsEeprom.Count + clsConfig.Count;
        }

        public void AddHexLine(HexLine hexline)
        {
            if (lAddressEepromEnd >0 && lAddressEepromStart <= hexline.lAddress && hexline.lAddress < lAddressEepromEnd)
            {
                clsEeprom.Add(hexline);
            }
            else if (lAddressConfigEnd>0 && lAddressConfigStart <= hexline.lAddress && hexline.lAddress < lAddressConfigEnd)
            {
                clsConfig.Add(hexline);
            }
            else
            {
                clsFlash.Add(hexline);
            }
        }
    }


    public class HexLine {

        public long lAddress;
        public byte[] arrbArray;
        public HexLine(long lAddress, byte[] arrbArray)
        {
            this.lAddress = lAddress;
            this.arrbArray = arrbArray;
        }
    }
}
