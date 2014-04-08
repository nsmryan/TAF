#include "ardc.h"
#include "/usr/share/arduino/libraries/EEPROM/EEPROM.h"


// #define push(x) (*sp = x);(sp++)
// #define pop(x) (*--sp)
int cmdBuf[CMD_BUF_CELLS]; 
int *ps; 
int *rs; 

int *ip;
int *ipEnd;
int *sp;
int *rp;

void setup()
{
  Serial.begin(DEFAULT_BAUD);
  pinMode(13, OUTPUT);
  ps = (int*)malloc(PS_SIZE * sizeof(int));
  rs = (int*)malloc(RS_SIZE * sizeof(int));
  sp = ps;
  rp = rs;
  resetInterpreter();
}

void loop()
{
}

void serialEvent()
{
  if (Serial.available() >= sizeof(int))
  {
    readCommand();
    if (interpretCmds() == OK)
      sendTerminate();
    resetInterpreter();
  }
}

void sendTerminate()
{
  send(COMMAND_FINISHED, NO_ARG);
}

void readCommand()
{
  int words = readCell();
  ipEnd = &cmdBuf[words];
  readCells(cmdBuf, words);
}

int interpretCmds()
{
  while (ip < ipEnd)
  {
    if (interpret() == ERROR)
    {
      return ERROR;
    }
  }
  return OK;
}

int interpret()
{
  long baud;
  int addr;
  int arg;
  int arg2;
  int returnVal = OK;
  int opcode = *ip++;

  switch (opcode)
  {
    case LIT:
      push(*(ip++));
      break;

    case FROM_ARD:
      arg = pop();
      send(SEND_VALUE, arg);
      break;

    case TO_ARD:
      send(GET_VALUE, NO_ARG);
      arg = readCell();
      push(arg);
      break;

    case PIN_MODE:
      arg = pop();
      pinMode(pop(), arg);
      break;
      
    case DIGITAL_WRITE:
      arg = pop();
      digitalWrite(pop(), arg);
      break;
      
    case DIGITAL_READ:
      arg = pop();
      push(digitalRead(arg));
      break;
      
    case DELAY:
      delay(pop());
      break;

    case RAND:
      push(random(65535));
      break;

    case RAND_RANGE:
      arg = pop();
      push(random(pop(), arg));
      break;

    case SET_BAUD:
      baud = getBaud((Baud)pop());
      if (baud != 0)
      {
        Serial.end();
        Serial.begin(baud);
      }
      else
      {
        send(ERROR_INVALID_ARG, baud);
        returnVal = ERROR;
      }
      break;

    case ANALOG_READ:
      push(analogRead(pop()));
      break;
    
    case ANALOG_WRITE:
      arg = pop();
      analogWrite(pop(), arg);
      break;

    case EEPROM_READ:
      addr = pop();
      if (EEPROM_VALID(addr))
      {
        push(EEPROM.read(addr));
      }
      else 
      {
        send(ERROR_INVALID_ARG, addr);
        returnVal = ERROR;
      }
      break;
    
    case EEPROM_WRITE:
      addr = pop();
      if (EEPROM_VALID(addr))
      {
        EEPROM.write(addr, pop());
      }
      else 
      {
        send(ERROR_INVALID_ARG, addr);
      }
      break;

    case MALLOC:
      push((int)malloc((size_t)pop()));
      break;

    case FREE:
      addr = pop();
      free((void*)addr);
      break;

    case STORE:
      addr = pop();
      *((int*)addr) = pop();
      break;

    case FETCH:
      addr = pop();
      push(*((int*)addr));
      break;
      
    case CSTORE:
      addr = pop();
      *((char*)addr) = lowByte(pop());
      break;

    case CFETCH:
      addr = pop();
      push(*((char*)addr));
      break;

    case SWAP:
      arg = pop();
      arg2 = pop();
      push(arg);
      push(arg2);
      break;
      
    case DROP:
      sp--;
      break;
      
    case DUP:
      arg = pop();
      push(arg);
      push(arg);
      break;

    case ECHO:
      send(GET_VALUE, NO_ARG);
      send(SEND_VALUE, readCell());
      break;

    case DO:
      arg = pop();
      arg2 = pop();
      if (arg > arg2)
      {
        rpush(arg);
        rpush(arg2);
      }
      break;

    case IF:
      arg = pop();
      arg2 = *ip++;
      if (arg == 0)ip += arg2;
      break;

    case THEN:
      break;

    case ELSE:
      break;

    case LOOP:
      arg = *ip++;
      arg2 = rpop();
      arg2++;
      rpush(arg2);
      ip -= arg;
      break;

    case AGAIN:
      arg = *ip++;
      ip -= arg;
      break;

    case WHILE:
      arg = pop();
      arg2 = *ip++;
      if (arg == 0)
      {
        ip += arg2;
      }
      break;

    case BEGIN:
      break;

    case REPEAT:
      arg = *ip++;
      ip -= arg;
      break;

    default:
      send(ERROR_INVALID_OPCODE, opcode); 
      returnVal = ERROR;
      break;
  }
  return returnVal;
}

int readCell()
{
  int n = 0;
  while (Serial.available() < sizeof(int)) { } //busy wait
  n |= (Serial.read() & 0x00FF) << 8;
  n |= (Serial.read() & 0x00FF) << 0;
  return n;
}

void readCells(int *buf, int n)
{
  for (int i = 0; i < n; i++)
  {
    buf[i] = readCell();
  }
}

void writeCell(int n)
{
  Serial.write(highByte(n));
  Serial.write(lowByte(n));
}

void writeCells(int *buf, int n)
{
  for (int i = 0; i < n; i++)
  {
    writeCell(buf[i]);
  }
}

long getBaud(Baud baud)
{
  switch (baud)
  {
    case Baud300: return 300L;
    case Baud1200: return 1200L;
    case Baud2400: return 2400L;
    case Baud4800: return 4800L;
    case Baud9600: return 9600L;
    case Baud14400: return 14400L;
    case Baud19200: return 19200L;
    case Baud28800: return 28800L;
    case Baud38400: return 38400L;
    case Baud57600: return 57600L;
    case Baud115200: return 115200L;
    default: return 0;
  }
}

void send(int code, int arg)
{
  writeCell(code);
  writeCell(arg);
}

void resetInterpreter()
{
  ip = (int*)cmdBuf;
  ipEnd = (int*)cmdBuf;
  while (Serial.available() > 0)Serial.read();
}

void blink(int ms)
{
  digitalWrite(13, HIGH);
  delay(ms);
  digitalWrite(13, LOW);
  delay(ms);
}

void push(int item)
{
  *sp = item;
  sp++;
}

int pop()
{
  sp--;
  return *sp;
}

void rpush(int item)
{
  *rp = item;
  rp++;
}

int rpop()
{
  rp--;
  return *rp;
}

