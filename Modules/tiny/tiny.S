         ORG   $0000

TEST1
         CLD              ; Binary mode (binary addition: 88 + 70 + 1 = 159)
         SEC              ; Note: carry is set, not clear!
         LDA   #$58       ; 88
         ADC   #$46       ; 70 (after this instruction, C = 0, A = $9F = 159)
         DB    0

TEST2
         SED              ; Decimal mode (BCD addition: 58 + 46 + 1 = 105)
         SEC              ; Note: carry is set, not clear!
         LDA   #$58
         ADC   #$46       ; After this instruction, C = 1, A = $05
         DB    0

TEST3
         SED              ; Decimal mode (BCD addition: 12 + 34 = 46)
         CLC
         LDA   #$12
         ADC   #$34       ; After this instruction, C = 0, A = $46
         DB    0

TEST4
         SED              ; Decimal mode (BCD addition: 15 + 26 = 41)
         CLC
         LDA   #$15
         ADC   #$26       ; After this instruction, C = 0, A = $41
         DB    0

TEST5
         SED              ; Decimal mode (BCD addition: 81 + 92 = 173)
         CLC
         LDA   #$81
         ADC   #$92       ; After this instruction, C = 1, A = $73
         DB    0

TEST6
         SED              ; Decimal mode (BCD subtraction: 46 - 12 = 34)
         SEC
         LDA   #$46
         SBC   #$12       ; After this instruction, C = 1, A = $34)
         DB    0

TEST7
         SED              ; Decimal mode (BCD subtraction: 40 - 13 = 27)
         SEC
         LDA   #$40
         SBC   #$13       ; After this instruction, C = 1, A = $27)
         DB    0

TEST8
         SED              ; Decimal mode (BCD subtraction: 32 - 2 - 1 = 29)
         CLC              ; Note: carry is clear, not set!
         LDA   #$32
         SBC   #$02       ; After this instruction, C = 1, A = $29)
         DB    0

TEST9
         SED              ; Decimal mode (BCD subtraction: 12 - 21)
         SEC
         LDA   #$12
         SBC   #$21       ; After this instruction, C = 0, A = $91)
         DB    0

TEST10
         SED              ; Decimal mode (BCD subtraction: 21 - 34)
         SEC
         LDA   #$21
         SBC   #$34       ; After this instruction, C = 0, A = $87)
         DB    0

TEST11
         SED              ; Decimal mode (BCD addition: NUM3 = NUM1 + NUM2)
         CLC
         LDA   NUM1L      ; Add low bytes first
         ADC   NUM2L
         STA   NUM3L
         LDA   NUM1H      ; Then add high bytes
         ADC   NUM2H
         STA   NUM3H
         DB    0

TEST12
         SED              ; Decimal mode (BCD subtraction: NUM3 = NUM1 - NUM2)
         SEC
         LDA   NUM1L      ; Subtract low bytes first
         SBC   NUM2L
         STA   NUM3L
         LDA   NUM1H      ; Then subtract high bytes
         SBC   NUM2H
         STA   NUM3H
         DB    0

TEST13
         SED              ; Decimal mode
         CLC
         LDA   #$90
         ADC   #$90
         DB    0

TEST14
         SED              ; Decimal mode
         SEC
         LDA   #$01
         SBC   #$01       ; expect A = 0, Z = 1
         DB    0

TESTA
         SED              ; Decimal mode
         LDA   #$05
         CLC
         ADC   #$05
         DB    0

TESTB
         SED              ; Decimal mode (has no effect on this sequence)
         LDA   #$05
         ASL
         DB    0

TESTC
         SED              ; Decimal mode
         LDA   #$09
         CLC
         ADC   #$01
         DB    0

TESTD
         SED              ; Decimal mode (has no effect on this sequence)
         LDA   #$09
         STA   NUM
         INC   NUM
         DB    0


NUM      EQU   $E0

NUM1L    EQU   $F0
NUM1H    EQU   $F1

NUM2L    EQU   $F2
NUM2H    EQU   $F3

NUM3L    EQU   $F4
NUM3H    EQU   $F5
