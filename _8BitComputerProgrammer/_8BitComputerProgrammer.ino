const byte numChars = 27;
char receivedChars[numChars]; 
int memData[16][12];
byte memCounter = 0;
boolean newData = false;
boolean dataStart = false;

//Pin assignments///////////////
const byte strtClkPin = 36;
const byte stopClkPin = 37;
const byte rsetPin = 2; 
const byte progEnPin = 3;
const byte clkStpPin = 4;
const byte clkSetPin1 = 5;
const byte clkSetPin2 = 6;
const byte clkSetPin3 = 7;
const byte clkSetPin4 = 8;
////////////////////////////////

void setup() {
  setPinModes();
  Serial.begin(115200);
  
  digitalWrite(progEnPin, LOW);
  stopClock();
  resetDevice();
  digitalWrite(clkSetPin1, LOW);
  digitalWrite(clkSetPin2, LOW);
  digitalWrite(clkSetPin3, LOW);
  digitalWrite(clkSetPin4, LOW);
}

void loop() {
    rxData(); 
    rxMemDataEnd();
}

void rxData() {
    static byte count = 0;
    char ch;
    
	//Check if serail data is available.
    while(Serial.available() > 0 && newData == false) {
        //Read and remove character from the serial buffer.
        ch = Serial.read();

        //Check if it's the start of the data.
        if(ch == '_'){
          dataStart = true;
        }

        if(dataStart && ch != '_'){   
          if(ch != '\n'){
              receivedChars[count] = ch;
              count++;
              if(count >= numChars){
                  count = numChars - 1;
              }
          }
          else{
              //Terminate string.
              receivedChars[count] = '\0';

              char command[5];
              
              getCommand(command);
              
              if(strcmp(command, "prog") == 0){
                getMemData(); 
              }else if(strcmp(command, "strt") == 0){
                startClock();
                Serial.print("strt done");
              }else if(strcmp(command, "stop") == 0){
                stopClock();
                Serial.print("stop done");
              }else if(strcmp(command, "step") == 0){
                clockStep();
                Serial.print("step done");
              }else if(strcmp(command, "rset") == 0){
                resetDevice();
                Serial.print("rset done");
              }else if(strcmp(command, "clks") == 0){           
                setClockSpeed();
                Serial.print("clks done");
              }else if(strcmp(command, "prst") == 0){
                programmerReset();
                Serial.print("prst done");
              }else{
                Serial.print("Bad command!");
              }
              
              count = 0;     
              newData = true;
              dataStart = false;              
            }
       }
    }   
}

static void programmerReset(){
  //Reset variables.
  memCounter = 0;
  newData = false;
  dataStart = false;

  //Empty serial buffer.
  while(Serial.available() > 0){
    Serial.read();
  }
}

//Currently not used.
static void getInfo(int *memInfo){
  char memAddressLengthStr[3];
  char memLocationLengthStr[3];
  char memLocationCountStr[6];

  for(int i = 4; i <= 12; i++){
    if(i <= 5){
      memAddressLengthStr[i-4] = receivedChars[i];
      if(i-4 == 1){
        memAddressLengthStr[i-3] = '\0';
      }
    }else if(i <= 7){
      memLocationLengthStr[i-6] =  receivedChars[i];
      if(i-6 == 1){
        memLocationLengthStr[i-5] = '\0';
      }
    }else if(i <= 12){
      memLocationCountStr[i-8] =  receivedChars[i];

      if(i-11 == 1){
        memLocationCountStr[i-7] = '\0';
      }
    }
  }

  //Convert char to 
  memInfo[0] = atoi(memAddressLengthStr);
  memInfo[1] = atoi(memLocationLengthStr);
  memInfo[2] = atoi(memLocationCountStr);

}

static void getClockSpeed(int *clkSpeedValues){
  for(int i = 4; i <= 7; i++){
    clkSpeedValues[i-4] = (receivedChars[i] - '0');
  } 
}

static void setClockSpeed(){
  int clkSpeedValues[4];
  
  getClockSpeed(clkSpeedValues);
  
  digitalWrite(clkSetPin1, getState(clkSpeedValues[0]));
  digitalWrite(clkSetPin2, getState(clkSpeedValues[1]));
  digitalWrite(clkSetPin3, getState(clkSpeedValues[2]));
  digitalWrite(clkSetPin4, getState(clkSpeedValues[3]));
}

static void setPinModes(){
  //Data pins///////////////
  pinMode(22, OUTPUT); //LSB
  pinMode(23, OUTPUT);
  pinMode(24, OUTPUT);
  pinMode(25, OUTPUT);
  
  pinMode(26, OUTPUT);
  pinMode(27, OUTPUT);
  pinMode(28, OUTPUT);
  pinMode(29, OUTPUT); //MSB
  //////////////////////////

  //Mem. Add. Pins/////////
  pinMode(30, OUTPUT);
  pinMode(31, OUTPUT);
  pinMode(32, OUTPUT);
  pinMode(33, OUTPUT);
  /////////////////////////

  //Clk
  pinMode(34, OUTPUT);
  //RAM read enable
  pinMode(35, OUTPUT);

  //Start
  pinMode(strtClkPin, OUTPUT);
  //Stop  
  pinMode(stopClkPin, OUTPUT);
  
  //Clear
  pinMode(rsetPin, OUTPUT);
  //Program enable
  pinMode(progEnPin, OUTPUT);
  //Step
  pinMode(clkStpPin, OUTPUT);

  //Clk speed selection//
  pinMode(clkSetPin1, OUTPUT);
  pinMode(clkSetPin2, OUTPUT);
  pinMode(clkSetPin3, OUTPUT);
  pinMode(clkSetPin4, OUTPUT);
  ///////////////////////
}

static void getMemData(){
  for(int i = 0; i <= 11 ; i++){
    memData[memCounter][i] = (receivedChars[i+13] - '0');
  }
                
  //Next mem. position.
  memCounter++;
}

static void getCommand(char *command){
  for(int i = 0; i <= 3; i++){
    command[i] = receivedChars[i];
  }

  //Terminate string.
  command[4] = '\0';
}

static void rxMemDataEnd(){
    //If new data is available.
    if(newData == true){
      //Check if data transfer is over.
      if(memCounter == 16){
        writeToRAM();
        
        //Reset.
        memCounter = 0;

        Serial.print("prog done");
      }else{
        //Signal that the data was read.
        Serial.print("ok");
      }

      //Reset.
      newData = false;
      delay(10);
    }
}

static void clockStep(){  
  digitalWrite(clkStpPin, HIGH);
  delay(50);
  digitalWrite(clkStpPin, LOW);
}

static void resetDevice(){
  digitalWrite(rsetPin, HIGH);
  delay(50);
  digitalWrite(rsetPin, LOW);
}

static void startClock(){
  digitalWrite(strtClkPin, HIGH);
  delay(50);
  digitalWrite(strtClkPin, LOW);
}

static void stopClock(){
  digitalWrite(stopClkPin, HIGH);
  delay(50);
  digitalWrite(stopClkPin, LOW);
}

static void writeToRAM(){
  stopClock();
  resetDevice();
  digitalWrite(progEnPin, HIGH);
  delay(10);
  
  //Iterate through the machine code array and toggle the appropriate pins.
  for(int i = 0; i < 16; i++){
      //Bus
      digitalWrite(22, getState(memData[i][0]));
      digitalWrite(23, getState(memData[i][1]));
      digitalWrite(24, getState(memData[i][2]));
      digitalWrite(25, getState(memData[i][3]));
      digitalWrite(26, getState(memData[i][4]));
      digitalWrite(27, getState(memData[i][5]));
      digitalWrite(28, getState(memData[i][6]));
      digitalWrite(29, getState(memData[i][7]));
    
      //RAM Adress
      digitalWrite(30, getState(memData[i][8]));
      digitalWrite(31, getState(memData[i][9]));
      digitalWrite(32, getState(memData[i][10]));
      digitalWrite(33, getState(memData[i][11]));


      //RAM RE
      digitalWrite(35, HIGH);
      delay(10);

      //Delay clock so the data can get to the destination before the rising edge of the clock.
      //delay(100);
      
      //CLK
      digitalWrite(34, HIGH);
      delay(10);
      digitalWrite(34, LOW);
      delay(10);
      
      //RAM RE
      digitalWrite(35, LOW);

      //Delay before next program cycle.
      delay(10);
  }

  //PROG EN 
  digitalWrite(progEnPin, LOW);
}

static bool getState(int bitValue){
    if(bitValue == 0){
      return false;
    }else if(bitValue == 1){
      return true;
    }
}

