from bluepy.btle import Scanner, DefaultDelegate, Peripheral
import threading, asyncio


scanned_devices_dict = dict()
connected_devices_dict = dict()
watching_devices = dict()
watching_devices['ESP32'] = ['85bc9e6c-9501-4bf4-819e-4f40b5e56372', '1a2ae85a-8118-4644-9e3b-387122d8cd9e']
changed_characteristics_dict = dict()
deserialization_ways = dict()
deserialization_ways['85bc9e6c-9501-4bf4-819e-4f40b5e56372'] = 'utf8'
deserialization_ways['1a2ae85a-8118-4644-9e3b-387122d8cd9e'] = 'utf8'
handles_characteristics_dict = dict()
moniter_dict = dict()


class NotificationDelegate(DefaultDelegate):
    def __init__(self):
        DefaultDelegate.__init__(self)

    def handleNotification(self, cHandle, data):
        characteristic_uuid = handles_characteristics_dict[cHandle]
        if characteristic_uuid not in changed_characteristics_dict:
            changed_characteristics_dict[characteristic_uuid] = bytearray()
        if changed_characteristics_dict[characteristic_uuid] != data:
            changed_characteristics_dict[characteristic_uuid] = data
            if characteristic_uuid in deserialization_ways and deserialization_ways[characteristic_uuid] == 'utf8':
                print(f"Data Changed: {characteristic_uuid} = {data.decode('utf-8')}", flush = True)
            else:
                print(f"Data Changed: {characteristic_uuid} = {data.hex(' ').upper()}", flush = True)


def disconnect(client):
    try:
        client.disconnect()
    except:
        pass
    if client.addr in connected_devices_dict:
        del connected_devices_dict[client.addr]
    if client.addr in scanned_devices_dict:
        del scanned_devices_dict[client.addr]
    if client.addr in moniter_dict:
        del moniter_dict[client.addr]


def moniter(client, stop_event):    
    while True:
        if stop_event.is_set():
            disconnect(client)
            break
        try:
            client.waitForNotifications(1.0)
        except:
            pass


def start_notify(client, characteristic):
    for descriptor in characteristic.getDescriptors():
        if descriptor.uuid == 0x2902:
            descriptor.write(b"\x01\x00", True)
            cHandle = characteristic.getHandle()
            if cHandle not in handles_characteristics_dict:
                handles_characteristics_dict[cHandle] = characteristic.uuid.getCommonName()
            if client.addr not in moniter_dict:
                threading.Thread(target = moniter, args = (client, stop_event,), daemon = True).start()    
                moniter_dict[client.addr] = True
            break


def get_name(device):
    name = ''
    for (adtype, desc, value) in device.getScanData():
        if adtype == 9:
            name = value
            break
    return name


def handle(device, device_name):          
    try:
        client = Peripheral(device.addr)
        client.withDelegate(NotificationDelegate())
        #print(f"Device Matched: {device.addr}", flush = True)

        watching_characteristics_count = 0
        services = client.getServices()
        if device_name in watching_devices and len(watching_devices[device_name]) > 0:
            for service in services:
                for watching_characteristic_uuid in watching_devices[device_name]:
                    characteristics = service.getCharacteristics(watching_characteristic_uuid)
                    if len(characteristics) > 0:
                        start_notify(client, characteristics[0])
                        watching_characteristics_count += 1
                        continue
        if device.addr in watching_devices and len(watching_devices[device.addr]) > 0:
            for service in services:
                for watching_characteristic_uuid in watching_devices[device.addr]:
                    characteristics = service.getCharacteristics(watching_characteristic_uuid)
                    if len(characteristics) > 0:
                        start_notify(client, characteristics[0])
                        watching_characteristics_count += 1
                        continue
        if watching_characteristics_count == 0:
            disconnect(client)
            print(f"Device Mismatched: {device.addr}", flush = True)
            return False

        connected_devices_dict[device.addr] = client
        print(f"Device Connected: {device.addr}", flush = True)
        return True
    except BaseException as ex:
        print(f"Error Occurred: {device.addr} {repr(ex)}", flush = True)
        return False


async def scan(stop_event: asyncio.Event):
    scanner = Scanner()
    #scanner.withDelegate(ScanDelegate())
    try:
        devices = scanner.scan(3.0)
        for device in devices:
            if device.addr in scanned_devices_dict:
                #print(f"Device Ignored: {device.addr}", flush = True)
                continue

            scanned_devices_dict[device.addr] = device
            print(f"Device Scanned: {device.addr}", flush = True)

            if not device.connectable:
                #print(f"Device Ignored: {device.addr}", flush = True)
                continue

            device_name = get_name(device)
            if device_name not in watching_devices and device.addr not in watching_devices:
                #print(f"Device Ignored: {device.addr}", flush = True)
                continue

            if not handle(device, device_name):
                del scanned_devices_dict[device.addr]
    except:
        pass

    # scanner stops when block exits
    ...


def quit(stop_event: asyncio.Event):
    try:
        input()
    except:
        pass
    # TODO: add something that calls stop_event.set()
    stop_event.set()


stop_event = asyncio.Event()


async def main():
    threading.Thread(target = quit, args = (stop_event,), daemon = True).start()

    while True:
        if stop_event.is_set():
            break
        try:
            await scan(stop_event)
        except (KeyboardInterrupt, asyncio.exceptions.CancelledError):
            stop_event.set()
            break
        except BaseException as ex:
            print(f"Error Occurred: {repr(ex)}", flush = True)
            await asyncio.sleep(1)

    devicesAddr = []
    for deviceAddr in connected_devices_dict:
        devicesAddr.append(deviceAddr)
    for deviceAddr in devicesAddr:
        disconnect(connected_devices_dict[deviceAddr])

    moniter_dict.clear()
    handles_characteristics_dict.clear()
    changed_characteristics_dict.clear()
    connected_devices_dict.clear()
    devicesAddr.clear()
    scanned_devices_dict.clear()


if __name__ == "__main__":
    asyncio.run(main())


'''
class ScanDelegate(DefaultDelegate):
    def __init__(self):
        DefaultDelegate.__init__(self)

    def handleDiscovery(self, device, isNewDev, isNewData):
        if device.addr not in scanned_devices_dict:
            scanned_devices_dict[device.addr] = device
            print(f"Device Scanned: {device.addr}", flush = True)

            if not device.connectable:
                #print(f"Device Ignored: {device.addr}", flush = True)
                return

            device_name = get_name(device)
            if device_name not in watching_devices and device.addr not in watching_devices:
                #print(f"Device Ignored: {device.addr}", flush = True)
                return

            try:
                client = Peripheral(device.addr)
                client.withDelegate(NotificationDelegate())
                #print(f"Device Matched: {device.addr}", flush = True)

                watching_characteristics_count = 0
                services = client.getServices()
                if device_name in watching_devices and len(watching_devices[device_name]) > 0:
                    for service in services:
                        for watching_characteristic_uuid in watching_devices[device_name]:
                            characteristics = service.getCharacteristics(watching_characteristic_uuid)
                            if len(characteristics) > 0:
                                start_notify(client, characteristics[0])
                                watching_characteristics_count += 1
                                continue
                if device.addr in watching_devices and len(watching_devices[device.addr]) > 0:
                    for service in services:
                        for watching_characteristic_uuid in watching_devices[device.addr]:
                            characteristics = service.getCharacteristics(watching_characteristic_uuid)
                            if len(characteristics) > 0:
                                start_notify(client, characteristics[0])
                                watching_characteristics_count += 1
                                continue
                if watching_characteristics_count == 0:
                    disconnect(client)
                    print(f"Device Mismatched: {device.addr}", flush = True)
                    return

                connected_devices_dict[device.addr] = client
                print(f"Device Connected: {device.addr}", flush = True)
            except BaseException as ex:
                print(f"Error Occurred: {device.addr} {repr(ex)}", flush = True)
'''