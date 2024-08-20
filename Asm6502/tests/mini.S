* A comment
                          ; another floating comment
         ORG   $300       ; org with a comment

E1       EQU   $20        ; the first equiv, with a comment
E2       =     $21

E3       =     $2020
E4       =     $2121


LABEL                     ; a floating label without a `:`
         CLD              ; a comment at the end of a code line
         LDY   #1
         CPY   #1

LABEL2:                   ; a floating label with a `:`

LOADIT   LDA   DAT
         ADC   #128
         JMP   LABEL
         JSR   $1234

         BCC   IMMHEX
         BCS   LOADIT
         BRA   LABEL

* implicits
         SED
         CLC

* acculumator - zp
         AND   $20
         ORA   $20

* acculumator - imm
         AND   #$20
         ORA   #$20

* addressing modes
IMMHEX   LDA   #$20
IMMDEC   LDA   #20
IMMZP    LDA   $01

         BCC   LOADIT     ; reverse branch
         BCS   FORWARD    ; forward branch

ABSX     LDA   DAT,X
ABSY     LDA   DAT,Y

IND      LDA   $2000
INDX     LDA   $2000,X
INDY     LDA   $2000,Y

ZP       LDA   $20
ZPX      LDA   ($20,X)
ZPY      LDA   ($20),Y

FORWARD
         RTS

         ORG   $400
DAT      DB    $20
         DB    $21
         DB    $22
         DB    $23
WORD     DW    $2020
         BRK