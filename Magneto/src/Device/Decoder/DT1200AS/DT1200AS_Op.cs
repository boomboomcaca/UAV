using Magneto.Device.DT1200AS.API;

namespace Magneto.Device.DT1200AS;

public partial class Dt1200As
{
    private const int LteSampleRate = 19200000;
    private const int UsbBuffSize = 65536;
    private const int LteBuffSize = 19200 * 100 / UsbBuffSize * UsbBuffSize;
    private const int LteBuffSizeSmall = 19200 * 30 / UsbBuffSize * UsbBuffSize;
    private const int LteScan = 14;
    private const int LteFilterBw = 15000000;
    private const int TdscdmaHighSrate = 12 * 1280000;
    private const int TdscdmaScanBuffSize = TdscdmaHighSrate / 1000 * 500 / UsbBuffSize * UsbBuffSize;
    private const int TdscdmaScan = 11;
    private const int GsmOverSampleRate = 64;
    private const int GsmSampleRate = 17333333;

    private const int GsmScanBuffSize =
        1250 * GsmOverSampleRate * 13 / UsbBuffSize * UsbBuffSize + UsbBuffSize; // 13 frames

    private const int GsmArfcnScan = 13;
    private const int WcdmaSampleRate = 4 * 3840000;
    private const int WcdmaBuffSize = WcdmaSampleRate / 3 / UsbBuffSize * UsbBuffSize; // 0.5秒钟的buffer
    private const int WcdmaScan = 9;
    private const int CdmaSampleRate2 = 8 * 1228800;
    private const int CdmaBuffSize2 = CdmaSampleRate2 / 2; // 0.5秒钟的buffer
    private const int CdmaScan = 10;

    /// <summary>
    ///     添加中国频段
    /// </summary>
    private void AddChinaFreqListAll()
    {
        //cdma_scan_enable
        Rx3GInterface.AddFreq2ScanList(160 * 30000 + 870000000, CdmaSampleRate2, CdmaSampleRate2, CdmaBuffSize2,
            CdmaScan);
        //wcdma_scan_enable
        //2.1G 60M频段
        //for (int64_t i=0;i<6;i++)
        for (var i = 2; i < 5; i++)
            Rx3GInterface.AddFreq2ScanList(2115100000 + 10000000 * i, WcdmaSampleRate, 10000000, WcdmaBuffSize,
                WcdmaScan);
        // U900 频段
        Rx3GInterface.AddFreq2ScanList(957000000, WcdmaSampleRate, WcdmaSampleRate / 4, WcdmaBuffSize,
            WcdmaScan);
        //tdscdma_scan_enable
        var centerFrequenies = new long[] { 2017500000, 1913000000, 1900000000, 1887000000 };
        //for (int i=0;i<4;i++)
        for (var i = 0; i < 1; i++)
            Rx3GInterface.AddFreq2ScanList(centerFrequenies[i], TdscdmaHighSrate, TdscdmaHighSrate,
                TdscdmaScanBuffSize, TdscdmaScan);
        //fddlte_scan_enable
        Rx3GInterface.AddFreq2ScanList(1850000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); // band 3 CUCC
        Rx3GInterface.AddFreq2ScanList(1867500000, LteSampleRate, 14400000, LteBuffSizeSmall,
            LteScan); // band 3 CTCC
        Rx3GInterface.AddFreq2ScanList(2120000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); // band 1 CTCC
        Rx3GInterface.AddFreq2ScanList(2155000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); // band 1 CUCC
        Rx3GInterface.AddFreq2ScanList(2160000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); // band 1 CUCC
        Rx3GInterface.AddFreq2ScanList(465000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); //450-470M集群，联通FDD
        Rx3GInterface.AddFreq2ScanList(782700000, LteSampleRate, 14400000, LteBuffSizeSmall, LteScan); //广电LTE
        Rx3GInterface.AddFreq2ScanList(875000000, LteSampleRate, 10000000, LteBuffSizeSmall,
            LteScan); //电信800M LTE
        Rx3GInterface.AddFreq2ScanList(1835000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); // CUCC
        Rx3GInterface.AddFreq2ScanList(1810000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); // CMCC
        Rx3GInterface.AddFreq2ScanList(1822500000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); // CMCC
        Rx3GInterface.AddFreq2ScanList(941000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); // CMCC
        Rx3GInterface.AddFreq2ScanList(954000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); // CMCC
        //tddlte_scan_enable
        Rx3GInterface.AddFreq2ScanList(1887000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); // band 39  CMCC
        Rx3GInterface.AddFreq2ScanList(1900000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); // band 39  CMCC
        Rx3GInterface.AddFreq2ScanList(1913000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); // band 39  CMCC
        Rx3GInterface.AddFreq2ScanList(2007500000, LteSampleRate, LteFilterBw, LteBuffSizeSmall, LteScan); // 
        Rx3GInterface.AddFreq2ScanList(2310000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); // band 40  CUCC
        Rx3GInterface.AddFreq2ScanList(2330000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); // band 40  CMCC
        Rx3GInterface.AddFreq2ScanList(2345000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); // band 40  CMCC
        Rx3GInterface.AddFreq2ScanList(2365000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); // band 40  CMCC
        Rx3GInterface.AddFreq2ScanList(2380000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); // band 40  CTCC
        Rx3GInterface.AddFreq2ScanList(2565000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); // band 41  CUCC
        Rx3GInterface.AddFreq2ScanList(2585000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); // band 41  CMCC
        Rx3GInterface.AddFreq2ScanList(2605000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); // band 41  CMCC
        Rx3GInterface.AddFreq2ScanList(2625000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); // band 41  CMCC
        Rx3GInterface.AddFreq2ScanList(2645000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); // band 41  CTCC
        Rx3GInterface.AddFreq2ScanList(1457000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); //数字集群 1447-1467M
        Rx3GInterface.AddFreq2ScanList(1790000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); //轨道交通1785-1805M
        Rx3GInterface.AddFreq2ScanList(1800000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); //轨道交通1785-1805M
        //gsm_scan_enable
        //// 1840-1860 联通4G，1860-1875 电信4G
        centerFrequenies = new long[]
        {
            933000000, 941000000, 949000000, 957000000, 1809000000, 1817000000, 1825000000, 1833000000, 1841000000,
            1849000000, 1857000000, 1865000000, 1873000000
        };
        for (var i = 0; i < 9; i++)
            Rx3GInterface.AddFreq2ScanList(centerFrequenies[i], GsmSampleRate, GsmSampleRate / 2,
                GsmScanBuffSize, GsmArfcnScan);
    }

    /// <summary>
    ///     添加边境频段
    /// </summary>
    private void AddBorderFreqListAll()
    {
        var f1 = new long[] { 2110000000, 925000000, 1930000000, 1805000000, 869000000 };
        var f2 = new long[] { 2170000000, 960000000, 1990000000, 1880000000, 894000000 };
        var numBand = 2;
        for (var i = 0; i < numBand; i++)
        {
            var f = f1[i] + 5000000;
            while (f <= f2[i])
            {
                Rx3GInterface.AddFreq2ScanList(f, WcdmaSampleRate, 10000000, WcdmaBuffSize, WcdmaScan);
                f += 7500000;
            }
        }

        // 450/800/1900/2100
        Rx3GInterface.AddFreq2ScanList(875000000, CdmaSampleRate2, CdmaSampleRate2, CdmaBuffSize2, CdmaScan);
        Rx3GInterface.AddFreq2ScanList(885000000, CdmaSampleRate2, CdmaSampleRate2, CdmaBuffSize2, CdmaScan);
        Rx3GInterface.AddFreq2ScanList(89000000, CdmaSampleRate2, CdmaSampleRate2, CdmaBuffSize2, CdmaScan);
        Rx3GInterface.AddFreq2ScanList(425750000, CdmaSampleRate2, CdmaSampleRate2, CdmaBuffSize2, CdmaScan);
        Rx3GInterface.AddFreq2ScanList(464000000, CdmaSampleRate2, CdmaSampleRate2, CdmaBuffSize2, CdmaScan);
        Rx3GInterface.AddFreq2ScanList(491000000, CdmaSampleRate2, CdmaSampleRate2, CdmaBuffSize2, CdmaScan);
        // Those Bands are from  Iphone7 and Qualcomm Snapgragon 820
        // FDD-LTE  Band 1, 2, 3, 4, 5, 7, 8, 12, 13, 17, 18, 19, 20, 25, 26, 27, 28, 29, 30 
        // TD-LTE  Band 38, 39, 40, 41
        var fL1 = new long[] { 725000000, 860000000, 925000000, 1805000000, 2110000000, 2300000000, 2496000000 };
        var fL2 = new long[] { 821000000, 894000000, 960000000, 1995000000, 2170000000, 2400000000, 2690000000 };
        for (var i = 0; i < 7; i++)
        {
            var f = fL1[i] + 8000000;
            while (f <= fL2[i])
            {
                Rx3GInterface.AddFreq2ScanList(f, LteSampleRate, LteFilterBw, LteBuffSizeSmall, LteScan);
                f += 15000000;
            }
        }

        Rx3GInterface.AddFreq2ScanList(1457000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); //数字集群 1447-1467M
        Rx3GInterface.AddFreq2ScanList(1790000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); //轨道交通1785-1805M
        Rx3GInterface.AddFreq2ScanList(1800000000, LteSampleRate, LteFilterBw, LteBuffSizeSmall,
            LteScan); //轨道交通1785-1805M
        //TDSCDMA
        var centerFrequenies = new long[] { 2017500000, 1913000000, 1900000000, 1887000000 };
        for (var i = 0; i < 1; i++)
            Rx3GInterface.AddFreq2ScanList(centerFrequenies[i], TdscdmaHighSrate, TdscdmaHighSrate,
                TdscdmaScanBuffSize, TdscdmaScan);
        //GSM  900/1800/850/1900
        long fgsm = 925000000 + 4000000; //GSM900
        while (fgsm < 960000000 + 4000000)
        {
            Rx3GInterface.AddFreq2ScanList(fgsm, GsmSampleRate, GsmSampleRate / 2, GsmScanBuffSize,
                GsmArfcnScan);
            fgsm += 8000000;
        }

        fgsm = 1805000000 + 4000000; //GSM1800
        while (fgsm < 1880000000 + 4000000)
        {
            Rx3GInterface.AddFreq2ScanList(fgsm, GsmSampleRate, GsmSampleRate / 2, GsmScanBuffSize,
                GsmArfcnScan);
            fgsm += 8000000;
        }
    }
}