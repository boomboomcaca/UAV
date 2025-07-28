


#ifdef _WIN32
#define IntersectionPositioning_API __declspec(dllexport)
#else
#define IntersectionPositioning_API
#endif 

	typedef struct
	{
		int pointLen;
		int azimuthNLength;
		double mindistance;
		int angleindexLength;
		int lonLength;
		int latLength;
		int angleindex[1000];
		float azimuthN[1000];
		double lat[1000];
		double lon[1000];
		double result[100];
		double resultLength;
	} Param;

extern "C" void IntersectionPositioning_API intersectionPositioning(Param *param);
