.data
const_255 dd 255.0
pow_arg dq 0.0 

vectorG dd 0.25

.code
PixelMod PROC


	vmovdqu ymm0, ymmword ptr [rdx] ;Getting the vector SegmentR from the adress pointed on the rcx and save it to ymm0.
	vmovdqu ymm1, ymmword ptr [r8] 
	vmovdqu ymm2, ymmword ptr [r9] 
	vmovdqu ymm4, ymmword ptr [rcx]
	

	vcvtdq2ps ymm0, ymm0 ;liczby zmiennoprzecinkowe jednej precyzji
	vcvtdq2ps ymm1, ymm1
	vcvtdq2ps ymm2, ymm2 

	vbroadcastss ymm3, [const_255]  ; Rozg³asza pojedyncz¹ wartoœæ 32-bitow¹ do wszystkich komórek ymm3
	
	vdivps ymm0, ymm0, ymm3;
	vdivps ymm1, ymm1, ymm3;
	vdivps ymm2, ymm2, ymm3;

	vmulps ymm2, ymm2, ymm4

	vmulps ymm0, ymm0, ymm3;
	vmulps ymm1, ymm1, ymm3; 
	vmulps ymm2, ymm2, ymm3; 

	vcvtps2dq ymm0, ymm0  ; Konwertuje zmiennoprzecinkowe wartoœci w ymm0 na ca³kowite
	vcvtps2dq ymm1, ymm1
	vcvtps2dq ymm2, ymm2
 

	vmovdqu ymmword ptr [rdx], ymm0
	vmovdqu ymmword ptr [r8], ymm1
	vmovdqu ymmword ptr [r9], ymm2


	ret
PixelMod ENDP
end