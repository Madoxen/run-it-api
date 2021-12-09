using Api.Utils;

namespace Api.Services
{
    public abstract class ServiceBase
    {
        protected static NotFoundServiceResult NotFound()
        {
            return new NotFoundServiceResult();
        }

        protected static NotFoundServiceResult NotFound(string message)
        {
            return new NotFoundServiceResult(message);
        }

        protected static SuccessServiceResult Success()
        {
            return new SuccessServiceResult();
        }

        protected static SuccessServiceResult Success(string message)
        {
            return new SuccessServiceResult(message);
        }

        protected static ConflictServiceResult Conflict()
        {
            return new ConflictServiceResult();
        }

        protected static ConflictServiceResult Conflict(string message)
        {
            return new ConflictServiceResult(message);
        }
    }
}