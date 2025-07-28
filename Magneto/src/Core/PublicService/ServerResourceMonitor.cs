using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Protocol.Data;

namespace Core.PublicService;

public class ServerResourceMonitor
{
    private readonly string _dataDir;
    private double _cpuUseage;
    private CancellationTokenSource _cts;
    private double _hddTotal;
    private double _hddUsed;
    private double _memoryTotal;
    private double _memoryUsed;
    private Task _monitoringTask;

    public ServerResourceMonitor()
    {
        _dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "iqData");
        if (!Directory.Exists(_dataDir)) Directory.CreateDirectory(_dataDir);
        FileInfo info = new(_dataDir);
        if (!string.IsNullOrEmpty(info.LinkTarget)) _dataDir = info.LinkTarget;
        RunningInfo.DataDir = _dataDir;
    }

    public void Initialized()
    {
        try
        {
            _cts = new CancellationTokenSource();
            _monitoringTask = Task.Run(() => MonitoringAsync(_cts.Token).ConfigureAwait(false));
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"初始化服务资源监控失败==>{ex.Message}");
        }
    }

    public void Close()
    {
        try
        {
            _cts?.Cancel();
        }
        catch
        {
            // ignored
        }

        try
        {
            _monitoringTask?.Dispose();
        }
        catch
        {
            // ignored
        }

        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _windowsCpuPro?.Dispose();
                _windowsMemoryPro?.Dispose();
            }
        }
        catch
        {
            // ignored
        }
    }

    private async Task MonitoringAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        var iniSign = false;
        // await Task.Delay(1000, token).ConfigureAwait(false);
        while (!token.IsCancellationRequested)
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    LinuxGetCpUandMemory();
                    LinuxGetHdd();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // windows下查询服务资源需要找个其他方法，这个方法在win7 x32下不可用
                    if (!iniSign)
                    {
                        Console.WriteLine("初始化服务资源监控开始");
                        _windowsCpuPro = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                        _windowsMemoryPro = new PerformanceCounter("Memory", "Available MBytes");
                        _windowsCpuPro.NextValue();
                        iniSign = true;
                        Console.WriteLine("初始化服务资源监控完毕");
                        await Task.Delay(1000, token).ConfigureAwait(false);
                    }

                    WindowsGetInfo();
                }
                else
                {
                    // 非windows与linux直接退出线程
                    break;
                }

                RunningInfo.CpuUseage = _cpuUseage;
                RunningInfo.MemoryTotal = _memoryTotal;
                RunningInfo.MemoryUsed = _memoryUsed;
                RunningInfo.HddTotal = _hddTotal;
                RunningInfo.HddUsed = _hddUsed;
                List<object> list = new();
                SDataSrmCpu cpu = new()
                {
                    Useage = _cpuUseage
                };
                list.Add(cpu);
                SDataSrmMemory mem = new()
                {
                    Used = _memoryUsed,
                    Total = _memoryTotal
                };
                list.Add(mem);
                SDataSrmHdd hdd = new()
                {
                    Used = _hddUsed,
                    Total = _hddTotal
                };
                list.Add(hdd);
                MessageManager.Instance.SendMessage(list);
                await Task.Delay(10000, token).ConfigureAwait(false);
            }
            catch
            {
                await Task.Delay(10000, token).ConfigureAwait(false);
            }
    }

    #region Windows

    /// <summary>
    ///     windows获取CPU使用率
    /// </summary>
    private void WindowsGetInfo()
    {
        // 查询CPU使用率，第一次为0
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
        _cpuUseage = Math.Round(_windowsCpuPro.NextValue(), 1);
        // Console.WriteLine($"CPU使用率：{_cpuUseage}%");
        // 查询内存使用
        using ManagementClass mc = new("Win32_ComputerSystem");
        foreach (var mo in mc.GetInstances().Cast<ManagementObject>())
        {
            var total = long.Parse(mo["TotalPhysicalMemory"].ToString() ?? string.Empty);
            _memoryTotal = Math.Round(total / (1024d * 1024d * 1024d), 1);
        }

        using ManagementClass mos = new("Win32_OperatingSystem");
        foreach (var mo in mos.GetInstances().Cast<ManagementObject>())
        {
            var free = 1024 * long.Parse(mo["FreePhysicalMemory"].ToString() ?? string.Empty);
            _memoryUsed = Math.Round(_memoryTotal - free / (1024d * 1024d * 1024d), 1);
        }

        // Console.WriteLine($"内存使用:{_memoryUsed}/{_memoryTotal}GB");
        // 查询硬盘使用
        using ManagementClass md = new("Win32_LogicalDisk");
        foreach (var item in md.GetInstances().Cast<ManagementObject>())
            try
            {
                // 磁盘名称
                var disk = item["Name"].ToString(); //C: D: E: 等
                // 磁盘描述
                // 磁盘总容量，可用空间，已用空间
                if (!long.TryParse(item["Size"].ToString(), out var total) ||
                    !long.TryParse(item["FreeSpace"].ToString(), out var free)) continue;
                var totalSize = Math.Round(total / (1024d * 1024d * 1024d), 1);
                var used = total - free;
                var usedSize = Math.Round(used / (1024d * 1024d * 1024d), 1);
                // Console.WriteLine($"磁盘:{disk},{name},{usedSize}/{totalSize}GB");
                if (disk != null &&
                    disk[0].ToString().Equals(_dataDir[0].ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    _hddTotal = totalSize;
                    _hddUsed = usedSize;
                }
            }
            catch
            {
                // ignored
            }
    }

    #endregion

    #region windows使用

    private PerformanceCounter _windowsCpuPro;
    private PerformanceCounter _windowsMemoryPro;

    #endregion

    #region Linux

    /// <summary>
    ///     获取进程CPU及内存使用情况
    /// </summary>
    private void LinuxGetCpUandMemory()
    {
        try
        {
            var str = ExecuteCommand("top -b -n 1 1");
            var strL = str.Where(o => !string.IsNullOrWhiteSpace(o))
                .Select(o => o.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            // top - 11:51:05 up  2:25,  2 users,  load average: 0.51, 0.31, 0.22
            // Tasks:  87 total,   1 running,  86 sleeping,   0 stopped,   0 zombie
            // %Cpu0  :  5.3 us, 15.8 sy,  0.0 ni, 73.7 id,  0.0 wa,  0.0 hi,  5.3 si,  0.0 st
            // %Cpu1  : 11.1 us, 22.2 sy,  0.0 ni, 66.7 id,  0.0 wa,  0.0 hi,  0.0 si,  0.0 st
            // %Cpu2  : 13.3 us,  6.7 sy,  0.0 ni, 80.0 id,  0.0 wa,  0.0 hi,  0.0 si,  0.0 st
            // %Cpu3  :  7.1 us, 14.3 sy,  0.0 ni, 78.6 id,  0.0 wa,  0.0 hi,  0.0 si,  0.0 st
            // MiB Mem :   3934.5 total,   3011.9 free,    662.2 used,    260.3 buff/cache
            // MiB Swap:    976.0 total,    976.0 free,      0.0 used.   3056.4 avail Mem 
            // 按照finalShell的计算，已用的内存=Mem的total-Swap的avail；
            var count = 0;
            double totalCpu = 0;
            double totalMemory = 0;
            double usedMemory = 0;
            foreach (var item in strL)
            {
                if (item.Length == 0) continue;
                if (item[0].Contains("%Cpu"))
                {
                    // CPU占用:
                    var use = Convert.ToDouble(item[2]) + Convert.ToDouble(item[4]);
                    count++;
                    totalCpu += use;
                    Console.WriteLine($"{item[0]}: {use}%");
                }
                else if (item[0].Equals("MiB") && item[1].Equals("Mem"))
                {
                    totalMemory = Convert.ToDouble(item[3]);
                }
                else if (item[0].Equals("MiB") && item[1].Equals("Swap:"))
                {
                    var availble = Convert.ToDouble(item[8]);
                    usedMemory = totalMemory - availble;
                }
            }

            _cpuUseage = Math.Round(totalCpu / count, 1);
            Console.WriteLine($"CPU使用率：{_cpuUseage}%");
            _memoryTotal = Math.Round(totalMemory / 1024, 1);
            _memoryUsed = Math.Round(usedMemory / 1024, 1);
            Console.WriteLine($"内存使用:{_memoryUsed}/{_memoryTotal}GB");
        }
        catch
        {
            // ignored
        }
    }

    private void LinuxGetHdd()
    {
        try
        {
            var str = ExecuteCommand($"df -hl {_dataDir}");
            var strL = str.Where(o => !string.IsNullOrWhiteSpace(o))
                .Select(o => o.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            // Filesystem      Size  Used Avail Use% Mounted on
            // /dev/sda2       468G  2.7G  441G   1% /
            //////////////////////////////////////////////////////
            // Filesystem      Size  Used Avail Use% Mounted on
            // udev            2.0G     0  2.0G   0% /dev
            // tmpfs           394M  5.5M  388M   2% /run
            // /dev/sda2       468G  2.7G  441G   1% /
            // tmpfs           2.0G     0  2.0G   0% /dev/shm
            // tmpfs           5.0M     0  5.0M   0% /run/lock
            // tmpfs           2.0G     0  2.0G   0% /sys/fs/cgroup
            // /dev/sda1       511M  5.2M  506M   2% /boot/efi
            // tmpfs           394M     0  394M   0% /run/user/0
            //////////////////////////////////////////////////////
            // Filesystem     1K-blocks      Used Available Use% Mounted on
            // drvfs          290457596 139157132 151300464  48% /mnt/f
            //////////////////////////////////////////////////////
            // 文件系统                     容量  已用  可用 已用% 挂载点
            // /dev/mapper/debian--vg-home  200G   15G  175G    8% /home
            double totalSize = 0;
            double totalUsed = 0;
            var unit = 'B';
            foreach (var item in strL)
            {
                if (item[0].Equals("Filesystem") || item[0].Equals("文件系统"))
                {
                    if (item[1].Contains("1K")) unit = 'K';
                    continue;
                }

                var sizeUnit = item[1].Last();
                if (!"GM".Contains(sizeUnit)) sizeUnit = unit;
                var size = ConvertMemory(item[1], sizeUnit);
                var usedUnit = item[2].Last();
                if (!"GM".Contains(sizeUnit)) usedUnit = unit;
                var used = ConvertMemory(item[2], usedUnit);
                totalSize += size;
                totalUsed += used;
            }

            _hddTotal = Math.Round(totalSize, 1);
            _hddUsed = Math.Round(totalUsed, 1);
            Console.WriteLine($"磁盘使用({_dataDir}):{_hddUsed}/{_hddTotal}GB");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"查询HDD失败:{ex}");
        }
    }

    /// <summary>
    ///     解析字符串为内存大小(单位GB)
    /// </summary>
    /// <param name="str"></param>
    /// <param name="unit"></param>
    private static double ConvertMemory(string str, char unit)
    {
        var tmp = str.TrimEnd(unit);
        switch (unit)
        {
            case 'G':
                return Convert.ToDouble(tmp);
            case 'M':
                return Convert.ToDouble(tmp) / 1024d;
            case 'K':
                return Convert.ToDouble(tmp) / (1024d * 1024d);
            case 'B': //字节
                return Convert.ToDouble(tmp) / (1024d * 1024d * 1024d);
        }

        return 0;
    }

    /// <summary>
    ///     执行 linux命令
    /// </summary>
    /// <param name="pmCommand"></param>
    private static string[] ExecuteCommand(string pmCommand)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo("/bin/bash", "")
        };
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.Start();
        process.StandardInput.WriteLine(pmCommand);
        //process.StandardInput.WriteLine("netstat -an |grep ESTABLISHED |wc -l");
        process.StandardInput.Close();
        var cpuInfo = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        process.Dispose();
        var lines = cpuInfo.Split('\n');
        return lines;
    }

    #endregion
}