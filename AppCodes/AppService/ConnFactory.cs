using mvcDapper3.AppCodes.AppConfig;

namespace mvcDapper3.AppCodes.AppService;

public class ConnFactory : IConnFactory
{
    private readonly string _connStr;

    public ConnFactory(IConfiguration config,
        IWebHostEnvironment env)
    {
        var dbConfig = config
        .GetSection(DatabaseConfig.SectionName)
        .Get<DatabaseConfig>();

        if (dbConfig == null)
        {
            throw new Exception("Database configuration section is missing.");
        }
        _connStr = dbConfig.GetSecureConnectionString(!env.IsDevelopment());
    }

    public IDbConnection CreateConn()
    {
        return new SqlConnection(_connStr);
    }

    public IDbConnection GetOpenConn()
    {
        var conn = CreateConn();
        conn.Open();
        return conn;
    }
}
