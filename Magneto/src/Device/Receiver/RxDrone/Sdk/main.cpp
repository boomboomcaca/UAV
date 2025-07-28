#if 1
#include "stdio.h"
#include "stdint.h"
#include "stdlib.h"
#include "interface.h"
#include "math.h"
#include "string.h"
#include "time.h"

#ifdef _WIN32
#include <Windows.h>
#else
#include <unistd.h>
#endif
 
#pragma comment(lib, "ERadiodll.lib")
#pragma warning(disable:4996)

int sample_rate = 13.44e6 * 2,  gain = 60; //

 double gps_distance(double lon1, double lat1, double lon2, double lat2)
{
	lat1 = lat1*3.14159265358979/ 180;
	lon1 = lon1*3.14159265358979 / 180;
	lat2 = lat2*3.14159265358979 / 180;
	lon2 = lon2*3.14159265358979 / 180;
	double vLon = fabs(lon1 - lon2);
	double vLat = fabs(lat1 - lat2);
	double h = sin(vLat / 2)*sin(vLat / 2) + cos(lat1)*cos(lat2)*sin(vLon / 2)*sin(vLon / 2);
	return 2.0 * 6371 * asin(sqrt(h));
}

 void spectrum_callback(float *spectrum, int64_t start_freq, int length, float resolution)
 {
	 //printf("spectrum_callback  freq=%.3f  len=%d\n", start_freq / 1e6, length);

	 //FILE *fid = fopen("psd.dat", "wb");
	 //fwrite(spectrum, 4, length, fid);
	 //fclose(fid);
 }
 double local_longitude = 121.5134, local_latitude = 31.3348; // sh
 //double local_longitude = 112.97764, local_latitude = 28.249499; //cs
void drone_callback(DJI_FLIGHT_INFO_Str *message)
{  

		if (message->decode_flag)
		{
			static int count_msg = 0; printf("%d ", count_msg++);
			if (message->product_type != 0)
			{
				printf(" drone GPS:%f/%f  Height|Altitude %.1f|%.1f  East|Noth|Up Speed %.1f|%.1f|%.1f\n", message->drone_longitude, message->drone_latitude, message->height, message->altitude, message->east_speed, message->north_speed, message->up_speed);
				printf("home GPS:%f/%f  pilot GPS:%f/%f\n", message->home_longitude, message->home_latitude, message->pilot_longitude, message->pilot_latitude);
				printf("serial: %s, model(%d): %s \n", message->drone_serial_num, message->product_type, message->product_type_str);
				float distance = 0;
				if (message->drone_longitude != 0)
				{
					distance = gps_distance(local_longitude, local_latitude, message->drone_longitude, message->drone_latitude);
					printf("the drone is %.2f km from here\n", distance);
				}
#if 1
				time_t timep;
				struct tm *currentTime;
				time(&timep);
				currentTime = localtime(&timep); /*取得当地时间*/
				static int last_t = 0;
				int t = currentTime->tm_hour * 3600 + currentTime->tm_min * 60 + currentTime->tm_sec;			 
				FILE *fid = fopen("scan_result.log", "a");
				fprintf(fid, "%d-%d-%d %d:%d:%d   tgap=%d\n", currentTime->tm_year + 1900, currentTime->tm_mon + 1, currentTime->tm_mday, currentTime->tm_hour, currentTime->tm_min, currentTime->tm_sec, t-last_t);
				fprintf(fid,"freq=%.1f, rssi=%.1f  sample_rate=%.2f  gain=%d  distance=%.2f\n", message->freq / 1e6, message->rssi, sample_rate/1e6, gain,distance);
				fprintf(fid," drone GPS:%f/%f  Height|Altitude %.1f|%.1f  East|Noth|Up Speed %.1f|%.1f|%.1f\n", message->drone_longitude, message->drone_latitude, message->height, message->altitude, message->east_speed, message->north_speed, message->up_speed);
				fprintf(fid,"home GPS:%f/%f  pilot GPS:%f/%f\n", message->home_longitude, message->home_latitude, message->pilot_longitude, message->pilot_latitude);
				fprintf(fid,"serial: %s, model(%d): %s \n\n", message->drone_serial_num, message->product_type, message->product_type_str);
				fclose(fid);
				last_t = t;
#endif 
			}
			else
			{
				printf("serial: %s\n", message->drone_serial_num);
			}

			printf("freq=%.1f, rssi=%.1f\n\n", message->freq / 1e6, message->rssi);
		}
		else
		{
			char msg[512];
			memset(msg, 0, sizeof(msg));

			if (message->packet_type == PACKET_DJI_ID)        sprintf(msg,"DJI drone id is detected at freq=%.1f, rssi=%.1f\n", message->freq / 1e6, message->rssi);
			if (message->packet_type == PACKET_VEDIO_DJI) sprintf(msg, "DJI OcuSync vedio  is detected at freq=%.1f, rssi=%.1f, bw=%dM\n", message->freq / 1e6, message->rssi, (int)(message->bandwidth / 1e6));
			if (message->packet_type == PACKET_VEDIO_LightBrdige_DJI)  sprintf(msg, "LightBrdige DJI P4 is detected at freq=%.1f, rssi=%.1f, bw=%dM\n", message->freq / 1e6, message->rssi, (int)(message->bandwidth / 1e6));
			if (message->packet_type == PACKET_VEDIO_LightBrdige_OTHERS)  sprintf(msg, "LightBrdige Daotong_Evo2_v2/Feimi/Kuxin type  is detected at freq=%.1f, rssi=%.1f, bw=%dM\n", message->freq / 1e6, message->rssi, (int)(message->bandwidth / 1e6));
			if (message->packet_type == PACKET_VEDIO_DAOTONG)  sprintf(msg, "daotong type  is detected at freq=%.1f, efingerprint=%d rssi=%.1f, bw=%dM\n", message->freq / 1e6, message->efingerprint, message->rssi, (int)(message->bandwidth / 1e6));
			if (message->packet_type == PACKET_VEDIO_Yuneec_H52e)  sprintf(msg, "Yuneec_H52e is detected at freq=%.1f, efingerprint=%d rssi=%.1f, bw=%dM\n", message->freq / 1e6, message->efingerprint, message->rssi, (int)(message->bandwidth / 1e6));
			if (message->packet_type == PACKET_VEDIO_WIFI_DJI_5M)  sprintf(msg, "DJI Mini/Mini SE/Air1 is detected at freq=%.1f, rssi=%.1f, bw=%dM\n", message->freq / 1e6, message->rssi, (int)(message->bandwidth / 1e6));
			if (message->packet_type == PACKET_VEDIO_WIFI_DJI )  sprintf(msg, "DJI wifi is detected at freq=%.1f, rssi=%.1f, bw=%dM\n", message->freq / 1e6, message->rssi, (int)(message->bandwidth / 1e6));
			if (message->packet_type == PACKET_VEDIO_HARBOSEN)  sprintf(msg, "Harbosen  is detected at freq=%.1f, rssi=%.1f, bw=%dM\n", message->freq / 1e6, message->rssi, (int)(message->bandwidth / 1e6));

			if (message->wifi_flag)
			{
				sprintf(msg, "%s wifi MAC=%02x:%02x:%02x:%02x:%02x:%02x SSID=%s,  freq=%.1f, rssi=%.1f\n", msg, message->wifi_source_address[0], message->wifi_source_address[1], message->wifi_source_address[2], message->wifi_source_address[3], message->wifi_source_address[4], message->wifi_source_address[5]
					, message->wifi_ssid, message->freq / 1e6, message->rssi);
			}


			printf("%s", msg);
			FILE *fid = fopen("scan_result.log", "a");
			fprintf(fid, "%s", msg);
			fclose(fid);

		}
		//PACKET_VEDIO_DJI_WIFI includes dji mini, mini se,dji mavic air1, dji spark, dji P3
		//PACKET_VEDIO_DAOTONG includes daotong nano, daotong nana+, daotong lite, daotong lite, daotong ev2_v3,
		//PACKET_VEDIO_LightBrdige includes daotong ev2_v2, dji P4,  feimi,
}


#ifdef _WIN32
bool ctrlexit(DWORD fd) {
	fprintf(stdout, "Exit\n");
	stop_device();
	exit(0);
}
#endif

int main(int argc, char* argv[])
{
#ifdef _WIN32
	SetConsoleCtrlHandler((PHANDLER_ROUTINE)ctrlexit, true);
#endif

	int64_t freq=0;
	char ip[20];
	int antenna = 1;
	memset(ip, 0, sizeof(ip));

	for (int j = 1; j < argc; j++)
	{
		if (!strcmp(argv[j], "-ip"))
		{
			j++;
			memcpy(ip, argv[j], strlen(argv[j]));
		}
		if (!strcmp(argv[j], "-gain"))
		{
			gain = atoi(argv[++j]);
		}
		if (!strcmp(argv[j], "-rate"))
		{
			double tmp = atof(argv[++j]);
			if (tmp < 10000) sample_rate = tmp*1e6; else sample_rate = tmp;
		}
		if (!strcmp(argv[j], "-freq"))
		{
			double tmp = atof(argv[++j]);
			if (tmp < 10000) freq = tmp*1e6; else freq = tmp;
		}
		if (!strcmp(argv[j], "-ant"))
		{
			antenna = atoi(argv[++j]);
		}
		if (!strcmp(argv[j], "-longi"))
		{
			local_longitude = atof(argv[++j]);
		}
		if (!strcmp(argv[j], "-lati"))
		{
			local_latitude = atof(argv[++j]);
		}
	}

	if (strlen(ip)>0)
	{
		config_ip(ip);
	}
	else
	{
		config_ip("192.168.2.1");//configure ip here
	}

	
	//int device_num = GetDeviceNum(); // open device, 
	int device_num = OpenDevice(antenna); // open device, 
	if (device_num == -1)
	{
		getchar(); return 0;
	} 
	else {
		printf("device_num =%d  is connected\n", device_num);
	}

	 
	//推荐使用start_drone_scan2函数，可以通过回调函数返回实时频谱
	// 第一个频率参数为0是扫描，频率若不为0是锁定频率，用于调试
	start_drone_scan2(freq, gain, sample_rate, drone_callback, spectrum_callback);//


	//set_gpio(1, 1);


  

	float spectrum[16384];
	int64_t spectrum_center_freq = 0;
	memset(spectrum, 0, sizeof(spectrum));

	while (1)
	{

#ifdef _WIN32
		Sleep(40);
#else
		sleep(100);
#endif



	}
	return 0;
}
#endif