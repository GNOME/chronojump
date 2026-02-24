# 
#  This file is part of ChronoJump
# 
#  ChronoJump is free software; you can redistribute it and/or modify
#   it under the terms of the GNU General Public License as published by
#    the Free Software Foundation; either version 2 of the License, or   
#     (at your option) any later version.
#     
#  ChronoJump is distributed in the hope that it will be useful,
#   but WITHOUT ANY WARRANTY; without even the implied warranty of
#    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
#     GNU General Public License for more details.
# 
#  You should have received a copy of the GNU General Public License
#   along with this program; if not, write to the Free Software
#    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
# 
#   Copyright (C) 2025   Yang Dejiu <joeries.young@gmail.com>
#   Copyright (C) 2025-2026   Xavier de Blas <xaviblas@gmail.com>
#

import asyncio
from bleak import BleakScanner, BleakClient, BleakGATTCharacteristic
from importlib.metadata import version #to know version if not using venv/ble
from argparse import ArgumentParser
import threading


parser = ArgumentParser()
parser.add_argument("--mode", help="mode is {SCAN/CONNECT}")
parser.add_argument("--value", help="on SCAN can be {ALL/CJ/CP4/or the devicename} for {All devices/Chronojump devices/Chronopic4/your device}\non CONNECT this is the name of the client")
args = parser.parse_args()
if args.mode is None:
    args.mode = "SCAN"
if args.value is None:
    args.value = "CJ"

print(f"bleak version: {version('bleak')}", flush = True) #to know version if not using venv/ble
print(f"args.mode: {args.mode}; args.value: {args.value}", flush = True)
print(f"press enter to quit\n", flush = True)

scanned_devices_dict = dict()
connected_devices_dict = dict()
watching_devices = dict()
#"CJ-CP4-12:5d" # xaviP
#"CJ-CP4-0e:e5" # xaviB

scanDevices = ""
if args.value == "ALL":
    scanDevices = ""
elif args.value == "CJ":
    scanDevices = "CJ-"
elif args.value == "CP4":
    scanDevices = "CJ-CP4-"
else:
    scanDevices = args.value

watching_devices[scanDevices] = ['588dc235-7184-4550-9053-0e6a82f37cee', #meu
                          '378b5d62-1fd3-4266-bbf7-6fec024d59a9',
                          'bde4d6e2-b970-42ff-b498-aeeca541ee07',
                          'e7331566-3aec-4a47-b8f1-d6f27850ad87',
                          'a2317307-e74a-4efe-b8ae-d615cd3be489']
changed_characteristics_dict = dict()
deserialization_ways = dict()
deserialization_ways['588dc235-7184-4550-9053-0e6a82f37cee'] = 'utf8'
deserialization_ways['378b5d62-1fd3-4266-bbf7-6fec024d59a9'] = 'utf8'
deserialization_ways['bde4d6e2-b970-42ff-b498-aeeca541ee07'] = 'utf8'
deserialization_ways['e7331566-3aec-4a47-b8f1-d6f27850ad87'] = 'utf8'
deserialization_ways['a2317307-e74a-4efe-b8ae-d615cd3be489'] = 'utf8'


def deviceIsInWatchingDevices (deviceName):
    if deviceName is None:
         return False
    for wd in watching_devices:
        if deviceName.startswith (wd):
                return True
    return False

async def scan(stop_event: asyncio.Event):
    def disconnected_callback(client: BleakClient):
        #client.set_disconnected_callback(None)
        
        if client.address in connected_devices_dict:
            try:
                device = connected_devices_dict[client.address]
                if deviceIsInWatchingDevices (device.name) and len(watching_devices[device.name]) > 0:
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

            if not deviceIsInWatchingDevices (device.name) and device.address not in watching_devices:
                print(f"Device Ignored: {device} {advertising_data}", flush = True)
                return

            print(f"\nDevice Scanned: {device} {advertising_data}", flush = True)

            if args.mode == "SCAN":
                return
            # args.mode == "CONNECT"
            if device.name is None:
                return
            if not device.name.startswith (scanDevices):
                return

            #this allows to connect when we called with CONNECT CJ/CP4 and not the specific device
            if args.value == 'ALL' or args.value == 'CJ' or args.value == 'CP4':
                print(f"fixing!", flush = True)
                watching_devices[device.name] = watching_devices[scanDevices]

            print(f"Trying to connect: {device} {advertising_data} ...", flush = True)

            try:
                client = BleakClient(address_or_ble_device = device, disconnected_callback = disconnected_callback)
                #ble don't need to pair
                #try:
                #    await client.pair()
                #except:
                #    pass
                await client.connect()
                                
                watching_characteristics_count = 0
                #print(f"B device.name: {device.name}", flush = True) #NimBLE device
                print(f"watching_devices[device.name]: {watching_devices[device.name]}", flush = True)
                if deviceIsInWatchingDevices (device.name) and len(watching_devices[device.name]) > 0:
                    print(f"is in watching_devices", flush = True)
                    for service in client.services:
                        #print(f"service: {service}", flush = True)
                        for watching_characteristic_uuid in watching_devices[device.name]:
                            if service.get_characteristic(watching_characteristic_uuid) != None:
                                await client.start_notify(char_specifier = watching_characteristic_uuid, callback = changed_callbak)      
                                watching_characteristics_count += 1
                                continue
                #print(f"device.address: {device.address}", flush = True)
                #print(f"watching_devices[device.address]: {watching_devices[device.address]}", flush = True)
                if device.address in watching_devices:# and len(watching_devices[device.address]) > 0:
                    for service in client.services:
                        for watching_characteristic_uuid in watching_devices[device.address]:
                            if service.get_characteristic(watching_characteristic_uuid) != None:
                                await client.start_notify(char_specifier = watching_characteristic_uuid, callback = changed_callbak)      
                                watching_characteristics_count += 1
                                continue
                if watching_characteristics_count == 0:
                    #ble don't need to pair
                    #try:
                    #    await client.unpair()
                    #except:
                    #    await client.disconnect()
                    #await client.disconnect()
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
        print(f"Exiting ...", flush = True)
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
