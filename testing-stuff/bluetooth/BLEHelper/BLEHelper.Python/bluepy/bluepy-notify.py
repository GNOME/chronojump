from bluepy.btle import Peripheral, DefaultDelegate
import binascii

class NotificationDelegate(DefaultDelegate):
    def __init__(self):
        DefaultDelegate.__init__(self)
    
    def handleNotification(self, cHandle, data):
        print(f"收到通知: 句柄={cHandle}, 数据={binascii.hexlify(data)}")

dev_addr = "XX:XX:XX:XX:XX:XX"
service_uuid = "0000180a-0000-1000-8000-00805f9b34fb"
char_uuid = "00002a29-0000-1000-8000-00805f9b34fb"

dev = Peripheral(dev_addr)
dev.withDelegate(NotificationDelegate())

try:
    # 启用通知
    svc = dev.getServiceByUUID(service_uuid)
    char = svc.getCharacteristics(char_uuid)[0]
    
    # 获取CCC描述符
    for desc in char.getDescriptors():
        if desc.uuid == 0x2902:  # CCC描述符UUID
            desc.write(b"\x01\x00", True)  # 启用通知
    
    print("等待通知...")
    while True:
        if dev.waitForNotifications(1.0):
            continue
        print("等待...")
        
except KeyboardInterrupt:
    print("用户中断")
finally:
    dev.disconnect()