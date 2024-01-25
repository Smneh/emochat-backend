using Core.Enums;
using Core.Extensions;

namespace Core.Exceptions;

public class AppException : Exception
{
    public new Messages Message { get; set; }
    public string MessageEn { get; set; }
    public object? CustomObject { get; set; }

    public AppException(Messages message)
    {
        Message = message;
        MessageEn = message.ToDescription();
    }

    public AppException(Messages message, params object[] parameters)
    {
        MessageEn = string.Format(message.ToDescription(), parameters);
        Message = message;
    }

    public AppException(object customObject, Messages message)
    {
        Message = message;
        MessageEn = message.ToDescription();
        CustomObject = customObject;
    }

    public AppException(object customObject, Messages message, params object[] parameters)
    {
        Message = message;
        MessageEn = string.Format(message.ToDescription(), parameters);
        CustomObject = customObject;
    }
}