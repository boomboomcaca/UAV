/*Copyright [2020] <Copyright Decentest>*/

#ifndef CODE_SRC_SERVER_TDOA_TDOA_H_
#define CODE_SRC_SERVER_TDOA_TDOA_H_
#include <stdint.h>

#include <string>

#endif  // CODE_SRC_SERVER_TDOA_TDOA_H_

#ifdef _WIN32
#define TDOA_API __declspec(dllexport)
#else
#define TDOA_API
#endif  // CODE_SRC_SERVER_TDOA_TDOA_H_


extern "C" void TDOA_API TDOA_M_fixedstation(double *bs, int bs_size, int *IQ,
                                             int IQlength, double fs,
                                             double *pos_ll, int *len_pos);

extern "C" void TDOA_API GridSelect(double *pos_l, int cN, double *point,
                                    int *len_point);
