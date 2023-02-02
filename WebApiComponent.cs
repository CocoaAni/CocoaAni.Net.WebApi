using System.Text.Json;
using CocoaAni.Net.WebApi.Enums;

namespace CocoaAni.Net.WebApi;

public class WebApiComponent
{
    public string? BaseUri { get; set; }
    protected HttpClient? HttpClient { get; set; }
    public RequestConfig RequestConfig { get; set; }

    public WebApiComponent(RequestConfig requestConfig,HttpClient? httpClient=null)
    {
        HttpClient = httpClient;
        RequestConfig = requestConfig ?? throw new ArgumentNullException(nameof(requestConfig));
    }


    protected Task<T> DoGetAsync<T>(
        string url,
        object? args = null,
        CancellationToken ctk = default)
        where T : IResult, new()
        => DoRequestAsync<T>(HttpMethod.Get, url, args, ctk);

    protected Task<T> DoPostAsync<T>(
        string url,
        object? args = null,
        CancellationToken ctk = default)
        where T : IResult, new()
        => DoRequestAsync<T>(HttpMethod.Post, url, args, ctk);

    protected Task<T> DoPatchAsync<T>(
        string url,
        object? args = null,
        CancellationToken ctk = default)
        where T : IResult, new()
        => DoRequestAsync<T>(HttpMethod.Patch, url, args, ctk);

    protected Task<T> DoPutAsync<T>(
        string url,
        object? args = null,
        CancellationToken ctk = default)
        where T : IResult, new()
        => DoRequestAsync<T>(HttpMethod.Put, url, args, ctk);

    protected Task<T> DoDeleteAsync<T>(
        string url,
        object? args = null,
        CancellationToken ctk = default)
        where T : IResult, new()
        => DoRequestAsync<T>(HttpMethod.Delete, url, args, ctk);

    protected Task<T> DoRequestAsync<T>(
        HttpMethod method,
        string url,
        object? args = null,
        CancellationToken ctk = default)
        where T : IResult, new()
    {
        if (!url.StartsWith("http"))
            url = BaseUri + url;
        // ReSharper disable once InvokeAsExtensionMethod
        return HttpClient != null 
            ? WebApi.DoRequest<T>(HttpClient, method, url, args, RequestConfig, ctk) 
            : WebApi.DoRequest<T>(method, url, args, RequestConfig, ctk);
    }
}