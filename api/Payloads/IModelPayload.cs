namespace Api.Payloads
{
    public interface IModelPayload<TModel>
    {
        void ApplyToModel(TModel model);        
    }
}