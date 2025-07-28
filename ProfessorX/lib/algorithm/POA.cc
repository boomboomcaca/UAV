/******************************************************/
/*Copyright [2021] <Copyright Decentest>
/*功能名称:固定站POA多站定位功能
/******************************************************/
#include <iostream>
#include <cstdio>
#include <complex>
#include <cmath>
#include <string>
#include <cstring>

#include "POA.h"

using namespace std;
#define PI 3.1415926

void Translate1(double Long_reference, double Lat_reference, double Long, double Lat, double *x, double *y) {
  double radius = 6371.004;
  double trans = PI / 180.0;
  double rad_rLong = Long_reference * trans;
  double rad_rLat = Lat_reference * trans;
  double rad_Long = Long * trans;
  double rad_Lat = Lat * trans;
  if (rad_rLat == 0) {
    *x = radius * cos(rad_rLat) * (rad_Long - rad_rLong);
    *y = radius * (rad_Lat - rad_rLat);
  } else {
    *x = radius * (cos(rad_rLat) - (rad_Lat - rad_rLat) * sin(rad_rLat)) *
         sin((rad_Long - rad_rLong) * sin(rad_rLat)) / sin(rad_rLat);
    *y = radius * (cos(rad_rLat) - (cos(rad_rLat) - (rad_Lat - rad_rLat)) *
         cos((rad_Long - rad_rLong) * sin(rad_rLat)));
  }
}

void Translate2(double Long_reference, double Lat_reference, double x, double y, double *Long, double *Lat) {
  double trans = PI / 180;
  double radius = 6371.004;
  double rad_rLong = Long_reference * trans;
  double rad_rLat = Lat_reference * trans;
  double transX = 0, transY = 0;
  if (Lat_reference == 0) {
    *Long = rad_rLong + x / (radius*cos(rad_rLat));
    *Lat = rad_rLat + y / radius;
  } else {
    transY = y - (x * x + y * y) / (2 * radius) * tan(rad_rLat);
    transX = x / (1 - (transY / radius) * tan(rad_rLat));
    *Long = rad_rLong + 1 / sin(rad_rLat) * atan(transX * sin(rad_rLat) / (radius * cos(rad_rLat)));
    *Lat = rad_rLat + (transY / radius) * (2 / (1 + sqrt(1 - 2 * transY * tan(rad_rLat) / radius)));
  }
  *Long = *Long / trans;
  *Lat = *Lat / trans;
}

void base_select_once(int n, int base_count, double *x, double *y, double *dbm, double *x_c, double *y_c, double *p_c) {
  double *xx = new double[base_count];
  double *yy = new double[base_count];
  double *pp = new double[base_count];
  for (int i = 0; i < base_count; i++) {
    xx[i] = x[i];
    yy[i] = y[i];
    pp[i] = dbm[i];
  }
  double mx1 = 0, mx2 = 0, mx3 = 0;
  for (int i = 0; i < base_count - 1; i++) {
    for (int j = i + 1; j < base_count; j++) {
      if (pp[j] > pp[i]) {
        mx1 = xx[i];
        xx[i] = xx[j];
        xx[j] = mx1;

        mx2 = yy[i];
        yy[i] = yy[j];
        yy[j] = mx2;

        mx3 = pp[i];
        pp[i] = pp[j];
        pp[j] = mx3;
      }
    }
  }
  for (int i = 0; i < n; i++) {
    x_c[i] = xx[i];
    y_c[i] = yy[i];
    p_c[i] = pp[i];
  }
  delete[] xx;
  delete[] yy;
  delete[] pp;
}

void AreaMth(double*P, double *x_tr, double *y_tr, int *flag) {
  double v0[2] = { x_tr[2] - x_tr[0], y_tr[2] - y_tr[0] };
  double v1[2] = { x_tr[1] - x_tr[0], y_tr[1] - y_tr[0] };
  double v2[2] = { P[0] - x_tr[0], P[1] - y_tr[0] };

  double dot00 = v0[0] * v0[0] + v0[1] * v0[1];
  double dot01 = v0[0] * v1[0] + v0[1] * v1[1];
  double dot02 = v0[0] * v2[0] + v0[1] * v2[1];
  double dot11 = v1[0] * v1[0] + v1[1] * v1[1];
  double dot12 = v1[0] * v2[0] + v1[1] * v2[1];

  double invDenom = 1 / (dot00*dot11 - dot01*dot01);
  double u = (dot11*dot02 - dot01*dot12)*invDenom;
  double v = (dot00*dot12 - dot01*dot02)*invDenom;

  if (u > 0 && v > 0 && u + v < 1) {
    *flag = 1;
  } else if (u == 0 || v == 0 || u + v == 1) {
    *flag = 0;
  } else {
    *flag = -1;
  }
}

void base_select_second(int n, int base_count, double *x, double *y,
                        double *dbm, double *x_s, double *y_s, double *p_s) {
  int select0 = 1; int select1 = 1;
  for (int i = base_count - n + 1; i < base_count + 1; i++) {
    select0 = select0 * i;
    select1 = select1 *(i - base_count + n);
  }
  int select = select0 / select1;

  int *point1 = new int[select];
  int *point2 = new int[select];
  int *point3 = new int[select];
  double *power_order = new double[select];
  int count = 0;
  for (int i = 0; i < base_count - 2; i++) {
    for (int j = i + 1; j < base_count - 1; j++) {
      for (int k = j + 1; k < base_count; k++) {
        point1[count] = i;
        point2[count] = j;
        point3[count] = k;
        double x_tr[3] = { x[i], x[j], x[k] };
        double y_tr[3] = { y[i], y[j], y[k] };
        double P[2] = { 0, 0 };
        int flag = 0;
        AreaMth(P, x_tr, y_tr, &flag);
        if (flag == 1) {
          power_order[count] = dbm[i] + dbm[j] + dbm[k];
        } else {
          power_order[count] = 0;
        }
        count = count + 1;
      }
    }
  }
  int pp_c = 0;
  double pp = 0;
  for (int i = 0; i < select; i++) {
    if (power_order[i] != 0) {
      pp = power_order[i];
      pp_c = i;
      break;
    }
  }
  if (pp == 0) {
    memset(x_s, 0, n*sizeof(double));
    memset(y_s, 0, n * sizeof(double));
    memset(p_s, 0, n * sizeof(double));
  } else {
    for (int i = 0; i < select; i++) {
      if (power_order[i] > pp && power_order[i] != 0) {
        pp = power_order[i];
        pp_c = i;
      }
    }
    int vc1 = point1[pp_c];
    int vc2 = point2[pp_c];
    int vc3 = point3[pp_c];
    x_s[0] = x[vc1]; x_s[1] = x[vc2]; x_s[2] = x[vc3];
    y_s[0] = y[vc1]; y_s[1] = y[vc2]; y_s[2] = y[vc3];
    p_s[0] = dbm[vc1]; p_s[1] = dbm[vc2]; p_s[2] = dbm[vc3];
  }
  delete[] point1;
  delete[] point2;
  delete[] point3;
  delete[] power_order;
}

void freespace(int base_count, double *x, double *y, double *dbm, double fc, double *x_fr, double *y_fr) {
  double Lp_a = 32.44 + 20 * log10(fc);
  double *Lp = new double[base_count];

  double x1 = 0, y1 = 9, d_fs = 0;
  for (int i = 0; i < base_count; i++) {
    x1 = x[i] / 1000;
    y1 = y[i] / 1000;
    d_fs = sqrt(x1*x1 + y1*y1);
    Lp[i] = Lp_a + 20 * log10(d_fs);
  }
  double pt = 0;
  for (int i = 0; i < base_count; i++) {
    pt = pt + Lp[i] + dbm[i];
  }
  pt = pt / base_count;
  double d = 0, sump = 0, d_n = 0, d_x_sum = 0, d_y_sum = 0;
  for (int i = 0; i < base_count; i++) {
    d = pow(10, (pt - Lp_a - dbm[i]) / 20);
    d_n = d_n + 1 / d;
    sump = sump + d;
    d_x_sum = d_x_sum + x[i] / (d * 1000);
    d_y_sum = d_y_sum + y[i] / (d * 1000);
  }

  if (sump == 0) {
    *x_fr = 0; *y_fr = 0;
  } else {
    *x_fr = d_x_sum / d_n * 1000;
    *y_fr = d_y_sum / d_n * 1000;
  }
  delete[] Lp;
}

/***********************************接口说明2021/5/10***************************************
  Long_lat：经纬度信息，格式：
  [监测站1的经度，监测站1的纬度，监测站2的经度，监测站2的纬度,...,监测站n的经度，监测站n的纬度]
  base_count：站点数量
，单位：dBm
  fc：当前工作频  dBm：各站点电平数据率
  pResult：经纬度结构体
*******************************************************************************************/
POA_API void POA_location(double* Long_Lat, int base_count, double* dBm, double fc, POAResult* pResult) {
  double Long_reference, Lat_reference;
  Long_reference = Long_Lat[0];
  Lat_reference = Long_Lat[1];
  double *x = new double[base_count];
  double *y = new double[base_count];
  for (int i = 0; i < base_count; i++) {
    Translate1(Long_reference, Lat_reference, Long_Lat[2 * i], Long_Lat[2 * i + 1], &x[i], &y[i]);
    x[i] = x[i] * 1000;
    y[i] = y[i] * 1000;
  }
  int n = 3;
  double *x_c = new double[3];
  double *y_c = new double[3];
  double *p_c = new double[3];
  base_select_once(n, base_count, x, y, dBm, x_c, y_c, p_c);
  double x_centroid = 0, y_centroid = 0;
  double sump = 0;
  double x_cen = 0;
  double y_cen = 0;
  for (int i = 0; i < n; i++) {
    sump = sump + dBm[i];
    x_cen = x_cen + dBm[i] * x[i];
    y_cen = y_cen + dBm[i] * y[i];
  }
  if (sump == 0) {
    x_centroid = 0;
    y_centroid = 0;
  } else {
    x_centroid = x_cen / sump;
    y_centroid = y_cen / sump;
  }
  double Long_ap = 0, Lat_ap = 0;
  Translate2(Long_reference, Lat_reference, x_centroid / 1000, y_centroid / 1000, &Long_ap, &Lat_ap);
  double * x_ap = new double[base_count];
  double * y_ap = new double[base_count];
  for (int i = 0; i < base_count; i++) {
    Translate1(Long_ap, Lat_ap, Long_Lat[2 * i], Long_Lat[2 * i + 1], &x_ap[i], &y_ap[i]);
    x_ap[i] = x_ap[i] * 1000;
    y_ap[i] = y_ap[i] * 1000;
  }
  double *x_consist = new double[3];
  double *y_consist = new double[3];
  double *p_consist = new double[3];

  base_select_second(n, base_count, x_ap, y_ap, dBm, x_consist, y_consist, p_consist);
  double x_con = 0, y_con = 0;
  freespace(n, x_consist, y_consist, p_consist, fc, &x_con, &y_con);
  Translate2(Long_ap, Lat_ap, x_con / 1000, y_con / 1000, &(pResult->Long), &(pResult->Lat));

  delete[] x;
  delete[] y;

  delete[] x_c;
  delete[] y_c;
  delete[] p_c;

  delete[] x_ap;
  delete[] y_ap;

  delete[] x_consist;
  delete[] y_consist;
  delete[] p_consist;
}
