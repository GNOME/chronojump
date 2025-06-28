from bluepy.btle import Scanner, DefaultDelegate

class ScanDelegate(DefaultDelegate):
    def __init__(self):
        DefaultDelegate.__init__(self)

    def handleDiscovery(self, dev, isNewDev, isNewData):
        if isNewDev:
            print(f"发现新设备: {dev.addr} ({dev.addrType}), RSSI={dev.rssi} dB")
        elif isNewData:
            print(f"收到设备 {dev.addr} 的新数据")

# 扫描10秒
scanner = Scanner().withDelegate(ScanDelegate())
devices = scanner.scan(10.0)

for dev in devices:
    print(f"设备 {dev.addr} ({dev.addrType}), RSSI={dev.rssi} dB")
    for (adtype, desc, value) in dev.getScanData():
        print(f"  {desc} = {value}")