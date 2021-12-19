

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Bootloader {
    enum eState { eStateInit = 0, eStateTriggerBootloader, eStateWaitForBootloader, eStateWaitForEcho, eStateConnectedMC9S08AC60, eStateErase, eStateProgram, eStateBridge, eStateProgramConfigurationPIC18F46K22,eStateWaitForPIC18F46K22, eStateProgramEepromPIC18F46K22, eStateConnectedPIC18F46K22, eStateProgramPIC18F46K22, eStateConfirmPIC18F46K22 }

    public partial class Gui : Form
    {

        private System.Windows.Forms.Timer formTimer;
        SerialPort clsSerialPort;
        Thread clsCommunicationThread;
		HexLoader clsHexLoader;

        ConcurrentQueue<LogObject> clsLogBuffer;
        ConcurrentQueue<byte[]> clsManualInputBuffer;

        StringBuilder clsLoggerBuilder;
        eState iCommunicatorState;

        public Gui()
        {   
            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            this.formTimer = new System.Windows.Forms.Timer();
            this.formTimer.Tick += new EventHandler(HandlerTimerTick);
            this.formTimer.Interval = 17;
        }

        [DllImport("msvcrt.dll", EntryPoint = "memcmp", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(byte[] b1, byte[] b2, long count);

        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl)]
        static extern int memset(byte[] b1, long value, long count);

        private void HandlerOnFormLoad(object sender, EventArgs e)
        {

            cboxUpdateList();
            cboxShowCommunication.Checked = false;
            cboxProgramConfiguration.Checked = true;
            clsManualInputBuffer = new ConcurrentQueue<byte[]>();
            clsLogBuffer = new ConcurrentQueue<LogObject>();
            clsSerialPort = new SerialPort();
			clsHexLoader = new HexLoader();
            clsLoggerBuilder = new StringBuilder(100);

            this.formTimer.Start();
        }


        private void cboxUpdateList() {
            // Get a list of serial port names.
            string[] ports = SerialPort.GetPortNames();
            Array.Sort(ports);

            // Display each port name to the console.
            this.cboxComs.Items.Clear();
            if (ports.Length > 0) {
                foreach (string port in ports)
                {
                    this.cboxComs.Items.Add(port);
                }
                this.cboxComs.SelectedIndex = 0;
            }
        }

        private void HandlerTimerTick(object sender, EventArgs e)
        {
            WriteChunk();
        }

        private void WriteChunk()
        {
            int iStart = Environment.TickCount;
            clsLoggerBuilder.Length = 0;
            do
            {
                if (clsLogBuffer.TryDequeue(out LogObject clsLogResult))
                {
                    switch (clsLogResult.iType)
                    {
                        /* */
                        case 0:
                            if (cboxShowCommunication.Checked)
                            {
                                RichTextBoxExtensions.AppendText(tboxOutput, clsLogResult.strTimestamp, Color.Tomato, Color.White);
                                RichTextBoxExtensions.AppendText(tboxOutput, clsLogResult.strLogLine, Color.DarkBlue, Color.PaleVioletRed);
                                RichTextBoxExtensions.AppendText(tboxOutput, Environment.NewLine, Color.DarkBlue, Color.White);
                            }
                            break;
                        case 1:
                            if (cboxShowCommunication.Checked)
                            {
                                RichTextBoxExtensions.AppendText(tboxOutput, clsLogResult.strTimestamp, Color.Tomato, Color.White);
                                RichTextBoxExtensions.AppendText(tboxOutput, clsLogResult.strLogLine, Color.DarkBlue, Color.LimeGreen);
                                RichTextBoxExtensions.AppendText(tboxOutput, Environment.NewLine, Color.DarkBlue, Color.White);
                            }
                            break;
                        case 2:
                            RichTextBoxExtensions.AppendText(tboxOutput, clsLogResult.strTimestamp, Color.Tomato, Color.White);
                            RichTextBoxExtensions.AppendText(tboxOutput, clsLogResult.strLogLine, Color.Red, Color.White);
                            RichTextBoxExtensions.AppendText(tboxOutput, Environment.NewLine, Color.DarkBlue, Color.White);
                            break;
                        case 3:
                            RichTextBoxExtensions.AppendText(tboxOutput, clsLogResult.strTimestamp, Color.Tomato, Color.White);
                            RichTextBoxExtensions.AppendText(tboxOutput, clsLogResult.strLogLine, Color.Gray, Color.White);
                            RichTextBoxExtensions.AppendText(tboxOutput, Environment.NewLine, Color.DarkBlue, Color.White);
                            break;
                    }
                }else {
                    break;
                }
            } while (Environment.TickCount - iStart < 17);
        }




        public int GetRequiredToSend(int iIndex, int iSendAmount, SerialPort clsSerialPort) {
            int iTransmitCount = clsSerialPort.BytesToWrite;
            if (iTransmitCount >0) { 
                return 0;
            }

            return iSendAmount - iIndex;

        }

        public int GetAvailableToReceive(int iIndex, SerialPort clsSerialPort) {
            int iReceiveCount = clsSerialPort.BytesToRead;
            if (iIndex + iReceiveCount < 32)
                return iReceiveCount;
            return 0;
        }

        public void PrintTextIncoming(byte[] Buffer, int lOffset, int iLength) {

            string timestamp = DateTime.UtcNow.ToString("HH:mm:ss.fff    ", CultureInfo.InvariantCulture);
            clsLogBuffer.Enqueue(new LogObject(timestamp, Helper.ConvertByteArrayToHexString(Buffer, lOffset, iLength), 0));
        }

        public void PrintTextOutgoing(byte[] Buffer, int lOffset, int iLength)
        {
            string timestamp = DateTime.UtcNow.ToString("HH:mm:ss.fff    ", CultureInfo.InvariantCulture);
            clsLogBuffer.Enqueue(new LogObject(timestamp, Helper.ConvertByteArrayToHexString(Buffer, lOffset, iLength), 1));
        }


        public void PrintErrorText(string sText)
        {
            string timestamp = DateTime.UtcNow.ToString("HH:mm:ss.fff    ", CultureInfo.InvariantCulture);
            clsLogBuffer.Enqueue(new LogObject(timestamp, sText, 2));
        }

        public void PrintText(string sText)
        {
            string timestamp = DateTime.UtcNow.ToString("HH:mm:ss.fff    ", CultureInfo.InvariantCulture);
            clsLogBuffer.Enqueue(new LogObject(timestamp, sText, 3));
        }


        public void Communicate()
        {
            byte[] arrbReceiveBuffer = new byte[128];
            byte[] arrbTransmitBuffer = new byte[128];
            byte[] arrbByteLine = new byte[64];
            int iRxIndex = 0;
            int iTxIndex = 0;
            int iSendAmount = 0;
            int iTimeout = 0;
            int iSubstate = 0;
            long lAddress = 0;
            long lIndex;
            long lOffset = 0;
            eState iPreviousState = eState.eStateInit;
            HexLine clsHexLine;



   
            iCommunicatorState = eState.eStateInit;
            //iCommunicatorState = eState.eStateConnectedPIC18F46K22;
            while (clsSerialPort.IsOpen)
            {
                // Reset RxIndex when timeout occurs
                if (iTimeout > 0 && --iTimeout == 0)
                {
                    iRxIndex = 0;
                }                

               
                try
                {
                    // State machine
                    switch (iCommunicatorState)
                    {

                        case eState.eStateTriggerBootloader:
                            PrintText("Triggering MC9S08AC60 bootloader...");
                            iSendAmount = Helper.PrepareConnectMC9S08AC60(arrbTransmitBuffer);
                            iCommunicatorState = eState.eStateWaitForBootloader;
                            break;
                        case eState.eStateWaitForBootloader:
                            if (iRxIndex == 10 && arrbReceiveBuffer[2] == 5)
                            {

                                if (arrbReceiveBuffer[8] == 0x00)
                                {
                                    iCommunicatorState = eState.eStateInit;
                                    PrintErrorText("Bootloader wrong bootloader password");
                                }
                                else
                                {
                                    iCommunicatorState = eState.eStateConnectedMC9S08AC60;
                                    PrintText(String.Format("Connected to bootloader:{0}", Helper.ConvertByteArrayToAsciiString(arrbReceiveBuffer, 4, 5)));
                                }

                            }
                            else if (iTimeout == 0)
                            {
                                iSendAmount = Helper.PrepareEchoMC9S08AC60(arrbTransmitBuffer);
                                iCommunicatorState = eState.eStateWaitForEcho;
                                PrintText("Bootloader trigger timed out. Trying echo...");
                            }
                            break;
                        case eState.eStateWaitForEcho:
                            if (iRxIndex == 6 && memcmp(arrbTransmitBuffer, arrbReceiveBuffer, 6) == 0)
                            {
                                iCommunicatorState = eState.eStateConnectedMC9S08AC60;
                                PrintText(String.Format("Connected to bootloader:{0}", Helper.ConvertByteArrayToAsciiString(arrbReceiveBuffer, 2, 4)));
                            }
                            else if (iTimeout == 0) {
                                iCommunicatorState = eState.eStateInit;
                                PrintErrorText("Bootloader not available. Restart Device and retry..");
                            }
                            break;
                        case eState.eStateErase:
                            if (iSubstate == 0)
                            {
                                PrintText("Erasing MC9S08AC60...");
                                iSendAmount = Helper.PrepareErasingBufferMC9S08AC60(arrbTransmitBuffer);
                                ++iSubstate;
                            }
                            else
                            {
                                if (iRxIndex == 3 && arrbReceiveBuffer[0] == iRxIndex)
                                {

                                    if (arrbReceiveBuffer[2] == 0)
                                    {

                                        PrintText("Command executed successfully!");
                                        switch (iSubstate)
                                        {
                                            case 1:
                                                PrintText("Checking if flash is blank");
                                                iSendAmount = Helper.PrepareBlankCheckBufferMC9S08AC60(arrbTransmitBuffer);
                                                ++iSubstate;
                                                break;
                                            case 2:
                                                PrintText("Unsecuring flash");
                                                iSendAmount = Helper.PrepareUnsecuringBufferMC9S08AC60(arrbTransmitBuffer);
                                                ++iSubstate;
                                                break;
                                            case 3:
                                                PrintText("Flash ready for programming");
                                                iCommunicatorState = eState.eStateConnectedMC9S08AC60;
                                                break;
                                        }

                                    }
                                    else {
                                        PrintErrorText("Erase Command did not execute properly!");
                                        iCommunicatorState = eState.eStateInit;
                                    }
                                }
                                else if (iTimeout == 0)
                                {
                                    PrintErrorText(String.Format("Timeout!{0} Could not Erase", iSubstate));
                                    iCommunicatorState = eState.eStateInit;
                                }
                            }
                            break;
                        case eState.eStateProgram:
                            if (iSubstate == 0)
                            {
                                // Init address and index
                                PrintText("Programming MC9S08AC60...");
                                clsHexLoader.vReset(0);
                                lIndex = 0;
                                lAddress = clsHexLoader.PeekNextLine(0).lAddress;
                                ++iSubstate;
                            }

                            else if (iSubstate == 1)
                            {
                                // Send Frame
                                clsHexLine = clsHexLoader.PeekNextLine(0);
                                if (clsHexLine == null)
                                {
                                    PrintText("Programming MC9S08AC60 has completed!");
                                    iCommunicatorState = eState.eStateConnectedMC9S08AC60;
                                }
                                else
                                {


                                    // Send the Line out if next ons is outside of erased area
                                    // Send data if Index>0 and:
                                    //      * nothing more to write. next address is lower than current. index>=64. nextaddress is not current+index
                                    lIndex = 0;
                                    while (true)
                                    {
                                        clsHexLine = clsHexLoader.PeekNextLine(0);
                                        if (lIndex > 0 && ((clsHexLine == null) || (clsHexLine.lAddress < lAddress) || ((lIndex + clsHexLine.arrbArray.Length) > 64) || (clsHexLine.lAddress != lAddress + lIndex)))
                                        {
                                            //PrintText("Writing lIndex bytes into flash");
                                            iSendAmount = Helper.PrepareProgrammingBufferMC9S08AC60(arrbTransmitBuffer, lAddress, (int)lIndex, arrbByteLine);
                                            Helper.Memset(arrbByteLine, 0xFF);
                                            if (clsHexLine != null)
                                            {
                                                lAddress = clsHexLine.lAddress;
                                            }

                                            ++iSubstate;
                                            break;
                                        }
                                        else
                                        {
                                            clsHexLine = clsHexLoader.GetNextLine(0);
                                            clsHexLine.arrbArray.CopyTo(arrbByteLine, lIndex);
                                            lIndex += clsHexLine.arrbArray.Length;
                                        }
                                    }
                                }
                            }
                            else if (iSubstate == 2)
                            {
                                if (iRxIndex == 3 && arrbReceiveBuffer[0] == iRxIndex)
                                {
                                    if (arrbReceiveBuffer[2] == 0x00)
                                    {
                                        // Receive Frame
                                        --iSubstate;
                                    }
                                    else
                                    {
                                        PrintErrorText("Programming procedure encountered an error!");
                                        iCommunicatorState = eState.eStateConnectedMC9S08AC60;
                                    }
                                }
                                else if (iTimeout == 0)
                                {
                                    PrintErrorText(String.Format("Timeout!{0}Could not program", iSubstate));
                                    iCommunicatorState = eState.eStateWaitForBootloader;
                                }
                            }

                            break;
                        case eState.eStateBridge:
                            if (iSubstate == 0)
                            {
                                PrintText("Setting MC9S08AC60 into bridge-mode...");
                                arrbTransmitBuffer[0] = 2;
                                arrbTransmitBuffer[1] = 0xA0;
                                iSendAmount = 2;
                                ++iSubstate;
                            }
                            else if (iTimeout == 0)
                            {
                                PrintErrorText(String.Format("Timeout!{0} Could not bridge to PIC18F46K22. Restart Device...", iSubstate));
                                iCommunicatorState = eState.eStateWaitForBootloader;
                            }

                            else if (iSubstate == 1)
                            {
                                if (iRxIndex == 2 && arrbReceiveBuffer[0] == iRxIndex)
                                {
                                    PrintText("Connecting to PIC18F46K22 bootloader...");
                                    arrbTransmitBuffer[0] = 0xC1;
                                    iSendAmount = 1;
                                    ++iSubstate;

                                }

                            } else if (iSubstate == 2)
                            {
                                if (iRxIndex == 4 && arrbReceiveBuffer[3] == 'K') {
                                    PrintText("Connected to PIC18F46K22 bootloader");
                                    iCommunicatorState = eState.eStateConnectedPIC18F46K22;
                                }
                            }

                            break;
                        case eState.eStateConfirmPIC18F46K22:
                            if (iRxIndex == 1)
                            {

                                if (arrbReceiveBuffer[0] == 'N')
                                {
                                    PrintText(String.Format("PIC18F46K22 bootloader bad checksum at {0}", lAddress.ToString("X4")));
                                    iCommunicatorState = eState.eStateConnectedPIC18F46K22;
                                }
                                else if (arrbReceiveBuffer[0] == 'P')
                                {
                                    PrintText(String.Format("PIC18F46K22 bootloader reached protected area {0}", lAddress.ToString("X4")));
                                    iSubstate = 0;

                                    if (iPreviousState == eState.eStateProgramPIC18F46K22)
                                    {
                                        iPreviousState = eState.eStateProgramConfigurationPIC18F46K22;
                                    }
                                    else {
                                        iPreviousState = eState.eStateConnectedPIC18F46K22;
                                    }
                                }
                                else
                                {
                                    // if((arrbReceiveBuffer[0] == 'K'))
                                }
                                iCommunicatorState = iPreviousState;
                            }
                            else if (iTimeout == 0)
                            {
                                PrintText("PIC18F46K22 bootloader timed out!");
                                iCommunicatorState = eState.eStateConnectedPIC18F46K22;
                            }
                            break;
                        case eState.eStateProgramPIC18F46K22:
                            iPreviousState = iCommunicatorState;
                            switch (iSubstate)
                            {
                                case 0:
                                    PrintText("Programming Flash PIC18F46K22..");
                                    clsHexLoader.vReset(1);
                                    lAddress = 0;
                                    iSubstate = 1;
                                    break;

                                case 1:
                                    clsHexLine = clsHexLoader.PeekNextLine(0);

                                    // Check for end of flash
                                    if (clsHexLine == null)
                                    {
                                        PrintText("Done with Flash for PIC18F46K22");
                                        iCommunicatorState = eState.eStateProgramConfigurationPIC18F46K22;
                                        iSubstate = 0;
                                    }
                                    else
                                    {
                                        iPreviousState = iCommunicatorState;
                                        if (lAddress <= clsHexLine.lAddress)
                                        {
                                            //PrintText("Erasing 64 bytes of flash");
                                            iSendAmount = Helper.PrepareEraseBufferPIC18F46K22(arrbTransmitBuffer, lAddress);
                                            lAddress = 64 + lAddress;

                                            Helper.Memset(arrbByteLine, 0xFF);
                                            iCommunicatorState = eState.eStateConfirmPIC18F46K22;
                                        }
                                        else
                                        {
                                            while (true)
                                            {

                                                if (clsHexLine == null)
                                                {
                                                    //PrintText("Writing 64 bytes into flash");
                                                    iSendAmount = Helper.PrepareFlashBufferPIC18F46K22(arrbTransmitBuffer, (lAddress - 64), arrbByteLine);
                                                    iCommunicatorState = eState.eStateConfirmPIC18F46K22;
                                                    break;
                                                }
                                                else
                                                {

                                                    lOffset = (int)(64 - (lAddress - clsHexLine.lAddress));
                                                    if (lOffset + clsHexLine.arrbArray.Length > 64)
                                                    {
                                                        clsHexLine = null;
                                                    }
                                                    else
                                                    {
                                                        clsHexLine = clsHexLoader.GetNextLine(0);
                                                        clsHexLine.arrbArray.CopyTo(arrbByteLine, lOffset);
                                                        clsHexLine = clsHexLoader.PeekNextLine(0);

                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                            }
                            break;
                        case eState.eStateProgramConfigurationPIC18F46K22:
                            iPreviousState = iCommunicatorState;
                            switch (iSubstate) {
                                case 0:
                                    // Configuration bits
                                    if (cboxProgramConfiguration.Checked)
                                    {
                                        PrintText("Programming Configuration for PIC18F46K22...");
                                        clsHexLoader.vReset(1);
                                        lOffset = 0;
                                        iSubstate = 1;
                                    }
                                    // Skip
                                    else
                                    {
                                        iCommunicatorState = eState.eStateConnectedPIC18F46K22;
                                    }
                                    break;
                                case 1:
                                    clsHexLine = clsHexLoader.PeekNextLine(2);

                                    // Check for end 
                                    if (clsHexLine == null)
                                    {
                                        PrintText("Done with Configuration for PIC18F46K22");
                                        iSubstate = 0;
                                        iCommunicatorState = eState.eStateConnectedPIC18F46K22;
                                    }
                                    else
                                    {
                                        
                                        iSendAmount = Helper.PrepareConfigurationBufferPIC18F46K22(arrbTransmitBuffer, clsHexLine.lAddress + lOffset, clsHexLine.arrbArray[lOffset]);
                                        iCommunicatorState = eState.eStateConfirmPIC18F46K22;
                                        ++lOffset;

                                        // Switch to next line
                                        if (clsHexLine.arrbArray.Length <= lOffset)
                                        {
                                            lAddress = clsHexLine.lAddress + lOffset;
                                            clsHexLine = clsHexLoader.GetNextLine(2);
                                            lOffset = 0;
                                        }
                                    }
                                    break;
                             }
                            break;
                        case eState.eStateProgramEepromPIC18F46K22:
                            iPreviousState = iCommunicatorState;
                            switch (iSubstate)
                            {
                                case 0:
                                    PrintText("Programming Eeprom for PIC18F46K22...");
                                    clsHexLoader.vReset(1);
                                    iSubstate = 1;
                                    break;
                                case 1:
                                    clsHexLine = clsHexLoader.PeekNextLine(1);

                                    // Check for end of eeprom
                                    if (clsHexLine == null)
                                    {
                                        PrintText("Done with Eeprom for PIC18F46K22");
                                        iCommunicatorState = eState.eStateConnectedPIC18F46K22;
                                    }
                                    else
                                    {


                                        iSendAmount = Helper.PrepareEepromBufferPIC18F46K22(arrbTransmitBuffer, (clsHexLine.lAddress + lOffset), clsHexLine.arrbArray, lOffset);
                                        iCommunicatorState = eState.eStateConfirmPIC18F46K22;
                                        lOffset += 1;

                                        // Switch to next line
                                        if (clsHexLine.arrbArray.Length <= lOffset)
                                        {
                                            lAddress = clsHexLine.lAddress + lOffset;
                                            clsHexLine = clsHexLoader.GetNextLine(1);
                                            lOffset = 0;
                                        }

                                    }
                                    break;
                            }
                            break;
                        case eState.eStateConnectedPIC18F46K22:
                        case eState.eStateConnectedMC9S08AC60:
                            if (iSendAmount == 0 && clsManualInputBuffer.TryDequeue(out byte[] arrbManualInput))
                            {
                                arrbManualInput.CopyTo(arrbTransmitBuffer, 0);
                                iSendAmount = arrbManualInput.Length;
                            }
                            iSubstate = 0;
                            break;
                        case eState.eStateInit:
                        default:
                            iSubstate = 0;
                            break;
                    }

                    /* Send and Receive Buffer*/
                    int iBufferedRxCount = GetAvailableToReceive(iRxIndex, clsSerialPort);
                    if (iBufferedRxCount > 0)
                    {
                        clsSerialPort.Read(arrbReceiveBuffer, iRxIndex, iBufferedRxCount);
                        PrintTextIncoming(arrbReceiveBuffer, iRxIndex, iBufferedRxCount);
                        iRxIndex += iBufferedRxCount;
                    }

                    int iBufferedTxCount = GetRequiredToSend(iTxIndex, iSendAmount, clsSerialPort);
                    if (iBufferedTxCount > 0)
                    {
                      
                        PrintTextOutgoing(arrbTransmitBuffer, iTxIndex, iBufferedTxCount);
                        clsSerialPort.Write(arrbTransmitBuffer, iTxIndex, iBufferedTxCount);
                        iTxIndex += iBufferedTxCount;

                        iTimeout = 2000;                    // Timeout is set when something is sent and a reply is expected
                        if (iTxIndex == iSendAmount) {
                            iTxIndex = 0;
                            iSendAmount = 0;
                            iRxIndex = 0;
                        }
                    }
                   
                    if (iBufferedTxCount == 0 && iBufferedRxCount == 0)
                    {
                        Thread.Sleep(1);
                    }

                }
                catch (TimeoutException)
                {
                    iRxIndex = 0;
                    iTxIndex = 0;
                    iSendAmount = 0;
                    iSubstate = 0;
                }
            }
        }
		
		private async Task LoadFileAsync(int iProcessorNumber){
				OpenFileDialog clsDialog = new OpenFileDialog{  
					Title = "Browse Hex Files",  
			  
					CheckFileExists = true,  
					CheckPathExists = true,  
			  
					DefaultExt = "hex",  
					Filter = "hex files (*.hex)|*.hex",  
					FilterIndex = 2,  
					RestoreDirectory = true,  
			  
					ReadOnlyChecked = true,  
					ShowReadOnly = true  
				};

                clsDialog.Multiselect = false;
				if (clsDialog.ShowDialog() == DialogResult.OK)  
				{  
					await clsHexLoader.LoadHex(clsDialog.FileName, iProcessorNumber);
                    if (iProcessorNumber == 0)
                    {
                        lHexNameMC09S08AC60.Text = clsDialog.SafeFileName;
                    }
                    else {
                        lHexNamePIC18F46K22.Text = clsDialog.SafeFileName;
                    }
                    PrintText(String.Format("{0} loaded with {1} entries", clsDialog.FileName, clsHexLoader.GetProcessor(iProcessorNumber).GetTotalEntries()));
                    
				}
				
		}
		

        private void HandlerRefreshPorts(object sender, EventArgs e)
        {
            cboxUpdateList();
        }

        private void HandlerCleartText(object sender, EventArgs e)
        {
            tboxOutput.Clear();
        }


        private void HandlerLoadMC9S08AC60Hex(object sender, EventArgs e)
        {
            LoadFileAsync(0);
        }

        private void HandlerLoadPIC18F46K22Hex(object sender, EventArgs e)
        {
            LoadFileAsync(1);
        }
		
        private void HandlerInitBootloader(object sender, EventArgs e)
        {
            if (iCommunicatorState == eState.eStateInit || iCommunicatorState == eState.eStateConnectedMC9S08AC60)
            {
                iCommunicatorState = eState.eStateTriggerBootloader;
            }
           
        }

        private void HandlerEraseMC9S08AC60(object sender, EventArgs e)
        {
            if (iCommunicatorState== eState.eStateConnectedMC9S08AC60 && clsHexLoader.FileLoaded(0)) {
                iCommunicatorState = eState.eStateErase;
            }
        }

        private void HandlerProgramMC9S08AC60(object sender, EventArgs e)
        {
            if(iCommunicatorState == eState.eStateConnectedMC9S08AC60 && clsHexLoader.FileLoaded(0)) {
                iCommunicatorState = eState.eStateProgram;
            }
        }

        private void HandlerBridgeToPIC18F46K22(object sender, EventArgs e)
        {
            if (iCommunicatorState == eState.eStateConnectedMC9S08AC60)
            {
                iCommunicatorState = eState.eStateBridge;
            }
        }

        private void HandlerProgramPIC18F46K22(object sender, EventArgs e)
        {
            if (iCommunicatorState == eState.eStateConnectedPIC18F46K22 && clsHexLoader.FileLoaded(1))
            {
                iCommunicatorState = eState.eStateProgramPIC18F46K22;
            }
        }

        private void HandlerProgramEepromPIC18F46K22(object sender, EventArgs e)
        {
            if (iCommunicatorState == eState.eStateConnectedPIC18F46K22 && clsHexLoader.GetEntriesCountEeprom(1)>0)
            {
                iCommunicatorState = eState.eStateProgramEepromPIC18F46K22;
            }
        }



        private void HandlerConnectPort(object sender, EventArgs e)
        {
            try
            {
                if (!clsSerialPort.IsOpen)
                {
                    string sComportName = this.cboxComs.SelectedItem.ToString();
                    if (sComportName != "")
                    {
         
                        clsCommunicationThread = new Thread(Communicate);

                        clsSerialPort.PortName = sComportName;
                        clsSerialPort.BaudRate = 57600;
                        clsSerialPort.Parity = Parity.None;
                        clsSerialPort.DataBits = 8;
                        clsSerialPort.StopBits = StopBits.One;
                        clsSerialPort.Handshake = Handshake.None;
                        //clsSerialPort.DtrEnable = true;

                        clsSerialPort.ReadTimeout  = 100;
                        clsSerialPort.WriteTimeout = 100;
                        clsSerialPort.Open();

                        clsCommunicationThread.Start();
                        btnConnect.Text = "Disconnect";
                        btnConnect.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
                    }
                }
                else
                {
                    clsSerialPort.Close();
                    btnConnect.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
                    btnConnect.Text = "Connect";
                    clsCommunicationThread.Join();
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }


        private void HandlerSendInput(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter)
            {
                string input = tboxSendInput.Text;
                if (input.Length > 0) {
                    try {

                        if (iCommunicatorState == eState.eStateConnectedPIC18F46K22 || iCommunicatorState == eState.eStateConnectedMC9S08AC60)
                        {
                            clsManualInputBuffer.Enqueue(Helper.ConvertHexStringToByteArray(input));
                            tboxSendInput.Clear();
                        }
             
                    }
                    catch (Exception ex) {
                        Console.WriteLine(ex.StackTrace);
                    }
                }
            }
        }


    }


    public class LogObject
    {

        public string strTimestamp;
        public string strLogLine;
        public int iType;
        public LogObject(string strTimestamp, string strLogLine, int iType)
        {
            this.strTimestamp = strTimestamp;
            this.strLogLine = strLogLine;
            this.iType = iType;
        }

    }


}
