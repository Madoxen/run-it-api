namespace Api.Payloads
{
    public interface IModelPayload<TModel>
    {
        TModel CreateModel();
    }
}