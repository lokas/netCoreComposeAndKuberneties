using System;
using Microsoft.Extensions.Logging;

namespace DemoApi.Controllers
{
    public static class EnvVariable
    {
        public const string Mongo = "MONGO_URI";
        public const string Rabbit = "RABBIT_URI";
        public static string GetValue(string key, ILogger logger)
        {
            var uri = Environment.GetEnvironmentVariable(key);
            logger.LogInformation($"Getting Uri From env: {key}");
            if (string.IsNullOrEmpty(uri))
            {
                logger.LogCritical($"Missing env key:{key}");
                throw new ApplicationException("URI -> NOT SET");
            }

            return uri;
        }
    }
}