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

    public async Task SumarMacronutrientesAUsuario(string usuario, Alimento consumo)
    {
        try
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            var fechaActual = DateTime.Today;

            // Comprobar si ya existe un registro para ese usuario y día
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"
            SELECT Calorias, Grasas, Hidratos, Proteinas 
            FROM macronutrientes 
            WHERE Usuario = @usuario AND Dia = @dia";
            selectCmd.Parameters.AddWithValue("@usuario", usuario);
            selectCmd.Parameters.AddWithValue("@dia", fechaActual);

            using var reader = await selectCmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                // Si existe, sumar a los valores actuales
                float caloriasActuales = reader.GetFloat(0);
                float grasas = reader.GetFloat(1);
                float hidratos = reader.GetFloat(2);
                float proteinas = reader.GetFloat(3);

                float nuevasCalorias = caloriasActuales + consumo.Kcal;
                float nuevasGrasas = grasas + consumo.Grasas;
                float nuevosHidratos = hidratos + consumo.Hidratos;
                float nuevasProteinas = proteinas + consumo.Proteinas;

                await reader.CloseAsync();

                var updateCmd = connection.CreateCommand();
                updateCmd.CommandText = @"
                UPDATE macronutrientes 
                SET Calorias = @calorias, Grasas = @grasas, Hidratos = @hidratos, Proteinas = @proteinas
                WHERE Usuario = @usuario AND Dia = @dia";
                updateCmd.Parameters.AddWithValue("@calorias", nuevasCalorias);
                updateCmd.Parameters.AddWithValue("@grasas", nuevasGrasas);
                updateCmd.Parameters.AddWithValue("@hidratos", nuevosHidratos);
                updateCmd.Parameters.AddWithValue("@proteinas", nuevasProteinas);
                updateCmd.Parameters.AddWithValue("@usuario", usuario);
                updateCmd.Parameters.AddWithValue("@dia", fechaActual);

                await updateCmd.ExecuteNonQueryAsync();

                Console.WriteLine("✅ Macronutrientes actualizados correctamente.");
            }
            else
            {
                await reader.CloseAsync();

                // Si no existe, insertar un nuevo registro
                var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = @"
                INSERT INTO macronutrientes (Usuario, Dia, Calorias, Grasas, Hidratos, Proteinas)
                VALUES (@usuario, @dia, @calorias, @grasas, @hidratos, @proteinas)";
                insertCmd.Parameters.AddWithValue("@usuario", usuario);
                insertCmd.Parameters.AddWithValue("@dia", fechaActual);
                insertCmd.Parameters.AddWithValue("@calorias", consumo.Kcal);
                insertCmd.Parameters.AddWithValue("@grasas", consumo.Grasas);
                insertCmd.Parameters.AddWithValue("@hidratos", consumo.Hidratos);
                insertCmd.Parameters.AddWithValue("@proteinas", consumo.Proteinas);

                await insertCmd.ExecuteNonQueryAsync();

                Console.WriteLine("🆕 Nuevo registro de macronutrientes creado para hoy.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error al sumar macronutrientes: {ex.Message}");
        }
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


    public async Task<Macronutrientes?> ObtenerMacronutrientesAsync(string usuario)
    {
        var hoy = DateTime.Today;

        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = @"
        SELECT Calorias, Grasas, Hidratos, Proteinas 
        FROM macronutrientes 
        WHERE Usuario = @usuario AND Dia = @dia";
        selectCmd.Parameters.AddWithValue("@usuario", usuario);
        selectCmd.Parameters.AddWithValue("@dia", hoy);

        using var reader = await selectCmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Macronutrientes
            {
                Usuario = usuario,
                Calorias = reader.GetFloat(0),
                Grasas = reader.GetFloat(1),
                Hidratos = reader.GetFloat(2),
                Proteinas = reader.GetFloat(3),
                Dia = hoy
            };
        }

        await reader.CloseAsync();

        // Si no existe el registro, lo insertamos con valores en cero
        var insertCmd = connection.CreateCommand();
        insertCmd.CommandText = @"
        INSERT INTO macronutrientes (Usuario, Calorias, Grasas, Hidratos, Proteinas, Dia)
        VALUES (@usuario, 0, 0, 0, 0, @dia)";
        insertCmd.Parameters.AddWithValue("@usuario", usuario);
        insertCmd.Parameters.AddWithValue("@dia", hoy);
        await insertCmd.ExecuteNonQueryAsync();

        return new Macronutrientes
        {
            Usuario = usuario,
            Calorias = 0,
            Grasas = 0,
            Hidratos = 0,
            Proteinas = 0,
            Dia = hoy
        };
    }


    public async Task ActualizarMacrosUsuarioAsync(string nombreUsuario, int calorias, float proteinas, float grasas, float hidratos)
    {
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        var query = @"
        UPDATE usuarios 
        SET caloriasnecesarias = @calorias,
            proteinasnecesarias = @proteinas,
            grasasnecesarias = @grasas,
            hidratosnecesarios = @hidratos
        WHERE nombre = @nombreUsuario";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@calorias", calorias);
        command.Parameters.AddWithValue("@proteinas", proteinas);
        command.Parameters.AddWithValue("@grasas", grasas);
        command.Parameters.AddWithValue("@hidratos", hidratos);
        command.Parameters.AddWithValue("@nombreUsuario", nombreUsuario);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<Tip>> ObtenerTipsPorCategoriaAsync(string categoria)
    {
        var tips = new List<Tip>();

        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        var query = "SELECT Id, Categoria, Consejo FROM Tips WHERE Categoria = @Categoria";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@Categoria", categoria);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tips.Add(new Tip
            {
                Id = reader.GetInt32("Id"),
                Categoria = reader.GetString("Categoria"),
                Consejo = reader.GetString("Consejo")
            });
        }

        return tips;
    }




}
