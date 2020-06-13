//Include library
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>

Adafruit_SSD1306 display(4);

void setup() {    
    //Initialize display by providing the diplay type and it's I2C address.
    display.begin(SSD1306_SWITCHCAPVCC, 0x3C);

    //Display setup.
    display.clearDisplay();
    display.setTextSize(1);
    display.setTextColor(WHITE);

	//Pins setup.
    pinMode(2, INPUT);
    pinMode(3, INPUT);
    pinMode(4, INPUT);
    pinMode(5, INPUT);
    pinMode(6, INPUT);
    pinMode(7, INPUT);
    pinMode(8, INPUT);
    pinMode(9, INPUT);
}
 
void loop() {    
      //Clear display.
      display.clearDisplay();

      int  binVal[8];
 
	  //Iterate through the inputs. 
      for(int i = 2; i < 10; i++){ 
		//Read the value and check if it's a 0 or 1.
        if(digitalRead(i) == HIGH){
          binVal[i-2] = 1; 
        }else{
          binVal[i-2] = 0;
        }   
      }

      //Convert bin to dec.
      int busValue = toDECfromBIN(binVal);
      
      //CPU Text value display.
      display.setCursor(0,5);
      //display.print("BUS Contents: " + String(busValue));
      display.print("Out. Register: " + String(busValue));

	   
      //1 
      display.drawCircle(10, 20, 4, WHITE); //Draw circle.
      
      if(binVal[0] == 1){
        display.fillCircle(10, 20, 4, WHITE); //Fill circle if a 1 is present.
      }

      //2
      display.drawCircle(20, 20, 4, WHITE);
      
      if(binVal[1] == 1){
        display.fillCircle(20, 20, 4, WHITE);
      }

      //3
      display.drawCircle(30, 20, 4, WHITE);
      
      if(binVal[2] == 1){
        display.fillCircle(30, 20, 4, WHITE);
      }

      //4
      display.drawCircle(40, 20, 4, WHITE);
      
      if(binVal[3] == 1){
        display.fillCircle(40, 20, 4, WHITE);
      }

      //5
      display.drawCircle(50, 20, 4, WHITE);
      
      if(binVal[4] == 1){
        display.fillCircle(50, 20, 4, WHITE);
      }
      
      //6
      display.drawCircle(60, 20, 4, WHITE);
      
      if(binVal[5] == 1){
        display.fillCircle(60, 20, 4, WHITE);
      }

      //7
      display.drawCircle(70, 20, 4, WHITE);
      
      if(binVal[6] == 1){
        display.fillCircle(70, 20, 4, WHITE);
      }

      //8
      display.drawCircle(80, 20, 4, WHITE);
      
      if(binVal[7] == 1){
        display.fillCircle(80, 20, 4, WHITE);
      }

      display.display();
      delay(1);
}

int toDECfromBIN(int binVal[]){
  int sum = 0;

  //Iterate through the bits.
  for(int i = 0; i < 8; i++){
    //Check if the bit is set to 1.
    if(binVal[i] == 1){
      //Then get the dec value for the binary position the bit is at and add it to the sum.
      sum += round(pow(2, i)); 
    }
  }
  
  return sum;
}
