// BLE stuff
#include <NimBLEDevice.h>

NimBLEServer* pServer = nullptr;
bool deviceConnected = false;
bool oldDeviceConnected = false;

#define FORCE_SERVICE_UUID        "786d20ea-6fdb-4fd0-bd18-b592fe179ec5"
#define FORCE_TIME_CHARACTERISTIC_UUID "8021dd36-f7bf-40f8-b448-9438b33eca1b"
#define FORCE_VALUE_CHARACTERISTIC_UUID "3bf2da15-f408-414f-a8dd-8cf1590b4a4a"
#define FORCE_COMMAND_CHARACTERISTIC_UUID "1a2ae85a-8118-4644-9e3b-387122d8cd9e"

#define POSITION_SERVICE_UUID        "e9532a51-5f34-41cb-b9ed-ba7015a1e564"
#define POSITION_TIME_CHARACTERISTIC_UUID "384002fb-1bb2-40f2-ab4c-8c5e53e728c0"
#define POSITION_VALUE_CHARACTERISTIC_UUID "384002fb-1bb2-40f2-ab4c-8c5e53e728c0"
#define POSITION_COMMAND_CHARACTERISTIC_UUID "17f17489-1856-46dd-ab45-c3eeae70b15e"

NimBLECharacteristic* pForceTime = nullptr;
NimBLECharacteristic* pForceValue = nullptr;
NimBLECharacteristic* pForceCommand = nullptr;

NimBLECharacteristic* pPositionTime = nullptr;
NimBLECharacteristic* pPositionValue = nullptr;
NimBLECharacteristic* pPositionCommand = nullptr;

class ServerCallbacks: public NimBLEServerCallbacks {
    void onConnect(NimBLEServer* pServer, NimBLEConnInfo& connInfo) override {
      //update period in notify? (min interval (4 * 1.25ms = 5ms))
      pServer->updateConnParams(connInfo.getConnHandle(), 4, 8, 0, 100);
      deviceConnected = true;
      //Serial.println("📲 Client connected:");
    };

    void onDisconnect(NimBLEServer* pServer, NimBLEConnInfo& connInfo, int reason) override {
      //Serial.printf("Client disconnected - start advertising\n");
      NimBLEDevice::startAdvertising();
    }
};

class CommandCallbacks : public NimBLECharacteristicCallbacks {
    void onWrite(NimBLECharacteristic* pForceCommand, NimBLEConnInfo& connInfo) override {
      std::string value = pForceCommand->getValue();
      String characteristicUUID = pForceCommand->getUUID().toString().c_str();
      if (value.length() <= 0) {
        return;
      }

      if (characteristicUUID == FORCE_COMMAND_CHARACTERISTIC_UUID) { commandFlag = true;}
    }
};

void initializeBLE(void) {

  NimBLEDevice::setSecurityAuth(false, false, false);
  // Create the BLE Device
  NimBLEDevice::init("Chronojump");
  delay(10);
  BLEAdvertisementData advertisementData;
  advertisementData.setName("EncForce");   // Nombre completo
  advertisementData.setShortName("EncFor");  // Nombre corto (opcional)
  advertisementData.setManufacturerData("Chronojump");  // Datos de fabricante

  // Create the BLE Server
  pServer = NimBLEDevice::createServer();
  pServer->setCallbacks(new ServerCallbacks());
  

  // Create the Force Service
  NimBLEService* pForceService = pServer->createService(FORCE_SERVICE_UUID);

  // Create a BLE Characteristic
  pForceCommand = pForceService->createCharacteristic(
                       FORCE_COMMAND_CHARACTERISTIC_UUID,
                       NIMBLE_PROPERTY::WRITE);

  NimBLEDescriptor *pForceCommandDesc = pForceCommand->createDescriptor(
                      "2901",
                      NIMBLE_PROPERTY::READ,
                      26);
  pForceCommandDesc->setValue("Commands for the load cell");
  pForceCommand->setCallbacks(new CommandCallbacks());

  pForceTime = pForceService->createCharacteristic(
                       FORCE_TIME_CHARACTERISTIC_UUID,
                       NIMBLE_PROPERTY::READ | NIMBLE_PROPERTY::NOTIFY);
  NimBLEDescriptor *pForceTimeDesc = pForceTime->createDescriptor(
                      "2901",
                      NIMBLE_PROPERTY::READ,
                      20);
  pForceTimeDesc->setValue("Time in microseconds");

  pForceValue = pForceService->createCharacteristic(
                       FORCE_VALUE_CHARACTERISTIC_UUID,
                       NIMBLE_PROPERTY::READ | NIMBLE_PROPERTY::NOTIFY);
  NimBLEDescriptor *pForceValueDesc = pForceValue->createDescriptor(
                      "2901",
                      NIMBLE_PROPERTY::READ,
                      16);
  pForceValueDesc->setValue("Force in Newtons");

  // Start the service
  pForceService->start();

  // Create the Position Service
  NimBLEService* pPositionService = pServer->createService(POSITION_SERVICE_UUID);

  // Create a BLE Characteristic
  pPositionCommand = pPositionService->createCharacteristic(
                       POSITION_COMMAND_CHARACTERISTIC_UUID,
                       NIMBLE_PROPERTY::WRITE);

  
  pPositionCommand->setCallbacks(new CommandCallbacks());

  pPositionTime = pPositionService->createCharacteristic(
                       POSITION_TIME_CHARACTERISTIC_UUID,
                       NIMBLE_PROPERTY::READ | NIMBLE_PROPERTY::NOTIFY);
  NimBLEDescriptor *pPositionTimeDesc = pPositionTime->createDescriptor(
                      "2901",
                      NIMBLE_PROPERTY::READ,
                      20);
  pPositionTimeDesc->setValue("Time in microseconds");

  pPositionValue = pPositionService->createCharacteristic(
                       POSITION_VALUE_CHARACTERISTIC_UUID,
                       NIMBLE_PROPERTY::READ | NIMBLE_PROPERTY::NOTIFY);
  NimBLEDescriptor *pPositionValueDesc = pPositionValue->createDescriptor(
                      "2901",
                      NIMBLE_PROPERTY::READ,
                      16);
  pPositionValueDesc->setValue("Number of pulses");
  
  // https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.descriptor.gatt.client_characteristic_configuration.xml
  // Create a BLE Descriptor
  // pForceCommand->addDescriptor(new BLE2902());
  // pForceValue->addDescriptor(new BLE2902());

  // Start the service
  pPositionService->start();

  // Start advertising
  pServer->startAdvertising();
  NimBLEAdvertising* pAdvertising = NimBLEDevice::getAdvertising();
  pAdvertising->setAdvertisementData(advertisementData);
  pAdvertising->addServiceUUID(pForceService->getUUID());
  pAdvertising->addServiceUUID(pPositionService->getUUID());
  pAdvertising->enableScanResponse(true);
  pAdvertising->start();  
  // pAdvertising->addServiceUUID(FORCE_SERVICE_UUID);
  // pAdvertising->setScanResponse(false);
  // pAdvertising->setMinPreferred(0x0);  // set value to 0x00 to not advertise this parameter
  // BLEDevice::startAdvertising();
  //Serial.println("Waiting a client connection to notify...");
}

void sendToBLE(sensor_t sensor) {

  /*
  TODO: as we are not using sample now, we commented this
  if (sensor == loadCell){
    //Text formated for DumbDisplay [Name of Var]:[optional space][value]
    // pForceValue->setValue((String(force)).c_str());
    pForceValue->setValue( String(sample.data).c_str() );
    pForceTime->setValue( String(sample.time).c_str() );
    pForceValue->notify();
    pForceTime->notify();
  }
  */

  /*
  TODO: as we are not using sample now, we commented this
  if (sensor == incEncoder){
    //Text formated for DumbDisplay [Name of Var]:[optional space][value]
    // pForceValue->setValue((String(force)).c_str());
    pPositionValue->setValue( String(sample.data).c_str() );
    pPositionTime->setValue( String(sample.time).c_str() );
    pPositionValue->notify();
    pPositionTime->notify();
  }
  */

  // TODO: Check that this is mandatory
  if (deviceConnected) {
    delay(3); // bluetooth stack will go into congestion, if too many packets are sent, in 6 hours test i was able to go as low as 3ms
  }

  // disconnecting
  if (!deviceConnected && oldDeviceConnected) {
    delay(5); // give the bluetooth stack the chance to get things ready
    pServer->startAdvertising(); // restart advertising
    //Serial.println("start advertising");
    oldDeviceConnected = deviceConnected;
  }
  // connecting
  if (deviceConnected && !oldDeviceConnected) {
    // do stuff here on connecting
    oldDeviceConnected = deviceConnected;
  }
}