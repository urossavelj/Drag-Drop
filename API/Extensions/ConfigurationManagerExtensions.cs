using API.Providers;

namespace API.Extensions
{
    public static class ConfigurationManagerExtensions
    {
        public static ConfigurationManager AddEntityConfiguration(
            this ConfigurationManager manager)
        {
            IConfigurationBuilder configBuilder = manager;
            configBuilder.Add(new EntityConfigurationSource());

            return manager;
        }
    }
}
