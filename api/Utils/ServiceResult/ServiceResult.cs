using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Api.Utils
{
    public abstract class ServiceResult
    {
        public string Message { get; set; }

        public static implicit operator ActionResult(ServiceResult result)
        {
            if (result is SuccessServiceResult)
            {
                if (!string.IsNullOrEmpty(result.Message))
                    return new OkObjectResult(result.Message);
                return new OkResult();
            }
            else if (result is NotFoundServiceResult)
            {
                if (!string.IsNullOrEmpty(result.Message))
                    return new NotFoundObjectResult(result.Message);
                return new NotFoundResult();
            }

            throw new InvalidCastException($"Could not cast underlaying result {result.GetType()} to ActionResult<T>");
        }

    }

    public sealed class ServiceResult<T>
    {
        public T Value { get; set; }
        public ServiceResult Result { get; set; }
        public static implicit operator ServiceResult<T>(ServiceResult result)
        {
            return new ServiceResult<T>()
            {
                Result = result
            };
        }

        public static implicit operator ServiceResult<T>(T value)
        {
            return new ServiceResult<T>()
            {
                Value = value
            };
        }

        public static implicit operator ActionResult<T>(ServiceResult<T> result)
        {
            if (result.Value != null)
                return new ActionResult<T>(result.Value);
            return (ActionResult)result.Result;
        }
    }
}

