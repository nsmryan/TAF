#include <Arduino.h>
#define BUF_SIZE 40
#define DUTY_CYCLE 1

char buffer[BUF_SIZE];

void setup()
{
  Serial.begin(9600);
  pinMode(13, OUTPUT);
}

void loop()
{
  digitalWrite(13, HIGH);
  delay(DUTY_CYCLE);
  digitalWrite(13, LOW);
  delay(DUTY_CYCLE);
}

  //int n;
  //if (Serial.available()>= sizeof(int))
  //{
  //  n = Serial.read() | Serial.read() << 8;
  //  Serial.write(n + 1 & 0x00FF);
  //  Serial.write(n + 1 & 0xFF00);
  //}
