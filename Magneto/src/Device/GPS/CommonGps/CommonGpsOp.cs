using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Magneto.Device.CommonGps;

public partial class CommonGps
{
    /// <summary>
    ///     运行Linux命令
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="arguments">参数</param>
    /// <param name="workingDirectory">工作目录</param>
    /// <param name="milliseconds">等待超时</param>
    /// <returns></returns>
    public string RunLinuxCommand(string fileName, string arguments, string workingDirectory = "",
        int milliseconds = 10000)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            using (var p = new Process())
            {
                p.StartInfo.FileName = fileName;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.WorkingDirectory = workingDirectory;
                p.Start();
                p.WaitForExit(milliseconds);
                return p.StandardOutput.ReadToEnd();
            }

        return string.Empty;
    }

    /// <summary>
    ///     设置系统本地时间
    /// </summary>
    /// <param name="dateTime">要设置的时间</param>
    /// <returns>是否成功</returns>
    public bool SetSystemDateTime(DateTime dateTime)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var result = RunLinuxCommand("date", $"-s \"{dateTime.ToString("yyyy-MM-dd HH:mm:ss")}\"");
            RunLinuxCommand("hwclock", "-w");
            return !string.IsNullOrEmpty(result);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            SystemTime st = dateTime;
            // 设置UTC时间，操作系统会根据当前系统设置的时区，自动调整为相应的本地时间
            return SetSystemTime(ref st);
        }

        return false;
    }

    #region P/Invoke

    /// <summary>
    ///     系统时间结构
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct SystemTime
    {
        public ushort Year;
        public ushort Month;
        public ushort DayOfWeek;
        public ushort Day;
        public ushort Hour;
        public ushort Minute;
        public ushort Second;
        public ushort Milliseconds;

        public static implicit operator SystemTime(DateTime dateTime)
        {
            var systemTime = new SystemTime
            {
                Year = (ushort)dateTime.Year,
                Month = (ushort)dateTime.Month,
                DayOfWeek = (ushort)dateTime.DayOfWeek,
                Day = (ushort)dateTime.Day,
                Hour = (ushort)dateTime.Hour,
                Minute = (ushort)dateTime.Minute,
                Second = (ushort)dateTime.Second,
                Milliseconds = (ushort)dateTime.Millisecond
            };
            return systemTime;
        }
    }

    // 设置系统时间
    [DllImport("Kernel32.dll")]
    private static extern bool SetSystemTime(ref SystemTime time);

    #endregion
}