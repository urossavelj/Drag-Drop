namespace API.Providers
{
    public sealed class EntityConfigurationSource : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder) =>
            new EntityConfigurationProvider();
    }
}
