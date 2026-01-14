#include <NimBLEDevice.h>

#define SERVICE_UUID        "6d4ae2f0-b5f7-4e6e-ba4a-e429e15d9aab"
#define COMMAND_CHARACTERISTIC_UUID "1f758ec2-50d0-4eaf-a93c-ac9991aecfdc"
#define PLATFORM0_CHARACTERISTIC_UUID "588dc235-7184-4550-9053-0e6a82f37cee"
#define PLATFORM1_CHARACTERISTIC_UUID "378b5d62-1fd3-4266-bbf7-6fec024d59a9"
#define PLATFORM2_CHARACTERISTIC_UUID "bde4d6e2-b970-42ff-b498-aeeca541ee07"
#define PLATFORM3_CHARACTERISTIC_UUID "e7331566-3aec-4a47-b8f1-d6f27850ad87"
#define BATTLEV_CHARACTERISTIC_UUID "a2317307-e74a-4efe-b8ae-d615cd3be489"

NimBLECharacteristic* pCommand = nullptr;
NimBLECharacteristic* pPlatform[4] = {nullptr, nullptr, nullptr, nullptr};
NimBLECharacteristic* pBattLev = nullptr;

// BLE stuff
NimBLEServer* pServer = nullptr;
bool deviceConnected = false;
bool oldDeviceConnected = false;
bool commandFlag = false;

class ServerCallbacks: public NimBLEServerCallbacks {
    void onConnect(NimBLEServer* pServer, NimBLEConnInfo& connInfo) override {
      //update period in notify? (min interval (4 * 1.25ms = 5ms))
      pServer->updateConnParams(connInfo.getConnHandle(), 24, 48, 0, 180);
      Serial.println("Connecting...");
      deviceConnected = true;
      Serial.println("📲 Client connected:");
    };

    void onDisconnect(NimBLEServer* pServer, NimBLEConnInfo& connInfo, int reason) override {
      Serial.printf("Client disconnected - start advertising\n");
      NimBLEDevice::startAdvertising();
    }
};

class CommandCallbacks : public NimBLECharacteristicCallbacks {
    void onWrite(NimBLECharacteristic* pCommand, NimBLEConnInfo& connInfo) override {
      std::string value = pCommand->getValue();
      String characteristicUUID = pCommand->getUUID().toString().c_str();
      Serial.println("Command");
      if (value.length() <= 0) {
        return;
      }

      if (characteristicUUID == COMMAND_CHARACTERISTIC_UUID) { commandFlag = true;}
    }
};


void initializeBLE(void) {

  NimBLEDevice::setSecurityAuth(false, false, false);
  // Create the BLE Device
  // NimBLEDevice::init("4Platforms");
  NimBLEDevice::init("Chronopic4");
  delay(10);
  BLEAdvertisementData advertisementData;

  //Read the Bluetooth mac address
  BLEAddress addr = NimBLEDevice::getAddress();
  String addressString = addr.toString().c_str();
  Serial.print("BLE MAC Address: ");
  Serial.println(addressString);
  addressString = "CP4-" + addressString.substring(6,17);
  Serial.print("BLE name: ");
  Serial.println(addressString);
  advertisementData.setName(addressString.c_str());   // Nombre completo
  // advertisementData.setShortName("CP-" + addressString.c_str());  // Nombre corto (opcional)
  advertisementData.setManufacturerData("Asociación Chronojump");  // Datos de fabricante

  // Create the BLE Server
  pServer = NimBLEDevice::createServer();
  pServer->setCallbacks(new ServerCallbacks());
  

  // Create the BLE Service
  // BLEService *pService = pServer->createService(SERVICE_UUID);
  NimBLEService* pService = pServer->createService(SERVICE_UUID);

  // Create a BLE Characteristics
  pCommand = pService->createCharacteristic(
                       COMMAND_CHARACTERISTIC_UUID,
                       NIMBLE_PROPERTY::WRITE);
  pCommand->setCallbacks(new CommandCallbacks());

  pPlatform[0] = pService->createCharacteristic(
                       PLATFORM0_CHARACTERISTIC_UUID,
                       NIMBLE_PROPERTY::READ | NIMBLE_PROPERTY::NOTIFY);

  pPlatform[1] = pService->createCharacteristic(
                       PLATFORM1_CHARACTERISTIC_UUID,
                       NIMBLE_PROPERTY::READ | NIMBLE_PROPERTY::NOTIFY);

  pPlatform[2] = pService->createCharacteristic(
                       PLATFORM2_CHARACTERISTIC_UUID,
                       NIMBLE_PROPERTY::READ | NIMBLE_PROPERTY::NOTIFY);

  pPlatform[3] = pService->createCharacteristic(
                       PLATFORM3_CHARACTERISTIC_UUID,
                       NIMBLE_PROPERTY::READ | NIMBLE_PROPERTY::NOTIFY);
  pBattLev = pService->createCharacteristic(
                       BATTLEV_CHARACTERISTIC_UUID,
                       NIMBLE_PROPERTY::READ | NIMBLE_PROPERTY::NOTIFY);


  // https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.descriptor.gatt.client_characteristic_configuration.xml
  // Create a BLE Descriptor
  // pCommand->addDescriptor(new BLE2902());

  // Start the service
  pService->start();

  // Start advertising
  pServer->startAdvertising();
  NimBLEAdvertising* pAdvertising = NimBLEDevice::getAdvertising();
  pAdvertising->setAdvertisementData(advertisementData);
  pAdvertising->addServiceUUID(pService->getUUID());
  pAdvertising->enableScanResponse(true);
  pAdvertising->start();  
  // pAdvertising->addServiceUUID(SERVICE_UUID);
  // pAdvertising->setScanResponse(false);
  // pAdvertising->setMinPreferred(0x0);  // set value to 0x00 to not advertise this parameter
  // BLEDevice::startAdvertising();

  Serial.println("Waiting a client connection to notify...");
}

void sendToBLE(int i, long value) {
  // Serial.println (String(i) + "->" + String(value));
  //Text formated for DumbDisplay [Name of Var]:[optional space][value]
  pPlatform[i]->setValue( String(value) );
  // pOutput->setValue((uint8_t*)force, sizeof(force));
  pPlatform[i]->notify();

  // TODO: Check that this is mandatory
  if (deviceConnected) {
    delay(3); // bluetooth stack will go into congestion, if too many packets are sent, in 6 hours test i was able to go as low as 3ms
  }

  // disconnecting
  if (!deviceConnected && oldDeviceConnected) {
    delay(5); // give the bluetooth stack the chance to get things ready
    pServer->startAdvertising(); // restart advertising
    Serial.println("start advertising");
    oldDeviceConnected = deviceConnected;
  }
  // connecting
  if (deviceConnected && !oldDeviceConnected) {
    // do stuff here on connecting
    oldDeviceConnected = deviceConnected;
  }
}

void updateBatteryCharacteristic (int i) {
  pBattLev->setValue( String(i));
  pBattLev->notify();
}