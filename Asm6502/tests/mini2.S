ZP_ADDR  EQU   $22
ROM_ADDR EQU   $C063

LOOP1    LDA   W

LOOP2    AND   ZP_ADDR
         AND   ZP_ADDR,X
         AND   (ZP_ADDR),Y
         AND   (ZP_ADDR,X)
         BCS   LOOP4

LOOP3    AND   $20
         AND   $20,X
         AND   ($20),Y
         AND   ($20,X)
         AND   #$20
         BCC   LOOP2

LOOP4    AND   ROM_ADDR
         AND   ROM_ADDR,X

W        DW    $2021
B        DB    $20
         DB    $21
         DB    $22
         DB    $23
         DB    $24
         DB    $25
         DB    $26