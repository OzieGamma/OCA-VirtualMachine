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

set s0 Hello_World_Str
add s0 s0 bp

set t0 PRINT
callr t0

set s0 0
set t0 EXIT
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
