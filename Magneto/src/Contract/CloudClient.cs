using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract.Http;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using MessagePack;
using Newtonsoft.Json;

namespace Magneto.Contract;

/// <summary>
///     边缘端向云端交互的类
/// </summary>
public class CloudClient
{
    private static readonly Lazy<CloudClient> _lazy = new(() => new CloudClient());
    private string _token;
    public static CloudClient Instance => _lazy.Value;

    #region 电视解调相关

    /// <summary>
    ///     提交录制文件的结果到云端
    /// </summary>
    /// <param name="file"></param>
    public async Task<bool> AddDvrFileToCloudAsync(DvrFileInfo file)
    {
        const string uri = "tv/programPlayback/add";
        var json = JsonConvert.SerializeObject(file);
        var responseMessage = await HttpHelper.Instance.PostAsync(uri, json, _token, CancellationToken.None)
            .ConfigureAwait(false);
        var str = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (!responseMessage.IsSuccessStatusCode) return false;
        try
        {
            JsonConvert.DeserializeObject<CloudResult<long[]>>(str);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region 射电天文电测相关

    /// <summary>
    ///     更新云端射电任务
    /// </summary>
    /// <param name="taskId"></param>
    public async Task<string> GetFastEmtTasksAsync(string taskId)
    {
        try
        {
            var url = $"fast/task/getOne?id={taskId}";
            var task = HttpHelper.Instance.GetAsync(url, _token);
            var res = await task.ConfigureAwait(false);
            if (!res.IsSuccessStatusCode) return null;
            var str = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!str.Contains("result"))
            {
                Trace.WriteLine($"查询云端射电天文电测任务失败,{str}");
                return null;
            }

            return str;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"查询云端射电天文电测任务失败,{ex}");
        }

        return null;
    }

    #endregion

    #region 登录与token相关

    /// <summary>
    ///     获取云端Token
    /// </summary>
    /// <param name="user">用户名</param>
    /// <param name="password">密码</param>
    private async Task<bool> GetCloudTokenAsync(string user, string password)
    {
        var dic = new Dictionary<string, object>
        {
            { "account", user },
            { "password", password }
        };
        var json = GetJsonData(dic);
        try
        {
            const string uri = "auth/user/login";
            var task = HttpHelper.Instance.PostAsync(uri, json);
            var res = await task.ConfigureAwait(false);
            var content = res.Content;
            var task1 = content.ReadAsStringAsync();
            var str = await task1.ConfigureAwait(false);
            if (res.IsSuccessStatusCode && str.Contains("result"))
            {
                var userLogin = JsonConvert.DeserializeObject<CloudResult<string>>(str);
                _token = userLogin.Result;
                return true;
            }

            Trace.WriteLine($"用户登录[auth/user/login]失败:{res.StatusCode},{str}");
            return false;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"用户登录[auth/user/login]失败:{ex.Message}");
            return false;
        }
    }

    /// <summary>
    ///     登录
    /// </summary>
    /// <param name="user"></param>
    /// <param name="password"></param>
    /// <param name="edgeId"></param>
    /// <param name="port"></param>
    /// <param name="type"></param>
    public async Task<EdgeLoginResult> EdgeLoginAsync(string user, string password, string edgeId, int port, int type)
    {
        var loginResult = new EdgeLoginResult
        {
            Result = false
        };
        if (!await GetCloudTokenAsync(user, password).ConfigureAwait(false))
        {
            loginResult.IpAddress = "获取云端Token失败";
            return loginResult;
        }

        var dic = new Dictionary<string, object>
        {
            { "id", edgeId },
            { "port", port },
            { "type", "edge" }
        };
        var json = GetJsonData(dic);
        var uri = type == 0 ? "rmbt/edge/login" : "rmbt/edge/loginControl";
        var token = CancellationToken.None;
        try
        {
            var task = HttpHelper.Instance.PostAsync(uri, json, _token, token);
            var responseMessage = await task.ConfigureAwait(false);
            var taskRes = responseMessage.Content.ReadAsStringAsync(token);
            var str = await taskRes.ConfigureAwait(false);
            if (responseMessage.IsSuccessStatusCode && str.Contains("result"))
            {
                var result = JsonConvert.DeserializeObject<CloudResult<EdgeLoginResult>>(str);
                Trace.WriteLine($"{DateTime.Now:HH:mm:ss.fff} 登录结果:{str}");
                if (result.Result != null)
                {
                    loginResult = result.Result;
                    loginResult.Result = true;
                }
                else
                {
                    loginResult.Result = false;
                }

                return loginResult;
            }

            loginResult.IpAddress = str;
            loginResult.Result = false;
            Trace.WriteLine($"登录失败,{responseMessage.StatusCode},{str}");
            return loginResult;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"边缘端登录({uri})错误{ex.Message}");
            return loginResult;
        }
    }

    #endregion

    #region 配置相关

    /// <summary>
    ///     查询配置版本
    /// </summary>
    /// <param name="edgeId">边缘端ID</param>
    public async Task<string> GetVersionAsync(string edgeId)
    {
        // Thread.Sleep(1000);
        var uri = $"rmbt/edge/getConfigurationVersion?edgeId={edgeId}";
        var token = CancellationToken.None;
        var task = HttpHelper.Instance.GetAsync(uri, _token, token);
        var res = await task.ConfigureAwait(false);
        var content = res.Content;
        var task1 = content.ReadAsStringAsync(token);
        var str = await task1.ConfigureAwait(false);
        if (res.IsSuccessStatusCode && str.Contains("result"))
        {
            var version = JsonConvert.DeserializeObject<CloudResult<string>>(str);
            return version.Result;
        }

        Trace.WriteLine($"查询配置版本失败{str}");
        throw new Exception($"查询配置版本失败{str}");
    }

    /// <summary>
    ///     查询边缘端配置
    /// </summary>
    /// <param name="edgeId">边缘端ID</param>
    /// <param name="isEdge">是否是边缘端</param>
    public async Task<StationRegisterInfo> GetEdgeConfigAsync(string edgeId, bool isEdge)
    {
        var uri = $"rmbt/edge/getConfiguration?edgeId={edgeId}&rmControl={isEdge.ToString().ToLower()}";
        var token = CancellationToken.None;
        var task = HttpHelper.Instance.GetAsync(uri, _token, token);
        var res = await task.ConfigureAwait(false);
        var content = res.Content;
        var task1 = content.ReadAsStringAsync(token);
        var str = await task1.ConfigureAwait(false);
        if (!res.IsSuccessStatusCode || !str.Contains("result"))
        {
            Trace.WriteLine($"{DateTime.Now:HH:mm:ss} 查询边缘端配置失败:{str}");
            throw new Exception($"查询边缘端配置失败:{str}");
        }

        // Trace.WriteLine(str);
        var result = JsonConvert.DeserializeObject<CloudResult<Dictionary<string, object>>>(str);
        var info = result.Result;
        var config = new StationRegisterInfo();
        var jArray = info["modules"];
        info.Remove("modules");
        config.Station = (StationInfo)info;
        var json = JsonConvert.SerializeObject(jArray);
        var modules = JsonConvert.DeserializeObject<ModuleInfo[]>(json);
        // Dictionary<string, object>[] dic = JsonConvert.DeserializeObject<Dictionary<string, object>[]>(json);
        // ModuleInfo[] modules = Array.ConvertAll(dic, item => (ModuleInfo)item);
        config.Modules = modules;
        return config;
    }

    #endregion

    #region 新信号截获相关

    /// <summary>
    ///     更新新信号截获模板
    /// </summary>
    /// <param name="data"></param>
    public async Task UpdateNsicTemplateDataAsync(TemplateDataSendToCloud data)
    {
        const string uri = "newSignal/newSignalTemplate/update";
        var json = JsonConvert.SerializeObject(data);
        var task = HttpHelper.Instance.PostAsync(uri, json, _token, CancellationToken.None);
        var responseMessage = await task.ConfigureAwait(true);
        var taskRes = responseMessage.Content.ReadAsStringAsync();
        var str = await taskRes.ConfigureAwait(true);
        if (responseMessage.IsSuccessStatusCode && str.Contains("result"))
            Trace.WriteLine($"模板保存成功,{str}");
        else
            throw new ArgumentException($"更新新信号截获模板失败:{str}");
    }

    /// <summary>
    ///     更新新信号截获比对结果
    /// </summary>
    /// <param name="data"></param>
    public async Task UpdateNsicResultDataAsync(ResultDataSendToCloud data)
    {
        const string uri = "newSignal/newSignalData/add";
        var json = MessagePackSerializer.SerializeToJson(data);
        var task = HttpHelper.Instance.PostAsync(uri, json, _token, CancellationToken.None);
        var responseMessage = await task.ConfigureAwait(false);
        var taskRes = responseMessage.Content.ReadAsStringAsync();
        var str = await taskRes.ConfigureAwait(false);
        if (responseMessage.IsSuccessStatusCode && str.Contains("result"))
            Trace.WriteLine($"新信号截获比对结果保存成功:{str}");
        else
            throw new ArgumentException($"更新新信号截获比对结果失败:{str}");
    }

    /// <summary>
    ///     获取新信号截获模板
    /// </summary>
    /// <param name="templateId"></param>
    public async Task<TemplateDataFromCloud> GetNsicTemplateDataAsync(string templateId)
    {
        var uri = $"newSignal/newSignalTemplate/getOne?id={templateId}";
        var task = HttpHelper.Instance.GetAsync(uri, _token, CancellationToken.None);
        var res = await task.ConfigureAwait(false);
        if (!res.IsSuccessStatusCode) return new TemplateDataFromCloud();
        var str = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (!str.Contains("result")) return new TemplateDataFromCloud();
        var data = JsonConvert.DeserializeObject<CloudResult<TemplateDataFromCloud>>(str);
        if (data?.Result is { Data: not null })
        {
            if (data.Result.Parameters.ContainsKey(ParameterNames.ScanSegments))
            {
                var value = data.Result.Parameters[ParameterNames.ScanSegments].ToString();
                if (value != null)
                    data.Result.Parameters[ParameterNames.ScanSegments] =
                        JsonConvert.DeserializeObject<Dictionary<string, object>[]>(value);
            }

            return data.Result;
        }

        return new TemplateDataFromCloud();
    }

    #endregion

    #region 考试保障相关

    /// <summary>
    ///     更新考试保障模板
    /// </summary>
    /// <param name="data"></param>
    public async Task UpdateEseTemplateDataAsync(TemplateDataSendToCloud data)
    {
        const string uri = "exam/examSignalTemplate/update";
        var json = JsonConvert.SerializeObject(data);
        var task = HttpHelper.Instance.PostAsync(uri, json, _token, CancellationToken.None);
        var responseMessage = await task.ConfigureAwait(false);
        var taskRes = responseMessage.Content.ReadAsStringAsync();
        var str = await taskRes.ConfigureAwait(false);
        if (responseMessage.IsSuccessStatusCode && str.Contains("result"))
            Trace.WriteLine($"模板保存成功,{str}");
        else
            throw new ArgumentException($"更新考试保障模板失败:{str}");
    }

    /// <summary>
    ///     更新考试保障比对结果
    /// </summary>
    /// <param name="data"></param>
    public async Task UpdateEseResultDataAsync(EseResultDataSendToCloud data)
    {
        const string uri = "exam/examSignalData/add";
        var json = MessagePackSerializer.SerializeToJson(data);
        var task = HttpHelper.Instance.PostAsync(uri, json, _token, CancellationToken.None);
        var responseMessage = await task.ConfigureAwait(false);
        var taskRes = responseMessage.Content.ReadAsStringAsync();
        var str = await taskRes;
        if (responseMessage.IsSuccessStatusCode && str.Contains("result"))
            Trace.WriteLine($"考试保障比对结果保存成功,{str}");
        else
            throw new ArgumentException($"更新考试保障比对结果失败:{str}");
    }

    /// <summary>
    ///     获取考试保障模板
    /// </summary>
    /// <param name="templateId"></param>
    public async Task<TemplateDataFromCloud> GetEseTemplateDataAsync(string templateId)
    {
        var uri = $"exam/examSignalTemplate/getOne?id={templateId}";
        var task = HttpHelper.Instance.GetAsync(uri, _token, CancellationToken.None);
        var res = await task.ConfigureAwait(false);
        if (!res.IsSuccessStatusCode) return new TemplateDataFromCloud();
        var str = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (!str.Contains("result")) return new TemplateDataFromCloud();
        var data = JsonConvert.DeserializeObject<CloudResult<TemplateDataFromCloud>>(str);
        if (data?.Result is { Data: not null })
        {
            if (data.Result.Parameters.ContainsKey(ParameterNames.ScanSegments))
            {
                var value = data.Result.Parameters[ParameterNames.ScanSegments].ToString();
                if (value != null)
                    data.Result.Parameters[ParameterNames.ScanSegments] =
                        JsonConvert.DeserializeObject<Dictionary<string, object>[]>(value);
            }

            return data.Result;
        }

        return new TemplateDataFromCloud();
    }

    /// <summary>
    ///     获取考试保障白名单
    /// </summary>
    public async Task<List<EseWhiteListFromCloud>> GetEseWhiteListAsync()
    {
        const string uri = "exam/examSignalIgnore/getList";
        var task = HttpHelper.Instance.GetAsync(uri, _token, CancellationToken.None);
        var res = await task.ConfigureAwait(false);
        if (!res.IsSuccessStatusCode) return new List<EseWhiteListFromCloud>();
        var str = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
        var data = JsonConvert.DeserializeObject<CloudResult<List<EseWhiteListFromCloud>>>(str);
        if (data is not { Error: null }) return new List<EseWhiteListFromCloud>();
        if (data.Result is { Count: > 0 }) return data.Result;
        return new List<EseWhiteListFromCloud>();
    }

    #endregion

    #region 计划、日志、台站相关

    /// <summary>
    ///     获取最近更新的计划任务
    /// </summary>
    /// <param name="edgeId"></param>
    /// <param name="updateTime"></param>
    public async Task<string> GetCrondTasksAsync(string edgeId, DateTime? updateTime)
    {
        try
        {
            var url = $"rmbt/plan/getList?edgeId={edgeId}";
            if (updateTime is not null)
            {
                var time = ((DateTime)updateTime).ToString("yyyy-MM-dd");
                url += $"&updateTime.gte={time}";
            }

            var task = HttpHelper.Instance.GetAsync(url, _token);
            var res = await task.ConfigureAwait(false);
            if (!res.IsSuccessStatusCode) return null;
            var str = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
            Console.WriteLine($"查询计划结果:{str}");
            if (!str.Contains("result"))
            {
                Trace.WriteLine($"查询计划失败,{str}");
                return null;
            }

            var result = JsonConvert.DeserializeObject<CloudResult<Dictionary<string, object>[]>>(str);
            return JsonConvert.SerializeObject(result.Result);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"查询计划失败,{ex}");
        }

        return null;
    }

    /// <summary>
    ///     添加业务日志
    /// </summary>
    /// <param name="dic"></param>
    public async Task<bool> AddBusinessLogAsync(Dictionary<string, object> dic)
    {
        const string uri = "log/logBusiness/add";
        var json = MessagePackSerializer.SerializeToJson(dic);
        var task = HttpHelper.Instance.PostAsync(uri, json, _token, CancellationToken.None);
        var responseMessage = await task.ConfigureAwait(false);
        var taskRes = responseMessage.Content.ReadAsStringAsync();
        var str = await taskRes.ConfigureAwait(false);
        return responseMessage.IsSuccessStatusCode && str.Contains("result");
    }

    /// <summary>
    ///     查询台站数据库
    /// </summary>
    /// <param name="startFreq"></param>
    /// <param name="stopFreq"></param>
    public async Task<StationSignalsInfo[]> GetStationSignalsInfoAsync(double startFreq, double? stopFreq)
    {
        var uri = $"rsbt/stationManager/getStationInfo?freqEfb={startFreq}";
        if (stopFreq != null) uri += $"&freqEfe={stopFreq}";
        var task = HttpHelper.Instance.GetAsync(uri, _token, CancellationToken.None);
        var res = await task.ConfigureAwait(false);
        var content = res.Content;
        var task1 = content.ReadAsStringAsync();
        var str = await task1.ConfigureAwait(false);
        if (!res.IsSuccessStatusCode || !str.Contains("result"))
            throw new Exception($"查询台站数据库失败,start:{startFreq},stop:{stopFreq},{str}");
        try
        {
            var data = JsonConvert.DeserializeObject<CloudResult<StationSignalsInfo[]>>(str);
            if (data?.Result is not null) return data.Result;
        }
        catch (Exception ex)
        {
            throw new Exception($"查询台站数据库成功但是解析失败,start:{startFreq},stop:{stopFreq},{ex.Message}:{str}");
        }

        throw new Exception($"查询台站数据库成功但是解析失败,start:{startFreq},stop:{stopFreq},{str}");
    }

    #endregion

    #region 电磁环境相关

    /// <summary>
    ///     向云端添加电磁环境的电磁数据
    /// </summary>
    /// <param name="elecData">电磁数据</param>
    public async Task<bool> AddElectromagneticDataAsync(Emdc2CloudData<ElectromagneticData> elecData)
    {
        const string uri = "feature/emdc/addOriginal";
        var json = JsonConvert.SerializeObject(elecData);
        var task = HttpHelper.Instance.PostAsync(uri, json, _token, CancellationToken.None);
        var responseMessage = await task.ConfigureAwait(false);
        var taskRes = responseMessage.Content.ReadAsStringAsync();
        var str = await taskRes.ConfigureAwait(false);
        return responseMessage.IsSuccessStatusCode && str.Contains("result");
    }

    /// <summary>
    ///     添加电磁环境采集的信号数据
    /// </summary>
    /// <param name="signalsData">信号数据</param>
    public async Task<bool> AddSignalsDataAsync(Emdc2CloudData<Signals2Cloud<SignalsResult>> signalsData)
    {
        const string uri = "feature/emdc/addSignal";
        var json = JsonConvert.SerializeObject(signalsData);
        var task = HttpHelper.Instance.PostAsync(uri, json, _token, CancellationToken.None);
        var responseMessage = await task.ConfigureAwait(false);
        var taskRes = responseMessage.Content.ReadAsStringAsync();
        var str = await taskRes.ConfigureAwait(false);
        return responseMessage.IsSuccessStatusCode && str.Contains("result");
    }

    /// <summary>
    ///     添加电磁环境采集的空闲频点数据
    /// </summary>
    /// <param name="freesData">空闲频点数据</param>
    public async Task<bool> AddFreeSignalsDataAsync(Emdc2CloudData<Signals2Cloud<FreeSignalsResult>> freesData)
    {
        const string uri = "feature/emdc/addUnuse";
        var json = JsonConvert.SerializeObject(freesData);
        var task = HttpHelper.Instance.PostAsync(uri, json, _token, CancellationToken.None);
        var responseMessage = await task.ConfigureAwait(false);
        var taskRes = responseMessage.Content.ReadAsStringAsync();
        var str = await taskRes.ConfigureAwait(false);
        return responseMessage.IsSuccessStatusCode && str.Contains("result");
    }

    #endregion

    #region 通用

    public async Task<DictionaryData> GetCloudDictionaryDataAsync(string dicType)
    {
        var uri = $"dic/dictionary/getDic?dicNo={dicType}";
        var responseMessage =
            await HttpHelper.Instance.GetAsync(uri, _token, CancellationToken.None).ConfigureAwait(false);
        var str = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (!responseMessage.IsSuccessStatusCode) return default;
        try
        {
            var res = JsonConvert.DeserializeObject<CloudResult<DictionaryData[]>>(str);
            if (res?.Result == null || res.Result.Length == 0) return default;
            return res.Result[0];
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    ///     查询云端时间
    /// </summary>
    /// <returns>返回值为long类型的时间戳 单位ms</returns>
    public async Task<long> GetCloudDateTimeAsync()
    {
        var uri = "/manager/runtime/getDate";
        try
        {
            var responseMessage = await HttpHelper.Instance.GetAsync(uri, _token, CancellationToken.None)
                .ConfigureAwait(false);
            var str = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!responseMessage.IsSuccessStatusCode) return 0;
            var res = JsonConvert.DeserializeObject<CloudResult<long>>(str);
            if (res == null) return -1;
            return res.Result;
        }
        catch
        {
            return -1;
        }
    }

    public bool PutModules(string url, string jsonContent)
    {
        var cancellation = CancellationToken.None;
        var res = HttpHelper.Instance.PostAsync(url, jsonContent, _token, cancellation).ConfigureAwait(false)
            .GetAwaiter().GetResult();
        return res.IsSuccessStatusCode;
    }

    /// <summary>
    ///     云端通用Post接口
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="data"></param>
    public async Task<bool> CommonPostAsync(string uri, Dictionary<string, object> data)
    {
        var json = JsonConvert.SerializeObject(data);
        var task = HttpHelper.Instance.PostAsync(uri, json, _token, CancellationToken.None);
        var responseMessage = await task.ConfigureAwait(false);
        var taskRes = responseMessage.Content.ReadAsStringAsync();
        var str = await taskRes.ConfigureAwait(false);
        return responseMessage.IsSuccessStatusCode && str.Contains("result");
    }

    private static string GetJsonData(Dictionary<string, object> dic)
    {
        var str = JsonConvert.SerializeObject(dic);
        return str;
    }

    #endregion
}

internal class CloudData<T>
{
    [JsonProperty("total")] public int Total { get; set; }

    [JsonProperty("rows")] public T[] Rows { get; set; }
}

public class EdgeLoginResult
{
    [JsonIgnore] public bool Result { get; set; }

    [JsonProperty("ip")] public string IpAddress { get; set; }

    /// <summary>
    ///     动环使用
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }
}

internal class CloudResult<T>
{
    [JsonProperty("error")] public CloudError Error { get; set; }

    [JsonProperty("result")] public T Result { get; set; }
}

internal class CloudError
{
    [JsonProperty("code")] public int Code { get; set; }

    [JsonProperty("message")] public string Message { get; set; }
}