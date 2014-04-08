require utils.fs
require ffl/car.fs
require arduino.fs

\ Creating commands.

\ Command structure.
struct
  cell% field ardcmd-opcode
  cell% field ardcmd-#args
end-struct ardcmd

1 := next-opcode
: has-next-opcode  ardcmd-opcode next-opcode @ swap ! next-opcode 1+! ;

: unpack-ardcmd dup ardcmd-#args @ swap ardcmd-opcode @ ;
: interpret-command unpack-ardcmd >r compile-args args->cmd r> compile-cell send-cmd ;
: compile-command ardcmd-opcode compile-cell ;
: create-command create ardcmd %allot dup has-next-opcode  ardcmd-#args ! ;
: simple-command create-command 
  does> ardinterpret? if interpret-command else compile-command then ;

: compile-opcode ardcmd-opcode @ compile-cell ;
: compile-backjmp cmd-length - dup ardcommand car-set ;

: chr     1+ ;
: short   1+ ;
: ushort  1+ ;
: int     1+ ;
: uint    1+ ;
: void       ;

\ Arduino word definitions.
get-current
vocabulary ArduinoWords
also ArduinoWords definitions

0 simple-command ard>
0 simple-command >ard
2 simple-command pinMode
2 simple-command digitalWrite
1 simple-command digitalRead
1 simple-command delay
0 simple-command random
2 simple-command randRange
1 simple-command setBaud
1 simple-command analogRead
2 simple-command analogWrite
1 simple-command eepromRead
2 simple-command eepromWrite
1 simple-command arduinoMalloc
1 simple-command arduinoFree
2 simple-command !ard
1 simple-command @ard
2 simple-command c!ard
1 simple-command c@ard
0 simple-command ardSwap
0 simple-command ardDrop
0 simple-command ardDup
0 simple-command echo

2 create-command arddo
  does> compile-opcode cmd-length 0 compile-cell ;

1 create-command ardif
  does> compile-opcode cmd-length 0 compile-cell ; 

0 create-command ardthen
  does> drop compile-backjmp ;

0 create-command ardelse
  does> drop compile-backjmp ;

0 create-command ardloop
  does> compile-opcode compile-backjmp ;

0 create-command ardagain
  does> compile-opcode compile-backjmp compile-cell ;

1 create-command ardwhile
  does> compile-opcode cmd-length  0 compile-cell ;

0 create-command ardbegin
  does> compile-opcode ardcommand car-length@ ;

0 create-command ardrepeat
  does> cmd-length - swap compile-opcode compile-cell ;

set-current
