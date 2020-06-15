void setup() {
  //Set pins.
  pinMode(0, OUTPUT);
  pinMode(1, OUTPUT);
  pinMode(2, OUTPUT);
  pinMode(3, OUTPUT);
  pinMode(4, OUTPUT);
  pinMode(5, OUTPUT);
  pinMode(6, OUTPUT);
  pinMode(7, OUTPUT);
  pinMode(8, OUTPUT);
  pinMode(9, OUTPUT);
  pinMode(10, OUTPUT);
  pinMode(11, OUTPUT);
  pinMode(12, OUTPUT);
  pinMode(13, OUTPUT);

  //Array containing your machine code.
  int dataArray [16][12]{
        //LSB                 MSB     MEM. ADDR.          
        { 1, 0, 0, 0,  0, 1, 1, 1,    0, 0, 0, 0},  //1
        { 0, 1, 0, 0,  1, 1, 1, 1,    1, 0, 0, 0},  //2
        { 1, 1, 0, 0,  0, 0, 0, 0,    0, 1, 0, 0},  //3
        { 0, 0, 1, 0,  0, 0, 0, 0,    1, 1, 0, 0},  //4
        { 0, 0, 0, 0,  0, 0, 0, 0,    0, 0, 1, 0},  //5
        { 0, 0, 0, 0,  0, 0, 0, 0,    1, 0, 1, 0},  //6
        { 0, 0, 0, 0,  0, 0, 0, 0,    0, 1, 1, 0},  //7
        { 0, 0, 0, 0,  0, 0, 0, 0,    1, 1, 1, 0},  //8
        { 0, 0, 0, 0,  0, 0, 0, 0,    0, 0, 0, 1},  //9
        { 0, 0, 0, 0,  0, 0, 0, 0,    1, 0, 0, 1},  //10
        { 0, 0, 0, 0,  0, 0, 0, 0,    0, 1, 0, 1},  //11
        { 0, 0, 0, 0,  0, 0, 0, 0,    1, 1, 0, 1},  //12
        { 0, 0, 0, 0,  0, 0, 0, 0,    0, 0, 1, 1},  //13
        { 0, 0, 0, 0,  0, 0, 0, 0,    1, 0, 1, 1},  //14
        { 0, 0, 1, 0,  0, 0, 0, 0,    0, 1, 1, 1},  //15
        { 0, 1, 0, 0,  0, 0, 0, 0,    1, 1, 1, 1},  //16
  };

  delay(1000);
  writeToRAM(dataArray);
  resetLines();
}

void loop() {

}

static void resetLines(){
  //Bus
  digitalWrite(0, LOW);
  digitalWrite(1, LOW);
  digitalWrite(2, LOW);
  digitalWrite(3, LOW);
  digitalWrite(4, LOW);
  digitalWrite(5, LOW);
  digitalWrite(6, LOW);
  digitalWrite(7, LOW);

  //RAM Adress
  digitalWrite(8, LOW);
  digitalWrite(9, LOW);
  digitalWrite(10, LOW);
  digitalWrite(11, LOW);

  //CLK
  digitalWrite(12, LOW);

  //RAM RE
  digitalWrite(13, LOW);
}

static void writeToRAM(int dataArray [16][12] ){
  //Iterate through the machine code array and toggle the appropriate pins.
  for(int i = 0; i < 16; i++){
      //Bus
      digitalWrite(0, getState(dataArray[i][0]));
      digitalWrite(1, getState(dataArray[i][1]));
      digitalWrite(2, getState(dataArray[i][2]));
      digitalWrite(3, getState(dataArray[i][3]));
      digitalWrite(4, getState(dataArray[i][4]));
      digitalWrite(5, getState(dataArray[i][5]));
      digitalWrite(6, getState(dataArray[i][6]));
      digitalWrite(7, getState(dataArray[i][7]));
    
      //RAM Adress
      digitalWrite(8, getState(dataArray[i][8]));
      digitalWrite(9, getState(dataArray[i][9]));
      digitalWrite(10, getState(dataArray[i][10]));
      digitalWrite(11, getState(dataArray[i][11]));


      //RAM RE
      digitalWrite(13, HIGH);
      delay(10);

      //Delay clock so the data can get to the destination before the rising edge of the clock.
      //delay(100);
      
      //CLK
      digitalWrite(12, HIGH);
      delay(100);
      digitalWrite(12, LOW);
      delay(10);
      
      //RAM RE
      digitalWrite(13, LOW);

      //Delay before next program cycle.
      delay(100);
  }
}

static bool getState(int bitValue){
    if(bitValue == 0){
      return false;
    }else{
      return true;
    }
}
