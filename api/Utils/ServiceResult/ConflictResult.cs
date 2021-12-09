namespace Api.Utils
{
    public class ConflictServiceResult : ServiceResult
    {
        public ConflictServiceResult()
        {
            Message = "";
        }

        public ConflictServiceResult(string message)
        {
            Message = message;
        }
    }
}