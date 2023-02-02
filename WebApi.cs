using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using CocoaAni.Net.WebApi.Enums;

namespace CocoaAni.Net.WebApi;

public static class WebApi
{
    
    public static RequestConfig DefaultRequestConfig { get; } = new RequestConfig(
        DataFormat.Auto,
        DataFormat.Auto,
        new HttpHeaders()
        {
            {"User-Agent",new HttpHeaderValue("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36 Edg/109.0.1518.69")}
        },
        new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        }
    )
    {
        Timeout = 100000,
    };


    #region 核心方法



    public static async Task<T> DoRequest<T>(HttpMethod method, string url, object? args = null, RequestConfig? config = null,
        CancellationToken? ctk = null)
        where T : IResult, new()
    {
        config ??= DefaultRequestConfig;
        var result = new T();
        var headers = config.Headers;
        var jsonSerializerOptions = config.JsonSerializerOptions;
        HttpWebResponse? resp = null!;
        MemoryStream? ms = null;
        try
        {
            var isGetMethod = method == HttpMethod.Get;
            //Get Query Args Url 构建
            if (isGetMethod && args != null)
            {
                if (url.EndsWith('&'))
                    url += Tools.CreateQueryArgsString(args);
                else
                    url = $"{url}?{Tools.CreateQueryArgsString(args)}";
            }
            if (config.IsUrlAutoEncoding)
            {
                url = WebApi.Tools.UriEncode(url);
            }
#pragma warning disable SYSLIB0014
            var request = (WebRequest.Create(url) as HttpWebRequest)!;
#pragma warning restore SYSLIB0014

            var requestBody = isGetMethod
                ? null
                : args is string strArgs
                    ? Encoding.UTF8.GetBytes(strArgs)
                    : JsonSerializer.SerializeToUtf8Bytes(args, jsonSerializerOptions);

            #region SetHttpHeader

            request.ContentType = config.GetHttpRequestContentType();
            request.Timeout = config.Timeout;
            request.Method = method.ToString();
            request.ContentLength = requestBody?.Length ?? 0;

            if (headers != null)
                foreach (var header in headers)
                    request.Headers.Set(header.Key, header.Value.ToString());

            #endregion

            #region WriteBody

            if (requestBody != null)
            {
                await using var requestStream = request.GetRequestStream();
                await requestStream.WriteAsync(requestBody);
                await requestStream.FlushAsync();
            }

            #endregion

            #region HandleResp

            resp = (HttpWebResponse)request.GetResponse()!;
            await using var respStream = resp.GetResponseStream();
            //Copy RespStream To Memory
            ms = resp.ContentLength == -1
                ? new MemoryStream()
                : resp.ContentLength < int.MaxValue
                    ? new MemoryStream((int)resp.ContentLength)
                    : throw new NotSupportedException($"ContentLength={resp.ContentLength} > int.Max , Is Too Large!");
            await respStream.CopyToAsync(ms, ctk ??= default);
            ms.Seek(0, SeekOrigin.Begin);
            result.HttpCode = resp.StatusCode;

            #region BuildResult

            var targetType = (int)resp.StatusCode < 300 ? result.ValueType : result.ErrorType;
            var value = GetObject(targetType, ms, config);

            if ((int)resp.StatusCode < 300)
                result.SetValue(value!);
            else
                result.SetError(value!);

            #endregion

            // ReSharper disable once InvertIf
            if (config.IsSaveCookie)
            {
                config.CookieContainer ??= new CookieContainer();
                config.CookieContainer.Add(new Uri(url), resp.Cookies);
                result.Cookies = resp.Cookies;
            }

            return result;

            #endregion

        }
        catch (WebException e)
        {
            if (e.Response == null)
            {
                result.SetError(e);
                return result;
            }
            resp = (HttpWebResponse)e.Response!;
            await using var respStream = resp.GetResponseStream();
            ms = resp.ContentLength == -1
                ? new MemoryStream()
                : resp.ContentLength < int.MaxValue
                    ? new MemoryStream((int)resp.ContentLength)
                    : throw new NotSupportedException($"ContentLength={resp.ContentLength} > int.Max , Is Too Large!");
            await respStream.CopyToAsync(ms, ctk ??= default);
            ms.Seek(0, SeekOrigin.Begin);
            result.SetError(GetObject(result.ErrorType, ms, config));
        }
        catch (Exception e)
        {
            result.SetError(e);
        }
        return result;
    }
    public static async Task<T> DoRequest<T>(this HttpClient client, HttpMethod method, string url, object? args = null,
        RequestConfig? config = null,
        CancellationToken ctk = default)
        where T : IResult, new()
    {
        config ??= DefaultRequestConfig;
        var result = new T();
        var headers = config.Headers;
        var jsonSerializerOptions = config.JsonSerializerOptions;
        try
        {
            var isGetMethod = method  == HttpMethod.Get;
            //Get Query Args Url 构建
            if (isGetMethod && args != null)
            {
                if (url.EndsWith('&'))
                    url += Tools.CreateQueryArgsString(args);
                else
                    url = $"{url}?{Tools.CreateQueryArgsString(args)}";
            }
            if (config.IsUrlAutoEncoding)
            {
                url = WebApi.Tools.UriEncode(url);
            }
            var requestBody = isGetMethod
                ? null
                : args is string strArgs
                    ? Encoding.UTF8.GetBytes(strArgs)
                    : JsonSerializer.SerializeToUtf8Bytes(args, jsonSerializerOptions);

            #region SetHttpHeader

            client.DefaultRequestHeaders.Add("Content-Type", config.GetHttpRequestContentType());
            client.Timeout= new TimeSpan(config.Timeout);
            client.DefaultRequestHeaders.Add("Content-Length",(requestBody?.Length ?? 0).ToString());
            if (headers != null)
                foreach (var header in headers)
                    client.DefaultRequestHeaders.Add(header.Key, header.Value.ToString());

            #endregion

            using var resp = method.ToString() switch
            {
                "GET" => await client.GetAsync(url, ctk),
                "DELETE" => await client.DeleteAsync(url, ctk),
                "POST" => await client.PostAsync(url, new ByteArrayContent(requestBody!), ctk),
                "PUT" => await client.PutAsync(url, new ByteArrayContent(requestBody!), ctk),
                "PATCH" => await client.PatchAsync(url, new ByteArrayContent(requestBody!), ctk),
                _ => throw new ArgumentException(method.ToString())
            };
            var respStream = await resp.Content.ReadAsStreamAsync(ctk);
            var contentLength = long.Parse(resp.Headers.GetValues("Content-Length").FirstOrDefault("-1"));
            var ms = contentLength == -1
                ? new MemoryStream()
                : contentLength < int.MaxValue
                    ? new MemoryStream((int)contentLength)
                    : throw new NotSupportedException($"ContentLength={contentLength} > int.Max , Is Too Large!");
            await respStream.CopyToAsync(ms, ctk);
            await ms.FlushAsync(ctk);
            ms.Seek(0, SeekOrigin.Begin);

            result.HttpCode = resp.StatusCode;
            var targetType = (int)resp.StatusCode < 300 ? result.ValueType : result.ErrorType;
            var value = GetObject(targetType, ms, config);

            if ((int)resp.StatusCode < 300)
                result.SetValue(value!);
            else
                result.SetError(value!);

            // ReSharper disable once InvertIf
            if (config.IsSaveCookie)
            {
                config.CookieContainer ??= new CookieContainer();
                var cookies = new CookieCollection();
                foreach (var ckStr in resp.Headers.GetValues("Set-Cookie"))
                {
                    var split = ckStr.Split('=');
                    if (split.Length != 2)
                        continue;
                    cookies.Add(new Cookie(split[0], split[1]));
                }
                config.CookieContainer.Add(new Uri(url),cookies);
                result.Cookies = cookies;
            }

            return result;
        }
        catch (Exception e)
        {
            result.SetError(e);
        }
        return result;
    }

   

    private static object GetObject(Type type, MemoryStream ms,RequestConfig config)
    {
        object? value = null;
        if (type == typeof(byte[]))
            value = ms.Capacity == ms.Length ? ms.GetBuffer() : ms.ToArray();
        else if (type == typeof(Memory<byte>))
            value = ms.GetBuffer().AsMemory(0, (int)ms.Length);
        else if (type == typeof(string))
            value = Encoding.UTF8.GetString(ms.GetBuffer().AsMemory(0, (int)ms.Length).Span);
        else if (type == typeof(object))
            value = string.Empty;
        else
        {
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            value = config.ResultFormat switch
            {
                DataFormat.Auto => DataFormat.Json,
                DataFormat.Json => JsonSerializer.Deserialize(ms, type, config.JsonSerializerOptions),
                DataFormat.Xml => new XmlSerializer(type).Deserialize(ms),
                _ => throw new NotSupportedException($"{nameof(config.ResultFormat)}={config.ResultFormat}")
            };
        }
        return value!;
    }

    #endregion 核心方法

    #region 工具方法

    



    public static class Tools
    {
        public static string CreateQueryArgsString(object arg, bool isEncode = true)
        {
            StringBuilder? sb = null;
            switch (arg)
            {
                case null:
                    throw new ArgumentNullException(nameof(arg));
                case string str:
                    if (!isEncode || string.IsNullOrWhiteSpace(str))
                        return str;
                    if (!str.StartsWith('?'))
                    {
                        throw new InvalidOperationException($"Arg Error: {str}");
                    }
                    sb = new StringBuilder();
                    sb.Append('?');
                    foreach (var s in str[1..].Split('&'))
                    {
                        var kv = string.Empty;
                        if (isEncode)
                        {
                            var arr = s.Split('=');
                            if (arr.Length == 1)
                            {
                                kv = s;
                            }
                            else if (arr.Length == 2)
                            {
                                kv = $"{HttpUtility.UrlEncode(arr[0])}={HttpUtility.UrlEncode(arr[1])}&";
                            }
                            else
                            {
                                throw new InvalidOperationException($"Arg Error: {s}");
                            }
                        }
                        sb.Append(kv);
                    }
                    break;

                default:
                    sb = new StringBuilder();
                    foreach (var propertyInfo in arg.GetType().GetProperties())
                    {
                        var value = propertyInfo.GetMethod?.Invoke(arg, null);
                        if (value == null) continue;
                        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                        var k = string.Empty;
                        var v = string.Empty;
                        if (isEncode)
                        {
                            v = propertyInfo.PropertyType.IsAssignableTo(typeof(Enum))
                                ? ((int)value).ToString()
                                : value.ToString();
                            k = System.Web.HttpUtility.UrlEncode(propertyInfo.Name);
                        }
                        sb.Append(k).Append('=').Append(v).Append('&');
                    }
                    break;
            }
            if (sb.Length > 1)
            {
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }
        public static string UriEncode(string url)
            => UriEncode(new Uri(url));

        public static string UriEncode(Uri uri)
        {
            var pathBuilder = new StringBuilder();
            var paths = uri.LocalPath.Split('/');
            foreach (var path in paths)
            {
                if (path == string.Empty)
                    continue;
                pathBuilder.Append('/').Append(HttpUtility.UrlEncode(path));
            }

            var args = uri.Query;
            if (args.Length > 2)
            {
                args = WebApi.Tools.CreateQueryArgsString(args);
            }
            return $"{uri.Scheme}://{HttpUtility.UrlEncode(uri.Host)}{pathBuilder}{args}";
        }
    }

    #endregion 工具方法

    public static Task<Result<string>> GetStringAsync(string url, object? args=null, RequestConfig? config = null,
        CancellationToken ctk = default)
        => DoRequest<Result<string>>(HttpMethod.Get, url, args, config ?? DefaultRequestConfig, ctk);

    public static Result<string> GetString(string url, object? args = null, RequestConfig? config=null,
        CancellationToken ctk = default)
        => GetStringAsync(url, args, config, ctk).Result;

    public static Task<Result<Memory<byte>>> GetBaseContentAsync(
        string url, object? args = null, RequestConfig? config = null,
        CancellationToken ctk = default)
        => DoRequest<Result<Memory<byte>>>(HttpMethod.Get, url, args, config ?? DefaultRequestConfig, ctk);

    public static Result<Memory<byte>> GetBaseContent(
        string url, object? args = null, RequestConfig? config = null,
        CancellationToken ctk = default)
        => GetBaseContentAsync(url, args, config, ctk).Result;
}