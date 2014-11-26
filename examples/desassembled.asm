addi s0, bp, 14
addi s1, zero, 3
callr s1
addi s6, zero, 256
stw one, 27(bp)
addi s0, bp, 27
sub s6, s6, one
stw s6, 28(bp)
addi s1, zero, 3
callr s1
bnez s6, -4
addi s0, zero, 0
addi t0, zero, 2
callr t0
add t2, zero, zero
add s12, zero, zero
add s41, zero, zero
add s48, zero, zero
add s48, zero, zero
add r111, zero, zero
add t22, zero, zero
add s27, zero, zero
add r111, zero, zero
add r114, zero, zero
add s48, zero, zero
add s40, zero, zero
add t23, zero, zero
