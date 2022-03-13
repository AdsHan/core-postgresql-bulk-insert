using Dapper;
using Npgsql;
using NpgsqlTypes;
using PerformanceBulkInsert.Data;
using PgPartner;

namespace PerformanceBulkInsert.Strategy;

public class ProductStrategy : IEntityStrategy
{
    private const string Table = "products";
    private const string TableTemporary = "products_temp";
    private const string FunctionMerge = "product_merge";

    private readonly string _schema;
    private readonly string _connectionString;

    public ProductStrategy(string schema, string connectionString)
    {
        _schema = schema;
        _connectionString = connectionString;
    }

    public async Task InsertTemporaryTableAsync()
    {
        DropTemporaryTableAsync();

        CreateTemporaryTableAsync();

        var products = GetDataToInsert();

        using var dbConnection = new NpgsqlConnection(_connectionString);

        dbConnection.Open();

        await dbConnection.BulkAddAsync(
            products,
            (mapper, row) =>
            {
                mapper.Map("id", row.Id, NpgsqlDbType.Integer);
                mapper.Map("sku", row.Sku, NpgsqlDbType.Varchar);
                mapper.Map("name", row.Name, NpgsqlDbType.Varchar);
                mapper.Map("url", row.Url, NpgsqlDbType.Varchar);
            },
            _schema,
            TableTemporary
        );

        dbConnection.Close();
    }

    public async Task<ResultMergeFunction> MergeMainTableAsync()
    {
        await CreateFunctionDBAsync();

        using var dbConnection = new NpgsqlConnection(_connectionString);

        var result = dbConnection.Query<ResultMergeFunction>
        (
            "product_merge",
            new
            {

            },
            commandType: System.Data.CommandType.StoredProcedure
        );

        return result.FirstOrDefault();

    }

    public async Task CreateFunctionDBAsync()
    {
        using var dbConnection = new NpgsqlConnection(_connectionString);

        dbConnection.Open();

        var query = string.Format(@"
                    CREATE OR REPLACE FUNCTION {0}.{1}()
                    RETURNS TABLE (CodeErro text, MessageErro text)
                    LANGUAGE plpgsql
                    AS $$
                    DECLARE
	                    v_state TEXT;
	                    v_msg TEXT;
	                    v_detail TEXT;
	                    v_hint TEXT;
	                    v_context TEXT;
                    BEGIN
	
	                    WITH origin AS (
		                    SELECT * FROM {0}.{2}
                        )

                        INSERT INTO {0}.{3}
                        SELECT* FROM origin
                        ON CONFLICT(id)
                        DO
                            UPDATE SET			                    
                                sku = EXCLUDED.sku,
                                name = EXCLUDED.name,
                                url = EXCLUDED.url;

                                RETURN QUERY SELECT '0' AS CodeErro,
                                            'Mege foi executado com sucesso!' AS MessageErro;

                        EXCEPTION
                            WHEN OTHERS THEN
                                get stacked diagnostics
                                    v_state = returned_sqlstate,
                                    v_msg = message_text,
                                    v_detail = pg_exception_detail,
                                    v_hint = pg_exception_hint,
                                    v_context = pg_exception_context;

                                RETURN QUERY SELECT
                                    v_state AS CodeErro,
                                    v_msg AS MessageErro;
                                END;
                    $$;", _schema, FunctionMerge, TableTemporary, Table);

        var command = new NpgsqlCommand(query, dbConnection);
        var result = await command.ExecuteReaderAsync();

        dbConnection.Close();
    }

    public async Task DropTemporaryTableAsync()
    {
        using var dbConnection = new NpgsqlConnection(_connectionString);

        dbConnection.Open();

        var query = string.Format(@"DROP TABLE IF EXISTS {0}.{1};", _schema, TableTemporary);
        var command = new NpgsqlCommand(query, dbConnection);
        var result = await command.ExecuteReaderAsync();

        dbConnection.Close();
    }

    public async Task CreateTemporaryTableAsync()
    {
        using var dbConnection = new NpgsqlConnection(_connectionString);

        dbConnection.Open();

        var query = string.Format(@"CREATE TABLE {0}.{1} AS (SELECT * FROM {0}.{2}) WITH NO DATA", _schema, TableTemporary, Table);
        var command = new NpgsqlCommand(query, dbConnection);
        var result = await command.ExecuteReaderAsync();

        dbConnection.Close();
    }

    private static List<ProductModel> GetDataToInsert()
    {
        return Enumerable.Range(1, 1000000).Select(p => new ProductModel
        {
            Id = p,
            Sku = "A1219500010002",
            Name = $"Sandália Amarração - {p}",
            Url = "https://www.google.com.br/imagem.png",
        }).ToList();
    }

}
