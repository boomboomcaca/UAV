/*Copyright [2021] <Copyright Decentest>*/

#ifndef CODE_SRC_SERVER_ALGORITHM_POA_H_
#define CODE_SRC_SERVER_ALGORITHM_POA_H_

#endif  // CODE_SRC_SERVER_ALGORITHM_POA_H_

#ifdef _WIN32
#define POA_API __declspec(dllexport)
#else
#define POA_API
#endif  // CODE_SRC_SERVER_ALGORITHM_POA_H_

typedef struct _poa {
  double Long;
  double Lat;
} POAResult, *pPOAResult;

extern "C" POA_API void POA_location(double* Long_Lat, int base_count, double* dBm, double fc, POAResult* pResult);
