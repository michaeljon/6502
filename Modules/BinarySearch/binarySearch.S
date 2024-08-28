**********************************************************
*
* BINARY SEARCH
*
* Searches an ordered array
* of unsigned bytes, with a
* maximum array size of 255
* elements.
*
* ENTRY: Top of stack
*   Low byte of return address
*   High byte of return address
*   Value to find
*   Length of array
*   Low byte of array address
*   High byte of array address
*
* EXIT: If value is found then
*           CARRY = 0
*           A = index of value in array
*       Else
*           CARRY = 1
*
* Registers used: All
*
* Time: Approx 52 cycles for each time through the 
*       search loop plus 80 cycles overhead.
*       A binary search will take on the order of
*       lg(N), where N is the number of elements.
*
* Size: Program 89 bytes
*       Data     3 bytes temporary
*                2 bytes in page 0 $EE $EF
* 
**********************************************************
*
         ORG   $6000
*
ARYADR   EQU   $EE        ; PAGE ZERO POINTER TO ARRAY

*
* FUNCTION PROLOG, ALL VALUES ON STACK
*

* GET THE RETURN ADDRESS
BINSRCH  PLA
         TAY
         PLA
         TAX

* GET THE VALUE TO SEARCH FOR
         PLA
         STA   VALUE

* GET THE LENGTH OF THE ARRAY
         PLA
         STA   UBND

* GET THE STARTING ADRESSES OF ARRAY
         PLA
         STA   ARYADR
         PLA
         STA   ARYADR+1

* RESTORE THE RETURN ADDRESS
         TXA
         PHA
         TYA
         PHA

*
* THE SEARCH ITSELF
*

* CHECK THAT LENGTH IS NOT ZERO
         LDX   UBND       ; GET LENGTH
         BEQ   NOTFND     ; EXIT NOT FOUND IF LENGTH EQUALS 0

* SET UPPER AND LOWER SEARCH BOUNDS
         DEX
         STX   UBND       ; UPPER BOUND EQUALS LENGTH MINUS 1
         LDA   #0
         STA   LBND       ; LOWER BOUND EQUALS 0

* COMPUTE NEXT INDEX TO BE HALF WAY BETWEEN UPPER AND
* LOWER BOUND
NXTBYT   LDA   UBND
         CLC
         ADC   LBND       ; ADD LOWER AND UPPER BOUNDS
         ROR              ; DIVIDE BY 2, LOSE THE FRACTIONAL PART
         TAY              ; MAKE Y BE THE INDEX

* IF INDEX > UPPER BOUND THEN ELEMENT IS NOT IN ARRAY
         CPY   UBND
         BEQ   TSTLB      ; BRANCH IF INDEX == UPPER BOUND
         BCS   NOTFND     ; BRANCH IF INDEX >  UPPER BOUND

* IF INDEX < LOWER BOUND THEN ELEMENT IS NOT IN ARRAY
TSTLB    CPY   LBND
         BCC   NOTFND     ; BRANCH IF INDEX < LOWER BOUND

* TEST IF WE FOUND THE ELEMENT
         LDA   VALUE
         CMP   (ARYADR),Y
         BCC   SMALL      ; BRANCH IF VALUE < ELEMENT
         BEQ   FND        ; BRANCH IF FOUND

* VALUE IS > ELEMENT SO SET LOWER BOUND
* TO Y + 1, VALUE IS HIGHER IN ARRAY
         INY
         STY   LBND
         BNE   NXTBYT     ; CONTINUE SEARCHING IF LOWER BOUND
                          ; DOES NOT OVERFLOW
         BEQ   NOTFND     ; BRANCH IF LOWER BOUND OVERFLOWS
                          ; FROM $FF TO $00

* VALUE IS < ELEMENT TO SET UPPER BOUND TO
* Y - 1, VALUE IS LOWER IN ARRAY
SMALL    DEY
         STY   UBND
         CPY   #$FF       ; ON DEY WE WRAP TO $FF IF Y WAS 0
         BNE   NXTBYT     ; CONTINUE SEARCH IF UPPER BOUND
                          ; DOESN'T OVERFLOW
         BEQ   NOTFND     ; BRANCH IF INDEX UNDERFLOWED

* FOUND IT
FND      CLC              ; CLEAR CARRY, WE FOUND IT
         TYA              ; PUT THE INDEX INTO A
         RTS

* DIDN'T FIND IT
NOTFND   SEC              ; SET CARRY, WE DIDN'T FIND IT
         RTS

*
* TEMPORARY STORAGE
*
VALUE    DB    0
UBND     DB    0
LBND     DB    0

*
* TEST PROGRAM FROM HERE DOWN...
*
POSCASE  LDA   BFADR+1
         PHA              ; PUSH THE HIGH BYTE OF THE ARRAY
         LDA   BFADR
         PHA              ; PUSH THE LOW BYTE OF THE ARRAY
         LDA   BFSZ
         PHA              ; PUSH THE LENGTH OF THE ARRAY
         LDA   #$07
         PHA              ; PUSH THE TARGET VALUE TO FIND
         JSR   BINSRCH    ; DO THE THING
         DB    0          ; (BRK) CARRY FLAG SHOULD BE ZERO AND A == 4


* SEARCH IN THE ARRAY FOR A KNOWN-NOT PRESENT VALUE
NEGCASE  LDA   BFADR+1
         PHA              ; PUSH THE HIGH BYTE OF THE ARRAY
         LDA   BFADR
         PHA              ; PUSH THE LOW BYTE OF THE ARRAY
         LDA   BFSZ
         PHA              ; PUSH THE LENGTH OF THE ARRAY
         LDA   #$00
         PHA              ; PUSH THE TARGET VALUE TO FIND
         JSR   BINSRCH    ; DO THE THING
         DB    0          ; (BRK)  CARRY FLAG SHOULD BE ONE

* SOME DATA
BFADR    DW    BF         ; START OF THE ARRAY
BFSZ     DB    16         ; SIZE OF THE ARRAY
BF       DB    #$01       ; THE ARRAY CONTENTS
         DB    #$02
         DB    #$04
         DB    #$05
         DB    #$07
         DB    #$09
         DB    #$0A
         DB    #$0B
         DB    #$17
         DB    #$32
         DB    #$51
         DB    #$7B
         DB    #$BF
         DB    #$C7
         DB    #$FA
         DB    #$FF



