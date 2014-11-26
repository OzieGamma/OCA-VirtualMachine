# Example files that tests various assembly instructions
# The following symbols hook into the VM FFI
# 
# ** EXIT ** 
# 	Arguments:
# 		s0: 0 if all went well, else an exit code
#	Returns: nothing
#	Effects: stops the program
.def EXIT 0x0002
# ** PRINT ** 
# 	Arguments:
# 		s0: address of a string to print (length in first word, rest is char array)
#	Returns: nothing
#	Effects: prints the string to a terminal
.def PRINT 0x0003

addi s0 bp Hello_World_Str

addi s1 zero PRINT
callr s1

addi s6 zero 256			
stw one RAM(bp)	#Each string will be 1 long

addi s0 bp RAM            #We will print the 2 words after RAM

LOOP:
sub s6 s6 one
stw s6 RAM+1(bp)

addi s1 zero PRINT			#
callr s1

bnez s6 LOOP


addi s0 zero 0
addi t0 zero EXIT
callr t0

Hello_World_Str:
	.word 0x0C
	.word 0x48
	.word 0x65
	.word 0x6C
	.word 0x6C
	.word 0x6F
	.word 0x20
	.word 0x57
	.word 0x6F
	.word 0x72
	.word 0x6C
	.word 0x64
	.word 0x21
RAM: