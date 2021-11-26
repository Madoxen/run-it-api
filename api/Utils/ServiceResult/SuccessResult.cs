namespace Api.Utils
{
    public class SuccessServiceResult : ServiceResult
    {
        public SuccessServiceResult()
        {
            Message = "";
        }

        public SuccessServiceResult(string message)
        {
            Message = message;
        }
    }
}