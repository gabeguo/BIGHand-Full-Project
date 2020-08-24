#include "VernierLib.h" //include Vernier functions in this sketch
VernierLib Vernier; //create an instance of the VernierLib library

//create global variable to store sensor reading
float sensorReading0; 
float sensorReading2;

//for calibration
float slope = 175.416;
float intercept = -19.295;

const long time_step = 25;

void setup() {
  Serial.begin(9600); //setup communication to display
  Vernier.autoID(); //identify the sensor being used
}
 
void loop() {
  //read sensor in port A0
  sensorReading0 = Vernier.readSensor(); //read one data value
  Serial.print(sensorReading0); //print data value 
  
  Serial.print(" "); //print a space

  //read sensor in port A2
  sensorReading2 = readOtherSensor();
  Serial.print(sensorReading2); //end line

  Serial.println(" ");  //print space to avoid format issues
  
  delay(time_step); //wait
}

/*
  SerialEvent occurs whenever a new data comes in the hardware serial RX. This
  routine is run between each time loop() runs, so using delay inside loop can
  delay response. Multiple bytes of data may be available.
*/
void serialEvent() {
  while (Serial.available()) {
    // get the new byte:
    char inChar = (char)Serial.read();
    setup();
  }
}

float readOtherSensor() {
  float rawCount = analogRead(A2); //read one data value (0-1023)
  float voltage = rawCount/1023*5; //convert raw count to voltage (0-5V)
  float theSensorValue = slope*voltage+intercept; //convert to sensor value with linear calibration equation
  return theSensorValue;
}
