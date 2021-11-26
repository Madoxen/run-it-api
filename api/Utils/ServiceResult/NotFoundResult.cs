namespace Api.Utils
{
    public class NotFoundServiceResult : ServiceResult
    {
        public NotFoundServiceResult()
        {
            Message = "";
        }

        public NotFoundServiceResult(string message)
        {
            Message = message;
        }
    }
}