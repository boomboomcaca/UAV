#pragma once

#include "stdint.h"


enum
{
	PACKET_DJI_ID = 1,    // incldue: all OcuSync protocal DJI drone
	PACKET_VEDIO_DJI, // incldue: all OcuSync protocal DJI drone
	PACKET_VEDIO_DAOTONG, //include: datong nano, nano+, lite, lite+, evo2_v3
	PACKET_VEDIO_LightBrdige_DJI, //include: DJI P4,   Inspire 2,
	PACKET_VEDIO_LightBrdige_OTHERS,//include daotong evo2_v3, Feimi, Yuneec H850 RTK,Zino Mini Pro
	PACKET_VEDIO_Yuneec_H52e,//include yuneec h52e
	PACKET_VEDIO_WIFI_DJI, //include  Spark, PHANTOM 3, PHANTOM 3s,  TELLO 
	PACKET_VEDIO_WIFI_DJI_5M, //include DJI Mini, Mini SE, Air 1
	PACKET_VEDIO_WIFI_PARROT, //parrot
	PACKET_VEDIO_WIFI_XIAOMI, //xiaomi
	PACKET_VEDIO_WIFI_SJF11, //世季F11
	PACKET_VEDIO_WIFI_POWEREGG,//PowerEgg
	PACKET_VEDIO_WIFI_OTHERS,
	PACKET_VEDIO_HARBOSEN,
};

#pragma pack(push)
#pragma pack(4)
typedef struct {
	uint16_t packet_type; //包类型
	uint16_t seq_num;  //包序号
	uint16_t state_info; //状态信息
	uint8_t drone_serial_num[17]; //无人机唯一序列号
	double drone_longitude;//飞机经度
	double drone_latitude;//飞机纬度
	float altitude;//海拔
	float height; //高度
	float north_speed;//向北速度
	float east_speed;//向东速度
	float up_speed; //向上速度
	int16_t pitch_angle; // only for ver1
	int16_t roll_angle; // only for ver1  翻滚角度
	int16_t yaw_angle; //航向
	uint64_t gpstime; //only for ver2// gps时间
	double pilot_longitude;  //飞手经度
	double pilot_latitude;  //飞手纬度
	double home_longitude;//返航经度
	double home_latitude;//返航纬度
	uint8_t product_type;//产品型号代码
	char product_type_str[32];//产品型号
	uint8_t uuid_length;//通用唯一识别码长度
	uint8_t uuid[18];// 通用唯一识别码
	uint8_t license[10];//执照
	char wifi_ssid[50];
	char wifi_vendor[50];
	uint8_t wifi_destination_address[6];
	uint8_t wifi_source_address[6];
	uint8_t wifi_bss_id_address[6];
	uint64_t efingerprint;//电子指纹
	int64_t freq;//频率
	float rssi; //信号电平
	float sync_corr; //同步头的相关值
	float bandwidth; //带宽
	char decode_flag;//是否CRC校验通过
	char wifi_flag;
}DJI_FLIGHT_INFO_Str;


#pragma pack(pop)

#ifdef _WIN32
#define ADD_EXPORTS
/* You should define ADD_EXPORTS *only* when building the DLL. */
#ifdef ADD_EXPORTS
#define ADDAPI __declspec(dllexport)
#else
#define ADDAPI __declspec(dllimport)
#endif

/* Define calling convention in one place, for convenience. */
#define ADDCALL __cdecl

#else /* _WIN32 not defined. */

/* Define with no value on non-Windows OSes. */
#define ADDAPI
#define ADDCALL

#endif

typedef void(*drone_cb_fn)(DJI_FLIGHT_INFO_Str *message);
typedef void(*spectrum_cb_fn)(float *spectrum, int64_t start_freq, int length, float resolution);

#ifdef __cplusplus
extern "C"
{
#endif

	extern ADDAPI void config_ip(char *ipstring);
	extern ADDAPI int ADDCALL  GetDeviceNum();
	extern ADDAPI int ADDCALL  OpenDevice(int antenna);
	//freq参数设置为0，
	//gain参数设置最大70
	//sample_rate  15.36e6  13.44e6, 11.52e6
	//drone_cb_fn是回调函数
	//extern ADDAPI void ADDCALL  start_drone_scan(int64_t freq, int gain, int sample_rate, drone_cb_fn cb); //	不推荐使用，
	extern ADDAPI void ADDCALL  start_drone_scan2(int64_t freq, int gain, int sample_rate, drone_cb_fn cb, spectrum_cb_fn cb_psd);// 推荐用这个
	extern ADDAPI void ADDCALL stop_device();
	extern ADDAPI int get_spectrum(float psd[],  int64_t *center_freq);	//	不推荐使用，请使用spectrum_callback
	extern ADDAPI int get_state(float *x);
	extern ADDAPI void set_gpio(int pin, int value);
	extern ADDAPI int ADDCALL config_second_device(char *ip_remote, char *ip_local, int64_t freq, int gain);

#ifdef __cplusplus
}
#endif



