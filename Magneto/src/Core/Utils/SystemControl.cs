using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Core.Utils;

public static class SystemControl
{
    /// <summary>
    ///     重启程序
    /// </summary>
    public static void Restart()
    {
        var path = "";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Trace.WriteLine("当前系统为Linux");
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "restart_linux.sh");
            Trace.WriteLine($"启动脚本：{path}");
            try
            {
                var p = ExecuteCmd("chmod", " +x restart_linux.sh", AppDomain.CurrentDomain.BaseDirectory);
                p.WaitForExit();
                p.OutputDataReceived -= OutputDataReceived;
                p.Dispose();
                var p1 = ExecuteCmd("chmod", " +x Magneto", AppDomain.CurrentDomain.BaseDirectory);
                p1.WaitForExit();
                p1.OutputDataReceived -= OutputDataReceived;
                p1.Dispose();
                Process.Start(path)?.WaitForExit();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error:{ex.Message}");
            }
            // ExecuteCmd(path, "");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Trace.WriteLine("当前系统为Windows");
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "restart_windows.bat");
            Process.Start(path);
            // ExecuteCmd("cmd", path);
        }

        Trace.WriteLine($"重启服务...{path}");
        System.Environment.Exit(0);
    }

    private static Process ExecuteCmd(string fileName, string arguments, string workingDirectory = "")
    {
        var p = new Process();
        p.StartInfo.FileName = fileName;
        p.StartInfo.Arguments = arguments;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.WorkingDirectory = workingDirectory;
        p.StartInfo.ErrorDialog = true;
        p.OutputDataReceived += OutputDataReceived;
        p.Start();
        p.BeginOutputReadLine();
        return p;
    }

    private static void OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        Trace.WriteLine($"外部进程输出:{e.Data}");
    }
}