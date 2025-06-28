import asyncio
import threading
import simplepyble

simplepyble.config.winrt.experimental_use_own_mta_apartment = False

scanned_devices_dict = dict()
connected_devices_dict = dict()
watching_devices = dict()
watching_devices['ESP32'] = ['85bc9e6c-9501-4bf4-819e-4f40b5e56372', '1a2ae85a-8118-4644-9e3b-387122d8cd9e']
changed_characteristics_dict = dict()
deserialization_ways = dict()
deserialization_ways['85bc9e6c-9501-4bf4-819e-4f40b5e56372'] = 'utf8'
deserialization_ways['1a2ae85a-8118-4644-9e3b-387122d8cd9e'] = 'utf8'


def disconnected_callback(device):
    if device.address() in connected_devices_dict:
        try:
            if device.identifier() in watching_devices and len(watching_devices[device.identifier()]) > 0:
                for service in device.services():
                        for watching_characteristic_uuid in watching_devices[device.identifier()]:
                            if device.read(service.uuid(), watching_characteristic_uuid) != None:
                                device.notify(service.uuid(), watching_characteristic_uuid, None)      
                                continue
            
            if device.address() in watching_devices and len(watching_devices[device.address()]) > 0:
                for service in device.services():
                        for watching_characteristic_uuid in watching_devices[device.address()]:
                            if device.read(service.uuid(), watching_characteristic_uuid) != None:
                                device.notify(service.uuid(), watching_characteristic_uuid, None)      
                                continue
        except:
            pass

        del connected_devices_dict[device.address()]
        del scanned_devices_dict[device.address()]


async def changed_callbak(characteristic_uuid, data):
    if characteristic_uuid not in changed_characteristics_dict:
        changed_characteristics_dict[characteristic_uuid] = bytearray()
    if changed_characteristics_dict[characteristic_uuid] != data: 
        changed_characteristics_dict[characteristic_uuid] = data       
        if characteristic_uuid in deserialization_ways and deserialization_ways[characteristic_uuid] == 'utf8':
            print(f"Data Changed: {characteristic_uuid} = {data.decode('utf-8')}", flush = True)
        else:  
            print(f"Data Changed: {characteristic_uuid} = {data.hex(' ').upper()}", flush = True)


def scanned_callback(device):
    if device.address() not in scanned_devices_dict:
        scanned_devices_dict[device.address()] = device
        print(f"Device Scanned: {device.identifier()} {device.address()}", flush = True)

        if not device.is_connectable():
            #print(f"Device Ignored: {device.identifier()} device.address()}", flush = True)
            return

        if device.identifier() not in watching_devices and device.address() not in watching_devices:
            #print(f"Device Ignored: {device.identifier()} device.address()}", flush = True)
            return
        
        try:
            device.connect()
                            
            watching_characteristics_count = 0
            if device.identifier() in watching_devices and len(watching_devices[device.identifier()]) > 0:
                for service in device.services():
                    for watching_characteristic_uuid in watching_devices[device.identifier()]:
                        try:
                            if device.read(service.uuid(), watching_characteristic_uuid) != None:
                                device.notify(service.uuid(), watching_characteristic_uuid, lambda data: print(f"Data Changed: {watching_characteristic_uuid} = {data}", flush = True))
                                watching_characteristics_count += 1
                                continue
                        except:
                            pass
            if device.address() in watching_devices and len(watching_devices[device.address()]) > 0:
                for service in device.services():
                    for watching_characteristic_uuid in watching_devices[device.address()]:
                        try:
                            if device.read(service.uuid(), watching_characteristic_uuid) != None:
                                device.notify(service.uuid(), watching_characteristic_uuid, lambda data: print(f"Data Changed: {watching_characteristic_uuid} = {data}", flush = True))
                                watching_characteristics_count += 1
                                continue
                        except:
                            pass
            if watching_characteristics_count == 0:
                device.disconnect()
                print(f"Device Mismatched: {device.identifier()} {device.address()}", flush = True)
                return

            connected_devices_dict[device.address] = device
            print(f"Device Connected: {device.address()}", flush = True)
        except BaseException as ex:
            print(f"Error Occurred: {device.identifier()} {device.address()}  {repr(ex)}", flush = True)


def scan(stop_event: asyncio.Event):
    adapters = simplepyble.Adapter.get_adapters()
    if len(adapters) == 0:
        print("Error Occurred: No adapters found")
        return
    adapter = adapters[0]
    print(f"Adapter Selected: {adapter.identifier()} {adapter.address()}")

    adapter.set_callback_on_scan_found(scanned_callback)
    # Scan for 3 seconds
    adapter.scan_for(3000)

    devicesAddr = []
    for deviceAddr in connected_devices_dict:
        devicesAddr.append(deviceAddr)
    for deviceAddr in devicesAddr:
        connected_devices_dict[deviceAddr].disconnect()

    devicesAddr.clear()
    scanned_devices_dict.clear()


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
            scan(stop_event)
        except (KeyboardInterrupt, asyncio.CancelledError, RuntimeError):
            stop_event.set()
        except BaseException as ex:
            print(f"Error Occurred: {repr(ex)}", flush = True)
            await asyncio.sleep(1)


if __name__ == "__main__":
    asyncio.run(main())