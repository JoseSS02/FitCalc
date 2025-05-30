using FitCalc.Models;
using MySqlConnector;
using System.Data;

namespace FitCalc.Services;

public class DatabaseService
{
    private readonly string connectionString = "server=localhost;user=root;password=usuario;database=fitcalc;";

    public async Task InsertarUsuarioAsync(Usuario usuario)
    {
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        string query = "INSERT INTO usuarios (nombre, correo, clave, caloriasnecesarias, proteinasnecesarias, grasasnecesarias, hidratosnecesarios) VALUES (@nombre, @correo, @clave, @caloriasnecesarias, @proteinasnecesarias, " +
            "@grasasnecesarias, @hidratosnecesarios)";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@nombre", usuario.Nombre);
        command.Parameters.AddWithValue("@correo", usuario.Correo);
        command.Parameters.AddWithValue("@clave", usuario.Clave);
        command.Parameters.AddWithValue("@caloriasnecesarias", 2000);
        command.Parameters.AddWithValue("@proteinasnecesarias", 100);
        command.Parameters.AddWithValue("@grasasnecesarias", 67);
        command.Parameters.AddWithValue("@hidratosnecesarios", 250);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> ValidarUsuarioAsync(string nombre, string clave)
    {
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        var query = "SELECT COUNT(*) FROM usuarios WHERE nombre = @nombre AND clave = @clave";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@nombre", nombre);
        command.Parameters.AddWithValue("@clave", clave);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    public async Task<bool> ExisteUsuarioAsync(string nombreUsuario)
    {
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        var query = "SELECT COUNT(*) FROM usuarios WHERE nombre = @nombre";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@nombre", nombreUsuario);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    //LEER ALIMENTOS 
    public async Task<List<Alimento>> ObtenerAlimentosAsync()
    {
        var alimentos = new List<Alimento>();

        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand("SELECT * FROM alimentos", connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            alimentos.Add(new Alimento
            {
                Id = reader.GetInt32("id"),
                Nombre = reader.GetString("nombre"),
                Kcal = reader.GetInt32("kcal"),
                Grasas = reader.GetFloat("grasas"),
                Hidratos = reader.GetFloat("hidratos"),
                Proteinas = reader.GetFloat("proteinas")
            });
        }

        return alimentos;
    }

    //INGRESAR ALIMENTO

    public async Task InsertarAlimentoAsync(Alimento alimento)
    {
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand(@"
        INSERT INTO alimentos (nombre, kcal, grasas, hidratos, proteinas)
        VALUES (@nombre, @kcal, @grasas, @hidratos, @proteinas)", connection);

        command.Parameters.AddWithValue("@nombre", alimento.Nombre);
        command.Parameters.AddWithValue("@kcal", alimento.Kcal);
        command.Parameters.AddWithValue("@grasas", alimento.Grasas);
        command.Parameters.AddWithValue("@hidratos", alimento.Hidratos);
        command.Parameters.AddWithValue("@proteinas", alimento.Proteinas);

        await command.ExecuteNonQueryAsync();
    }


    public async Task<Usuario?> ObtenerUsuarioAsync(string nombre)
    {
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand("SELECT * FROM usuarios WHERE nombre = @nombre", connection);
        command.Parameters.AddWithValue("@nombre", nombre);

        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Usuario
            {
                Id = reader.GetInt32("id"),
                Nombre = reader.GetString("nombre"),
                Correo = reader.GetString("correo"),
                Clave = reader.GetString("clave"),
                CaloriasNecesarias = reader.IsDBNull("caloriasnecesarias") ? null : reader.GetInt32("caloriasnecesarias"),
                ProteinasNecesarias = reader.IsDBNull("proteinasnecesarias") ? null : reader.GetInt32("proteinasnecesarias"),
                GrasasNecesarias = reader.IsDBNull("grasasnecesarias") ? null : reader.GetInt32("grasasnecesarias"),
                HidratosNecesarios = reader.IsDBNull("hidratosnecesarios") ? null : reader.GetInt32("hidratosnecesarios")
            };
        }

        return null;
    }

    public async Task<Usuario?> ObtenerUsuarioPorNombreAsync(string nombre)
    {
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand("SELECT * FROM usuarios WHERE nombre = @nombre", connection);
        command.Parameters.AddWithValue("@nombre", nombre);

        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Usuario
            {
                Id = reader.GetInt32("id"),
                Nombre = reader.GetString("nombre"),
                Correo = reader.GetString("correo"),
                Clave = reader.GetString("clave"),
                CaloriasNecesarias = reader.IsDBNull(reader.GetOrdinal("caloriasnecesarias"))
                    ? null : reader.GetInt32("caloriasnecesarias"),
                ProteinasNecesarias = reader.IsDBNull(reader.GetOrdinal("proteinasnecesarias"))
                    ? null : reader.GetInt32("proteinasnecesarias"),
                GrasasNecesarias = reader.IsDBNull(reader.GetOrdinal("grasasnecesarias"))
                    ? null : reader.GetInt32("grasasnecesarias"),
                HidratosNecesarios = reader.IsDBNull(reader.GetOrdinal("hidratosnecesarios"))
                    ? null : reader.GetInt32("hidratosnecesarios"),
            };
        }

        return null;
    }





    //LEER MACROS

    public async Task<Macronutrientes?> ObtenerMacronutrientesAsync(string usuario)
    {
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        var command = new MySqlCommand("SELECT * FROM macronutrientes WHERE usuario = @usuario", connection);
        command.Parameters.AddWithValue("@usuario", usuario);

        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Macronutrientes
            {
                Usuario = reader.GetString("usuario"),
                Calorias = reader.GetString("calorias"),
                Grasas = reader.GetFloat("grasas"),
                Hidratos = reader.GetFloat("hidratos"),
                Proteinas = reader.GetFloat("proteinas")
            };
        }

        return null;
    }


}
