
#include "IntersectionPositioning.h"
#include <math.h>
#include <cstring>
#include <exception>
#include <iostream>
//https://github.com/song0071000/extra-examples-for-node-addon-api
//https://github.com/lvandeve/lodepng
// g++ IntersectionPositioning.cc -shared -o IntersectionPositioning.dll
// g++ IntersectionPositioning.cc -fPIC -shared -o IntersectionPositioning.so
using namespace std;
namespace algorithm
{
	static double pi = 3.14159265;
	static double a = 6378245.0;
	static double ee = 0.00669342162296594323;

	struct MoveStationxy
	{
		double x;
		double y;
		float azimuthN; // 角度值
	};

	/*----求数组最大值----*/
	double Max(double a[], double n)
	{
		int i, maxi = 0;
		double a_max = a[0];
		for (i = 1; i < n; i++)
			if (a[i] > a[maxi])
			{
				maxi = i;
				a_max = a[i];
			}

		return a_max;
	}

	/*----求数组最小值----*/
	double Min(double a[], double n)
	{
		int i, mini = 0;
		double a_min = a[0];
		for (i = 1; i < n; i++)
			if (a[i] < a[mini])
			{
				mini = i;
				a_min = a[i];
			}

		return a_min;
	}

	/*----经纬度坐标转直角坐标----*/
	void Translate1(double ReferenceLong, double ReferenceLat, double Long, double Lat, double *x, double *y)
	{
		double trans = pi / 180; //角度转化为弧度
		double RADIUS = 6371.004;
		double radReferenceLong = ReferenceLong * trans;
		double radReferenceLat = ReferenceLat * trans;
		double radLong = Long * trans;
		double radLat = Lat * trans;
		if (radReferenceLat == 0)
		{
			*x = RADIUS * cos(radReferenceLat) * (radLong - radReferenceLong);
			*y = RADIUS * (radLat - radReferenceLat);
		}
		else
		{
			*x = RADIUS * (cos(radReferenceLat) - (radLat - radReferenceLat) * sin(radReferenceLat)) * sin((radLong - radReferenceLong) * sin(radReferenceLat)) / sin(radReferenceLat);
			*y = RADIUS * (1 / tan(radReferenceLat) - (1 / tan(radReferenceLat) - (radLat - radReferenceLat)) * cos((radLong - radReferenceLong) * sin(radReferenceLat)));
		}
	}

	/*----直角坐标转经纬度坐标----*/
	void Translate2(double ReferenceLong, double ReferenceLat, double x, double y, double *Long, double *Lat)
	{
		double trans = pi / 180; //角度转化为弧度
		double RADIUS = 6371.004;
		double radReferenceLong = ReferenceLong * trans;
		double radReferenceLat = ReferenceLat * trans;
		if (ReferenceLat != 0)
		{
			double transY = y - (x * x + y * y) / (2 * RADIUS) * tan(radReferenceLat);
			double transX = x / (1 - (transY / RADIUS) * tan(radReferenceLat));
			*Long = radReferenceLong + 1 / sin(radReferenceLat) * atan(transX * sin(radReferenceLat) / (RADIUS * cos(radReferenceLat)));
			*Lat = radReferenceLat + (transY / RADIUS) * (2 / (1 + sqrt(1 - 2 * transY * tan(radReferenceLat) / RADIUS)));
		}
		else
		{
			*Long = radReferenceLong + x / (RADIUS * cos(radReferenceLat));
			*Lat = radReferenceLat + y / RADIUS;
		}
		*Long = *Long / trans;
		*Lat = *Lat / trans;
	}

	/*----计算一组点的中心点----*/
	void FindMiddlePoint(double **pos, int N, int *mIndex)
	{
		double *rangeSum = new double[N];
		double *X = new double[N];
		double *Y = new double[N];
		for (int n = 0; n < N; n++)
		{
			Translate1(pos[0][0], pos[1][0], pos[0][n], pos[1][n], &(X[n]), &(Y[n]));
		}
		double minSum = 99999;
		int minIndex = 0;
		for (int n = 0; n < N; n++)
		{
			rangeSum[n] = 0;
			for (int m = 0; m < N; m++)
				rangeSum[n] += sqrt((X[n] - X[m]) * (X[n] - X[m]) + (Y[n] - Y[m]) * (Y[n] - Y[m]));
			if (rangeSum[n] < minSum)
			{
				minSum = rangeSum[n];
				minIndex = n;
			}
		}
		*mIndex = minIndex;

		delete[] X;
		X = NULL;
		delete[] Y;
		Y = NULL;
		delete[] rangeSum;
		rangeSum = NULL;
	}

	/*-----冒泡法排序，从小到大排序并返回下标序列-----*/
	template <typename T>
	void BubbleSort_S2L(T *p, int length)
	{
		//for (int m = 0; m < length; m++)
		//{
		//	ind_diff[m] = m;
		//}

		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length - i - 1; j++)
			{
				if (p[j] > p[j + 1])
				{
					T temp = p[j];
					p[j] = p[j + 1];
					p[j + 1] = temp;

					//int ind_temp = ind_diff[j];
					//ind_diff[j] = ind_diff[j + 1];
					//ind_diff[j + 1] = ind_temp;
				}
			}
		}
	}

	/*----计算一组点(经纬度)的范围大小----*/
	double calRange(double **pos, int N)
	{
		double max_range = 0;
		double right_long = Max(pos[0], (double)N);
		double left_long = Min(pos[0], (double)N);
		double up_lat = Max(pos[1], (double)N);
		double down_lat = Min(pos[1], (double)N);

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
		max_range = (max_x - min_x) > (max_y - min_y) ? (max_x - min_x) : (max_y - min_y);
		return max_range;
	}

	/*----计算一组角度的平均值,以360度为周期----*/
	float avg_degree(float *azimuth, int n)
	{
		double last = azimuth[0];
		double sum = azimuth[0];
		for (int i = 1; i < n; i++)
		{
			double diff = fmod(azimuth[i] - azimuth[i - 1] + 180, 360) - 180;
			if (azimuth[i] - azimuth[i - 1] < -180)
				diff += 360;
			last += diff;
			sum += last;
		}
		float avg = fmod(sum / n, 360);
		return avg;
	}

	/*-----计算示向线交点------*/
	void CrossPoint(MoveStationxy MS1, MoveStationxy MS2, double *pos, int &posLen)
	{
		//求斜率
		float theta1 = 90 - MS1.azimuthN; // 以x轴正方向为基准
		float theta2 = 90 - MS2.azimuthN;
		double K1 = tan(theta1 * pi / 180);
		double K2 = tan(theta2 * pi / 180);

		// 判断是否平行（包括近似平行）
		if (fabs(K1 - K2) < 0.001)
		{
			posLen = 0;
			return;
		}

		// 计算交点
		double cp_x = (MS1.x * K1 - MS2.x * K2 - (MS1.y - MS2.y)) / (K1 - K2);
		double cp_y = K1 * (cp_x - MS1.x) + MS1.y;

		// 去除太远的交点
		double d = sqrt((cp_x - MS1.x) * (cp_x - MS1.x) + (cp_y - MS1.y) * (cp_y - MS1.y));
		if (d > 100)
		{
			posLen = 0;
			return;
		}

		// 判断是不是反方向交点
		//double cp_d = sqrt((cp_x - MS1.x) ^ 2 + (cp_y - MS1.y) ^ 2);
		if ((cp_x - MS1.x) * cos(theta1 * pi / 180) < 0 || (cp_x - MS2.x) * cos(theta2 * pi / 180) < 0 || (cp_y - MS1.y) * sin(theta1 * pi / 180) < 0 || (cp_y - MS2.y) * sin(theta2 * pi / 180) < 0) // 是反向延长线交点,就去除
		{
			posLen = 0;
			return;
		}

		pos[0] = cp_x;
		pos[1] = cp_y;
		posLen = 2;
		return;
	}

	/*-----找到在同一位置收到的示向度中方较平滑的数据-----*/
	void FindSmoothSegment(float *azimuth, int azimuthLen, float *smoothAzimuth, int &smoothLen)
	{
		int M = 10; // 分段点数

		int startInd = 0;
		int endInd = 0;
		if (azimuthLen < M)
			endInd = azimuthLen - 1;
		else
			endInd = M - 1;
		int *index = new int[M * azimuthLen]; //  长度？
		int indCount = 0;
		while (startInd < azimuthLen && endInd < azimuthLen)
		{
			int tempCount = endInd - startInd + 1;
			float *tempAzimuth = new float[tempCount];
			memcpy(tempAzimuth, azimuth + startInd, tempCount * sizeof(float));

			float mavg = avg_degree(tempAzimuth, tempCount); // 循环角度求均值
			double sum = 0;
			for (int j = 0; j < tempCount; j++)
			{
				double diff = fmod(tempAzimuth[j] - mavg + 180, 360) - 180; // 两相邻角度之间的差值
				if (tempAzimuth[j] - mavg + 180 < 0)
					diff += 360;
				sum += diff * diff;
			}
			double std = sqrt(sum / tempCount); // 该分段的均方差

			if (std < 10) // 如果平滑,加入索引
			{
				for (int k = startInd; k <= endInd; k++)
				{
					index[indCount++] = k;
				}
			}
			else
				int aaaa = 1;

			// 开始新的一段
			int hM = (int)floor(M / 2.0);
			if (startInd + hM < azimuthLen - 1 && endInd + hM > azimuthLen - 1)
			{
				startInd += hM;
				endInd = azimuthLen - 1;
			}
			else
			{
				startInd += hM;
				endInd += hM;
			}
			//
			delete[] tempAzimuth;
			tempAzimuth = NULL;
		}
		if (indCount > 0)
		{
			// 先从小到大排序
			BubbleSort_S2L<int>(index, indCount);
			// 再去重
			int *SmoothIndex = new int[indCount];
			SmoothIndex[0] = index[0];
			int SindCount = 1;
			for (int i = 1; i < indCount; i++)
			{
				if (index[i] != index[i - 1])
					SmoothIndex[SindCount++] = index[i];
			}

			for (int i = 0; i < SindCount; i++)
				smoothAzimuth[i] = azimuth[SmoothIndex[i]];
			smoothLen = SindCount;

			delete[] SmoothIndex;
			SmoothIndex = NULL;
		}
		else
			smoothLen = 0;

		//delete
		delete[] index;
		index = NULL;
	}

	void GridSelectXY(double *pos_l, int cN, double *point, int *len_point)
	{
		int ge = 7;
		int time = 1;
		int N = cN / 2;
		int m_i = 0, m_k = 0;

		double *long_ge = new double[ge + 1];
		double *lat_ge = new double[ge + 1];

		int **grid_ind = new int *[ge * ge];
		for (int i = 0; i < ge * ge; i++)
			grid_ind[i] = new int[N];

		int **grid_num = new int *[ge];
		for (int i = 0; i < ge; i++)
			grid_num[i] = new int[ge];

		double **pos = new double *[2];
		for (int i = 0; i < 2; i++)
			pos[i] = new double[N];
		for (int i = 0; i < N; i++)
		{
			pos[0][i] = pos_l[2 * i];
			pos[1][i] = pos_l[2 * i + 1];
		}

		double xmax = Max(pos[0], N);
		double xmin = Min(pos[0], N);
		double ymax = Max(pos[1], N);
		double ymin = Min(pos[1], N);
		double range = (xmax - xmin) > (ymax - ymin) ? (xmax - xmin) : (ymax - ymin);
		while (range > 0.2)
		{
			//	划分栅格
			double right_long = Max(pos[0], (double)N) + 0.000001;
			double left_long = Min(pos[0], (double)N) - 0.000001;
			double up_lat = Max(pos[1], (double)N) + 0.000001;
			double down_lat = Min(pos[1], (double)N) - 0.000001;
			double deta_long = (right_long - left_long) / ge;
			double deta_lat = (up_lat - down_lat) / ge;
			double long_index = 0, lat_index = 0;
			int ge_count = 0;

			for (int i = 0; i < ge; i++)
			{
				double lat0 = up_lat - i * deta_lat;
				double lat1 = up_lat - (1 + i) * deta_lat;
				for (int k = 0; k < ge; k++)
				{
					grid_num[i][k] = 0;
					ge_count = 0;
					double long0 = left_long + k * deta_long;
					double long1 = left_long + (k + 1) * deta_long;

					for (int n = 0; n < N; n++)
					{
						if (long0 <= pos[0][n] && pos[0][n] < long1 && lat0 >= pos[1][n] && pos[1][n] > lat1)
						{
							grid_num[i][k] += 1;
							grid_ind[i * ge + k][ge_count++] = n;
						}
					}
				}
			}
			// 找点数最多的栅格位置
			for (int i = 0; i < ge; i++)
			{
				for (int k = 0; k < ge; k++)
				{
					if (grid_num[i][k] > grid_num[m_i][m_k])
					{
						m_i = i;
						m_k = k;
					}
				}
			}
			// 检查是否有一个以上的最大值
			int *max_i = new int[ge * ge];
			int *max_k = new int[ge * ge];
			int max_num = 0;
			for (int i = 0; i < ge; i++)
			{
				for (int k = 0; k < ge; k++)
				{
					if (grid_num[i][k] == grid_num[m_i][m_k])
					{
						max_i[max_num] = i;
						max_k[max_num] = k;
						max_num++;
					}
				}
			}

			double *grid_mse = new double[max_num];
			if (1 != max_num) // 若存在点数同样多的格子，计算mse
			{
				if (grid_num[m_i][m_k] == 1) // 各个栅格点数都为1
				{
					if (N == 2) // 只剩2个点，不输出
						N = 0;
					else
					{
						int mIndex = 0;
						FindMiddlePoint(pos, N, &mIndex);
						N = 1;
						pos[0][0] = pos[0][mIndex];
						pos[1][0] = pos[1][mIndex];
					}
					delete[] max_i;
					max_i = NULL;
					delete[] max_k;
					max_k = NULL;
					delete[] grid_mse;
					grid_mse = NULL;
					break;
				}
				for (int j = 0; j < max_num; j++)
				{
					int ilen = grid_num[m_i][m_k];
					double *mr = new double[ilen];
					double m_long = 0, m_lat = 0;
					double m_x = 0, m_y = 0;
					double xr = 0, yr = 0;

					for (int n = 0; n < ilen; n++)
					{
						m_long += pos[0][grid_ind[max_i[j] * ge + max_k[j]][n]];
						m_lat += pos[1][grid_ind[max_i[j] * ge + max_k[j]][n]];
					}
					m_x = m_long / ilen;
					m_y = m_lat / ilen;
					grid_mse[j] = 0;
					for (int n = 0; n < ilen; n++)
					{
						xr = pos[0][grid_ind[max_i[j] * ge + max_k[j]][n]];
						yr = pos[1][grid_ind[max_i[j] * ge + max_k[j]][n]];
						mr[n] = sqrt((xr - m_x) * (xr - m_x) + (yr - m_y) * (yr - m_y));
						grid_mse[j] += mr[n] * mr[n];
					}
					grid_mse[j] = sqrt(grid_mse[j] / ilen);

					delete[] mr;
					mr = NULL;
				}
				int m_mse = 0;
				for (int j = 1; j < max_num; j++)
					if (grid_mse[j] < grid_mse[m_mse])
						m_mse = j;
				m_i = max_i[m_mse];
				m_k = max_k[m_mse];
			}
			// 替换pos
			double **pos_old = new double *[2];
			for (int i = 0; i < 2; i++)
				pos_old[i] = new double[N];
			for (int i = 0; i < N; i++)
			{
				pos_old[0][i] = pos[0][i];
				pos_old[1][i] = pos[1][i];
			}

			int *ind = new int[N];
			int len = grid_num[m_i][m_k];
			memcpy(ind, grid_ind[m_i * ge + m_k], len * sizeof(int));
			// 取中间9个格子
			if (m_i != 0 && m_k != 0)
			{
				memcpy(ind + len, grid_ind[(m_i - 1) * ge + m_k - 1], grid_num[m_i - 1][m_k - 1] * sizeof(int));
				len = len + grid_num[m_i - 1][m_k - 1];
			}
			if (m_i != 0)
			{
				memcpy(ind + len, grid_ind[(m_i - 1) * ge + m_k], grid_num[m_i - 1][m_k] * sizeof(int));
				len = len + grid_num[m_i - 1][m_k];
			}
			if (m_i != 0 && m_k != ge - 1)
			{
				memcpy(ind + len, grid_ind[(m_i - 1) * ge + m_k + 1], grid_num[m_i - 1][m_k + 1] * sizeof(int));
				len = len + grid_num[m_i - 1][m_k + 1];
			}
			if (m_k != 0)
			{
				memcpy(ind + len, grid_ind[m_i * ge + m_k - 1], grid_num[m_i][m_k - 1] * sizeof(int));
				len = len + grid_num[m_i][m_k - 1];
			}
			if (m_k != ge - 1)
			{
				memcpy(ind + len, grid_ind[m_i * ge + m_k + 1], grid_num[m_i][m_k + 1] * sizeof(int));
				len = len + grid_num[m_i][m_k + 1];
			}
			if (m_i != ge - 1 && m_k != 0)
			{
				memcpy(ind + len, grid_ind[(m_i + 1) * ge + m_k - 1], grid_num[m_i + 1][m_k - 1] * sizeof(int));
				len = len + grid_num[m_i + 1][m_k - 1];
			}
			if (m_i != ge - 1)
			{
				memcpy(ind + len, grid_ind[(m_i + 1) * ge + m_k], grid_num[m_i + 1][m_k] * sizeof(int));
				len = len + grid_num[m_i + 1][m_k];
			}
			if (m_i != ge - 1 && m_k != ge - 1)
			{
				memcpy(ind + len, grid_ind[(m_i + 1) * ge + m_k + 1], grid_num[m_i + 1][m_k + 1] * sizeof(int));
				len = len + grid_num[m_i + 1][m_k + 1];
			}
			N = len;

			for (int i = 0; i < N; i++)
			{
				pos[0][i] = pos_old[0][ind[i]];
				pos[1][i] = pos_old[1][ind[i]];
			}

			// 计算新的range
			xmax = Max(pos[0], N);
			xmin = Min(pos[0], N);
			ymax = Max(pos[1], N);
			ymin = Min(pos[1], N);
			range = (xmax - xmin) > (ymax - ymin) ? (xmax - xmin) : (ymax - ymin);
			time += 1;

			// delete
			delete[] max_i;
			max_i = NULL;
			delete[] max_k;
			max_k = NULL;
			delete[] grid_mse;
			grid_mse = NULL;
			delete[] ind;
			ind = NULL;

			for (int i = 0; i < 2; i++)
				delete[] pos_old[i];
			delete[] pos_old;
			pos_old = NULL;
		}

		for (int i = 0; i < N; i++)
		{
			point[2 * i] = pos[0][i];
			point[2 * i + 1] = pos[1][i];
		}
		*len_point = 2 * N;

		delete[] long_ge;
		long_ge = NULL;
		delete[] lat_ge;
		lat_ge = NULL;

		for (int i = 0; i < ge * ge; i++)
			delete[] grid_ind[i];
		delete[] grid_ind;
		grid_ind = NULL;

		for (int i = 0; i < ge; i++)
			delete[] grid_num[i];
		delete[] grid_num;
		grid_num = NULL;

		for (int i = 0; i < 2; i++)
			delete[] pos[i];
		delete[] pos;
		pos = NULL;
	}

	// 输入一段时间的示向度和对应经纬度
	// 输入参数: posLen，跑车GPS点的个数（每秒取一次）
	// 输入参数：longitude，各点经度[lon1,lon2,...]，长度为posLen
	// 输入参数：latitude，各点纬度[lat1,lat2,...]，长度为posLen
	// 输入参数：indexArray，每个GPS点对应的示向度个数，长度为posLen
	// 输入参数：azimuthN,各点示向度（以北为基准）[a1,a2,...],取值范围[0,360)
	// 输入参数: azimuthLen,输入示向度的总个数
	// 输出参数：result，定位结果（一组点）[lon1,lat1,lon2,lat2,...]
	// 输出参数：resultLen,result的长度，即输出点数的2倍
	//输入输出重新定义 便于js把参数传递过来
	void SingleStationLocate(int posLen, double *longitude, double *latitude, int *indexArray, int azimuthLen, float *azimuthN, double *result, double minDistan, int &resultLen)
	{
		//double minDistan = 0.05; // minDistan,最小可以交会的距离
		double *X = new double[posLen];
		double *Y = new double[posLen];
		double Rlon = longitude[0];
		double Rlat = latitude[0];
		for (int i = 0; i < posLen; i++)
		{
			Translate1(Rlon, Rlat, longitude[i], latitude[i], &X[i], &Y[i]);
		}

		MoveStationxy *movestation = new MoveStationxy[posLen];

		// 提取有效位置和示向度,找到在同一位置收到的示向度中方差较小的点
		int count_m = 0; // 有效位置数
		int start_ind = 0;
		int end_ind = 0;
		for (int i = 0; i < posLen; i++)
		{
			end_ind = start_ind + indexArray[i];

			int count_k = indexArray[i];
			float *temp_azimuthN = new float[count_k];
			float *smoothAzimuthN = new float[count_k];
			memcpy(temp_azimuthN, azimuthN + start_ind, count_k * sizeof(float));

			int smoothLen = 0;
			for (int t = 0; t < count_k; t++)
			{
				if (temp_azimuthN[t] < 0)
					temp_azimuthN[t] += 360;
				else if (temp_azimuthN[t] >= 360)
					temp_azimuthN[t] -= 360;
			}
			FindSmoothSegment(temp_azimuthN, count_k, smoothAzimuthN, smoothLen);
			if (smoothLen > 0.3 * count_k)
			{
				float avgAzimuthN = avg_degree(smoothAzimuthN, smoothLen);
				movestation[count_m].azimuthN = avgAzimuthN;
				movestation[count_m].x = X[i];
				movestation[count_m].y = Y[i];
				count_m++;
			}
			start_ind = end_ind;
			//
			delete[] temp_azimuthN;
			temp_azimuthN = NULL;
			delete[] smoothAzimuthN;
			smoothAzimuthN = NULL;
		}

		// 计算交点
		int detaj = 1;
		double maxPoint = 1000;
		if (count_m > maxPoint)
		{
			detaj = ceil(count_m / maxPoint) * ceil(count_m / maxPoint);
		}
		double *crPoint = new double[count_m / detaj * count_m * 2];
		//printf("%d\n", count_m / detaj*count_m * 2);
		int crPointCount = 0;
		int isfull = 0;
		for (int i = 0; i < count_m; i++)
		{
			for (int j = i + 1; j < count_m; j += detaj)
			{
				double dd = sqrt((movestation[i].x - movestation[j].x) * (movestation[i].x - movestation[j].x) + (movestation[i].y - movestation[j].y) * (movestation[i].y - movestation[j].y));
				if (dd > minDistan) // 两点之间大于 minDistan (km)时求交点
				{
					double pos[2] = {0};
					int posLen = 0;
					CrossPoint(movestation[i], movestation[j], pos, posLen);
					if (posLen > 0)
					{
						for (int k = 0; k < posLen; k++)
						{
							crPoint[crPointCount++] = pos[k];
						}
					}
				}
			}
		}

		double *resultXY = new double[crPointCount];
		int resultXYlen = 0;
		GridSelectXY(crPoint, crPointCount, resultXY, &resultXYlen);

		//输出结果变成一个对象返回
		resultLen = resultXYlen;

        //i 最大值为49//保证最大100个点
		for (int i = 0; i < resultXYlen/2&&i<50; i++)
		{
			Translate2(Rlon, Rlat, resultXY[2 * i], resultXY[2 * i + 1], &result[2 * i], &result[2 * i + 1]);
		}
		//
		delete[] resultXY;
		resultXY = NULL;
		delete[] crPoint;
		crPoint = NULL;
		delete[] movestation;
		movestation = NULL;
		delete[] X;
		X = NULL;
		delete[] Y;
		Y = NULL;
	}

	extern "C" void IntersectionPositioning_API intersectionPositioning(Param *param)
	{
		try
		{
      double *result = new double[300];
      int resultLen = 0;
      SingleStationLocate(param->pointLen, &param->lon[0], &param->lat[0], &param->angleindex[0], param->azimuthNLength, &param->azimuthN[0], result, param->mindistance, resultLen);
      param->resultLength = resultLen;
      if (resultLen > 0)
      {
        for (int i = 0; i < resultLen && i < 100; i++)
        {
          param->result[i] = result[i];
        }
      }
      delete[] result;
		}
		catch (exception &e)
		{
      std::cout << "c++ exception" << std::endl;
		}
	}
}