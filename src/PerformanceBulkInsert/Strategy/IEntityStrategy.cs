using PerformanceBulkInsert.Data;

namespace PerformanceBulkInsert.Strategy;

public interface IEntityStrategy
{
    Task InsertTemporaryTableAsync();
    Task<ResultMergeFunction> MergeMainTableAsync();
    Task CreateFunctionDBAsync();
    Task DropTemporaryTableAsync();
    Task CreateTemporaryTableAsync();
}
