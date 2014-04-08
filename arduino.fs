require ffl/str.fs
require ffl/spf.fs
require ffl/car.fs
require ffl/enm.fs
require utils.fs


0xFFFF constant ERROR

s" Arduino exception" create-exception arduino-error
s" Invalid error code from Arduino" create-exception invalid-error-code
s" Invalid return in command definition" create-exception invalid-return-error 
s" Invalid buad rate" create-exception invalid-baud 
: process-error ." Arduino returned error: " type ." , with return value: " . arduino-error ;

\ Communication functions
0 value arduino-fh

: wrds 2* ;
: write-arduino wrds arduino-fh write-file throw ;
: read-arduino wrds arduino-fh read-file throw drop ;

: arduino>byte arduino-fh key-file ;
: combine-bytes swap 8 lshift or ;
: arduino> arduino>byte arduino>byte combine-bytes ;

: byte>arduino arduino-fh emit-file throw ;
: first-byte 0x00FF and ;
: second-byte 0xFF00 and 8 rshift ;
: >arduino dup second-byte byte>arduino first-byte byte>arduino ;

\ Return code handlers.
0 car-create return-handlers 
: add-return-handler return-handlers car-append ;

:noname drop rdrop rdrop ;  add-return-handler \ 0 is end command.
:noname ;                   add-return-handler \ 1 is get value.
:noname drop >arduino ;     add-return-handler \ 2 is send value.
:noname s" opcode not recognized" process-error ; add-return-handler
:noname s" invalid argument"      process-error ; add-return-handler

\ Enumerations
begin-enumeration
  enum: READ
  enum: WRITE
end-enumeration

begin-enumeration
  enum: LOW
  enum: HIGH
end-enumeration

begin-enumeration
  enum: INPUT
  enum: OUTPUT
end-enumeration

\ State.
0 constant ardinterpret
1 constant ardcompile
ardinterpret := ardstate
: ardinterpret?  ardstate @ ardinterpret = ;
: ardcompile?  ardstate @ ardcompile = ;
: ardinterpreting ardinterpret ardstate ! ;
: ardcompiling ardcompile ardstate ! ;

\ Commands.
0 car-create ardcommand
0 car-create ardargs
0 := cmdsize
0 constant ardlit-opcode

: cmd-length ardcommand car-length@ ;

\ Executing commands.
: clear-command  ardcommand car-length@ 0 ?do ardcommand car-pop drop loop ;

: invalid-return   ." Return code from Arduino: " . invalid-error-code ;
: process-return   return-handlers car-get execute ;
: get-return       arduino> arduino> swap ;
: +return-handler  return-handlers car-index? invert if nip invalid-return then ;
: process-loop     begin get-return dup +return-handler process-return again ;

: >cmd-length ardcommand car-length@ >arduino ;
: not-empty? dup car-length@ 0> ;
: ship-command  ( - )
  >cmd-length ardcommand begin not-empty? while dup car-pop >arduino repeat drop ;

: send-cmd  ship-command process-loop ;

: compile-cell  ardcommand car-prepend ;
: compile-arg ardargs car-append ;
: ardliteral   ardlit-opcode compile-cell compile-cell ;
: compile-args  0 ?do compile-arg ardlit-opcode compile-arg loop ;
: args->cmd ardargs car-length@ 0 ?do ardargs car-pop compile-cell loop ;

