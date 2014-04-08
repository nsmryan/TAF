require ffl/str.fs
require ffl/enm.fs


begin-enumeration
  enum: Baud300
  enum: Baud1200
  enum: Baud2400
  enum: Baud4800 
  enum: Baud9600
  enum: Baud14400
  enum: Baud19200
  enum: Baud28800
  enum: Baud38400
  enum: Baud57600
  enum: Baud115200 
end-enumeration

: cells% cell% rot * ;

: for-each postpone cells
           postpone over
           postpone +
           postpone swap
           postpone do ; immediate
: end-each postpone cell postpone +loop ; immediate

: 1+! 1 swap +! ;
: !++ tuck ! cell+ ;
: @++ dup cell+ swap @ ;

: latest-xt latest name>int ;
: := variable latest-xt execute ! ;

: str-system str-get system ;
: str-type str-get type ;

: create-exception exception create , does> @ throw ;

: :q bye ;

: off? @ false = ;
: on?  @ true = ;

: rdrop postpone r> postpone drop ; immediate

: call >r >r swap r> r> ;
