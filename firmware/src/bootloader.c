/* Cannot directly use mBootloaderBase for Jumping. Does not assemble! So use a ROM area of 2 Bytes to load into X then jump*/

#include "bootloader.h"
/* Don't use switch case Runtime Support rtshc08. Very Cumbersome because there is not enough RAM to load it into*/
/* Always put constants in the If(...) to avoid JSR to rtshc08*/

#define BOOTLOADER_FLAG_EXECUTE_FROM_RAM					
#define BOOTLOADER_USB_RXBUF_SIZE				 72		// Keep this aligned to 2 Bytes. N.B! Check if necessary! N.B Max 128 bytes because "carry" is nto considered in bootloader.
#define BOOTLOADER_USB_TXBUF_SIZE				 32
#define BOOTLOADER_RAMBASE 					 (0x100)	// Check with Linker File and Datasheet
#define BOOTLOADER_SECTIONBASE				(0xF7B2)	// Check with Linker File
#define BOOTLOADER_SIZE						  (1257)	// Check with .map file
#define BOOTLOADER_STACKBASE				(0x086F)	// Check with Linker File and Datasheet. Should be the end of RAM

#define mBootloaderUsbRxBuffer 			 (BOOTLOADER_RAMBASE)				 						 // Usb Rx Buffer
#define mBootloaderUsbTxQueue 			 (mBootloaderUsbRxBuffer+BOOTLOADER_USB_RXBUF_SIZE)	 	 	 // Usb Tx Queue
#define mBootloaderUsbRxReceived 	 	 (mBootloaderUsbTxQueue+BOOTLOADER_USB_TXBUF_SIZE)  		 // Usb Received Count
#define mBootloaderUsbRxExpected 	 	 (mBootloaderUsbRxReceived+1)			 				 	 // Usb Head
#define mBootloaderUsbTxHead        	 (mBootloaderUsbRxExpected+1)		 					 	 // Usb Tail
#define mBootloaderUsbTxTail    	 	 (mBootloaderUsbTxHead+1)		 						 	 // Usb Required Count
			 				 	 	
#define mBootloaderState 				 (mBootloaderUsbTxTail+1)			     	 				 // Bootloader State
#define mBootloaderCommand 				 (mBootloaderState+1)								 		 // Bootloader Command
#define mBootloaderBridge				 (mBootloaderCommand+1)
#define mBootloaderIndex 				 (mBootloaderBridge+1)									     // Bootloader offset to the address that needs flashing
#define mBootloaderFlashLineSize	 	 (mBootloaderIndex+1)		    			    	 		 // Bootloader number of bytes to flash
#define mBootloaderFlashError	 		 (mBootloaderFlashLineSize+1)				  			 	 // Bootloader temp variable
#define mBootloaderFlashLineAddress  	 (mBootloaderFlashError+1)							     	 // Bootloader address of line
#define mBootloaderActionTimeout 	 	 (mBootloaderFlashLineAddress+2) 							 // Bootloader Timeout

#define mBootloaderFunctionLoop			 (mBootloaderActionTimeout+2)								 // Bootloader: Function Address Loop
#define mBootloaderFunctionState0		 (mBootloaderFunctionLoop+2)								 // Bootloader: Function Address State0
#define mBootloaderFunctionState1		 (mBootloaderFunctionState0+2)								 // Bootloader: Function Address State1
#define mBootloaderFunctionTxQueueHasSpace (mBootloaderFunctionState1+2)							 // Bootloader: Function Address bBootloaderTxQueueHasSpace
#define mBootloaderFunctionTxQueuePutByte  (mBootloaderFunctionTxQueueHasSpace+2)					 // Bootloader: Function Address vBootloaderTxQueuePutByte
#define mBootloaderFunctionExecuteFcmd		(mBootloaderFunctionTxQueuePutByte+2)					 // Bootloader: Function Address bBootloaderExecuteFcmd

#ifdef BOOTLOADER_FLAG_EXECUTE_FROM_RAM
	#define mBootloaderBase 				 (mBootloaderFunctionExecuteFcmd+2)					 			 // Bootloader variables end. INFO change if more functions implemented
#else
	#define mBootloaderBase 				 (BOOTLOADER_SECTIONBASE)
#endif

#define STATE0 0
#define STATE1 1
#define STATE2 2


#define COMMAND_ECHO  				0xFE
#define COMMAND_READ_MEMORY			0xFD
#define COMMAND_PROGRAM_FLASH 		0xCF
#define COMMAND_ERASE_FLASH 		0xBF
#define COMMAND_BLANKCHECK_FLASH	0xBE
#define COMMAND_UNSECURE			0xBD

#define COMMAND_BRIDGE_TO_PIC		0xA0



#define FCMD_BLANK_CHECK	  		0x05
#define FCMD_PROGRAM_BYTE  			0x20
#define FCMD_PROGRAM_BURST 			0x25
#define FCMD_ERASE_MASS	  			0x41

/******************************************************************
 *
 *  C-return compiled to RTS no need to call it explicitly
 *  Values are returned in A-Register
 *  Last Parameters are passed in X and A Registers
 *  >Problem. If Functions are called using H:X last parameters cannot be accessed
 *******************************************************************/
#pragma CODE_SEG BOOTLOADER_CODE
BYTE bBootloaderExecuteFcmd(UINT uiAddress, BYTE bCommand,  BYTE bCompleteFlag, BYTE bData, BYTE bDummyA,BYTE bDummyX){ 
	BYTE bResult;
	asm{	
			LDA   bDummyX
			LDA   bDummyA
			LDA   #0x30								
			STA   _FSTAT							// Reset Flash Status Register
			LDA   bData							// Load Data into A-Register
			LDHX  uiAddress
			STA,  X									// Write 0xFF into a flash address
			LDA   bCommand					
			STA   _FCMD								// Load Command into FCMD			
			LDA   #0x80
			STA   _FSTAT							// Start xecuting
			NOP
			NOP
			NOP
			NOP
lEraseFlashWaitComplete:
			LDX   #0x55
			STX   _SRS
			LDA   _FSTAT
			AND   bCompleteFlag
			CMP   #0
			BEQ	  lEraseFlashWaitComplete
			LDA   _FSTAT
			STA	  bResult
	};
	
	return bResult;
}


#pragma CODE_SEG BOOTLOADER_CODE
BYTE bBootloaderTxQueueHasSpace(BYTE bRequired){
	BYTE bEnoughSpace = 0;
	asm{
eFunctionStart:
			LDA   mBootloaderUsbTxHead
			SUB   mBootloaderUsbTxTail
			CMP   #0
			BGT   lTailIsBeforeHead 
			ADD   #BOOTLOADER_USB_TXBUF_SIZE-1
lTailIsBeforeHead:		
			CMP   bRequired
			BGE   lEnoughSpace					// Signed. Store Length + 2 
			LDX   #0
lEnoughSpace:
			STA   bEnoughSpace
eFunctionEnd:
	}
	return bEnoughSpace;
}

#pragma CODE_SEG BOOTLOADER_CODE
void vBootloaderTxQueuePutByte(BYTE bData){
	asm{
			LDA   mBootloaderUsbTxHead
			ADD   #mBootloaderUsbTxQueue			// Copy Temp into Tx-Queue
			LDHX  #mBootloaderUsbTxQueue		    
			TAX 
			
			LDA   bData
			STA,  X
			
			LDA   mBootloaderUsbTxHead
			INCA									// ++Head
			CMP   #BOOTLOADER_USB_TXBUF_SIZE    	// Is there a rollover of tail?
			BNE   lUpdateHead
			CLRA
lUpdateHead:
			STA   mBootloaderUsbTxHead				// Update Head
	};
	return;
}


#pragma CODE_SEG BOOTLOADER_CODE
void vBootloaderState0(){
	BYTE bData;
	asm{
eFunctionStart:
			LDA   mBootloaderBridge
			CMP   #0
			BEQ	  eCommunicateWithMotorola
eCommandBridgeCheckPcIncoming:
			LDA   _SCI1S1	
			AND   #0x20												// Never get out of this loop
			TAX
			CPX   #0
			BEQ   eCommandBridgeCheckPicIncoming					// Check if there is Rx-Data from Pc. Send it to Pic
			LDA   _SCI1D
			STA   _SCI2D
eCommandBridgeCheckPicIncoming:
			LDA   _SCI2S1	
			AND   #0x20
			TAX
			CPX   #0										
			BEQ   eFunctionEnd							// Check if there is Rx Data from Pic. Send it to Pc
			LDA   _SCI2D
			STA   _SCI1D
			BRA	  eFunctionEnd		
eCommunicateWithMotorola:			
			LDA   _SCI1S1	
			AND   #0x20
			TAX
			CPX   #0
			BEQ   eFunctionEnd					// Check if there is Rx-Data
			LDA   _SCI1D
			STA   bData							// Read Data into var
			LDX   mBootloaderUsbRxExpected 		// Load Expected
			CPX   #0
			BNE   lExpectedAlreadySet
			LDA	  bData							// Range Tests before setting the length
			CMP   #BOOTLOADER_USB_RXBUF_SIZE-1
			BHI   eFunctionEnd					//  if Expected>16
			CMP   #2
			BLO   eFunctionEnd					//  if Expected<2
			STA	  mBootloaderUsbRxExpected
			
lExpectedAlreadySet:
			LDHX  #0x1FFF				
			STHX  mBootloaderActionTimeout		// Set Timeout
			LDA   mBootloaderUsbRxReceived
			ADD   #mBootloaderUsbRxBuffer		// LSB of adrress
			LDHX  #mBootloaderUsbRxBuffer		// Whole 16-bit Address
			TAX 								// Address + Offset   : This works because the size is limited to 128 Bytes and LSBS of addresses are within 128 bytes
			
			LDA   bData			
			STA,  X								// Store Data into Buffer
			
			LDA   mBootloaderUsbRxReceived							
			INCA						    	
			STA   mBootloaderUsbRxReceived		// ++(Rx-Index)		
			CMP   mBootloaderUsbRxExpected		
			BNE   eFunctionEnd					// Expected != Received
lParseStart:
			LDA   #STATE1
			STA   mBootloaderState				// State = eBootloaderProcessing
			LDA   #1					
			ADD   #mBootloaderUsbRxBuffer
			LDHX  #mBootloaderUsbRxBuffer
			TAX
			LDA,  X
			STA   mBootloaderCommand
			CMP   #COMMAND_ECHO
			BEQ   lParseCommandEcho				// Goto Command
			CMP   #COMMAND_READ_MEMORY
			BEQ   lParseCommandReadMemory		// Goto Command
			CMP   #COMMAND_PROGRAM_FLASH
			BEQ   lParseCommandProgramFlash		// Goto Command
			CMP	  #COMMAND_BRIDGE_TO_PIC
			BEQ   lParseCommandBridgeToPic		// Goto Command
			CMP   #COMMAND_ERASE_FLASH
			BLS   lParseCommandEraseFlash		// Goto Command
			BRA   eFunctionEnd
lParseCommandBridgeToPic:
			LDA   #1
			STA   mBootloaderBridge
lParseCommandEcho:								
lParseCommandUnsecureFlash:
lParseCommandBlankCheckFlash:
lParseCommandEraseFlash:
			NOP
			BRA   eFunctionEnd
lParseCommandProgramFlash:									
			NOP
			LDA   #2
			ADD   #mBootloaderUsbRxBuffer
			LDHX  #mBootloaderUsbRxBuffer
			TAX
			LDHX, X
			STHX  mBootloaderFlashLineAddress	// Load Address
			
			LDA   #4					
			ADD   #mBootloaderUsbRxBuffer
			LDHX  #mBootloaderUsbRxBuffer
			TAX
			LDA,  X
			STA   mBootloaderFlashLineSize		// Load Size
			
			LDA   #0
			STA   mBootloaderIndex		// Reset Index
			BRA   eFunctionEnd
lParseCommandReadMemory:
			NOP
			LDA   #2
			ADD   #mBootloaderUsbRxBuffer
			LDHX  #mBootloaderUsbRxBuffer
			TAX
			LDHX, X
			STHX  mBootloaderFlashLineAddress	// Load Address
			
			LDA   #4					
			ADD   #mBootloaderUsbRxBuffer
			LDHX  #mBootloaderUsbRxBuffer
			TAX
			LDA,  X
			STA   mBootloaderFlashLineSize		// Load Size
lParseEnd:
eFunctionEnd:	
	};
	return;
}

#pragma CODE_SEG BOOTLOADER_CODE
void vBootloaderState1(){
	BYTE bTemp = 0;
	asm{

eCommandBridgeStart:
			LDX   mBootloaderCommand	
			CPX   #COMMAND_BRIDGE_TO_PIC
			BNE   eCommandBridgeEnd
			LDA   #2
			LDHX  mBootloaderFunctionTxQueuePutByte	    // Put Command
			JSR,  X
			LDA   mBootloaderCommand
			LDHX  mBootloaderFunctionTxQueuePutByte	    // Put Command
			JSR,  X
eCommandBridgeFinish:
			LDA   #0
			STA   mBootloaderCommand
			LDA   #STATE0
			STA	  mBootloaderState
eCommandBridgeEnd:
		
//----------------------------------
//--- Echo Tx Frame  ---
//----------------------------------
eCommandsEchoStart:
			LDX   mBootloaderCommand	
			CPX   #COMMAND_ECHO
			BNE   eCommandsEchoEnd
			LDA   mBootloaderUsbRxExpected
			LDHX  mBootloaderFunctionTxQueueHasSpace
			JSR,  X
			CMP   #0	
			BEQ	  eCommandsEchoEnd					// Not enough space
			LDA   #0
			STA   mBootloaderIndex
lEchoCopyWhile:
			LDA   mBootloaderIndex
			ADD   #mBootloaderUsbRxBuffer			// LSB of adrress
			LDHX  #mBootloaderUsbRxBuffer			// Whole 16-bit Address
			TAX 
			LDA,  X
			LDHX  mBootloaderFunctionTxQueuePutByte
			JSR,  X
			LDX   mBootloaderIndex
			INCX
			STX   mBootloaderIndex					// ++Index
			CPX   mBootloaderUsbRxExpected
			BNE   lEchoCopyWhile
			LDA   #STATE0
			STA	  mBootloaderState
eCommandsEchoEnd:

//----------------------------------
//--- Read Device Memory  ---
//----------------------------------
eCommandsReadMemoryStart:
			LDX   mBootloaderCommand	
			CPX   #COMMAND_READ_MEMORY
			BNE   eCommandsReadMemoryEnd
			LDA   mBootloaderFlashLineSize
			ADD	  #2
			LDHX  mBootloaderFunctionTxQueueHasSpace
			JSR,  X
			CMP   #0	
			BEQ	  lMemoryWhileEnd					// Not enough space
			LDA   mBootloaderFlashLineSize
			ADD	  #2
			LDHX  mBootloaderFunctionTxQueuePutByte
			JSR,  X 
			LDA   mBootloaderCommand
			LDHX  mBootloaderFunctionTxQueuePutByte
			JSR,  X								   // Put Command
			LDA   #0
			STA   mBootloaderIndex
lMemoryWhile:
			LDA   mBootloaderIndex					// Address + Offset
			ADD   mBootloaderFlashLineAddress+1			
			PSHA
			LDA   mBootloaderFlashLineAddress+0
			ADC   #0    									
			PSHA
			PULH
			PULX									// Full 16-bit Address

			LDA,  X									// Read Memory
			LDHX  mBootloaderFunctionTxQueuePutByte
			JSR,  X 
			
			LDX   mBootloaderIndex
			INCX
			STX   mBootloaderIndex					// ++Index
			CPX   mBootloaderFlashLineSize
			BNE   lMemoryWhile
lMemoryWhileEnd:
			LDA   #0
			STA   mBootloaderCommand
			LDA   #STATE0
			STA	  mBootloaderState
eCommandsReadMemoryEnd:	


//----------------------------------
//--- Erase Flash Memory  ---
//----------------------------------
eCommandsEraseFlashStart:
			LDX   mBootloaderCommand	
			CPX   #COMMAND_ERASE_FLASH	
			BNE   eCommandsEraseFlashEnd
			LDA   #0xFF
			STA   _FPROT							
			LDHX  #0x1860
			PSHX
			PSHH
			LDA   #FCMD_ERASE_MASS
			PSHA  
			LDA   #0x40									// Complete flag
			PSHA
			LDA   #0xFF									// Data Byte
			PSHA	
			LDHX  mBootloaderFunctionExecuteFcmd
			JSR,  X
			AIS   #5
			LDX   #1								
			CMP   #0xC0
			BNE   lEraseFlashDone						// Error: Flash has not been erased
			LDX   #0
lEraseFlashDone:
			STX	  mBootloaderFlashError
			LDA   #3
			LDHX  mBootloaderFunctionTxQueueHasSpace
			JSR,  X
			CMP   #0
			BEQ   lEraseNoSpace
			LDA   #3
			LDHX  mBootloaderFunctionTxQueuePutByte		// Put Length
			JSR,  X
			
			LDA   mBootloaderCommand
			LDHX  mBootloaderFunctionTxQueuePutByte	    // Put Command
			JSR,  X
			
			LDA	  mBootloaderFlashError					// Put Status
			LDHX  mBootloaderFunctionTxQueuePutByte
			JSR,  X
lEraseNoSpace:
			LDA   #0
			STA   mBootloaderCommand
			LDA   #STATE0
			STA	  mBootloaderState
eCommandsEraseFlashEnd:				
			
//----------------------------------
//--- Blank Check  Flash Memory  ---
//----------------------------------
eCommandsBlankCheckFlashStart:
			LDX   mBootloaderCommand	
			CPX   #COMMAND_BLANKCHECK_FLASH
			BNE   eCommandsBlankCheckFlashEnd		
			LDA   #FCMD_BLANK_CHECK
			LDHX  #0x1860
			PSHX
			PSHH
			PSHA  
			LDA   #0x40									// Complete flag
			PSHA
			LDA   #0xCC									// Data Byte
			PSHA	
			LDHX  mBootloaderFunctionExecuteFcmd
			JSR,  X
			AIS   #5
			LDX   #2
			CMP   #0xC4
			BNE   lBlankCheckFlashhDone					// Error: Flash is not blank
			LDX   #0/**/
lBlankCheckFlashhDone:
			STX	  mBootloaderFlashError
			LDA   #3
			LDHX  mBootloaderFunctionTxQueueHasSpace
			JSR,  X
			CMP   #0
			BEQ   lBlankCheckNoSpace
			LDA   #3
			LDHX  mBootloaderFunctionTxQueuePutByte		// Put Length
			JSR,  X
			
			LDA   mBootloaderCommand
			LDHX  mBootloaderFunctionTxQueuePutByte	    // Put Command
			JSR,  X
			
			LDA	  mBootloaderFlashError					// Put Status
			LDHX  mBootloaderFunctionTxQueuePutByte
			JSR,  X
lBlankCheckNoSpace:
			LDA   #0
			STA   mBootloaderCommand
			LDA   #STATE0
			STA	  mBootloaderState
eCommandsBlankCheckFlashEnd:				
			
			
//----------------------------------
//--- Blank Check  Flash Memory  ---
//----------------------------------
eCommandsUnsecureFlashStart:
			LDX   mBootloaderCommand	
			CPX   #COMMAND_UNSECURE
			BNE   eCommandsUnsecureFlashEnd		
			LDA   #FCMD_BLANK_CHECK			
lUnsecureFlash:
			LDHX  #0xFFBF
			PSHX
			PSHH
			LDA   #FCMD_PROGRAM_BYTE
			PSHA  
			LDA   #0x40									// Complete flag
			PSHA
			LDA   #0xFE									// Data Byte
			PSHA	
			LDHX  mBootloaderFunctionExecuteFcmd
			JSR,  X
			AIS   #5
			LDX   #3								
			LDA   0xFFBF
			CMP   #0xFE
			BNE   lUnsecureNoSpace						// Error: Flash is not unsecured
			LDX   #0	
lUnsecureFlashDone:
			STX	  mBootloaderFlashError
			LDA   #3
			LDHX  mBootloaderFunctionTxQueueHasSpace
			JSR,  X
			CMP   #0
			BEQ   lUnsecureNoSpace
			LDA   #3
			LDHX  mBootloaderFunctionTxQueuePutByte		// Put Length
			JSR,  X
			
			LDA   mBootloaderCommand
			LDHX  mBootloaderFunctionTxQueuePutByte	    // Put Command
			JSR,  X
			
			LDA	  mBootloaderFlashError					// Put Status
			LDHX  mBootloaderFunctionTxQueuePutByte
			JSR,  X
lUnsecureNoSpace:
			LDA   #0
			STA   mBootloaderCommand
			LDA   #STATE0
			STA	  mBootloaderState
eCommandsUnsecureFlashEnd:	


//----------------------------------
//--- Program Flash Memory  ---
//----------------------------------
eCommandsProgramFlashStart:
			LDX    mBootloaderCommand	
			CPX    #COMMAND_PROGRAM_FLASH	
			BNE    eCommandsProgramFlashEnd	
lProgramFlashLoop:
			LDA   #0x55
			STA   _SRS							
			LDA   mBootloaderIndex
			CMP   mBootloaderFlashLineSize
			BEQ   lProgramDone					// All Bytes flashed? Then done.
			ADD   mBootloaderFlashLineAddress+1		
			PSHA								// Push Low Byte of the address
			LDA   mBootloaderFlashLineAddress   // x = (RxBuffer + 4 + Offset)
			ADC	  #0
			PSHA								// Push High Byte of the address
			
			LDA   #FCMD_PROGRAM_BURST
			PSHA								// Push Command
			
			LDA   #0x80
			PSHA								// Push Completion Flag
			
			LDA   mBootloaderIndex
			ADD   #mBootloaderUsbRxBuffer		// LSB of adrress
			ADD   #5
			LDHX  #mBootloaderUsbRxBuffer		// Whole 16-bit Address
			TAX 
			LDA,  X
			PSHA								// Push Data
			LDHX  mBootloaderFunctionExecuteFcmd			
			JSR,  X								// Flash Byte into the address
			AIS  #5								// Pop Parameters
			AND  #0x10
			CMP  #0x10							
			BEQ  lProgramError					// Flashing Error FACCR
			LDX	 #0								// Load #0 into X. Error = 0
			LDA   mBootloaderIndex				// Increment Index
			INCA
			STA   mBootloaderIndex
			BRA   lProgramFlashLoop
lProgramError:
			LDA  #1
			ADD  mBootloaderIndex
			TAX
lProgramDone:	
			STX	  mBootloaderFlashError
			LDA   #3
			LDHX  mBootloaderFunctionTxQueueHasSpace
			JSR,  X
			CMP   #0
			BEQ   lProgramNoSpace
			LDA   #3
			LDHX  mBootloaderFunctionTxQueuePutByte		// Put Length
			JSR,  X
			
			LDA   mBootloaderCommand
			LDHX  mBootloaderFunctionTxQueuePutByte	    // Put Command
			JSR,  X
			
			LDA	  mBootloaderFlashError					// Put Status
			LDHX  mBootloaderFunctionTxQueuePutByte
			JSR,  X
lProgramNoSpace:
			LDA   #0
			STA   mBootloaderCommand
			LDA   #STATE0
			STA	  mBootloaderState
eCommandsProgramFlashEnd:
			CLRX
			STX	  mBootloaderUsbRxReceived
			STX   mBootloaderUsbRxExpected
eFunctionEnd:
	};
	return;
}

#pragma CODE_SEG BOOTLOADER_CODE
void vBootloaderLoop(){
	asm{
			LDA   #0x55						// Can't stop the watchdog once it is active
			STA   _SRS		
//---------------------------------------
// Timeout: Resets state
//---------------------------------------
bTimeoutStart:
			LDHX  mBootloaderActionTimeout
			CPHX  #0
			BEQ   lTimeoutReached
lTimeoutDecrement:
			LDA   mBootloaderActionTimeout+1
			SUB   #1
			STA   mBootloaderActionTimeout+1
			LDA   mBootloaderActionTimeout
			SBC   #0
			STA   mBootloaderActionTimeout		// --Timeout16
			BRA   bTimeoutEnd					// if Timeout16 == 0 Reset
lTimeoutReached:
lTimeoutResetVariables:			
			LDA   #STATE0							
			STA   mBootloaderState				// State = eBootloaderInitCommunication
			LDA   #0
			STA   mBootloaderIndex
			STA   mBootloaderFlashLineSize
			STA   mBootloaderUsbRxExpected
			STA   mBootloaderUsbRxReceived
			STA   mBootloaderCommand
bTimeoutEnd:			

//---------------------------------------
// Transmission: Usb Transmission
//---------------------------------------
bTransmissionStart:
			LDA    mBootloaderUsbTxTail
			CMP    mBootloaderUsbTxHead
			BEQ    bTransmissionEnd				// Check if Tail is not equal to Head
lTransmissionWaitForTx:
			LDA    _SCI1S1
			AND    #0x80
			BEQ    lTransmissionWaitForTx		// Wait for Data-Register to become empty

			LDA    mBootloaderUsbTxTail
			ADD    #mBootloaderUsbTxQueue
			LDHX   #mBootloaderUsbTxQueue
			TAX
			LDA,   X
			STA    _SCI1D						// Send Data Out
			
			LDA    mBootloaderUsbTxTail
			INCA								// ++Tail
			CMP    #BOOTLOADER_USB_TXBUF_SIZE   // Is there a rollover of tail?
			BNE    lTransmissionUpdateTail
			CLRA
lTransmissionUpdateTail:
			STA    mBootloaderUsbTxTail			// Update Tail
bTransmissionEnd:

//---------------------------------------
// Switcher: Switches between States
//---------------------------------------
eBootloaderStateSwitcherStart:
			LDX   mBootloaderState	
			CPX   #STATE0
			BEQ   eBootloaderState0
			CPX   #STATE1
			BEQ	  eBootloaderState1
			NOP
			BRA	  eBootloaderStateSwitcherEnd
eBootloaderState0:
			LDHX  mBootloaderFunctionState0
			JSR,  X  
			BRA	  eBootloaderStateSwitcherEnd
eBootloaderState1:
			LDHX  mBootloaderFunctionState1
			JSR,  X  
			;BRA	  eBootloaderStateSwitcherEnd
eBootloaderStateSwitcherEnd:	

//------------------------------
// Jump to the start if the loop
//------------------------------
			LDHX mBootloaderFunctionLoop		
			JMP, X	
		
	}
}
/**/
/* Load Bootloader into memory and jump to it
 * Do not used any Variables after memcpy, because they may overwrite a bootloader section.
 * Do a straight jump because Stack at this stage is overwritten, NO CALLS!*/

#pragma CODE_SEG BOOTLOADER_CODE
NO_RETURN nrGotoBootloader(void) {
	
#pragma CONST_SEG BOOTLOADER_CONST
	// Function offsets relative to Bootloader Flash Section. Calculated at compile time.
	const static UINT uiFunctionLoop 	= (((UINT)(void*)vBootloaderLoop)-(BOOTLOADER_SECTIONBASE) +mBootloaderBase);
	const static UINT uiFunctionState0  = (((UINT)(void*)vBootloaderState0)-(BOOTLOADER_SECTIONBASE) +mBootloaderBase);
	const static UINT uiFunctionState1  = (((UINT)(void*)vBootloaderState1)-(BOOTLOADER_SECTIONBASE) +mBootloaderBase);
	const static UINT uiFunctionTxQueueHasSpace = (((UINT)(void*)bBootloaderTxQueueHasSpace)-(BOOTLOADER_SECTIONBASE) +mBootloaderBase);
	const static UINT uiFunctionTxQueuePutByte =  (((UINT)(void*)vBootloaderTxQueuePutByte)-(BOOTLOADER_SECTIONBASE) +mBootloaderBase);
	const static UINT uiFunctionExecuteFcmd = (((UINT)(void*)bBootloaderExecuteFcmd)-(BOOTLOADER_SECTIONBASE) +mBootloaderBase);
	set_int;

	asm {	
		
			LDHX  #BOOTLOADER_STACKBASE
			TXS									// Smash stack. New Stack is set at the end of RAM
			
			//-------------------------------------------------------------
			// Load Standard Frame into Tx
			//-------------------------------------------------------------
			LDA   #'B'
			STA   mBootloaderUsbTxQueue
			LDA   #'L'
			STA   mBootloaderUsbTxQueue+1
			LDA   #5
			STA   mBootloaderUsbTxQueue+2
			LDA   #1
			STA   mBootloaderUsbTxQueue+3
			LDA   #'V'
			STA   mBootloaderUsbTxQueue+4
			LDA   #'0'
			STA   mBootloaderUsbTxQueue+5
			LDA   #'1'
			STA   mBootloaderUsbTxQueue+6
			LDA   #0
			STA   mBootloaderUsbTxQueue+7
			LDA   #0x0B
			STA   mBootloaderUsbTxQueue+8		// CRC
			LDA   #0xF2
			STA   mBootloaderUsbTxQueue+9		// CRC
			
			LDA   #10
			STA   mBootloaderUsbTxHead			// Update Head
			LDA   #0
			STA   mBootloaderUsbTxTail			// Reset Tail
			//-------------------------------------------------------------
			// Initialize  Hardware Config for Bootloader
			//-------------------------------------------------------------
			LDA   #0xE2
			STA   _ICGC1						//Go into Self-Clocked Mode ~8Mhz. Disable Loss of Lock Interrupt. High Frequency Range with P=1
			LDA   #0x10
			STA   _ICGC2						// Set Multiplier x6 and Divider x1 to generate 24 Mhz Clock from 4Mhz External Clock
			LDA   #0xFA	
			STA   _ICGC1						// Enable FLL Engaged Mode
lWaitPllLock:
			LDA   _ICGS1
			AND   #0x08
			TAX
			CPX   #0
			BEQ   lWaitPllLock					// Wait for Pll to lock again				
lWaitPllEngage:
			LDA   _ICGS1
			AND   #0xC0
			TAX
			CPX   #0xC0
			BNE   lWaitPllEngage				// Wait for FLL Engaged Mode to kick in. Does it happen instantly?
												// ICGOUT Output of ICG Module as Cpu-Clock. Cpu-Clock/2 = Bus-Clock
			LDA   #60
			STA   _FCDIV						// Set Flash-Clock to ~200 Khz
lWaitFlashActive:			
			LDA   _FCDIV
			AND   #-128
			CLRH
			TAX
			CPHX  #0
			BEQ   lWaitFlashActive				// Wait for Flash to become active for erase and flashing operations
			CLRA
			STA   _FCNFG						// Reset Key access
			
			LDA	  #0xFF
			STA   _FSTAT						// Reset Flash Status Register

			//-------------------------------------------------------------
			// SCI2 Master Uart:	higher than 57600 is too inaccurate
			// (Sys_clock/2)/(16*57600) = x
			//--------------------------------------------------------------
			CLRH
			CLRX
			STHX   _SCI2BD
			STX	   _SCI2S1
			STX    _SCI2S2
			MOV   #13,_SCI2BD:1					// Baud 57600
			CLR   _SCI2C1						// 8 bit Transfer  10h for 9 bit
			MOV   #12,_SCI2C2					// Resetwert !b7==polling b3==TXon B2==RXon

			//-------------------------------------------------------------
			// SCI1 Master Uart:	higher than 57600 is too inaccurate
			// (Sys_clock/2)/(16*57600) = x
			//--------------------------------------------------------------
			CLRH
			CLRX
			STHX   _SCI1BD
			STX	   _SCI1S1
			STX    _SCI1S2
			MOV   #13,_SCI1BD:1					// Baud 57600
			CLR   _SCI1C1						// 8 bit Transfer
			MOV   #12,_SCI1C2					// Resetwert !b7==polling b3==TXon B2==RXon
		
	};
	
#ifdef BOOTLOADER_FLAG_EXECUTE_FROM_RAM
	// Load bootloader into RAM
	(void) memcpy((void*)mBootloaderBase, (void*) BOOTLOADER_SECTIONBASE, BOOTLOADER_SIZE);
#endif
	
	asm{		
			//---------------------------------------------------------------
			// Load precalculated function addresses as they will appear in RAM
			//---------------------------------------------------------------
			LDHX  uiFunctionLoop
			STHX  mBootloaderFunctionLoop			 // Load function address into variable
			LDHX  uiFunctionState0
			STHX  mBootloaderFunctionState0			 // Load function address into variable
			LDHX  uiFunctionState1
			STHX  mBootloaderFunctionState1			 // Load function address into variable
			LDHX  uiFunctionTxQueueHasSpace
			STHX  mBootloaderFunctionTxQueueHasSpace // Load function address into variable
			LDHX  uiFunctionTxQueuePutByte
			STHX  mBootloaderFunctionTxQueuePutByte  // Load function address into variable
			LDHX uiFunctionExecuteFcmd
			STHX mBootloaderFunctionExecuteFcmd		// Load function address into variable
			
			CLRH
			CLRX									// Reset variables
			STX   mBootloaderState					// Set Initialization State
			STX   mBootloaderIndex
			STX   mBootloaderFlashLineSize
			STX   mBootloaderUsbRxExpected
			STX   mBootloaderUsbRxReceived
			STX   mBootloaderCommand
			STX   mBootloaderBridge
			
			LDHX  #0x1FFF				
			STHX  mBootloaderActionTimeout			// Set Timeout to 0x1FFF
			LDHX  mBootloaderFunctionLoop
			JMP, X									// Goto Bootloader
	};	
}

