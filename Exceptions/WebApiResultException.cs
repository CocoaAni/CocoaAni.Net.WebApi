using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CocoaAni.Net.WebApi.Exceptions;

public class WebApiResultException:WebApiException
{
    public WebApiResultException()
    {
    }

    protected WebApiResultException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public WebApiResultException(string? message) : base(message)
    {
    }

    public WebApiResultException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}