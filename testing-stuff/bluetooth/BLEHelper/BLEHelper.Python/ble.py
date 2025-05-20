import asyncio
from bleak import BleakScanner, BleakClient, BleakGATTCharacteristic
import threading


scanned_devices_dict = dict()
connected_devices_dict = dict()
watching_characteristics_uuid = ['85bc9e6c-9501-4bf4-819e-4f40b5e56372']
changed_characteristics_dict = dict()


async def scan(stop_event: asyncio.Event):
    def disconnected_callback(client: BleakClient):
        #client.set_disconnected_callback(None)
        
        if client.address in connected_devices_dict:
            try:
                for service in client.services:
                        for watching_characteristic_uuid in watching_characteristics_uuid:
                            if service.get_characteristic(watching_characteristic_uuid) != None:
                                client.stop_notify(char_specifier = watching_characteristic_uuid)      
                                continue
            except:
                pass

            del connected_devices_dict[client.address]
            del scanned_devices_dict[client.address]

    async def changed_callbak(sender: BleakGATTCharacteristic, data: bytearray):
        if sender.uuid not in changed_characteristics_dict:
            changed_characteristics_dict[sender.uuid] = ''
        if changed_characteristics_dict[sender.uuid] != data: 
            changed_characteristics_dict[sender.uuid] = data           
            print(f"Data Changed: {sender.uuid} = {data.decode('utf-8')}")

    async def scanned_callback(device, advertising_data):
        if device.address not in scanned_devices_dict:
            scanned_devices_dict[device.address] = device
            print(f"Device Scanned: {device}    {advertising_data}") 

            if device.name != 'ESP32':
                return
            
            try:
                client = BleakClient(address_or_ble_device = device, disconnected_callback = disconnected_callback)
                await client.connect()
                                
                watching_characteristics_count = 0
                for service in client.services:
                    for watching_characteristic_uuid in watching_characteristics_uuid:
                        if service.get_characteristic(watching_characteristic_uuid) != None:
                            await client.start_notify(char_specifier = watching_characteristic_uuid, callback = changed_callbak)      
                            watching_characteristics_count += 1
                            continue
                if watching_characteristics_count == 0:
                    client.disconnect()
                    return

                connected_devices_dict[device.address] = client
                print(f"Device Connected: {device}")
            except Exception as ex:
                print(ex)

    async with BleakScanner(scanned_callback) as scanner:
        ...
        # Important! Wait for an event to trigger stop, otherwise scanner
        # will stop immediately.
        await stop_event.wait()
        #scanner.register_detection_callback(None)

        devicesAddr = []
        for deviceAddr in connected_devices_dict:
            devicesAddr.append(deviceAddr)
        for deviceAddr in devicesAddr:
            await connected_devices_dict[deviceAddr].disconnect()

        devicesAddr.clear()
        scanned_devices_dict.clear()

    # scanner stops when block exits
    ...


def quit(stop_event: asyncio.Event):
    try:
        input('Press any key to quit...\n')
        # TODO: add something that calls stop_event.set()
        stop_event.set()
    except:
        pass


async def main():
    stop_event = asyncio.Event()
    threading.Thread(target = quit, args = (stop_event,), daemon = False).start()

    try:
        await scan(stop_event)
    except (KeyboardInterrupt, asyncio.CancelledError, RuntimeError):
        stop_event.set()
    except Exception as ex:
        print(ex)
        await asyncio.sleep(3)
        await scan(stop_event)
    except:
        pass


asyncio.run(main())