using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Magneto.Protocol.Define;

namespace Magneto.Device.AP510;

public partial class Ap510
{
    #region 设备内部指令

    /// <summary>
    ///     开始任务指令
    ///     注：此方法与AbortCmd完成事务的处理，
    ///     一个完整的任务生命周期必须由此方法与AbortCmd成对出现。
    ///     此方法获取任务结束时AbortCmd或任务参数修改时AlterCmd必须的任务编号
    ///     最重要的目的为了获得新任务的编号，否则任务的执行将没有意义
    /// </summary>
    public void StartCmd()
    {
        try
        {
            var parameters = GetParameters();
            if (string.IsNullOrEmpty(parameters)) throw new Exception("参数为空");
            if (_devTaskPref.Equals(Constants.RxSingle))
            {
                ServiceCall($"MEAS:BAND:XDB {(int)XdB}", out _);
                ServiceCall($"MEAS:BAND:BETA {Beta}", out _);
            }

            // 判断当前任务是否为离散扫描或多频率测向
            if (_devTaskPref.Equals(Constants.RxMscan) || _devTaskPref.Equals(Constants.DfList))
            {
                //添加频率表
                var b = ServiceCall(_devTaskPref + Constants.Newtable, out var ret);
                if (!b) throw new Exception($"参数异常，{ret}");
                _taskId = ret.GetValueByKey("taskid");
                if (string.IsNullOrWhiteSpace(_taskId)) throw new Exception("无效的任务编号");
                var parameterArray = parameters.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in parameterArray)
                    // 使用更新后的_taskID值，组合添加频率表
                    ServiceCall(
                        CombineCmd(_devTaskPref, Constants.Addpoint, Constants.Space, "taskid=", _taskId, "," + item),
                        out _);
                /* 执行成功，同步等待返回"OK"
                 * 当已经同步返回“OK”过后才能添加新指令，否则可能出现前后两次指 令粘包的情况（下发指令数据没有帧头）
                 */
                ServiceCall(CombineCmd(_devTaskPref, Constants.Servicecall, Constants.Space, "task=", _taskId), out _);
            }
            else if (_devTaskPref.Equals(Constants.RxCiq))
            {
                var b = ServiceCall(CombineCmd(_devTaskPref, Constants.Start, Constants.Space, parameters),
                    out var ret);
                if (!b) throw new Exception($"参数异常，{ret}");
                _taskId = ret.GetValueByKey("taskid");
            }
            else
            {
                var b = ServiceCall(CombineCmd(_devTaskPref, Constants.Servicecall, Constants.Space, parameters),
                    out var ret);
                if (!b) throw new Exception($"参数异常，{ret}");
                _taskId = ret.GetValueByKey("taskid");
            }
        }
        catch (IOException e)
        {
            Trace.WriteLine($"操作失败，设备不可读写，网络中断或设备已关闭，异常信息：{e.Message}");
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"启动任务失败\r\n异常信息：{ex}");
        }
    }

    /// <summary>
    ///     终止任务指令
    ///     调用之前，需要调用方明确任务是通过StartCmd实现了的
    /// </summary>
    public bool AbortCmd()
    {
        ClearMsgQueues();
        // 如果任务编号不存在，停止任务指令是没有意义的
        if (string.IsNullOrWhiteSpace(_taskId)) return false;
        if (_devTaskPref.Equals(Constants.RxCiq))
        {
            var command = CombineCmd(Constants.RxCiq, Constants.Stop, Constants.Space, _taskId);
            var success = SingleCall(command, out var ret);
            // 为保证任务终止，需要同步等待任务返回"OK"
            if (!success || ret != "OK") throw new Exception("没有获得正确的结束任务响应，任务可能处于后台继续运行！");
        }

        try
        {
            // 任务终止指令
            var command = CombineCmd(Constants.Abort, Constants.Space, _taskId);
            // 执行成功同步返回“OK”
            var success = SingleCall(command, out var ret);
            // 为保证任务终止，需要同步等待任务返回"OK"
            if (success && ret == "OK")
            {
                _taskId = null;
                return true;
            }
            else
            {
                throw new Exception("没有获得正确的结束任务响应，任务可能处于后台继续运行！");
            }
        }
        catch (IOException e)
        {
            Trace.WriteLine($"操作失败，设备不可读写，网络中断或设备已关闭，异常信息：{e.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"终止任务失败\r\n异常信息：{ex}");
            return false;
        }
        finally
        {
            // 注: 非常任要的一点，在获得任务已经成功终止时，一定要将当前任号编号成员置为空，否则将视任务继续运行
            _taskId = null;
        }
    }

    /// <summary>
    ///     修改任务（参数）指令
    ///     需要确保在修改参数时，任务编号是有效的
    /// </summary>
    private bool AlterCmd()
    {
        // 如果任务编号不存在，修改运行时参数指令是没有意义的
        if (string.IsNullOrWhiteSpace(_taskId)) return false;
        try
        {
            // 参数集字符串
            var parameters = GetParameters();
            var command = CombineCmd(_devTaskPref, Constants.ServicecallStar, Constants.Space, "taskid=", _taskId, ",",
                parameters);
            var success = SingleCall(command, out var ret);
            // 获取“OK”，判断是否命令是否成功
            if (success && ret == "OK")
                return true;
            throw new Exception("没有获得正确的参数设置响应！");
        }
        catch (IOException e)
        {
            Trace.WriteLine($"操作失败，设备不可读写，网络中断或设备已关闭，异常信息：{e.Message}");
            return false;
        }
    }

    #endregion

    #region 任务辅助方法

    /// <summary>
    ///     获取最新参数-值
    /// </summary>
    /// <returns>参数-值字符串</returns>
    private string GetParameters()
    {
        var parameters = string.Empty;
        try
        {
            if (!_features.ContainsKey(_devTaskPref))
            {
                // 如果当前功能不被支持，需要重置相关成员，以免当前状态影响后续功能的执行
                CurFeature = FeatureType.None;
                _devTaskPref = string.Empty;
                throw new Exception("设备不支持对应功能");
            }

            // 设备返回的参数列表
            var respond = _features[_devTaskPref];
            var devParameters = respond.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            // 过滤实例成员
            var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(item => item.FieldType == typeof(string));
            var fieldsDic = fields.ToDictionary(field => field.Name);
            // 离散扫描或频率表扫描需要将参数组合
            if (_devTaskPref.Equals(Constants.RxMscan) || _devTaskPref.Equals(Constants.DfList))
            {
                foreach (var discreteFreq in MscanPoints)
                {
                    foreach (var kvPair in discreteFreq)
                    {
                        var key = kvPair.Key == ParameterNames.FilterBandwidth
                            ? ParameterNames.IfBandwidth
                            : kvPair.Key; // 因AP510的带宽只有一个，所以都对应频谱带宽SpectrumSpan
                        SetParameter(key, kvPair.Value);
                    }

                    foreach (var item in devParameters)
                        if (fieldsDic.TryGetValue(item, out var value))
                            parameters += $"{item}={value.GetValue(this)},";
                    parameters = parameters.TrimEnd(new[] { ',' });
                    parameters += "|";
                }
            }
            else
            {
                foreach (var item in devParameters)
                    if (fieldsDic.TryGetValue(item, out var value))
                        parameters += $"{item}={value.GetValue(this)},";
                parameters = parameters.TrimEnd(new[] { ',' });
            }
        }
        catch (Exception ex)
        {
#if DEBUG
            Trace.WriteLine($"获取参数异常，异常信息：{ex}");
#endif
        }

        return parameters;
    }

    /// <summary>
    ///     组合指令
    /// </summary>
    /// <param name="commands">功能指令（常量数组）</param>
    /// <returns>返回组合好的完整功能指令</returns>
    private static string CombineCmd(params string[] commands)
    {
        if (commands == null) return string.Empty;
        var sb = new StringBuilder();
        foreach (var cmd in commands) sb.Append(cmd);
        return sb.ToString();
    }

    #endregion
}