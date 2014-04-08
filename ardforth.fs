require ffl/arg.fs
require ffl/car.fs
require definitions.fs
require arduino.fs


str-create device-name s" /dev/ttyACM0" device-name str-set 
19200 value baud-rate
0 car-create valid-bauds
: add-baud valid-bauds car-append ;
300   add-baud  1200 add-baud 2400   add-baud 4800  add-baud
9600  add-baud 14400 add-baud 19200  add-baud 28800 add-baud
38400 add-baud 57600 add-baud 115200 add-baud
: valid-baud? valid-bauds car-has? ;
: >baud valid-bauds car-get ;
: baud> valid-bauds car-find -1 = if invalid-baud then ;

: upload-sketch s" cd interpreter; make upload; cd .." system ;

: open-arduino device-name str-get r/w bin open-file throw is arduino-fh ;
: +open         ?dup 0= if rdrop then ;
: close-arduino arduino-fh +open close-file throw  0 is arduino-fh ;

\ TTY setup command
str-create tty-cmd
: configure-tty  
  tty-cmd str-clear
  baud-rate device-name str-get s" stty -F %s %d ignbrk -brkint -icrnl -imaxbel -opost -onlcr -isig -icanon -iexten -echo -echoe -echok -echoctl -echoke noflsh -ixon -crtscts"
  tty-cmd spf-set
  tty-cmd str-system ;


\ Basic high level definitions
: <[ ardcompiling ;
: ]> ;
: ]>send send-cmd ;
: <( ardinterpreting ;
: )> ;

: seconds 1000 * ;

13 constant LED
: led-start LED OUTPUT pinMode ;
: led-on LED HIGH digitalWrite ;
: led-off LED LOW digitalWrite ;

: blink dup led-on ms led-off ms ;
: blinking swap 0 ?do dup blink loop drop ;

: starting-delay 1000 ms ; \ Seems to need a delay before first use.

\ Main entry point
: arduino close-arduino configure-tty open-arduino  also ArduinoWords starting-delay led-start ;

\ Argument parsing.

true := startup \ start arduino after argument parsing?

\ Create parser
s" ardforth"      \ program name
s" [OPTIONS]"     \ program usage
s" v0.1"          \ program version
s" "              \ program extra info
arg-new value cmd-line-args

\ Add the default help and version options.
cmd-line-args arg-add-help-option
cmd-line-args arg-add-version-option

\ Variable for the verbose switch>
variable verbose  verbose off

\ Option ids are stored as xts in a linked list.
0 car-create option-list
: next-option option-list car-length@ ;
: add-option option-list car-append ;
s" Arg parser finished early" exception constant parser-error
 
\ Options.
:noname startup off parser-error throw ;               add-option
:noname startup off ." Non option found: " type cr ;   add-option
:noname startup off cmd-line-args arg-print-help ;     add-option
:noname startup off cmd-line-args arg-print-version ;  add-option

\ Add the -v/--verbose option switch.
char v                              \ Short option name
s" verbose"                         \ Long option name
s" activate verbose mode"           \ Description
true                                \ Switch
next-option                         \ Option id
cmd-line-args arg-add-option
:noname verbose on ." Verbose is on" cr ;  add-option

\ Add sketch upload option.
char s                              \ Short option name
s" sketch"                          \ Long option name
s" compile and upload sketch"       \ Description
true                                \ Parameter
next-option                         \ Option id
cmd-line-args arg-add-option
:noname upload-sketch ; add-option

\ Add device name option.
char f                              \ Short option name
s" file=FILE"                       \ Long option name
s" set path to Arduino device"      \ Description
false                               \ Parameter
next-option                         \ Option id
cmd-line-args arg-add-option
:noname 2dup device-name str-set ." Arduino device name: " type cr ; add-option

\ Add baud rate option.
char b                              \ Short option name
s" baud=BAUD"                       \ Long option name
s" set baud rate of connection"     \ Description
false                               \ Parameter
next-option                         \ Option id
cmd-line-args arg-add-option
:noname s>unumber? 2drop dup valid-baud?
        if is baud-rate ." Arduino baud rate: " baud-rate . cr
        else . ."  is not a valid baud rate" invalid-baud then ; add-option

: parse-successful? arg.done = ;
: process-option option-list car-get execute ;
: parse-not-done? dup arg.done <> swap arg.error <> and ;
: parse-next cmd-line-args arg-parse ;
: parse-options begin parse-next dup parse-not-done? while process-option repeat ;

