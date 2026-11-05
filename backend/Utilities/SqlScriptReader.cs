using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data.Common;
using System.Text.RegularExpressions;
namespace Api;
public class SqlScriptReader
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private string _connectionString = string.Empty;
    private readonly string FileScriptName= "database.sql";
    private readonly string DatabaseName = "VirtualShop";

    public SqlScriptReader(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
        _connectionString = GetConnectionString();
    }
    public string GetConnectionString()
    {
        try
        {
            string conn = _configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
            "No se encontró la cadena de conexión 'DefaultConnection'.");
            return conn;
        }
        catch (Exception ex)
        {
            throw new Exception("Error en la cadena de conexión", ex);
        }
    }
    public async Task<bool> CheckExistsDatabaseAsync()
    {
        // Esta conexión debe apuntar a master, no a VirtualShop.
        var builder = new SqlConnectionStringBuilder(_connectionString)
        {
            InitialCatalog = "master"
        };
        await using var cnn = new SqlConnection(builder.ConnectionString);
        await cnn.OpenAsync();

        await using var check = new SqlCommand(
            "SELECT CASE WHEN DB_ID(@name) IS NULL THEN 0 ELSE 1 END;",
            cnn);

        check.Parameters.AddWithValue("@name", DatabaseName);

        var exists = Convert.ToInt32(await check.ExecuteScalarAsync()) == 1;

        if (exists)
        {
            throw new Exception("La base de datos VirtualShop ya existe.");
        }
        else
        {
            // Esta conexión debe apuntar a master, no a VirtualShop.
            _connectionString = builder.ConnectionString;
            exists = true;
        }
        return exists;

    }

    public async Task<string> CheckWebRootPath()
    {
        try
        {
            bool exists = await CheckExistsDatabaseAsync();
            if (exists)
            {
                var WebRootPath = _environment.WebRootPath
                        ?? throw new InvalidOperationException("No se encontró wwwroot.");

                var rute = Path.Combine(WebRootPath, FileScriptName);

                if (!File.Exists(rute))
                    throw new FileNotFoundException("No se encontró el script SQL.", rute);

                return rute;
            }
            else return "Error al verificar Carpeta WWWROOT";
        }
        catch (Exception ex)
        {
            throw new Exception("Error al leer el archivo SQL");
        }
    }

    public async Task<bool> ReadSqlScriptAsync()
    {
        try
        {
            string rute = await CheckWebRootPath();
            string script = await File.ReadAllTextAsync(rute);
            await using var cnn = new SqlConnection(_connectionString);
            await cnn.OpenAsync();

            var lotes = Regex.Split(script, @"^\s*GO\s*(?:--.*)?$",
                RegexOptions.IgnoreCase | RegexOptions.Multiline);

            foreach (var lote in lotes)
            {
                if (string.IsNullOrWhiteSpace(lote))
                    continue;

                await using SqlCommand command = new SqlCommand(lote, cnn)
                {
                    CommandTimeout = 120
                };

                await command.ExecuteNonQueryAsync();
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Error al leer los lotes del archivo SQL");
        }
    }
}
