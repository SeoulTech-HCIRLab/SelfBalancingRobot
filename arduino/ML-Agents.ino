#include <Kalman.h>// Source: https://github.com/TKJElectronics/KalmanFilter
char buffer [10];
int cnt = 0;

Kalman kalmanX;
Kalman kalmanY;

double accX, accY, accZ;
double gyroX, gyroY, gyroZ;

double gyroXangle, gyroYangle;
double compAngleX, compAngleY;
double kalAngleX, kalAngleY;

uint32_t timer;
uint8_t i2cData[14];

void setup() {
    pinMode(6, OUTPUT);
    pinMode(9, OUTPUT);
    pinMode(10, OUTPUT);
    pinMode(11, OUTPUT);

    analogWrite(6, LOW);
    analogWrite(9, LOW);
    analogWrite(10, LOW);
    analogWrite(11, LOW);
    
    Serial.begin(115200);
    
    Wire.begin();
    
    #if ARDUINO >= 157
        Wire.setClock(400000UL);
    #else
        TWBR = ((F_CPU / 400000UL) - 16) / 2;
    #endif

    i2cData[0] = 7;
    i2cData[1] = 0x00;
    i2cData[2] = 0x00;
    i2cData[3] = 0x00;
    while (i2cWrite(0x19, i2cData, 4, false));
    while (i2cWrite(0x6B, 0x01, true));

    while (i2cRead(0x75, i2cData, 1));
    if (i2cData[0] != 0x68) {
        Serial.print(F("Error reading sensor"));
        while (1);
    }

    delay(100); // Wait for sensor to stabilize

    // Set kalman and gyro starting angle
    while (i2cRead(0x3B, i2cData, 6));
    accX = (int16_t)((i2cData[0] << 8) | i2cData[1]);
    accY = (int16_t)((i2cData[2] << 8) | i2cData[3]);
    accZ = (int16_t)((i2cData[4] << 8) | i2cData[5]);

    double roll  = atan2(accY, accZ) * RAD_TO_DEG;
    double pitch = atan(-accX / sqrt(accY * accY + accZ * accZ)) * RAD_TO_DEG;

    kalmanX.setAngle(roll); // Set starting angle
    kalmanY.setAngle(pitch);
    gyroXangle = roll;
    gyroYangle = pitch;
    compAngleX = roll;
    compAngleY = pitch;

    timer = micros();
}

void loop() {

    // Update all the values
    while (i2cRead(0x3B, i2cData, 14));
    accX = (int16_t)((i2cData[0] << 8) | i2cData[1]);
    accY = (int16_t)((i2cData[2] << 8) | i2cData[3]);
    accZ = (int16_t)((i2cData[4] << 8) | i2cData[5]);
    gyroX = (int16_t)((i2cData[8] << 8) | i2cData[9]);
    gyroY = (int16_t)((i2cData[10] << 8) | i2cData[11]);
    gyroZ = (int16_t)((i2cData[12] << 8) | i2cData[13]);;
    
    double dt = (double)(micros() - timer) / 1000000; // Calculate delta time
    timer = micros();

    double roll  = atan2(accY, accZ) * RAD_TO_DEG;
    double pitch = atan(-accX / sqrt(accY * accY + accZ * accZ)) * RAD_TO_DEG;

    double gyroXrate = gyroX / 131.0; // Convert to deg/s
    double gyroYrate = gyroY / 131.0; // Convert to deg/s

    if ((roll < -90 && kalAngleX > 90) || (roll > 90 && kalAngleX < -90)) {
        kalmanX.setAngle(roll);
        compAngleX = roll;
        kalAngleX = roll;
        gyroXangle = roll;
    } else
        kalAngleX = kalmanX.getAngle(roll, gyroXrate, dt); // Calculate the angle using a Kalman filter
    
    if (abs(kalAngleX) > 90)
        gyroYrate = -gyroYrate; // Invert rate, so it fits the restriced accelerometer reading
    kalAngleY = kalmanY.getAngle(pitch, gyroYrate, dt);

    //gyroXangle += gyroXrate * dt; // Calculate gyro angle without any filter
    gyroYangle += gyroYrate * dt;
    //gyroXangle += kalmanX.getRate() * dt; // Calculate gyro angle using the unbiased rate
    //gyroYangle += kalmanY.getRate() * dt;

    //compAngleX = 0.93 * (compAngleX + gyroXrate * dt) + 0.07 * roll; // Calculate the angle using a Complimentary filter
    compAngleY = 0.93 * (compAngleY + gyroYrate * dt) + 0.07 * pitch;

    //if (gyroXangle < -180 || gyroXangle > 180)
    //    gyroXangle = kalAngleX;
    if (gyroYangle < -180 || gyroYangle > 180)
        gyroYangle = kalAngleY;

    Serial.print((PI / 180) * (gyroY / 100), 3);
    Serial.print("/");
    Serial.println(kalAngleY);
    Motor();
    delay(5);
}

void Motor(){
    while(Serial.available()){
        char c = Serial.read();
        buffer[cnt++] = c;
        if ((c == '\n') || (cnt == sizeof(buffer)-1)){
            buffer[cnt] = '\0';
            cnt = 0;
            if(atoi(buffer) > 0){
                //Foward(map(atoi(buffer), 0, 127, 0, 255));
                Foward(atoi(buffer));
            }else{
                Reverse(-atoi(buffer));
                //Reverse(map(atoi(buffer), -127, 0, 0, -255));
            }
        }
    }
}

void Foward(int output){
    analogWrite(6, output);
    analogWrite(9, 0);
    analogWrite(10, output);
    analogWrite(11, 0);
}

void Reverse(int output){
    analogWrite(6, 0);
    analogWrite(9, output);
    analogWrite(10, 0);
    analogWrite(11, output);
}
