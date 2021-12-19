/*
 * mm35_bootloader.h
 *
 *  Created on: Mar 30, 2021
 *      Author: am
 */

#ifndef MM35_BOOTLOADER_H_
#define MM35_BOOTLOADER_H_

typedef unsigned char BYTE;
typedef unsigned char CHAR;
typedef signed int INT;
typedef unsigned int UINT;
typedef unsigned long ULONG;
typedef signed long LONG;

#define NO_RETURN void


#pragma CODE_SEG BOOTLOADER_CODE
NO_RETURN nrGotoBootloader(void);
#pragma CODE_SEG DEFAULT

#endif /* MM35_BOOTLOADER_H_ */
