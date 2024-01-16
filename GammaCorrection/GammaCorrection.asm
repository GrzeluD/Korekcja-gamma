;extern vectorPow:PROC
.data
const_255 dd 255.0
const_1 dd 1.0
const_0 dd 0.0

myGamma REAL4 0.0


.code

PixelMod PROC
	;vcvtdq2ps ymm4, ymm4
	;ymm 3 is for 255, ymm 4 contains the gamma value, ymm5 contain ones, 
	;ymm6,7 comparision results, ymm8 for zeros, ymm9 10 11 temporary results
	vmovdqu ymm0, ymmword ptr [rdx] ;Getting the vector SegmentR from the adress pointed on the rcx and save it to ymm0.
	vmovdqu ymm1, ymmword ptr [r8] 
	vmovdqu ymm2, ymmword ptr [r9]
	vmovdqu ymm4, ymmword ptr [rcx]
	
	vcvtdq2ps ymm0, ymm0 ;vector to float
	vcvtdq2ps ymm1, ymm1
	vcvtdq2ps ymm2, ymm2 

	vbroadcastss ymm3, [const_255]  ; Put 255 mask
	vbroadcastss ymm5, [const_1] ;Put 1 mask
	vbroadcastss ymm8, [const_0] ;Put 0 mask

	vdivps ymm0, ymm0, ymm3
	vdivps ymm1, ymm1, ymm3
	vdivps ymm2, ymm2, ymm3

	vmovdqu ymm9 , ymm0
	vmovdqu ymm10 , ymm1
	vmovdqu ymm11 , ymm2

	; Porównanie ymm4 i ymm5. 
	vcmppd ymm6, ymm4, ymm5, 1   ; bit mask of comparrision
	vcmppd ymm7, ymm4, ymm5, 6  

	; Sprawdzanie wyniku porównania. U¿ywamy instrukcji test do sprawdzenia, czy jakiekolwiek bity s¹ ustawione
	vptest ymm6, ymm6 
	jnz less           ; ymm4 < ymm5

	vptest ymm7, ymm7  
	jnz greater        ; ymm4 > ymm5

	less:
	
	vsubps ymm9, ymm5, ymm9
	vsubps ymm10, ymm5, ymm10
	vsubps ymm11, ymm5, ymm11

	vmulps ymm9, ymm9, ymm4
	vmulps ymm10, ymm10, ymm4
	vmulps ymm11, ymm11, ymm4

	vaddps ymm0, ymm0, ymm9
	vaddps ymm1, ymm1, ymm10
	vaddps ymm2, ymm2, ymm11

	jmp programexit

	greater:

	vsubps ymm4, ymm4, ymm5

	vmulps ymm9, ymm9, ymm4
	vmulps ymm10, ymm10, ymm4
	vmulps ymm11, ymm11, ymm4
	
	vsubps ymm0, ymm0, ymm9
	vsubps ymm1, ymm1, ymm10
	vsubps ymm2, ymm2, ymm11

	jmp programexit

		;vcmppd ymm6, ymm4, ymm5, 0 ;Testing if the vector is 0
		;vptest ymm6, ymm6
		;jnz programexitgreater
		;vmulps ymm9, ymm9, ymm0
		;vmulps ymm10, ymm10, ymm1
		;vmulps ymm11, ymm11, ymm2
		;vsubps ymm4, ymm4, ymm5
		;jmp greater
	;programexitgreater:

	;vmovdqu ymm0 , ymm9
	;vmovdqu ymm1 , ymm10
	;vmovdqu ymm2 , ymm11

	programexit:

	vmulps ymm0, ymm0, ymm3
	vmulps ymm1, ymm1, ymm3 
	vmulps ymm2, ymm2, ymm3 

	vcvtps2dq ymm0, ymm0  ; Float to int
	vcvtps2dq ymm1, ymm1
	vcvtps2dq ymm2, ymm2
 

	vmovdqu ymmword ptr [rdx], ymm0
	vmovdqu ymmword ptr [r8], ymm1
	vmovdqu ymmword ptr [r9], ymm2


	ret
PixelMod ENDP
END
