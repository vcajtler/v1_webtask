using Microsoft.Extensions.Configuration;

namespace V1_WebTask.Utilities;

public class TestDataUtility
{
    public DatabaseData? DatabaseData { get; }
    public FormData? FormData { get; }
    public string? BaseUrl { get; }

    public TestDataUtility()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Utilities/TestData.json", optional: false, reloadOnChange: true)
            .Build();

        DatabaseData = configuration.GetSection("TestData:DatabaseData").Get<DatabaseData>()
                       ?? throw new InvalidOperationException("DatabaseData section in TestData is invalid.");
        FormData = configuration.GetSection("TestData:FormData").Get<FormData>()
                   ?? throw new InvalidOperationException("FormData section in TestData is invalid.");
        BaseUrl = configuration.GetSection("TestData:BaseUrl").Value
                  ?? throw new InvalidOperationException("BaseUrl section in TestData is invalid.");

    }
}

public class DatabaseData
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string DatabaseName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
}

public class FormData
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}