using Microsoft.Extensions.Configuration;

public static class ConfigurationHelper {
    public static IConfiguration LoadConfiguration() {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        return config;
    }
}
