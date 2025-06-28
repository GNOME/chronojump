import asyncio
from bleak import BleakScanner, BleakClient, BleakGATTCharacteristic
import threading


scanned_devices_dict = dict()
connected_devices_dict = dict()
watching_devices = dict()
watching_devices['ESP32'] = ['85bc9e6c-9501-4bf4-819e-4f40b5e56372', '1a2ae85a-8118-4644-9e3b-387122d8cd9e']
changed_characteristics_dict = dict()
deserialization_ways = dict()
deserialization_ways['85bc9e6c-9501-4bf4-819e-4f40b5e56372'] = 'utf8'
deserialization_ways['1a2ae85a-8118-4644-9e3b-387122d8cd9e'] = 'utf8'


async def scan(stop_event: asyncio.Event):
    def disconnected_callback(client: BleakClient):
        #client.set_disconnected_callback(None)
        
        if client.address in connected_devices_dict:
            try:
                device = connected_devices_dict[client.address]
                if device.name in watching_devices and len(watching_devices[device.name]) > 0:
                    for service in client.services:
                            for watching_characteristic_uuid in watching_devices[device.name]:
                                if service.get_characteristic(watching_characteristic_uuid) != None:
                                    client.stop_notify(char_specifier = watching_characteristic_uuid)      
                                    continue
                
                if device.address in watching_devices and len(watching_devices[device.address]) > 0:
                    for service in client.services:
                            for watching_characteristic_uuid in watching_devices[device.address]:
                                if service.get_characteristic(watching_characteristic_uuid) != None:
                                    client.stop_notify(char_specifier = watching_characteristic_uuid)      
                                    continue
            except:
                pass

            del connected_devices_dict[client.address]
            del scanned_devices_dict[client.address]

    async def changed_callbak(sender: BleakGATTCharacteristic, data: bytearray):
        if sender.uuid not in changed_characteristics_dict:
            changed_characteristics_dict[sender.uuid] = bytearray()
        if changed_characteristics_dict[sender.uuid] != data: 
            changed_characteristics_dict[sender.uuid] = data       
            if sender.uuid in deserialization_ways and deserialization_ways[sender.uuid] == 'utf8':
                print(f"Data Changed: {sender.uuid} = {data.decode('utf-8')}", flush = True)
            else:  
                print(f"Data Changed: {sender.uuid} = {data.hex(' ').upper()}", flush = True)

    async def scanned_callback(device, advertising_data):
        if device.address not in scanned_devices_dict:
            scanned_devices_dict[device.address] = device
            print(f"Device Scanned: {device} {advertising_data}", flush = True) 

            if device.name not in watching_devices and device.address not in watching_devices:
                #print(f"Device Ignored: {device} {advertising_data}", flush = True)
                return
            
            try:
                client = BleakClient(address_or_ble_device = device, disconnected_callback = disconnected_callback)
                #print(f"Device Matched: {device} {advertising_data}", flush = True)
                try:
                    await client.pair()
                except:
                    pass
                await client.connect()
                                
                watching_characteristics_count = 0
                if device.name in watching_devices and len(watching_devices[device.name]) > 0:
                    for service in client.services:
                        for watching_characteristic_uuid in watching_devices[device.name]:
                            if service.get_characteristic(watching_characteristic_uuid) != None:
                                await client.start_notify(char_specifier = watching_characteristic_uuid, callback = changed_callbak)      
                                watching_characteristics_count += 1
                                continue
                if device.address in watching_devices and len(watching_devices[device.address]) > 0:
                    for service in client.services:
                        for watching_characteristic_uuid in watching_devices[device.address]:
                            if service.get_characteristic(watching_characteristic_uuid) != None:
                                await client.start_notify(char_specifier = watching_characteristic_uuid, callback = changed_callbak)      
                                watching_characteristics_count += 1
                                continue
                if watching_characteristics_count == 0:
                    try:
                        await client.unpair()
                    except:
                        await client.disconnect()
                    print(f"Device Mismatched: {device} {advertising_data}", flush = True)
                    return

                connected_devices_dict[device.address] = client
                print(f"Device Connected: {device}", flush = True)
            except BaseException as ex:
                print(f"Error Occurred: {device} {advertising_data} {repr(ex)}", flush = True)

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
        input()
        # TODO: add something that calls stop_event.set()
        stop_event.set()
    except:
        pass


async def main():
    stop_event = asyncio.Event()
    threading.Thread(target = quit, args = (stop_event,), daemon = False).start()

    while True:
        if stop_event.is_set():
            break
        try:
            await scan(stop_event)
        except (KeyboardInterrupt, asyncio.CancelledError, RuntimeError):
            stop_event.set()
        except BaseException as ex:
            print(f"Error Occurred: {repr(ex)}", flush = True)
            await asyncio.sleep(1)


if __name__ == "__main__":
    asyncio.run(main())