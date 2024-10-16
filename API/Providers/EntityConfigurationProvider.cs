﻿using API.Db;
using API.Models;

namespace API.Providers
{
    public sealed class EntityConfigurationProvider
        : ConfigurationProvider
    {
        public override void Load()
        {
            using var dbContext = new SettingsContext();

            dbContext.Database.EnsureCreated();

            Data = dbContext.Settings.Any()
                ? dbContext.Settings.ToDictionary(
                    static c => c.Id,
                    static c => c.Value, StringComparer.OrdinalIgnoreCase)
                : CreateAndSaveDefaultValues(dbContext);
        }

        static Dictionary<string, string?> CreateAndSaveDefaultValues(
            SettingsContext context)
        {
            var settings = new Dictionary<string, string?>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["ServerOptions:InboundAddress"] = "https://localhost",
                ["ServerOptions:InboundPort"] = "7185",
            };

            context.Settings.AddRange(
                [.. settings.Select(static kvp => new Settings(kvp.Key, kvp.Value))]);

            context.SaveChanges();

            return settings;
        }
    }
}
