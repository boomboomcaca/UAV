using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Magneto.Contract.Http;

/// <summary>
///     Http连接帮助类，实现http的Post,Put与Delete操作
/// </summary>
public sealed class HttpHelper
{
    private static readonly Lazy<HttpHelper> _lazy = new(() => new HttpHelper());
    private readonly HttpClient _httpClient;

    private HttpHelper()
    {
#if DEBUG
        // wsl调试时，改用旧 System.Net.Http.HttpClientHandler 类。
        AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);
#endif
        _httpClient = new HttpClient
        {
            Timeout = new TimeSpan(0, 0, 5)
        };
    }

    public static HttpHelper Instance => _lazy.Value;
    public bool IsInitialized { get; private set; }

    /// <summary>
    ///     初始化
    /// </summary>
    /// <param name="ipAddress"></param>
    /// <param name="port"></param>
    public void Initialized(string ipAddress, int port)
    {
        _httpClient.BaseAddress = new Uri($"http://{ipAddress}:{port}/");
        IsInitialized = true;
    }

    public void Initialized(string dns)
    {
        _httpClient.BaseAddress = new Uri($"http://{dns}/");
        IsInitialized = true;
    }

    /// <summary>
    ///     Post
    /// </summary>
    /// <param name="uri">调用的附加uri，不包含前面的"/"，比如调用"edge"而非"/edge"</param>
    /// <param name="jsonContent"></param>
    /// <param name="token"></param>
    /// <param name="cancellation"></param>
    public Task<HttpResponseMessage> PostAsync(string uri, string jsonContent, string token = "",
        CancellationToken cancellation = default)
    {
        HttpContent content = new StringContent(jsonContent);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        if (!string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
        //content.Headers.Add("Authorization", $"Bearer {token}");
        return _httpClient.PostAsync(uri, content, cancellation);
    }

    public Task<HttpResponseMessage> GetAsync(string uri, string token = "", CancellationToken cancellation = default)
    {
        if (!string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
        //content.Headers.Add("Authorization", $"Bearer {token}");
        return _httpClient.GetAsync(uri, cancellation);
    }

    /// <summary>
    ///     Put
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="jsonContent"></param>
    public Task<HttpResponseMessage> PutAsync(string uri, string jsonContent)
    {
        HttpContent content = new StringContent(jsonContent);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return _httpClient.PutAsync(uri, content);
    }

    /// <summary>
    ///     Delete
    /// </summary>
    /// <param name="uri"></param>
    public Task<HttpResponseMessage> DeleteAsync(string uri)
    {
        return _httpClient.DeleteAsync(uri);
    }
}