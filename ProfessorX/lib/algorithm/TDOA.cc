/******************************************************/
/*Copyright [2020] <Copyright Decentest>
/*功能名称:固定站TDOA多站定位功能
/*作者:
/*完成日期:
/*当前版本:
/*修改内容:
/******************************************************/

#include "TDOA.h"

#include <math.h>
#include <stdint.h>
#include <stdio.h>
#include <time.h>

#include <complex>
#include <iostream>
#include <string>
#include <cstring>
#include <vector>

#define pi 3.14159265
#define sol 3e5  // speed of light,光速,Km/s
typedef std::complex<double>
    Complex;  // typedef的作用是将std::complex<double>这一变量类型重新命名为Complex，简化后面复数类型变量的定义

void TDMA(double *E, int n, double *A, double *B, double *C,
          double *D) {  // 求解三对角矩阵
  double *gama = new double[n];
  double *ro = new double[n];
  // 上三角矩阵
  gama[0] = C[0] / B[0];
  ro[0] = D[0] / B[0];
  int i;
  double tmp;
  for (i = 1; i < n; i++) {
    tmp = (B[i] - A[i] * gama[i - 1]);
    gama[i] = C[i] / tmp;
    ro[i] = (D[i] - A[i] * ro[i - 1]) / tmp;
  }

  // 直接求出E的最后一个值
  E[n - 1] = ro[n - 1];

  // 逆向迭代， 求出X
  for (i = n - 2; i >= 0; i--) {
    E[i] = ro[i] - gama[i] * E[i + 1];
  }
  delete[] gama;
  delete[] ro;
}

/*----插值函数----*/
void spline(int n, int *x, int *y, int m, int *sp_x, double *sp_y) {
  int i;
  double *hx = new double[n - 1];
  double *hy = new double[n - 1];
  double *div = new double[n - 1];
  // 计算步长
  for (i = 0; i < n - 1; i++) {
    hx[i] = x[i + 1] - x[i];
    hy[i] = y[i + 1] - y[i];
    div[i] = 1.0 * hy[i] / hx[i];
  }
  // ABCD矩阵的复制参考MATLAB中函数spline第71-82行
  double *A = new double[n];
  double *B = new double[n];
  double *C = new double[n];
  double *D = new double[n];
  // 指定系数
  double x20 = x[2] - x[0];
  double xn = x[n - 1] - x[n - 3];

  A[0] = x20;
  A[n - 1] = 0;
  B[0] = hx[1];
  B[n - 1] = hx[n - 3];
  C[0] = 0;
  C[n - 1] = xn;
  for (i = 1; i < n - 1; i++) {
    A[i] = hx[i - 1];
    B[i] = 2 * (hx[i - 1] + hx[i]);
    C[i] = hx[i];
  }
  // 指定常数D
  D[0] = ((hx[0] + 2 * x20) * hx[1] * div[0] + hx[0] * hx[0] * div[1]) / x20;
  D[n - 1] = (hx[n - 2] * hx[n - 2] * div[n - 3] +
              (2 * xn + hx[n - 2]) * hx[n - 3] * div[n - 2]) /
             xn;
  for (i = 1; i < n - 1; i++) {
    D[i] = 3 * (hx[i] * div[i - 1] + hx[i - 1] * div[i]);
  }
  // 由于MATLAB中的D = E * Diag;其中D,E均为行向量, Diag为三对角矩阵
  // 为了应用TDMA求解调频斜率k，进行转置 (E * Diag).' = D.'   => Diag.' * E.' =
  // D.' 在C语言中，其中D与D.'为行转列, k与k.'为行转列，
  // Diag矩阵为三对角矩阵，Diag.'需要三对角矩阵中对应的ABC中的A与C调换
  double *E = new double[n];
  // 求解三对角矩阵， 结果复制给E--调频斜率
  TDMA(E, n, C, B, A, D);
  // 判断sp_x属于哪个区域，并进行插值
  double delta, lamta, s0, s1, k1, k2;
  int j;
  for (i = 0; i < m; i++) {
    for (j = 0; j < n - 1; j++) {
      if (sp_x[i] >= x[j] && sp_x[i] < x[j + 1]) {
        delta = pow((sp_x[i] - x[j + 1]) * 1.0 / (x[j] - x[j + 1]), 2.0);
        lamta = pow((sp_x[i] - x[j]) * 1.0 / (x[j + 1] - x[j]), 2.0);
        s0 =
            y[j] * (1 + 2 * (sp_x[i] - x[j]) * 1.0 / (x[j + 1] - x[j])) * delta;
        s1 = y[j + 1] *
             (1 + 2 * (sp_x[i] - x[j + 1]) * 1.0 / (x[j] - x[j + 1])) * lamta;
        k1 = E[j] * (sp_x[i] - x[j]) * delta;
        k2 = E[j + 1] * (sp_x[i] - x[j + 1]) * lamta;
        sp_y[i] = s0 + s1 + k1 + k2;
        break;
      }
    }
  }
  sp_y[m - 1] = y[n - 1];

  delete[] hx;
  delete[] hy;
  delete[] div;
  delete[] A;
  delete[] B;
  delete[] C;
  delete[] D;
  delete[] E;
}

/*----最大值索引修正----*/
void trans(int *index, int N) {
  if (*index > N / 4) *index = *index - N / 2;
}

/*----经纬度坐标转直角坐标----*/
void Translate1(double ReferenceLong, double ReferenceLat, double Long,
                double Lat, double *x, double *y) {
  double trans = pi / 180;  //角度转化为弧度
  double RADIUS = 6371.004;
  double radReferenceLong = ReferenceLong * trans;
  double radReferenceLat = ReferenceLat * trans;
  double radLong = Long * trans;
  double radLat = Lat * trans;
  if (radReferenceLat == 0) {
    *x = RADIUS * cos(radReferenceLat) * (radLong - radReferenceLong);
    *y = RADIUS * (radLat - radReferenceLat);
  } else {
    *x = RADIUS *
         (cos(radReferenceLat) -
          (radLat - radReferenceLat) * sin(radReferenceLat)) *
         sin((radLong - radReferenceLong) * sin(radReferenceLat)) /
         sin(radReferenceLat);
    *y =
        RADIUS * (1 / tan(radReferenceLat) -
                  (1 / tan(radReferenceLat) - (radLat - radReferenceLat)) *
                      cos((radLong - radReferenceLong) * sin(radReferenceLat)));
  }
}

/*----直角坐标转经纬度坐标----*/
void Translate2(double ReferenceLong, double ReferenceLat, double x, double y,
                double *Long, double *Lat) {
  double trans = pi / 180;  //角度转化为弧度
  double RADIUS = 6371.004;
  double radReferenceLong = ReferenceLong * trans;
  double radReferenceLat = ReferenceLat * trans;
  if (ReferenceLat != 0) {
    double transY = y - (x * x + y * y) / (2 * RADIUS) * tan(radReferenceLat);
    double transX = x / (1 - (transY / RADIUS) * tan(radReferenceLat));
    *Long = radReferenceLong + 1 / sin(radReferenceLat) *
                                   atan(transX * sin(radReferenceLat) /
                                        (RADIUS * cos(radReferenceLat)));
    *Lat = radReferenceLat +
           (transY / RADIUS) *
               (2 / (1 + sqrt(1 - 2 * transY * tan(radReferenceLat) / RADIUS)));
  } else {
    *Long = radReferenceLong + x / (RADIUS * cos(radReferenceLat));
    *Lat = radReferenceLat + y / RADIUS;
  }
  *Long = *Long / trans;
  *Lat = *Lat / trans;
}

/*----时域抽取的FFT算法----*/
void fft(Complex f[], int N) {
  int i, j, k, m, n, l, r, M;
  int la, lb, lc;
  Complex t;
  /*----计算分解的级数M=log2(N)----*/
  for (i = N, M = 1; (i = i / 2) != 1; M++) {
  }
  /*----按照倒位序重新排列原信号----*/
  for (i = 1, j = N / 2; i <= N - 2; i++) {
    if (i < j) {
      t = f[j];
      f[j] = f[i];
      f[i] = t;
    }
    k = N / 2;
    while (k <= j) {
      j = j - k;
      k = k / 2;
    }
    j = j + k;
  }
  /*----FFT算法----*/
  for (m = 1; m <= M; m++) {
    la = pow(static_cast<double>(2), m);  // la=2^m代表第m级每个分组所含节点数
    lb = la / 2;  // lb代表第m级每个分组所含碟形单元数
    /*----碟形运算----*/
    for (l = 1; l <= lb; l++) {
      r = (l - 1) * pow(static_cast<double>(2), M - m);
      for (n = l - 1; n < N - 1; n = n + la) {  // 遍历每个分组，分组总数为N/la
        lc = n + lb;  // n,lc分别代表一个碟形单元的上、下节点编号
        t = f[lc] * Complex(cos(2 * pi * r / N), -sin(2 * pi * r / N));
        f[lc] = f[n] - t;
        f[n] = f[n] + t;
      }
    }
  }
}

/*----时域抽取的IFFT算法----*/
void ifft(Complex f[], int N) {
  int i, j, k, m, n, l, r, M;
  int la, lb, lc;
  Complex t;

  /*----计算分解的级数M=log2(N)----*/
  for (i = N, M = 1; (i = i / 2) != 1; M++) {
  }

  /*----将信号乘以1/N----*/
  for (i = 0; i < N; i++) f[i] = f[i] / Complex(N, 0.0);

  /*----IFFT算法----*/
  for (m = 1; m <= M; m++) {
    la = pow(static_cast<double>(2),
             M + 1 - m);  // la=2^m代表第m级每个分组所含节点数
    lb = la / 2;          // lb代表第m级每个分组所含碟形单元数
    //同时它也表示每个碟形单元上下节点之间的距离
    /*----碟形运算----*/
    for (l = 1; l <= lb; l++) {
      r = (l - 1) * pow(static_cast<double>(2), m - 1);
      for (n = l - 1; n < N - 1; n = n + la) {  // 遍历每个分组，分组总数为N/la
        lc = n + lb;  // n,lc分别代表一个碟形单元的上、下节点编号
        t = f[n] + f[lc];
        f[lc] =
            (f[n] - f[lc]) * Complex(cos(2 * pi * r / N), sin(2 * pi * r / N));
        f[n] = t;
      }
    }
  }
  /*----按照倒位序重新排列变换后信号----*/
  for (i = 1, j = N / 2; i <= N - 2; i++) {
    if (i < j) {
      t = f[j];
      f[j] = f[i];
      f[i] = t;
    }
    k = N / 2;
    while (k <= j) {
      j = j - k;
      k = k / 2;
    }
    j = j + k;
  }
}

/*----求数组最大值及其对应的下标值--- */
int MaxIndex(double a[], double n) {
  int maxi = 0;
  for (int i = 1; i < n; i++)
    if (a[i] > a[maxi]) {
      maxi = i;
    }

  return maxi;
}

/*----求数组最大值----*/
double Max(double a[], double n) {
  int i, maxi = 0;
  double a_max = a[0];
  for (i = 1; i < n; i++)
    if (a[i] > a[maxi]) {
      maxi = i;
      a_max = a[i];
    }

  return a_max;
}

/*----求数组最小值----*/
double Min(double a[], double n) {
  int i, mini = 0;
  double a_min = a[0];
  for (i = 1; i < n; i++)
    if (a[i] < a[mini]) {
      mini = i;
      a_min = a[i];
    }
  return a_min;
}

/*----计算一组点的范围大小----*/
double calRange(double **pos, int N) {
  double max_range = 0;
  double right_long = Max(pos[0], static_cast<double>(N));
  double left_long = Min(pos[0], static_cast<double>(N));
  double up_lat = Max(pos[1], static_cast<double>(N));
  double down_lat = Min(pos[1], static_cast<double>(N));

  double RLong = left_long;
  double RLat = up_lat;
  double x[4] = {0};
  double y[4] = {0};
  Translate1(RLong, RLat, left_long, up_lat, &(x[0]), &(y[0]));
  Translate1(RLong, RLat, right_long, up_lat, &(x[1]), &(y[1]));
  Translate1(RLong, RLat, left_long, down_lat, &(x[2]), &(y[2]));
  Translate1(RLong, RLat, right_long, down_lat, &(x[3]), &(y[3]));

  double max_x = Max(x, 4);
  double min_x = Min(x, 4);
  double max_y = Max(y, 4);
  double min_y = Min(y, 4);
  max_range =
      (max_x - min_x) > (max_y - min_y) ? (max_x - min_x) : (max_y - min_y);
  return max_range;
}

/*----计算一组点的中心点----*/
void FindMiddlePoint(double **pos, int N, int *mIndex) {
  double *rangeSum = new double[N];
  double *X = new double[N];
  double *Y = new double[N];
  for (int n = 0; n < N; n++) {
    Translate1(pos[0][0], pos[1][0], pos[0][n], pos[1][n], &(X[n]), &(Y[n]));
  }
  double minSum = 99999;
  int minIndex = 0;
  for (int n = 0; n < N; n++) {
    rangeSum[n] = 0;
    for (int m = 0; m < N; m++)
      rangeSum[n] +=
          sqrt((X[n] - X[m]) * (X[n] - X[m]) + (Y[n] - Y[m]) * (Y[n] - Y[m]));
    if (rangeSum[n] < minSum) {
      minSum = rangeSum[n];
      minIndex = n;
    }
  }
  *mIndex = minIndex;

  delete[] rangeSum;
  rangeSum = NULL;
}

/*矩阵的上下三角分解*/
void MatrixLU(double **A, double **Low, double **Up, int N) {
  for (int i = 0; i < N; i++)
    for (int j = 0; j < N; j++) {
      Low[i][j] = 0;
      Up[i][j] = 0;
    }
  for (int i = 0; i < N; i++) {
    for (int j = 0; j < N; j++) {
      if (i == j) Low[i][i] = 1;
    }
  }
  for (int i = 0; i < N; i++) {
    Up[0][i] = A[0][i];
    Low[i][0] = A[i][0] / Up[0][0];
  }
  for (int r = 1; r < N; r++) {
    for (int j = r; j < N; j++) {
      double temp = 0;
      for (int k = 0; k < r; k++) temp += Low[r][k] * Up[k][j];
      Up[r][j] = A[r][j] - temp;
    }
    for (int i = r + 1; i < N; i++) {
      double temp = 0;
      for (int k = 0; k < r; k++) temp += Low[i][k] * Up[k][r];
      Low[i][r] = (A[i][r] - temp) / Up[r][r];
    }
  }
}

/*矩阵求逆*/
void MatrixInverse(double **A, double **invA, int N) {
  double **Low = new double *[N];
  double **Up = new double *[N];
  double **RLow = new double *[N];
  double **RUp = new double *[N];
  for (int i = 0; i < N; i++) {
    Low[i] = new double[N];
    RLow[i] = new double[N];
    Up[i] = new double[N];
    RUp[i] = new double[N];
  }

  MatrixLU(A, Low, Up, N);
  // L矩阵求逆
  for (int j = 0; j < N; j++) {
    for (int i = 0; i < N; i++) {
      if (i == j) {
        RLow[j][i] = 1 / Low[j][i];
      } else if (i < j) {
        double temp = 0;
        for (int k = 0; k < j; k++) temp += Low[j][k] * RLow[k][i];
        RLow[j][i] = -RLow[i][i] * temp;
      } else {
        RLow[j][i] = 0;
      }
    }
  }
  // U 矩阵求逆

  for (int j = 0; j < N; j++) {
    for (int i = N - 1; i >= 0; i--) {
      if (i == j) {
        RUp[i][i] = 1 / Up[i][i];
      } else if (i < j) {
        double temp = 0;
        for (int k = i + 1; k <= j; k++) temp += Up[i][k] * RUp[k][j];
        RUp[i][j] = -temp / Up[i][i];
      } else {
        RUp[i][j] = 0;
      }
    }
  }
  //
  for (int i = 0; i < N; i++) {
    for (int j = 0; j < N; j++) {
      invA[i][j] = 0;
      for (int k = 0; k < N; k++) {
        invA[i][j] += RUp[i][k] * RLow[k][j];
      }
      // printf("%lf", invA[i][j]);
    }
    // printf("\n");
  }

  delete[] Up;
  delete[] RUp;
  delete[] Low;
  delete[] RLow;
}

/*----计算两个经纬度之间的距离----*/
double distanceLL(double long1, double lat1, double long2, double lat2) {
  double x2 = 0;
  double y2 = 0;
  Translate1(long1, lat1, long2, lat2, &x2, &y2);
  double dd = sqrt(x2 * x2 + y2 * y2);
  return dd;
}

/*----时延估计函数----*/
void TimeDelay(Complex *Sx, Complex *Sy, int fpoint, int *sample_delay_xy) {
  Complex *conj_Sy = new Complex[fpoint];
  Complex *Sxy = new Complex[fpoint];
  Complex *Rxy = new Complex[fpoint];

  for (int i = 0; i < fpoint; i++) {
    conj_Sy[i] = Complex(Sy[i].real(), -Sy[i].imag());  // Sy的共轭函数
    Sxy[i] = Sx[i] * conj_Sy[i];  // Sx与Sy的互功率谱函数
  }
  ifft(Sxy, fpoint);
  for (int i = 0; i < fpoint; i++) {
    Rxy[i] = Complex(Sxy[i].real(), Sxy[i].imag());  // 信号x与y的互相关函数
  }
  /*求互相关函数的模值*/
  double *abs_Rxy = new double[fpoint];
  for (int i = 0; i < fpoint; i++) {
    abs_Rxy[i] = sqrt(pow(static_cast<double>(Rxy[i].real()), 2) +
                      pow(static_cast<double>(Rxy[i].imag()), 2));
  }

  /*数据前后置换*/
  double *Nor_abs_Rxy = new double[fpoint];
  for (int i = 0; i < fpoint / 2; i++) {
    Nor_abs_Rxy[i] = abs_Rxy[i + fpoint / 2];
    Nor_abs_Rxy[i + fpoint / 2] = abs_Rxy[i];
  }

  /*求两路信号延迟时间和延迟距离*/
  int max_index_Rxy = MaxIndex(Nor_abs_Rxy, fpoint);

  /*归一化互相关函数的模值*/
  double max_abs_Rxy = Nor_abs_Rxy[max_index_Rxy];
  for (int i = 0; i < fpoint; i++) {
    Nor_abs_Rxy[i] = Nor_abs_Rxy[i] / max_abs_Rxy;
  }

  *sample_delay_xy = (max_index_Rxy - fpoint / 2);

  delete[] conj_Sy;
  conj_Sy = NULL;
  delete[] Sxy;
  Sxy = NULL;
  delete[] Rxy;
  Rxy = NULL;
  delete[] abs_Rxy;
  abs_Rxy = NULL;
  delete[] Nor_abs_Rxy;
  Nor_abs_Rxy = NULL;
}

/*三站TDOA定位chan算法*/
void TDOA_3Chan(double fs, double (*bs_M)[2], int *sampleDelayRM,
                double *pos_chan, int *len_pos_chan, double *Rchan) {
  double pos_t[2][2] = {0};
  int result_ct = 0;
  double X[3] = {0};
  double Y[3] = {0};
  double RLong = bs_M[0][0];
  double RLat = bs_M[0][1];
  // 时差转换
  int sampleDelay12 = 0;
  int sampleDelay13 = 0;
  sampleDelay12 = sampleDelayRM[1] - sampleDelayRM[0];
  sampleDelay13 = sampleDelayRM[2] - sampleDelayRM[0];
  // CHAN
  Translate1(RLong, RLat, bs_M[0][0], bs_M[0][1], &X[0], &Y[0]);
  Translate1(RLong, RLat, bs_M[1][0], bs_M[1][1], &X[1], &Y[1]);
  Translate1(RLong, RLat, bs_M[2][0], bs_M[2][1], &X[2], &Y[2]);
  double X10 = X[1] - X[0];
  double X20 = X[2] - X[0];
  double Y10 = Y[1] - Y[0];
  double Y20 = Y[2] - Y[0];

  double K[3] = {0};  // K=pow(X,2)+pow(Y,2)
  for (int i = 0; i < 3; i++) {
    K[i] = pow(X[i], 2) + pow(Y[i], 2);
  }

  double dd_21 = -sampleDelay12 * sol /
                 fs;  // dd：distance difference 距离差（以度为单位）
  double dd_31 =
      -sampleDelay13 * sol /
      fs;  // fre.fs:代表取fre这个类（结构体是一种类类型）中的fs这个成员，
  // “.”是点操作符，它是通过左操作数取得右操作数，点操作符仅应用于类类型的对象，其左操作数必须是类类型的对象，右操作数必须是该类型的成员，而不能是对象或者数值。
  double R[2] = {dd_21, dd_31};

  double a1[2] = {Y20, -Y10};
  double a2[2] = {pow(R[0], 2) - K[1] + K[0], pow(R[1], 2) - K[2] + K[0]};
  double a;
  a = 0.5 * (a1[0] * a2[0] + a1[1] * a2[1]) / (Y10 * X20 - Y20 * X10);
  double b1[2] = {Y20, -Y10};
  double b2[2] = {R[0], R[1]};
  double b;
  b = (b1[0] * b2[0] + b1[1] * b2[1]) / (Y10 * X20 - Y20 * X10);

  double c1[2] = {-X20, X10};
  double c2[2] = {pow(R[0], 2) - K[1] + K[0], pow(R[1], 2) - K[2] + K[0]};
  double c;
  c = 0.5 * (c1[0] * c2[0] + c1[1] * c2[1]) / (Y10 * X20 - Y20 * X10);

  double d1[2] = {-X20, X10};
  double d2[2] = {R[0], R[1]};
  double d;
  d = (d1[0] * d2[0] + d1[1] * d2[1]) / (Y10 * X20 - Y20 * X10);
  double e = pow(b, 2) + pow(d, 2) - 1;
  double f = 2 * (a * b - b * X[0] + c * d - d * Y[0]);
  double g = pow((a - X[0]), 2) + pow((c - Y[0]), 2);
  double R1_1 = (-f + sqrt(pow(f, 2) - 4 * e * g)) / 2 / e;
  double R1_2 = (-f - sqrt(pow(f, 2) - 4 * e * g)) / 2 / e;

  if (R1_1 >= 0 && R1_2 >= 0) {
    pos_t[result_ct][0] = a + b * R1_1;
    pos_t[result_ct][1] = c + d * R1_1;
    result_ct++;

    pos_t[result_ct][0] = a + b * R1_2;
    pos_t[result_ct][1] = c + d * R1_2;
    result_ct++;
  } else if (R1_1 >= 0) {
    pos_t[result_ct][0] = a + b * R1_1;
    pos_t[result_ct][1] = c + d * R1_1;
    result_ct++;
  } else if (R1_2 >= 0) {
    pos_t[result_ct][0] = a + b * R1_2;
    pos_t[result_ct][1] = c + d * R1_2;
    result_ct++;
  }

  *len_pos_chan = result_ct * 2;
  double Long = 0;
  double Lat = 0;
  for (int i = 0; i < result_ct; i++) {
    Translate2(RLong, RLat, pos_t[i][0], pos_t[i][1], &Long, &Lat);
    pos_chan[2 * i] = Long;
    pos_chan[2 * i + 1] = Lat;
    // 计算加权系数
    double Rc1 = sqrt((pos_t[i][0] - X[0]) * (pos_t[i][0] - X[0]) +
                      (pos_t[i][1] - Y[0]) * (pos_t[i][1] - Y[0]));
    double Rc2 = sqrt((pos_t[i][0] - X[1]) * (pos_t[i][0] - X[1]) +
                      (pos_t[i][1] - Y[1]) * (pos_t[i][1] - Y[1]));
    double Rc3 = sqrt((pos_t[i][0] - X[2]) * (pos_t[i][0] - X[2]) +
                      (pos_t[i][1] - Y[2]) * (pos_t[i][1] - Y[2]));
    double RRchan = (dd_21 - (Rc2 - Rc1)) * (dd_21 - (Rc2 - Rc1)) +
                    (dd_31 - (Rc3 - Rc1)) * (dd_31 - (Rc3 - Rc1));
    Rchan[i] = RRchan / 2;
  }
}

/*多站站TDOA定位chan算法*/
void TDOAChan_Mstation(int M, double fs, double (*bs_M)[2], int *sampleDelay1M,
                       double *pos_chan, int *len_pos_chan, double *Rchan) {
  if (M < 3) {
    *len_pos_chan = 0;  // 站点个数小于3，返回
    return;
  }
  double Zpt[2] = {0};
  double RLong = bs_M[0][0];
  double RLat = bs_M[0][1];

  double *RM1 = new double[M - 1];
  for (int i = 1; i < M; i++) {
    RM1[i - 1] = -sampleDelay1M[i] * 3.0e5 / fs;
  }
  ///*********  CHAN  ******///
  double *X = new double[M];
  double *Y = new double[M];
  double *XM1 = new double[M - 1];
  double *YM1 = new double[M - 1];
  double *K = new double[M];
  for (int i = 0; i < M; i++) {
    Translate1(RLong, RLat, bs_M[i][0], bs_M[i][1], &X[i], &Y[i]);
    K[i] = X[i] * X[i] + Y[i] * Y[i];
  }

  for (int i = 1; i < M; i++) {
    XM1[i - 1] = X[i] - X[0];
    YM1[i - 1] = Y[i] - Y[0];
  }
  //// STEP1////
  double *H = new double[M - 1];
  double *B = new double[M - 1];
  double(*Ga)[3] = new double[M - 1][3];  // (M - 1)*3
  double *Q = new double[M - 1];  // 对角阵Q，简化为包含对角元素的(M-1)维数组
  double *Phi =
      new double[M - 1];  // 对角阵Phi，简化为包含对角元素的(M-1)维数组
  double **temp_Ma = new double *[3];  // 3*(M-1)
  for (int i = 0; i < 3; i++) temp_Ma[i] = new double[M - 1];
  double **temp_inv = new double *[3];  // 3*3
  double **G_inv = new double *[3];     // 3*3
  for (int i = 0; i < 3; i++) {
    temp_inv[i] = new double[3];
    G_inv[i] = new double[3];
  }

  double temp_Za[3] = {0};
  for (int i = 0; i < M - 1; i++) {
    H[i] = 0.5 * (RM1[i] * RM1[i] - K[i + 1] + K[0]);
    Ga[i][0] = -1 * XM1[i];
    Ga[i][1] = -1 * YM1[i];
    Ga[i][2] = -1 * RM1[i];
    Q[i] = 1;  // Q先定义为单位阵
  }
  for (int i = 0; i < 3; i++)
    for (int j = 0; j < M - 1; j++)
      temp_Ma[i][j] = Ga[j][i] / Q[j];  // temp_Ma=Ga.'*i_Q;
  for (int i = 0; i < 3; i++)
    for (int j = 0; j < 3; j++) {
      temp_inv[i][j] = 0;
      for (int k = 0; k < M - 1; k++)
        temp_inv[i][j] +=
            temp_Ma[i][k] * Ga[k][j];  //  temp_inv = temp_Ma*Ga = Ga.'*i_Q*Ga;
    }
  MatrixInverse(temp_inv, G_inv,
                3);  //  G_inv = inv(temp_inv) = inv(Ga.'*i_Q*Ga);
  for (int i = 0; i < 3; i++)
    for (int j = 0; j < M - 1; j++) {
      temp_Ma[i][j] = 0;
      for (int k = 0; k < 3; k++) temp_Ma[i][j] += G_inv[i][k] * Ga[j][k];
      temp_Ma[i][j] =
          temp_Ma[i][j] /
          Q[j];  // temp_Ma = G_inv*Ga.'*i_Q = inv(Ga.'*i_Q*Ga)*Ga.'*i_Q;
    }
  for (int i = 0; i < 3; i++) {
    temp_Za[i] = 0;
    for (int k = 0; k < M - 1; k++)
      temp_Za[i] += temp_Ma[i][k] *
                    H[k];  // temp_Za = temp_Ma*H = inv(Ga.'*i_Q*Ga)*Ga.'*i_Q*H;
  }
  //  估计B,重新计算Q
  for (int i = 0; i < M - 1; i++)
    B[i] = sqrt((X[i + 1] - temp_Za[0]) * (X[i + 1] - temp_Za[0]) +
                (Y[i + 1] - temp_Za[1]) * (Y[i + 1] - temp_Za[1]));
  for (int i = 0; i < M - 1; i++) Phi[i] = sol * sol * B[i] * B[i];

  // 带入新的Q=Phi
  for (int i = 0; i < 3; i++)
    for (int j = 0; j < M - 1; j++)
      temp_Ma[i][j] = Ga[j][i] / Phi[j];  // temp_Ma=Ga.'*i_phi;
  for (int i = 0; i < 3; i++)
    for (int j = 0; j < 3; j++) {
      temp_inv[i][j] = 0;
      for (int k = 0; k < M - 1; k++)
        temp_inv[i][j] += temp_Ma[i][k] *
                          Ga[k][j];  //  temp_inv = temp_Ma*Ga = Ga.'*i_phi*Ga;
    }
  MatrixInverse(temp_inv, G_inv,
                3);  //  G_inv = inv(temp_inv) = inv(Ga.'*i_phi*Ga);
  for (int i = 0; i < 3; i++)
    for (int j = 0; j < M - 1; j++) {
      temp_Ma[i][j] = 0;
      for (int k = 0; k < 3; k++) temp_Ma[i][j] += G_inv[i][k] * Ga[j][k];
      temp_Ma[i][j] = temp_Ma[i][j] / Phi[j];  // temp_Ma = G_inv**Ga.'*i_phi =
                                               // inv(Ga.'*i_phi*Ga)*Ga.'*i_phi;
    }
  for (int i = 0; i < 3; i++) {
    temp_Za[i] = 0;
    for (int k = 0; k < M - 1; k++)
      temp_Za[i] +=
          temp_Ma[i][k] *
          H[k];  // temp_Za = temp_Ma*H = inv(Ga.'*i_phi*Ga)*Ga.'*i_phi*H;
  }
  //// STEP2 ////
  double **i_Phi1 = new double *[3];  // 3*3
  for (int i = 0; i < 3; i++) i_Phi1[i] = new double[3];
  double **temp_inv2 = new double *[2];  // 2*2
  double **G_inv2 = new double *[2];     // 2*2
  for (int i = 0; i < 2; i++) {
    temp_inv2[i] = new double[2];
    G_inv2[i] = new double[2];
  }

  double H1[3] = {0};
  H1[0] = (temp_Za[0] - X[0]) * (temp_Za[0] - X[0]);
  H1[1] = (temp_Za[1] - Y[0]) * (temp_Za[1] - Y[0]);
  H1[2] = temp_Za[2] * temp_Za[2];
  double B1[3] = {0};
  B1[0] = temp_Za[0] - X[0];
  B1[1] = temp_Za[1] - Y[0];
  B1[2] = temp_Za[2];
  double Ga1[3][2] = {{1, 0}, {0, 1}, {1, 1}};  // 3*2
  double temp_Ma2[2][3] = {0};                  // 2*3
  double temp_Ma3[2][3] = {0};                  // 2*3
  double Za1[2] = {0};
  double Zp1[2] = {0};
  double Zp2[2] = {0};

  for (int i = 0; i < 3; i++)
    for (int j = 0; j < 3; j++)
      temp_inv[i][j] =
          4 * B1[i] * G_inv[i][j] * B1[j];  //   temp_inv = phi1=4*B1*cov_za*B1;
  MatrixInverse(temp_inv, i_Phi1, 3);
  for (int i = 0; i < 2; i++)
    for (int j = 0; j < 3; j++) {
      temp_Ma2[i][j] = 0;
      for (int k = 0; k < 3; k++)
        temp_Ma2[i][j] += Ga1[k][i] * i_Phi1[k][j];  // temp_Ma2 = Ga1.'*i_phi1
    }
  for (int i = 0; i < 2; i++)
    for (int j = 0; j < 2; j++) {
      temp_inv2[i][j] = 0;
      for (int k = 0; k < 3; k++)
        temp_inv2[i][j] +=
            temp_Ma2[i][k] *
            Ga1[k][j];  // temp_inv2 = temp_Ma2*Ga1 = Ga1.'*i_phi1*Ga1;
    }
  MatrixInverse(temp_inv2, G_inv2, 2);  // G_inv2= inv(Ga1.'*i_phi1*Ga1)
  for (int i = 0; i < 2; i++)
    for (int j = 0; j < 3; j++) {
      temp_Ma2[i][j] = 0;
      for (int k = 0; k < 2; k++)
        temp_Ma2[i][j] +=
            G_inv2[i][k] * Ga1[j][k];  // inv(Ga1.'*i_phi1*Ga1)*Ga1.'
    }
  for (int i = 0; i < 2; i++)
    for (int j = 0; j < 3; j++) {
      temp_Ma3[i][j] = 0;
      for (int k = 0; k < 3; k++)
        temp_Ma3[i][j] += temp_Ma2[i][k] *
                          i_Phi1[k][j];  // inv(Ga1.'*i_phi1*Ga1)*Ga1.'*i_phi1
    }
  for (int i = 0; i < 2; i++) {
    Za1[i] = 0;
    for (int j = 0; j < 3; j++) Za1[i] += temp_Ma3[i][j] * H1[j];
  }
  if ((Za1[0] < 0 && Za1[1] < 0) || (isnan(Za1[0]) || isnan(Za1[1]))) {
    *len_pos_chan = 0;
  } else {
    Zp1[0] = sqrt(Complex(Za1[0], 0)).real() + X[0];
    Zp1[1] = sqrt(Complex(Za1[1], 0)).real() + Y[0];
    Zp2[0] = -sqrt(Complex(Za1[0], 0)).real() + X[0];
    Zp2[1] = -sqrt(Complex(Za1[1], 0)).real() + Y[0];
    ////// 去模糊点 ////
    double delay_error1 = 0;
    double delay_error2 = 0;
    for (int i = 0; i < M - 1; i++) {
      double temp_tr1 = (sqrt((X[0] - Zp1[0]) * (X[0] - Zp1[0]) +
                              (Y[0] - Zp1[1]) * (Y[0] - Zp1[1])) -
                         sqrt((X[i + 1] - Zp1[0]) * (X[i + 1] - Zp1[0]) +
                              (Y[i + 1] - Zp1[1]) * (Y[i + 1] - Zp1[1]))) *
                        fs / sol;
      double temp_tr2 = (sqrt((X[0] - Zp2[0]) * (X[0] - Zp2[0]) +
                              (Y[0] - Zp2[1]) * (Y[0] - Zp2[1])) -
                         sqrt((X[i + 1] - Zp2[0]) * (X[i + 1] - Zp2[0]) +
                              (Y[i + 1] - Zp2[1]) * (Y[i + 1] - Zp2[1]))) *
                        fs / sol;
      delay_error1 += abs(temp_tr1 - sampleDelay1M[i]);
      delay_error2 += abs(temp_tr2 - sampleDelay1M[i]);
    }
    if (delay_error1 < delay_error2) {
      Zpt[0] = Zp1[0];
      Zpt[1] = Zp1[1];
    } else {
      Zpt[0] = Zp2[0];
      Zpt[1] = Zp2[1];
    }
    Translate2(RLong, RLat, Zpt[0], Zpt[1], &pos_chan[0], &pos_chan[1]);
    *len_pos_chan = 2;
    // 计算加权系数
    double RRchan = 0;
    double Rc1 = sqrt((Zpt[0] - X[0]) * (Zpt[0] - X[0]) +
                      (Zpt[1] - Y[0]) * (Zpt[1] - Y[0]));
    for (int i = 1; i < M; i++) {
      double Rcn = sqrt((Zpt[0] - X[i]) * (Zpt[0] - X[i]) +
                        (Zpt[1] - Y[i]) * (Zpt[1] - Y[i]));
      RRchan += (RM1[i - 1] - (Rcn - Rc1)) * (RM1[i - 1] - (Rcn - Rc1));
    }
    *Rchan = RRchan / (M - 1);
  }

  delete[] RM1;
  RM1 = NULL;
  delete[] X;
  X = NULL;
  delete[] Y;
  Y = NULL;
  delete[] XM1;
  XM1 = NULL;
  delete[] YM1;
  YM1 = NULL;
  delete[] K;
  K = NULL;
  delete[] H;
  H = NULL;
  delete[] B;
  B = NULL;
  delete[] Ga;
  Ga = NULL;
  delete[] Q;
  Q = NULL;
  delete[] Phi;
  Phi = NULL;

  for (int i = 0; i < 3; i++) delete[] temp_Ma[i];
  delete[] temp_Ma;
  temp_Ma = NULL;
  for (int i = 0; i < 3; i++) delete[] temp_inv[i];
  delete[] temp_inv;
  temp_inv = NULL;
  for (int i = 0; i < 3; i++) delete[] G_inv[i];
  delete[] G_inv;
  G_inv = NULL;
  for (int i = 0; i < 3; i++) delete[] i_Phi1[i];
  delete[] i_Phi1;
  i_Phi1 = NULL;
  for (int i = 0; i < 2; i++) delete[] temp_inv2[i];
  delete[] temp_inv2;
  temp_inv2 = NULL;
  for (int i = 0; i < 2; i++) delete[] G_inv2[i];
  delete[] G_inv2;
  G_inv2 = NULL;
}

/*多站TDOA定位taylor算法*/
void TDOATaylor(int M, double fs, double (*bs_M)[2], int *sampleDelay1M,
                double long0, double lat0, double *pos_taylor,
                int *len_pos_taylor, double *Rtaylor) {
  if (M < 3) {
    *len_pos_taylor = 0;  // 站点个数小于3，返回
    return;
  }
  double *RM1 = new double[M - 1];

  double RLong = bs_M[0][0];
  double RLat = bs_M[0][1];
  // 时差转换
  for (int i = 1; i < M; i++) {
    RM1[i - 1] = -sampleDelay1M[i] * 3.0e5 / fs;
  }
  ///*********  Taylor  ******///
  double *X = new double[M];
  double *Y = new double[M];
  double *Q = new double[M - 1];
  double *Rt = new double[M];
  double *Ht = new double[M - 1];
  double(*Gt)[2] = new double[M - 1][2];  // (M-1)*2
  double **temp_Ma = new double *[2];     // 2*(M-1)
  for (int i = 0; i < 2; i++) temp_Ma[i] = new double[M - 1];
  double **temp_binv = new double *[2];  // 2*2
  for (int i = 0; i < 2; i++) temp_binv[i] = new double[2];
  double **temp_inv = new double *[2];  // 2*2
  for (int i = 0; i < 2; i++) temp_inv[i] = new double[2];
  for (int i = 0; i < M; i++) {
    Translate1(RLong, RLat, bs_M[i][0], bs_M[i][1], &X[i], &Y[i]);
  }
  double ee = 0.001;  // 阈值
  int count = 1;
  double Xe = 0;
  double Ye = 0;
  double deta_Z[2] = {0};
  Translate1(RLong, RLat, long0, lat0, &Xe, &Ye);
  while (count++ < 1000) {
    for (int i = 0; i < M; i++)
      Rt[i] = sqrt((X[i] - Xe) * (X[i] - Xe) + (Y[i] - Ye) * (Y[i] - Ye));
    for (int i = 0; i < M - 1; i++) {
      Ht[i] = RM1[i] - Rt[i + 1] + Rt[0];
      Gt[i][0] = (X[0] - Xe) / Rt[0] - (X[i + 1] - Xe) / Rt[i + 1];
      Gt[i][1] = (Y[0] - Ye) / Rt[0] - (Y[i + 1] - Ye) / Rt[i + 1];
      Q[i] = 1;
    }
    for (int i = 0; i < 2; i++)
      for (int j = 0; j < M - 1; j++)
        temp_Ma[i][j] = Gt[j][i] * Q[j];  // Gt.'*Q

    for (int i = 0; i < 2; i++)
      for (int j = 0; j < 2; j++) {
        temp_binv[i][j] = 0;
        for (int k = 0; k < M - 1; k++)
          temp_binv[i][j] += temp_Ma[i][k] * Gt[k][j];  // Gt.'*Q*Gt
      }
    MatrixInverse(temp_binv, temp_inv, 2);  // inv(Gt.'*Q*Gt)

    for (int i = 0; i < 2; i++)
      for (int j = 0; j < M - 1; j++) {
        temp_Ma[i][j] = 0;
        for (int k = 0; k < 2; k++)
          temp_Ma[i][j] += temp_inv[i][k] * Gt[j][k];  // inv(Gt.'*Q*Gt)*Gt.'
      }
    for (int i = 0; i < 2; i++) {
      deta_Z[i] = 0;
      for (int j = 0; j < M - 1; j++)
        deta_Z[i] +=
            temp_Ma[i][j] * Q[j] * Ht[j];  // matlab中deta_Z为nan时此处为何值？
    }
    if (isnan(deta_Z[0]) || isnan(deta_Z[1])) break;
    Xe += deta_Z[0];
    Ye += deta_Z[1];
    if (sqrt(deta_Z[0] * deta_Z[0] + deta_Z[1] * deta_Z[1]) < ee) break;
  }
  Translate2(RLong, RLat, Xe, Ye, &pos_taylor[0], &pos_taylor[1]);
  *len_pos_taylor = 2;
  //计算加权系数
  double RRtaylor = 0;
  double Rt1 = sqrt((Xe - X[0]) * (Xe - X[0]) + (Ye - Y[0]) * (Ye - Y[0]));
  for (int i = 1; i < M; i++) {
    double Rtn = sqrt((Xe - X[i]) * (Xe - X[i]) + (Ye - Y[i]) * (Ye - Y[i]));
    RRtaylor += (RM1[i - 1] - (Rtn - Rt1)) * (RM1[i - 1] - (Rtn - Rt1));
  }
  *Rtaylor = RRtaylor / (M - 1);

  for (int i = 0; i < 2; i++) delete[] temp_inv[i];
  delete[] temp_inv;
  temp_inv = NULL;
  for (int i = 0; i < 2; i++) delete[] temp_binv[i];
  delete[] temp_binv;
  temp_binv = NULL;
  for (int i = 0; i < 2; i++) delete[] temp_Ma[i];
  delete[] temp_Ma;
  temp_Ma = NULL;

  delete[] Gt;
  Gt = NULL;
  delete[] Ht;
  Ht = NULL;
  delete[] Rt;
  Rt = NULL;
  delete[] Q;
  Q = NULL;
  delete[] X;
  X = NULL;
  delete[] Y;
  Y = NULL;

  delete[] RM1;
  RM1 = NULL;
}

extern "C" void TDOA_API TDOA_M_fixedstation(double *bs, int bs_size, int *IQ,
                                             int IQlength, double fs,
                                             double *pos_ll, int *len_pos) {
  int bs_num = bs_size / 2;
  int M = bs_num;  //最大参与计算的站个数
  if (bs_num < 3) {
    *len_pos = 0;
    return;
  }
  IQlength = IQlength / bs_num;
  int len = IQlength / 2;
  for (int i = 0; i < 32; i++) {
    if (len < pow(2.0, i)) {
      len = pow(2.0, i);
      break;
    }
  }

  // short *x_IQ = new short[IQlength];
  int *x_IQ = new int[IQlength];
  Complex *x_complex = new Complex[len];

  double(*BS)[2] = new double[bs_num][2];  // new一个二维数组，bs_size/2是行数
  for (int i = 0; i < bs_num;
       i++) {  //将一维数组转换为二维数组，在这里是三行两列
    for (int j = 0; j < 2; j++) {
      BS[i][j] = bs[2 * i + j];
    }
  }

  //计算各站频谱
  int fpoint = len / 4;  // 只保留部分频谱,保留1/2频谱,,频谱长度任然为len
  Complex **SS = new Complex *[bs_num];  // new一个二维数组，存放频谱
  for (int i = 0; i < bs_num; i++) {
    SS[i] = new Complex[len];  //列数
  }
  for (int r = 0; r < bs_num; r++) {
    // memcpy(x_IQ,IQ+IQlength*r,IQlength*sizeof(short));
    memcpy(x_IQ, IQ + IQlength * r, IQlength * sizeof(int));
    for (int i = 0; i < IQlength / 2; i++) {
      x_complex[i] = Complex(x_IQ[2 * i], x_IQ[2 * i + 1]);
    }
    memset(x_complex + IQlength / 2, 0,
           (len - IQlength / 2) * sizeof(Complex));  // 时域数据补零到2*N点
                                                     // FFT求频谱
    fft(x_complex, len);
    // 只保留部分频谱,频谱长度任然为len
    int zero_point = len - fpoint;                           // 偶数
    memcpy(SS[r], x_complex, sizeof(Complex) * fpoint / 2);  // 信号x的fft结果
    memset(SS[r] + fpoint / 2, 0, sizeof(Complex) * zero_point);
    memcpy(SS[r] + len - fpoint / 2, x_complex + len - fpoint / 2,
           sizeof(Complex) * fpoint / 2);
  }

  int *sampleDelay1bs = new int[bs_num];
  for (int r = 0; r < bs_num; r++) {
    TimeDelay(SS[0], SS[r], len,
              &sampleDelay1bs[r]);  // 计算第一个站与其余站的时差
  }

  //打印时差数据
  // std::cout << "Time delay: ";
  // for (int i = 1; i < bs_num; i++) {
  //   std::cout << sampleDelay1bs[i] << "  ";
  // }
  // std::cout << std::endl;

  //判断时差是否超过理论最大值，来选站
  int *ind = new int[bs_num];
  int count = 0;
  for (int i = 0; i < bs_num; i++) {
    double di = distanceLL(BS[i][0], BS[i][1], BS[0][0], BS[0][1]);
    if (abs(sampleDelay1bs[i]) <= ceil(di * fs / 3e5) + 2) {
      ind[count++] = i;
    }
  }

  if (count < 3) {
    *len_pos = 0;  // 时差超过最大值
    delete[] ind;
    ind = NULL;
    delete[] x_IQ;
    x_IQ = NULL;
    delete[] x_complex;
    x_complex = NULL;
    delete[] BS;
    BS = NULL;
    for (int i = 0; i < bs_num; i++) {
      delete[] SS[i];
    }
    delete[] SS;
    SS = NULL;

    return;
  }
  int *sampleDelay1M = new int[M];
  double(*bs_M)[2] = new double[M][2];
  if (count > M) {
    // 随机选M个站
    int *rand_num = new int[M];
    int *sampleDelayRM = new int[M];
    int ifsame = 0;
    for (int i = 0; i < M; i++) {
      int a = -1;
      ifsame = 0;
      do {
        ifsame = 0;
        srand(static_cast<int>(time(NULL)));
        a = std::rand() % count;
        for (int j = 0; j < i; j++)
          if (a == rand_num[j]) {
            ifsame = 1;
            break;
          }
      } while (1 == ifsame);
      rand_num[i] = a;
      bs_M[i][0] = BS[ind[a]][0];
      bs_M[i][1] = BS[ind[a]][1];
      sampleDelayRM[i] = sampleDelay1bs[ind[a]];
    }
    for (int i = 0; i < M; i++)
      sampleDelay1M[i] = sampleDelayRM[i] - sampleDelayRM[0];
    delete[] rand_num;
    rand_num = NULL;
    delete[] sampleDelayRM;
    sampleDelayRM = NULL;
  } else {
    M = count;
    // 时差转换,及位置赋值
    for (int i = 0; i < M; i++) {
      sampleDelay1M[i] = sampleDelay1bs[ind[i]] - sampleDelay1bs[ind[0]];
      bs_M[i][0] = BS[ind[i]][0];
      bs_M[i][1] = BS[ind[i]][1];
    }
  }
  double RLong = bs_M[0][0];
  double RLat = bs_M[0][1];

  //// 随机选站
  // int *rand_num = new int[M];
  // double(*bs_M)[2] = new double[M][2];
  // double *sampleDelayRM = new double[M];////////////  当前的1不是选站的1
  // ！！！   要转换 int ifsame = 0; for (int i = 0;i < M;i++)
  //{
  //    int a = -1;
  //    ifsame = 0;
  //    do
  //    {
  //        ifsame = 0;
  //        srand((int)time(NULL));
  //        a = rand() % bs_num;
  //        for (int j = 0;j < i;j++)
  //            if (a == rand_num[j])
  //            {
  //                ifsame = 1;
  //                break;
  //            }
  //    } while (1 == ifsame);
  //    rand_num[i] = a;
  //    bs_M[i][0] = BS[a][0];
  //    bs_M[i][1] = BS[a][1];
  //    sampleDelayRM[i] = sampleDelay1bs[a];
  //}
  // delete[] rand_num;
  // rand_num = NULL;
  double pos_chan[2] = {0};
  int len_pos_chan = -1;
  double pos_taylor[2] = {0};
  int len_pos_taylor = -1;
  double Rchan = 0;
  double Rtaylor = 0;
  if (3 == M) {
    double pos_chan1[4] = {0};
    int len_chan1 = -1;
    double pos_chan2[2] = {0};
    int len_chan2 = -1;
    double Rchan1[2] = {0};
    TDOA_3Chan(fs, bs_M, sampleDelay1M, pos_chan1, &len_chan1, Rchan1);
    TDOAChan_Mstation(M, fs, bs_M, sampleDelay1M, pos_chan2, &len_chan2,
                      &Rchan);
    if (len_chan1 > 2 && len_chan2 > 0) {
      double d1 =
          distanceLL(pos_chan1[0], pos_chan1[1], pos_chan2[0], pos_chan2[1]);
      double d2 =
          distanceLL(pos_chan1[2], pos_chan1[3], pos_chan2[0], pos_chan2[1]);
      if (d1 < d2) {
        pos_chan[0] = pos_chan1[0];
        pos_chan[1] = pos_chan1[1];
        len_pos_chan = 2;
        Rchan = Rchan1[0];
      } else {
        pos_chan[0] = pos_chan1[2];
        pos_chan[1] = pos_chan1[3];
        len_pos_chan = 2;
        Rchan = Rchan1[1];
      }
    } else if (len_chan1 > 0 && len_chan1 < 3) {
      pos_chan[0] = pos_chan1[0];
      pos_chan[1] = pos_chan1[1];
      len_pos_chan = 2;
      Rchan = Rchan1[0];
    } else if (len_chan2 > 0) {
      pos_chan[0] = pos_chan2[0];
      pos_chan[1] = pos_chan2[1];
      len_pos_chan = 2;
    } else {
      len_pos_chan = 0;
    }
  } else {
    TDOAChan_Mstation(M, fs, bs_M, sampleDelay1M, pos_chan, &len_pos_chan,
                      &Rchan);
  }
  if (len_pos_chan == 0) {
    *len_pos = 0;
  } else {
    TDOATaylor(M, fs, bs_M, sampleDelay1M, pos_chan[0], pos_chan[1], pos_taylor,
               &len_pos_taylor, &Rtaylor);
    if (len_pos_taylor == 0) {
      *len_pos = 2;
      pos_ll[0] = pos_chan[0];
      pos_ll[1] = pos_chan[1];
    } else {
      double RLong = bs_M[0][0];
      double RLat = bs_M[0][1];
      double Xf, Xc, Xe, Yf, Yc, Ye = 0;
      Translate1(RLong, RLat, pos_chan[0], pos_chan[1], &Xc, &Yc);
      Translate1(RLong, RLat, pos_taylor[0], pos_taylor[1], &Xe, &Ye);
      double distance = sqrt((Xc - Xe) * (Xc - Xe) + (Yc - Ye) * (Yc - Ye));
      if (distance < 100) {
        // Xf = (Xc / Rchan + Xe / Rtaylor) / (1 / Rchan + 1 / Rtaylor);
        // Yf = (Yc / Rchan + Ye / Rtaylor) / (1 / Rchan + 1 / Rtaylor);
        // Translate2(RLong, RLat, Xf, Yf, &pos_ll[0], &pos_ll[1]);
        pos_ll[0] = pos_taylor[0];
        pos_ll[1] = pos_taylor[1];
        *len_pos = 2;
      } else {
        *len_pos = 2;
        pos_ll[0] = pos_chan[0];
        pos_ll[1] = pos_chan[1];
      }
    }
  }

  delete[] sampleDelay1bs;
  sampleDelay1bs = NULL;
  delete[] ind;
  ind = NULL;
  delete[] sampleDelay1M;
  sampleDelay1M = NULL;
  delete[] bs_M;
  bs_M = NULL;
  delete[] x_IQ;
  x_IQ = NULL;
  delete[] x_complex;
  x_complex = NULL;
  delete[] BS;
  BS = NULL;

  for (int i = 0; i < bs_num; i++) {
    delete[] SS[i];
  }
  delete[] SS;
  SS = NULL;
}

extern "C" void TDOA_API GridSelect(double *pos_l, int cN, double *point,
                                    int *len_point) {
  int ge = 7;
  int time = 1;
  int N = cN / 2;
  int m_i = 0, m_k = 0;
  if (N == 1) {
    point[0] = pos_l[0];
    point[1] = pos_l[1];
    *len_point = 2;
    return;
  }

  double *long_ge = new double[ge + 1];
  double *lat_ge = new double[ge + 1];

  int **grid_ind = new int *[ge * ge];
  for (int i = 0; i < ge * ge; i++) grid_ind[i] = new int[N];

  int **grid_num = new int *[ge];
  for (int i = 0; i < ge; i++) grid_num[i] = new int[ge];

  double **pos = new double *[2];
  for (int i = 0; i < 2; i++) pos[i] = new double[N];
  for (int i = 0; i < N; i++) {
    pos[0][i] = pos_l[2 * i];
    pos[1][i] = pos_l[2 * i + 1];
  }

  double range = calRange(pos, N);
  while (range > 0.2) {
    // 划分栅格
    double right_long = Max(pos[0], static_cast<double>(N)) + 0.000001;
    double left_long = Min(pos[0], static_cast<double>(N)) - 0.000001;
    double up_lat = Max(pos[1], static_cast<double>(N)) + 0.000001;
    double down_lat = Min(pos[1], static_cast<double>(N)) - 0.000001;
    double deta_long = (right_long - left_long) / ge;
    double deta_lat = (up_lat - down_lat) / ge;
    double long_index = 0, lat_index = 0;
    int ge_count = 0;

    for (int i = 0; i < ge; i++) {
      double lat0 = up_lat - i * deta_lat;
      double lat1 = up_lat - (1 + i) * deta_lat;
      for (int k = 0; k < ge; k++) {
        grid_num[i][k] = 0;
        ge_count = 0;
        double long0 = left_long + k * deta_long;
        double long1 = left_long + (k + 1) * deta_long;

        for (int n = 0; n < N; n++) {
          if (long0 <= pos[0][n] && pos[0][n] < long1 && lat0 >= pos[1][n] &&
              pos[1][n] > lat1) {
            grid_num[i][k] += 1;
            grid_ind[i * ge + k][ge_count++] = n;
          }
        }
      }
    }
    // 找点数最多的栅格位置
    for (int i = 0; i < ge; i++) {
      for (int k = 0; k < ge; k++) {
        if (grid_num[i][k] > grid_num[m_i][m_k]) {
          m_i = i;
          m_k = k;
        }
      }
    }
    // 检查是否有一个以上的最大值
    int *max_i = new int[ge * ge];
    int *max_k = new int[ge * ge];
    int max_num = 0;
    for (int i = 0; i < ge; i++) {
      for (int k = 0; k < ge; k++) {
        if (grid_num[i][k] == grid_num[m_i][m_k]) {
          max_i[max_num] = i;
          max_k[max_num] = k;
          max_num++;
        }
      }
    }

    double *grid_mse = new double[max_num];
    if (1 != max_num) {  // 若存在点数同样多的格子，计算mse
      if (grid_num[m_i][m_k] == 1) {  // 各个栅格点数都为1
        int mIndex = 0;
        FindMiddlePoint(pos, N, &mIndex);
        N = 1;
        pos[0][0] = pos[0][mIndex];
        pos[1][0] = pos[1][mIndex];
        delete[] max_i;
        max_i = NULL;
        delete[] max_k;
        max_k = NULL;
        delete[] grid_mse;
        grid_mse = NULL;
        break;
      }
      for (int j = 0; j < max_num; j++) {
        int ilen = grid_num[m_i][m_k];
        double *mr = new double[ilen];
        double m_long = 0, m_lat = 0;
        double m_x = 0, m_y = 0;
        double xr = 0, yr = 0;

        for (int n = 0; n < ilen; n++) {
          m_long += pos[0][grid_ind[max_i[j] * ge + max_k[j]][n]];
          m_lat += pos[1][grid_ind[max_i[j] * ge + max_k[j]][n]];
        }
        m_long = m_long / ilen;
        m_lat = m_lat / ilen;
        Translate1(m_long, m_lat, m_long, m_lat, &m_x, &m_y);
        grid_mse[j] = 0;
        for (int n = 0; n < ilen; n++) {
          Translate1(m_long, m_lat,
                     pos[0][grid_ind[max_i[j] * ge + max_k[j]][n]],
                     pos[1][grid_ind[max_i[j] * ge + max_k[j]][n]], &xr, &yr);
          mr[n] = sqrt((xr - m_x) * (xr - m_x) + (yr - m_y) * (yr - m_y));
          grid_mse[j] += mr[n] * mr[n];
        }
        grid_mse[j] = sqrt(grid_mse[j] / ilen);

        delete[] mr;
        mr = NULL;
      }
      int m_mse = 0;
      for (int j = 1; j < max_num; j++)
        if (grid_mse[j] < grid_mse[m_mse]) m_mse = j;
      m_i = max_i[m_mse];
      m_k = max_k[m_mse];
    }
    // 替换pos
    double **pos_old = new double *[2];
    for (int i = 0; i < 2; i++) pos_old[i] = new double[N];
    for (int i = 0; i < N; i++) {
      pos_old[0][i] = pos[0][i];
      pos_old[1][i] = pos[1][i];
    }

    int *ind = new int[N];
    int len = grid_num[m_i][m_k];
    memcpy(ind, grid_ind[m_i * ge + m_k], len * sizeof(int));
    // 取中间9个格子
    if (m_i != 0 && m_k != 0) {
      memcpy(ind + len, grid_ind[(m_i - 1) * ge + m_k - 1],
             grid_num[m_i - 1][m_k - 1] * sizeof(int));
      len = len + grid_num[m_i - 1][m_k - 1];
    }
    if (m_i != 0) {
      memcpy(ind + len, grid_ind[(m_i - 1) * ge + m_k],
             grid_num[m_i - 1][m_k] * sizeof(int));
      len = len + grid_num[m_i - 1][m_k];
    }
    if (m_i != 0 && m_k != ge - 1) {
      memcpy(ind + len, grid_ind[(m_i - 1) * ge + m_k + 1],
             grid_num[m_i - 1][m_k + 1] * sizeof(int));
      len = len + grid_num[m_i - 1][m_k + 1];
    }
    if (m_k != 0) {
      memcpy(ind + len, grid_ind[m_i * ge + m_k - 1],
             grid_num[m_i][m_k - 1] * sizeof(int));
      len = len + grid_num[m_i][m_k - 1];
    }
    if (m_k != ge - 1) {
      memcpy(ind + len, grid_ind[m_i * ge + m_k + 1],
             grid_num[m_i][m_k + 1] * sizeof(int));
      len = len + grid_num[m_i][m_k + 1];
    }
    if (m_i != ge - 1 && m_k != 0) {
      memcpy(ind + len, grid_ind[(m_i + 1) * ge + m_k - 1],
             grid_num[m_i + 1][m_k - 1] * sizeof(int));
      len = len + grid_num[m_i + 1][m_k - 1];
    }
    if (m_i != ge - 1) {
      memcpy(ind + len, grid_ind[(m_i + 1) * ge + m_k],
             grid_num[m_i + 1][m_k] * sizeof(int));
      len = len + grid_num[m_i + 1][m_k];
    }
    if (m_i != ge - 1 && m_k != ge - 1) {
      memcpy(ind + len, grid_ind[(m_i + 1) * ge + m_k + 1],
             grid_num[m_i + 1][m_k + 1] * sizeof(int));
      len = len + grid_num[m_i + 1][m_k + 1];
    }
    N = len;

    for (int i = 0; i < N; i++) {
      pos[0][i] = pos_old[0][ind[i]];
      pos[1][i] = pos_old[1][ind[i]];
    }

    // 计算新的range
    time += 1;
    range = calRange(pos, N);

    delete[] max_i;
    max_i = NULL;
    delete[] max_k;
    max_k = NULL;
    delete[] grid_mse;
    grid_mse = NULL;
    delete[] ind;
    ind = NULL;

    for (int i = 0; i < 2; i++) delete[] pos_old[i];
    delete[] pos_old;
    pos_old = NULL;
  }

  for (int i = 0; i < N; i++) {
    point[2 * i] = pos[0][i];
    point[2 * i + 1] = pos[1][i];
  }
  *len_point = 2 * N;

  delete[] long_ge;
  long_ge = NULL;
  delete[] lat_ge;
  lat_ge = NULL;

  for (int i = 0; i < ge * ge; i++) delete[] grid_ind[i];
  delete[] grid_ind;
  grid_ind = NULL;

  for (int i = 0; i < ge; i++) delete[] grid_num[i];
  delete[] grid_num;
  grid_num = NULL;

  for (int i = 0; i < 2; i++) delete[] pos[i];
  delete[] pos;
  pos = NULL;
}