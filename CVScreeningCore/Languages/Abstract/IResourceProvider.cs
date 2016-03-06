namespace CVScreeningCore.Languages.Abstract
{
    public interface IResourceProvider
    {
        object GetResource(string name, string culture);   
    }
}