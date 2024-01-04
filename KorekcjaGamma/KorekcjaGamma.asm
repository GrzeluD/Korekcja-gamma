.data
const_255 dd 255.0
pow_arg dq 0.0    ; drugi argument dla funkcji pow

EXTERN pow: PROC
PixelMod proc

cvtsi2ss xmm0, RCX               ; Konwertuje wartoœæ na zmiennoprzecinkow¹ i przechowuje w xmm0
movss xmm1, dword ptr [const_255]; Za³aduj sta³¹ 255.0 do xmm1
divss xmm0, xmm1                ; Dzieli wartoœæ w xmm0 przez wartoœæ w xmm1

; Za³aduj drugi argument dla funkcji pow do pow_argw
mov [pow_arg], RDX
cvtsi2ss xmm1, dword ptr [pow_arg] ; Konwertuje wartoœæ pow_arg na zmiennoprzecinkow¹ i przechowuje w xmm1

    ; Wywo³aj funkcjê pow z biblioteki C
call pow                ; Wywo³anie funkcji pow

    ; Wynik z pow znajduje siê w xmm0, konwertuj go z powrotem na wartoœæ ca³kowit¹
cvttss2si rax, xmm0

ret
PixelMod endp
end