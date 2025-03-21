; comments - ;.*

; numbers  - -?[0-9]+
; binary numbers - 0[bB][01]+
; hexadecimal number - 0x[0-9a-fA-F]
; whitespace - [, \t]
; colon         -  :
; LParen        - (
; RParen        - )
; string        - "([^\]|(\[nrt"\]))    \r \n \\
; word          - anything else. [a-zA-Z_][a-zA-Z_0-9]*
;       addi
;       hello_world
;       _hello
;       hello123_




addi   2, 2, -16      ; make space for 4 words
sw   10, 2, 0         ; store argc
sw   11, 2, 4         ; store argv
sw   0, 2, 8          ; store i
sw   1, 2, 12         ; store return address.

j   main_looptest

main_loopbody:
lw   13, 2, 8         ; get i
beq   13, 0, main_write_arg ; only write space if i != 0

auipc   10, %pcrel_hi(space)
addi   10, 10, %pcrel_lo(-4)
jal   1, putstring

main_write_arg:
lw   10, 2, 4         ; x10 = argv
slli   14, 13, 2      ; x14 = i*4
add   10, 10, 14      ; x10 = argv + i*4
lw   10, 10, 0        ; x10 = argv[i]
jal   1, putstring

addi   13, 13, 1      ; move to next argument
sw   13, 2, 8         ; save i

main_looptest:
lw   13, 2, 8     ; x13 = i
lw   10, 2, 0     ; x10 = argc
blt   13, 10, main_loopbody

auipc   10, %pcrel_hi(nl)
addi   10, 10, %pcrel_lo(-4)
jal   1, putstring

li   10, 0
li   17, OsEmulator.SYSCALL_EXIT
ecall   
; lw   1, 2, 12         ; restore return address.
; addi   2, 2, 16      ; restore stack ptr
; jalr   0, 1, 0

; putstring ;;;;;;;;
putstring:
addi   2, 2, -8       ; allocate space for two words
sw   1, 2, 0          ; store return address
sw   10, 2, 4         ; store initial value of the string ptr.

jal   1, strlen
addi   12, 10, 0      ; Copy x10 into x12
lw   11, 2, 4         ; reload the string ptr
li   10, 1            ; std output in x10
li   17, OsEmulator.SYSCALL_WRITE

ecall   

lw   1, 2, 0          ; retore link register
addi   2, 2, 8         ; restore stack ptr
jalr   0, 1, 0

; strlen ;;;;;;;
strlen:
addi   11, 10, 0
j   strlen_test

strlen_body:
addi   10, 10, 1

strlen_test:
lbu   12, 10, 0       ; read a byte pointed to by x0 into x12
bne   12, 0, strlen_body

sub   10, 10, 11
jalr   0, 1, 0

; data
space: ds   " \0"
nl: ds   "\r\n\0":
