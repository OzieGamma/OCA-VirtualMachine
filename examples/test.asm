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
addi s1 zero 0x72
calli PRINTLN
calli STR_CONTAINS

addi s0 t0 0
calli BOOL_PRINTLN


add s0 zero t0
addi t0 zero EXIT
callr t0

# ** PRINTLN ** 
# 	Arguments:
# 		s0: address of a string to print (length in first word, rest is char array)
#	Returns: nothing
#	Effects: prints the string to a terminal with a \n at the end
PRINTLN:
	# Put ra on the stack
	addi sp sp -3
	stw ra sp(0)
	stw s0 sp(1)
	stw s1 sp(2)
	
	# Print the string
	addi s1 zero PRINT
	callr s1
	
	# Print
	addi s0 bp New_Line_Str
	callr s1
	
	# Remove ra from the stack
	ldw ra sp(0)
	ldw s0 sp(1)
	ldw s1 sp(2)
	addi sp sp 3
	
	ret

# ** BOOL_PRINTLN ** 
# 	Arguments:
# 		s0: a boolean to print
#	Returns: nothing
#	Effects: prints the boolean to a terminal with a \n at the end
BOOL_PRINTLN:
	# Put ra on the stack
	sub sp sp one
	sub sp sp one
	stw ra sp(0)
	stw s0 sp(1)
	
	beqz s0 else
	addi s0 bp True_Str
	br if_end

	else:
	addi s0 bp False_Str

	if_end:
	calli PRINTLN
		
	# Remove ra from the stack
	ldw ra sp(0)
	ldw s0 sp(1)
	addi sp sp 2
	
	ret
	
# ** STR_CONTAINS: ** 
# 	Arguments:
# 		s0: address of the string (length in first word, rest is char array)
# 		s1: char to check for
#	Returns: t0, 1 if the string contains the char, otherwise 0
#	Effects: NONE
STR_CONTAINS:
	add t0 zero zero
	add t2 s0 one # first index of str
	
	ldw t1 s0(0) # t1 = length
	add t3 t2 t1 #t3 = first index after str
	
	STR_CONTAINS_LOOP:
	compeq t4 t2 t3
	bnez t4 STR_CONTAINS_END
		ldw t5 t2(0)
		compeq t0 t5 s1
		bnez t0 STR_CONTAINS_END
		
		add t2 t2 one
		br STR_CONTAINS_LOOP
	
	
	STR_CONTAINS_END:
	ret

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

True_Str:
	.word 0x04
	.word 0x54
	.word 0x72
	.word 0x75
	.word 0x65

False_Str:
	.word 0x05
	.word 0x46
	.word 0x61
	.word 0x6C
	.word 0x73
	.word 0x65

New_Line_Str:
	.word 0x01
	.word 0x0A
	
RAM: