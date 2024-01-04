.data
const_255 dd 255.0
pow_arg dq 0.0    ; drugi argument dla funkcji pow

EXTERN pow: PROC
PixelMod proc

cvtsi2ss xmm0, RCX               ; Konwertuje warto�� na zmiennoprzecinkow� i przechowuje w xmm0
movss xmm1, dword ptr [const_255]; Za�aduj sta�� 255.0 do xmm1
divss xmm0, xmm1                ; Dzieli warto�� w xmm0 przez warto�� w xmm1

; Za�aduj drugi argument dla funkcji pow do pow_argw
mov [pow_arg], RDX
cvtsi2ss xmm1, dword ptr [pow_arg] ; Konwertuje warto�� pow_arg na zmiennoprzecinkow� i przechowuje w xmm1

    ; Wywo�aj funkcj� pow z biblioteki C
call pow                ; Wywo�anie funkcji pow

    ; Wynik z pow znajduje si� w xmm0, konwertuj go z powrotem na warto�� ca�kowit�
cvttss2si rax, xmm0

ret
PixelMod endp
end