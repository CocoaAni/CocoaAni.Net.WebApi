using System.Text;

namespace CocoaAni.Net.WebApi;

public class HttpHeaders : Dictionary<string, HttpHeaderValue>
{
    public string? GetHeaderValueOrDefault(string name, object defaultV)
        => !ContainsKey(name) ? defaultV.ToString() : this[name].Value;

    public string GetHeaderValue(string name) => this[name].Value;

    public IEnumerable<string>? GetHeaderValues(string name)
    {
        return !ContainsKey(name) ? null : this[name].Values;
    }

    public void AddHeaderValue(string name, string value)
    {
        if (!ContainsKey(name))
        {
            this[name] = new HttpHeaderValue(value);
            return;
        }
        var v = this[name].InternalValue;
        switch (v)
        {
            case List<string> valueList:
                valueList.Add(value);
                break;

            case string strValue:
                this[name] = new HttpHeaderValue(new List<string>()
                {
                    strValue, value
                });
                break;

            default: throw new NotSupportedException($"Header Value = {v.GetType()}");
        }
    }

    public void AddHeaderValues(string name, IEnumerable<string> values)
    {
        if (!ContainsKey(name))
        {
            this[name] = new HttpHeaderValue(new List<string>(values));
        }
        else
        {
            var v = this[name].InternalValue;
            switch (v)
            {
                case string strValue:
                    {
                        var list = new List<string> { strValue };
                        list.AddRange(values);
                        this[name] = new HttpHeaderValue(list);
                        break;
                    }
                case List<string> list:
                    list.AddRange(values);
                    break;

                default: throw new NotSupportedException($"Header Value = {v.GetType()}");
            }
        }
    }

    public void SetHeader(string name, HttpHeaderValue value)
        => this[name] = value;

    public void SetHeader(string name, string value)
        => this[name] = new HttpHeaderValue(value);

    public void SetHeader(string name, IEnumerable<string> value)
        => this[name] = new HttpHeaderValue(value);
}

public readonly struct HttpHeader
{
    public HttpHeader(string name, HttpHeaderValue value)
    {
        Name = name;
        Value = value;
    }

    public readonly string Name { get; }
    public readonly HttpHeaderValue Value { get; }
}

public struct HttpHeaderValue
{
    public static readonly HttpHeaderValue Default = new HttpHeaderValue();
    public object InternalValue;

    public HttpHeaderValue(object value)
    {
        InternalValue = value ?? throw new ArgumentNullException(nameof(value));
    }

    public IEnumerable<string> Values
    {
        get
        {
            switch (InternalValue)
            {
                case IEnumerable<string> ie:
                    return ie;

                case string str:
                    {
                        var list = str.Split(';').ToList();
                        InternalValue = list;
                        return list;
                    }
                default:
                    throw new NotSupportedException();
            }
        }
    }

    public string Value => ToString();

    public override string ToString()
    {
        switch (InternalValue)
        {
            case string strValue:
                return strValue;

            case IEnumerable<string> ie:
                {
                    var sb = new StringBuilder();
                    foreach (var s in ie)
                    {
                        sb.Append(s).Append(" ;");
                    }
                    return sb.ToString();
                }
            default:
                throw new NotSupportedException();
        }
    }
}