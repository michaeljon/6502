* A comment
                          ; another floating comment
         ORG   $300

LABEL                     ; a floating label without a `:`
         CLD              ; a comment at the end of a code line
         LDY   #1
         CPY   #1
LABEL2:                   ; a floating label with a `:`
LOADIT   LDA   DAT
         ADC   #128
         JMP   LABEL

IMMHEX   LDA   #$01
IMMDEC   LDA   10
INDX     LDA   DAT,X

DAT      DB    0
