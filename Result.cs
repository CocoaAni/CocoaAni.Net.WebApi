using System.Net;

namespace CocoaAni.Net.WebApi;

public interface IResult : Results.IResult
{
    public HttpStatusCode HttpCode { get; set; }
    public CookieCollection? Cookies { get; set; }
}

public class Result<T> : Result<T, Exception>
{
    public Result()
    {
    }

    public Result(T target, bool isError)
    {
        if (isError)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            SetError(target!);
        }
        else
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            SetValue(target);
        }
    }
}

public class Result<TV, TE> :
    CocoaAni.Results.Result<TV, TE>, IResult
{
    public Result()
    {
    }

    public Result(TV value) : base(value)
    {
    }

    public Result(TE error) : base(error)
    {
    }

    public HttpStatusCode HttpCode { get; set; }
    public CookieCollection? Cookies { get; set; }
    public Exception? Exception { get; set; }
    public bool IsException => Exception != null;
    public WebApiResultState State => (WebApiResultState)this.StateCode;

    public override void SetError(object error)
    {
        switch (error)
        {
            case TE apiError:
                SetError(apiError);
                break;

            case Exception ex:
                Exception = ex;
                StateCode = (byte)WebApiResultState.Exception;
                break;

            default:
                throw new NotSupportedException();
        }
    }
}