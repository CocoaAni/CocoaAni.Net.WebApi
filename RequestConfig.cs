using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CocoaAni.Net.WebApi.Enums;

namespace CocoaAni.Net.WebApi;

public class RequestConfig
{
    public RequestConfig(DataFormat argsFormat, DataFormat resultFormat, HttpHeaders? headers, JsonSerializerOptions? jsonSerializerOptions)
    {
        ArgsFormat=argsFormat;
        ResultFormat=resultFormat;
        JsonSerializerOptions = jsonSerializerOptions;
        Headers = headers;
        IsUrlAutoEncoding = true;
        Timeout = 1000000;
    }

    public RequestConfig()
    {
        Timeout = 1000000;
    }

    public int Timeout { get; set; }
    public bool IsUrlAutoEncoding { get; set; }
    public HttpHeaders? Headers { get; set; }
    public DataFormat ArgsFormat { get; set; }
    public DataFormat ResultFormat { get; set; }
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }
    public bool IsSaveCookie { get; set; }
    public CookieContainer? CookieContainer { get; set; }

    public string GetHttpRequestContentType()
    {
        return ResultFormat switch
        {
            DataFormat.Auto => "application/json; charset=UTF-8",
            DataFormat.Json => "application/json; charset=UTF-8",
            DataFormat.Xml => "text/xml; charset=UTF-8",
            DataFormat.Form => "application/x-www-form-urlencoded",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public RequestConfig Clone()
    {
        return new RequestConfig(ArgsFormat, ResultFormat, Headers, JsonSerializerOptions);
    }
}