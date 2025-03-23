using System.Text.RegularExpressions;

namespace QuickLogger.Infrastructure.Utils;

public static class MySqlUtil
{
    public static bool TryMySqlUrlToConnectionString(string mysqlUrl,out string connectionString )
    {
        var pattern = @"mysql://(?<user>[^:]+):(?<password>[^@]+)@(?<host>[^:]+):(?<port>\d+)/(?<database>[^?]+)(\?ssl-mode)?";
        var match = Regex.Match(mysqlUrl, pattern);

        if (!match.Success)
        {
            connectionString = "";
            return false;
        }


        connectionString = $"Server={match.Groups["host"].Value};" +
               $"Port={match.Groups["port"].Value};" +
               $"Database={match.Groups["database"].Value};" +
               $"User Id={match.Groups["user"].Value};" +
               $"Password={match.Groups["password"].Value};" +
               $"SslMode=Required;";
        return true;
    }
}
