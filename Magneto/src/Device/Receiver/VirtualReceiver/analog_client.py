import socket
import datetime
import threading
import traceback
import struct
import time
import gc
import sys
import os
path = os.path.abspath(os.path.dirname(os.path.dirname(__file__)))
sys.path.append(path)
from device.parameter import TASK_JSON, SERVER_JSON, SET_PARAMETER_JSON
from data.json_helper import JsonHelper
class AnalogClient():
    """
    模拟客户端str
    """
    max_bytes = 1024 * 1024 * 100
    iqmax_bytes = 1024 * 4
    is_start_server = False
    udp_address = None
    upd_processor = None
    is_new_data = True
    def __init__(self):
        '''
        初始化
        '''
        self.__lock = threading.Lock()
        self.__server_socket = None
        self.__tcp_address = None
        self.__json_hepler = JsonHelper()
    def __recive_data(self, client_socket):
        '''
        开始接收虚拟设备发送的数据 
        '''
        data_count = 0
        while True:
            try:
                # 1.接收数据包头
                head_data, addr = client_socket.recvfrom(53)  # 数据头41个字节
                data_struct = struct.unpack("=sqqqqqid",
                                            head_data)  # 解析数据头-struct封装的二进制
                print(data_struct)
                # 2.接收剩余数据
                data_len = data_struct[6]
                all_data = bytes()
                # 3.循环接收数据，直到数据接收完成
                while data_len > self.iqmax_bytes:
                    data, addr = client_socket.recvfrom(self.iqmax_bytes)
                    all_data += data
                    data_len -= self.iqmax_bytes
                # 4.接收最后一节数据
                data, addr = client_socket.recvfrom(data_len)
                all_data += data
                # 5.每收到5个包打印一次
                data_count += 1
                if data_count == 10:
                    data_count = 0
                    print(str(all_data))
                    print("\n")
                # 6.清理数据
                del head_data, data_struct, all_data
                gc.collect()  # 加入垃圾回收，避免内存溢出
            except ConnectionResetError:
                break
            except Exception:
                traceback.print_exc()
                continue
        # self.__remove_data_transmission(client_socket)
        client_socket.close()
    def connect_device(self):
        """开启客户端"""
        client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        client.settimeout(30)
        client.connect(('127.0.0.1', 5055))
        print("-------------------------------------------------------")
        print(self.get_nowtime())
        print("已连接到服务器。")
        # 开启数据通道并等待输入命令
        self.start_data_endpoint(client)
        # 关闭客户端
        client.close()
    def start_udpsocket(self):
        """
        开启UDP数据传输（备用代码）
        """
        s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        s.bind(('127.0.0.1', 9999))
        # 获取当前IP地址并组装UDP服务命令
        ip = s.getsockname()[0]
        AnalogClient.udp_address = "*udp={0}:{1}\n".format(ip, 9999)
        while True:
            try:
                # 接收数据包头
                head_data, addr = s.recvfrom(41)  # 数据包头41个字节
                data_struct = struct.unpack("=sqqqiif", head_data)
                print(data_struct)
                # 接收剩余数据
                data_len = data_struct[5]
                data, addr = s.recvfrom(data_len)
                str_data = str(data)
                print(str_data)
            except Exception:
                traceback.print_exc()
                continue
    def start_tcpsocket(self, host='127.0.0.1', port=8088):
        '''
        开启本地的tcpsocket并监听虚拟设备发送的数据
        '''
        print('开始监听并接收虚拟设备发送的数据...\n')
        self.__server_socket = socket.socket(socket.AF_INET,
                                             socket.SOCK_STREAM)
        self.__server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR,
                                        True)
        self.__server_socket.bind((host, port))
        self.__server_socket.listen(1)
        print('开启数据通道监听 127.0.0.1:{0}\n'.format(port))
        # 获取当前IP地址并组装UDP服务命令
        ip = self.__server_socket.getsockname()[0]
        # self.__tcp_address = "*tcp={0}:{1}\n".format(ip, port)
        # 保存tcp服务地址端口信息
        SERVER_JSON["params"]["ip"] = ip
        SERVER_JSON["params"]["port"] = port
        while True:
            try:
                client_socket, client_addr = self.__server_socket.accept()
                print('虚拟设备已连接 \"{0}:{1}\"\n'.format(client_addr[0],
                                                     client_addr[1]))
                try:
                    self.__lock.acquire()
                    processor = threading.Thread(target=self.__recive_data,
                                                 args=[client_socket])
                    processor.setDaemon(True)
                    processor.start()
                except Exception:
                    raise
                finally:
                    self.__lock.release()
            except KeyboardInterrupt:
                raise
            except Exception:
                traceback.print_exc()
        self.__server_socket.close()
    def start_data_endpoint(self, client):
        '''
        开启数据通道
        '''
        # 开启udp线程
        upd_processor = threading.Thread(target=self.start_tcpsocket)
        upd_processor.setDaemon(True)
        upd_processor.start()
        print(self.get_nowtime())
        print("客户端tcp数据通道已开启。\n")
        self.is_new_data = True
        # 等待udp开启成功
        while not SERVER_JSON["params"]["ip"]:
            time.sleep(0.1)
            if SERVER_JSON["params"]["ip"]:
                break
        # 将客户端upd地址发送给服务端
        bytes_data = self.__json_hepler.encode_command(SERVER_JSON)
        client.send(bytes_data)
        # 开启等待输入命令状态
        self.wait_enter_command(client)
    def wait_enter_command(self, client):
        '''
        等待命令/发送命令
        '''
        # 0.开启任务
        bytes_data = self.__json_hepler.encode_command(TASK_JSON)
        client.send(bytes_data)
        # 1.长发信号测试
        time.sleep(3)
        SET_PARAMETER_JSON["params"]["frequency"] = 111800000
        bytes_data = self.__json_hepler.encode_command(SET_PARAMETER_JSON)
        client.send(bytes_data)
        # # 2.瞬发信号测试
        # time.sleep(10)
        # SET_PARAMETER_JSON["params"]["frequency"] = 301700000
        # bytes_data = self.__json_hepler.encode_command(SET_PARAMETER_JSON)
        # client.send(bytes_data)
        # # 3.跳频信号测试
        # time.sleep(10)
        # SET_PARAMETER_JSON["params"]["frequency"] = 650000000
        # bytes_data = self.__json_hepler.encode_command(SET_PARAMETER_JSON)
        # client.send(bytes_data)
        command_title = "\n\n命令格式如下：\n命令为Json格式\n\n"
        print(command_title)
        while True:
            input_data = input()
            try:
                bytes_data = self.__json_hepler.encode_command(input_data)
                client.send(bytes_data)
                print("参数设置成功。\n")
                # 从新显示接收到的数据
                self.is_new_data = True
            except Exception:
                print("参数输入错误，命令必须为Json格式。\n")
    def get_nowtime(self):
        """获取当前时间"""
        now_time = "\n" + datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        return now_time
if __name__ == '__main__':
    client = AnalogClient()
    client.connect_device()
