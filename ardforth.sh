#! /usr/bin/gforth

warnings off

require ardforth.fs

\ For development
: edit s" vim -S forth" system ;

\ Parse the command line arguments.
parse-options parse-successful? startup on? and

\ Start up arduino.
[if] arduino [else] bye [then]
