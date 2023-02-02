using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CocoaAni.Net.WebApi.Exceptions;

public class WebApiResultParseException:WebApiException
{

    public WebApiResultParseException()
    {
    }

    protected WebApiResultParseException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public WebApiResultParseException(string? message) : base(message)
    {
    }

    public WebApiResultParseException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}