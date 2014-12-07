# Test-Kernel version 0.0.0 for OCA
# This kernel exposes basic functionalities to run tests.

.def KERNEL_VER 0x00000000
.def MESSAGE_QUEUE 0x00001000



# ** PRINT_STR **
# Prints a string to the terminal
# Arguments
#	s0 stream to print to. (0 stdout, 1 stderr)
# 	s1 pointer to the string to print
# Returns: nothing
PRINT_STR:
		
	
	
# Starts a test
START_TEST:

# Ends a test
END_TEST:

# Receives a word from the message queue
RECEIVE:

# Sends a word through the message queue
SEND:

# Value containing the current test that is executed.
CURRENT_TEST:
	.word 0