import asyncio
from bleak import BleakScanner, BleakClient, BleakGATTCharacteristic
from bleak.exc import BleakDeviceNotFoundError
import threading
import platform


scanned_devices_dict = dict()
connected_devices_dict = dict()
watching_devices = dict()
watching_devices['*'] = [] #To connect all available devices and notify all available characteristics.
#watching_devices['ESP32'] = ['85bc9e6c-9501-4bf4-819e-4f40b5e56372',
#                             '1a2ae85a-8118-4644-9e3b-387122d8cd9e']
#watching_devices['4P'] = ['588dc235-7184-4550-9053-0e6a82f37cee',
#                          '378b5d62-1fd3-4266-bbf7-6fec024d59a9',
#                          'bde4d6e2-b970-42ff-b498-aeeca541ee07',
#                          'e7331566-3aec-4a47-b8f1-d6f27850ad87'] #To connect devices with specified name and notify specified characteristics.
changed_characteristics_dict = dict()
deserialization_ways = dict()
#deserialization_ways[''] = 'hex'
#deserialization_ways[''] = 'utf8' #Default as UTF-8


def is_linux():
    return platform.system().lower() == 'linux'


async def stop_notify(client: BleakClient, service_uuid: str, characteristic_uuid: str):
    try:
        client.stop_notify(char_specifier = characteristic_uuid)
    except BaseException as ex:
        print(f"[Error Occurred] Location=stop_notify, Exception={repr(ex)}, Address={client.address}, Name={client.name}, Service={service_uuid}, Characteristic={characteristic_uuid}", flush = True)
        pass


def disconnected_callback(client: BleakClient):
    #client.set_disconnected_callback(None)
    
    if client.address in connected_devices_dict:            
        device = connected_devices_dict[client.address]
        print(f"[Device Disconnected] Address={device.address}, Name={device.name}", flush = True) 

        try:
            if device.name in watching_devices and len(watching_devices[device.name]) > 0:
                for service in client.services:
                    for watching_characteristic_uuid in watching_devices[device.name]:
                        if service.get_characteristic(watching_characteristic_uuid) != None:
                            asyncio.create_task(stop_notify(client = client, service_uuid = service.uuid, char_specifier = watching_characteristic_uuid))
                            continue                
            elif device.address in watching_devices and len(watching_devices[device.address]) > 0:
                for service in client.services:
                    for watching_characteristic_uuid in watching_devices[device.address]:
                        if service.get_characteristic(watching_characteristic_uuid) != None:
                            asyncio.create_task(stop_notify(client = client, service_uuid = service.uuid, char_specifier = watching_characteristic_uuid))
                            continue
            elif "*" in watching_devices:
                for service in client.services:
                        for characteristic in service.characteristics:
                            if 'notify' in characteristic.properties:
                                asyncio.create_task(stop_notify(client = client, service_uuid = service.uuid, char_specifier = characteristic))
                                continue
        except:
            pass

    try:
        del connected_devices_dict[client.address]
    except:
        pass

    try:
        del scanned_devices_dict[client.address]
    except:
        pass


async def changed_callbak(sender: BleakGATTCharacteristic, data: bytearray):
    if sender.uuid not in changed_characteristics_dict:
        changed_characteristics_dict[sender.uuid] = bytearray()
    if changed_characteristics_dict[sender.uuid] != data: 
        changed_characteristics_dict[sender.uuid] = data       
        if sender.uuid in deserialization_ways and deserialization_ways[sender.uuid] == 'hex':
            print(f"[Data Changed] Service={sender.service_uuid}, Characteristic={sender.uuid}, Value={data.hex(' ').upper()}", flush = True)
        else:                  
            print(f"[Data Changed] Service={sender.service_uuid}, Characteristic={sender.uuid}, Value={data.decode('utf-8')}", flush = True)


async def scanned_callback(device, advertising_data):
    if device.address not in scanned_devices_dict:
        scanned_devices_dict[device.address] = device
        print(f"[Device Scanned] Address={device.address}, Name={device.name}", flush = True) 
        
        if "*" not in watching_devices and device.address not in watching_devices and device.name not in watching_devices:
            print(f"[Device Ignored] Address={device.address}, Name={device.name}", flush = True)
            return
        
        try:
            if is_linux():
                client = BleakClient(address_or_ble_device = device, disconnected_callback = disconnected_callback, timeout = 30, pair = False)
            else:
                client = BleakClient(address_or_ble_device = device.address, disconnected_callback = disconnected_callback, timeout = 30, pair = False)
            print(f"[Device Queued] Address={device.address}, Name={device.name}", flush = True)
            try:
                await client.pair()
            except:
                pass
            try:
                await client.connect()
            except BleakDeviceNotFoundError as ex:                    
                print(f"[Error Occurred] Location=connect, Exception={repr(ex)}, Address={device.address}, Name={device.name}", flush = True)
                return
            except BaseException as ex:
                if device.address in scanned_devices_dict:
                    del scanned_devices_dict[device.address]
                print(f"[Error Occurred] Location=connect, Exception={repr(ex)}, Address={device.address}, Name={device.name}", flush = True)
                return
                            
            watching_characteristics_count = 0
            if device.name in watching_devices and len(watching_devices[device.name]) > 0:
                for service in client.services:
                    for watching_characteristic_uuid in watching_devices[device.name]:
                        if service.get_characteristic(watching_characteristic_uuid) != None:
                            try:
                                await client.start_notify(char_specifier = watching_characteristic_uuid, callback = changed_callbak)      
                                watching_characteristics_count += 1
                            except BaseException as ex:
                                print(f"[Error Occurred] Location=start_notify, Exception={repr(ex)}, Address={device.address}, Name={device.name}, Service={service.uuid}, Characteristic={characteristic.uuid}", flush = True)
                                pass
                            continue
            elif device.address in watching_devices and len(watching_devices[device.address]) > 0:
                for service in client.services:
                    for watching_characteristic_uuid in watching_devices[device.address]:
                        if service.get_characteristic(watching_characteristic_uuid) != None:
                            try:
                                await client.start_notify(char_specifier = watching_characteristic_uuid, callback = changed_callbak)      
                                watching_characteristics_count += 1
                            except BaseException as ex:
                                print(f"[Error Occurred] Location=start_notify, Exception={repr(ex)}, Address={device.address}, Name={device.name}, Service={service.uuid}, Characteristic={characteristic.uuid}", flush = True)
                                pass
                            continue
            elif "*" in watching_devices:
                for service in client.services:
                    for characteristic in service.characteristics:
                        if 'notify' in characteristic.properties:
                            try:
                                await client.start_notify(char_specifier = characteristic, callback = changed_callbak)      
                                watching_characteristics_count += 1
                            except BaseException as ex:
                                print(f"[Error Occurred] Location=start_notify, Exception={repr(ex)}, Address={device.address}, Name={device.name}, Service={service.uuid}, Characteristic={characteristic.uuid}", flush = True)
                                pass
                            continue
            if watching_characteristics_count == 0:
                try:
                    await client.unpair()
                except:
                    pass
                
                try:
                    await client.disconnect()
                except:
                    pass

                print(f"[Device Mismatched] Address={device.address}, Name={device.name}", flush = True)
                return

            connected_devices_dict[device.address] = client
            print(f"[Device Connected] Address={device.address}, Name={device.name}", flush = True)
        except BaseException as ex:
            if device.address in scanned_devices_dict:
                del scanned_devices_dict[device.address]
            print(f"[Error Occurred] Location=scanned_callback, Exception={repr(ex)}, Address={device.address}, Name={device.name}", flush = True)


async def scan(stop_event: asyncio.Event):
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
            try:
                await connected_devices_dict[deviceAddr].disconnect()
            except:
                pass

        devicesAddr.clear()
        scanned_devices_dict.clear()
        changed_characteristics_dict.clear()

    # scanner stops when block exits
    ...


def stop(stop_event: asyncio.Event):
    try:
        input()
        # TODO: add something that calls stop_event.set()
        stop_event.set()
    except:
        pass


async def main():
    stop_event = asyncio.Event()
    threading.Thread(target = stop, args = (stop_event,), daemon = True).start()
    
    while True:
        if stop_event.is_set():
            break
        try:
            await scan(stop_event)
        except (KeyboardInterrupt, asyncio.CancelledError, RuntimeError):
            stop_event.set()
        except BaseException as ex:
            print(f"[Error Occurred] Location=main, Exception={repr(ex)}", flush = True)
            await asyncio.sleep(1)


if __name__ == "__main__":
    asyncio.run(main())
