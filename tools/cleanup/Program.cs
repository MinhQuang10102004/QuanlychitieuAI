using Microsoft.Data.SqlClient;
using System.Text;

var resultLines = new StringBuilder();
try
{
    var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=QuanLyChiTieuDev;Trusted_Connection=True;TrustServerCertificate=True;");
    await conn.OpenAsync();

    var checkSql = @"SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__EFMigrationsHistory'";
    using (var checkCmd = conn.CreateCommand())
    {
        checkCmd.CommandText = checkSql;
        var exists = (await checkCmd.ExecuteScalarAsync()) != null;
        var line = $"__EFMigrationsHistory exists: {exists}";
        Console.WriteLine(line);
        resultLines.AppendLine(line);
        if (exists)
        {
            using var mcmd = conn.CreateCommand();
            mcmd.CommandText = "SELECT COUNT(*) FROM __EFMigrationsHistory WHERE MigrationId = '20251225062204_AddChatHistory'";
            var mcount = (int)(await mcmd.ExecuteScalarAsync() ?? 0);
            var mline = $"Migration record present: {mcount}";
            Console.WriteLine(mline);
            resultLines.AppendLine(mline);
        }
    }

    var sql = @"DELETE FROM ChatHistories
WHERE AiReply LIKE N'%Có lỗi khi liên hệ trợ lý AI%'
   OR AiReply LIKE N'%AI không phản hồi%'
   OR AiReply LIKE N'%Not signed in%'
   OR AiReply LIKE N'%Lỗi:%'
;
SELECT @@ROWCOUNT;";

    using var cmd = conn.CreateCommand();
    cmd.CommandText = sql;
    var res = await cmd.ExecuteScalarAsync();
    var rline = $"Removed: {res}";
    Console.WriteLine(rline);
    resultLines.AppendLine(rline);
    await conn.CloseAsync();
}
catch (Exception ex)
{
    var err = $"EXCEPTION: {ex.GetType()}: {ex.Message}";
    Console.WriteLine(err);
    resultLines.AppendLine(err);
}

try
{
    var outPath = Path.Combine(AppContext.BaseDirectory, "cleanup_result.txt");
    await File.WriteAllTextAsync(outPath, resultLines.ToString());
}
catch
{
    // ignore file write errors
}
