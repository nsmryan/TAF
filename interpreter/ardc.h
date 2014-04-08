//General constants.
#define ERROR         (-1)
#define OK             (0)

#define PS_SIZE (16)
#define RS_SIZE (16)

#define CMD_BUF_CELLS (128)

#define DEFAULT_BAUD (19200L)

//Test address is within eeprom address range.
#define EEPROM_VALID(x) (x >= 0 && x < 512)

//Wait time for serial commands.
#define SERIAL_WAIT   (1000)
//Delay between checks for serial command.
#define SERIAL_DELAY    (10)
//Iterations before giving up on receiving a command.
#define SERIAL_COUNT (SERIAL_WAIT / SERIAL_DELAY)

//Opcodes.
#define LIT            (0)
#define FROM_ARD       (1)
#define TO_ARD         (2)
#define PIN_MODE       (3)
#define DIGITAL_WRITE  (4)
#define DIGITAL_READ   (5)
#define DELAY          (6)
#define RAND           (7)
#define RAND_RANGE     (8)
#define SET_BAUD       (9)
#define ANALOG_READ    (10)
#define ANALOG_WRITE   (11)
#define EEPROM_READ    (12)
#define EEPROM_WRITE   (13)
#define MALLOC         (14)
#define FREE           (15)
#define STORE          (16)
#define FETCH          (17)
#define CSTORE         (18)
#define CFETCH         (19)
#define SWAP           (20)
#define DROP           (21)
#define DUP            (22)
#define ECHO           (23)
#define DO             (24)
#define IF             (25)
#define THEN           (26)
#define ELSE           (27)
#define LOOP           (28)
#define AGAIN          (29)
#define WHILE          (30)
#define BEGIN          (31)
#define REPEAT         (32)

#define CMD_MAX_ARGS  (10)
#define MSG_MAX_LENGTH (CMD_MAX_ARGS + 1)

#define NO_ARG (0)

//Return codes.
#define COMMAND_FINISHED     (0)
#define SEND_VALUE           (1)
#define GET_VALUE            (2)
#define ERROR_INVALID_OPCODE (3)
#define ERROR_INVALID_ARG    (4)

typedef enum 
{
  Baud300,
  Baud1200,
  Baud2400,
  Baud4800,
  Baud9600,
  Baud14400,
  Baud19200,
  Baud28800,
  Baud38400,
  Baud57600,
  Baud115200
} Baud;

void push(int item);
int pop();
void rpush(int item);
int rpop();

void readCommand();
int interpret();
void sendTerminate();
int interpretCmds();
void send(int code, int arg);
void resetInterpreter();
void blink(int ms);

int readCell();
void readCells(int *buf, int n);

void writeCell(int n);
void writeCells(int *buf, int n);

long getBaud(Baud baud);


