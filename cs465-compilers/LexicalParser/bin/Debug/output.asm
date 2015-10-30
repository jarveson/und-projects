.386
.model flat,stdcall
option casemap:none
include windows.inc
include masm32.inc
include kernel32.inc
includelib kernel32.lib
includelib masm32.lib
Main PROTO

.data

.data?
inbuf db 10 dup (?)
v1 dd ?
v2 dd ?
v3 dd ?
v4 dd ?
v5 dd ?
v6 dd 10dup (?)
v7 dd 10dup (?)
v8 dd 10dup (?)
v9 dd 10dup (?)
v10 dd 10dup (?)
v11 dd 10dup (?)

.code
start:
invoke Main
invoke ExitProcess, 0
Main Proc
invoke dwtoa, 0, addr inbuf
invoke StdOut, addr inbuf
RET
Main EndP
end start
