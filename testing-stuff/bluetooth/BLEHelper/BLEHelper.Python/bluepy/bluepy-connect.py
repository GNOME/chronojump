from bluepy.btle import Peripheral, UUID

# 替换为你的设备地址
dev_addr = "XX:XX:XX:XX:XX:XX"

try:
    print(f"正在连接 {dev_addr}...")
    dev = Peripheral(dev_addr)
    
    print("已连接，正在获取服务...")
    services = dev.getServices()
    
    for svc in services:
        print(f"\n服务: {str(svc.uuid)}")
        characteristics = svc.getCharacteristics()
        for char in characteristics:
            print(f"  特性: {str(char.uuid)}")
            if char.supportsRead():
                try:
                    print(f"    值: {char.read()}")
                except Exception as e:
                    print(f"    读取失败: {e}")
    
except Exception as e:
    print(f"错误: {e}")
finally:
    dev.disconnect()
    print("已断开连接")