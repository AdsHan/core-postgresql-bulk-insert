namespace PerformanceBulkInsert.Strategy;

public class EntityStrategy
{
    public IEntityStrategy SetEntityStrategy(EntityStrategyType type, string schema, string connectionString)
    {
        IEntityStrategy entityStrategy;
        switch (type)
        {
            case EntityStrategyType.Product:
                entityStrategy = new ProductStrategy(schema, connectionString);
                break;

            default:
                entityStrategy = new ProductStrategy(schema, connectionString);
                break;
        }
        return entityStrategy;
    }
}

public enum EntityStrategyType
{
    Product = 0,
}


