

using System.Text;

namespace mvcDapper3.AppCodes.AppClass;

public class DynamicQueryBuilder
{
    private readonly StringBuilder _selectPart = new();
    private readonly StringBuilder _fromPart = new();
    private readonly StringBuilder _joinPart = new();
    private readonly StringBuilder _wherePart = new();
    private readonly StringBuilder _groupByPart = new();
    private readonly StringBuilder _havingPart = new();
    private readonly StringBuilder _orderByPart = new();
    private readonly StringBuilder _limitPart = new();
    private readonly Dictionary<string, object> _parameters = new();
    private int _paramCounter = 0;
    private bool _hasFromClause = false;

    public DynamicQueryBuilder Select(string columns)
    {
        _selectPart.Clear();
        _selectPart.Append($"SELECT {columns}");
        return this;
    }

    public DynamicQueryBuilder From(string table)
    {
        _fromPart.Clear();
        _fromPart.Append($" FROM {table}");
        _hasFromClause = true;
        return this;
    }

    public DynamicQueryBuilder Join(string joinType, string table, string condition)
    {
        _joinPart.Append($" {joinType} JOIN {table} ON {condition}");
        return this;
    }

    /// <summary>
    /// Adds a LEFT JOIN clause to the query
    /// </summary>
    /// <param name="table">The table to join</param>
    /// <param name="condition">The join condition</param>
    /// <returns>The DynamicQueryBuilder instance for method chaining</returns>
    public DynamicQueryBuilder LeftJoin(string table, string condition)
    {
        return Join("LEFT", table, condition);
    }

    /// <summary>
    /// Adds an INNER JOIN clause to the query
    /// </summary>
    /// <param name="table">The table to join</param>
    /// <param name="condition">The join condition</param>
    /// <returns>The DynamicQueryBuilder instance for method chaining</returns>
    public DynamicQueryBuilder InnerJoin(string table, string condition)
    {
        return Join("INNER", table, condition);
    }

    /// <summary>
    /// Adds a RIGHT JOIN clause to the query
    /// </summary>
    /// <param name="table">The table to join</param>
    /// <param name="condition">The join condition</param>
    /// <returns>The DynamicQueryBuilder instance for method chaining</returns>
    public DynamicQueryBuilder RightJoin(string table, string condition)
    {
        return Join("RIGHT", table, condition);
    }

    /// <summary>
    /// Adds a FULL JOIN clause to the query
    /// </summary>
    /// <param name="table">The table to join</param>
    /// <param name="condition">The join condition</param>
    /// <returns>The DynamicQueryBuilder instance for method chaining</returns>
    public DynamicQueryBuilder FullJoin(string table, string condition)
    {
        return Join("FULL", table, condition);
    }

    /// <summary>
    /// Adds a CROSS JOIN clause to the query
    /// </summary>
    /// <param name="table">The table to join</param>
    /// <returns>The DynamicQueryBuilder instance for method chaining</returns>
    public DynamicQueryBuilder CrossJoin(string table)
    {
        _joinPart.Append($" CROSS JOIN {table}");
        return this;
    }

    public DynamicQueryBuilder Where(string condition)
    {
        if (_wherePart.Length == 0)
            _wherePart.Append(" WHERE ");
        else
            _wherePart.Append(" AND ");

        _wherePart.Append(condition);
        return this;
    }

    public DynamicQueryBuilder OrderBy(string orderBy)
    {
        _orderByPart.Clear();
        _orderByPart.Append($" ORDER BY {orderBy}");
        return this;
    }

    public DynamicQueryBuilder GroupBy(string groupBy)
    {
        _groupByPart.Clear();
        _groupByPart.Append($" GROUP BY {groupBy}");
        return this;
    }

    public DynamicQueryBuilder Having(string condition)
    {
        _havingPart.Clear();
        _havingPart.Append($" HAVING {condition}");
        return this;
    }

    public DynamicQueryBuilder Limit(int limit, int offset = 0)
    {
        _limitPart.Clear();
        _limitPart.Append($" LIMIT {limit} OFFSET {offset}");
        return this;
    }


    public DynamicQueryBuilder AddParameter(string name, object value)
    {
        _parameters[name] = value;  // Use indexer to handle duplicate keys
        return this;
    }

    // Add methods for auto-parameter generation
    public string CreateParameter(object value)
    {
        string paramName = $"@p{++_paramCounter}";
        _parameters.Add(paramName, value);
        return paramName;
    }

    public string BuildQuery()
    {
        var query = new StringBuilder();

        // Append each part in the correct SQL order
        if (_selectPart.Length > 0)
            query.Append(_selectPart);
        else
            query.Append("SELECT *");

        if (_fromPart.Length > 0)
            query.Append(_fromPart);
        else
            throw new InvalidOperationException("FROM clause is required");

        if (_joinPart.Length > 0)
            query.Append(_joinPart);

        if (_wherePart.Length > 0)
            query.Append(_wherePart);

        if (_groupByPart.Length > 0)
            query.Append(_groupByPart);

        if (_havingPart.Length > 0)
            query.Append(_havingPart);

        if (_orderByPart.Length > 0)
            query.Append(_orderByPart);

        if (_limitPart.Length > 0)
            query.Append(_limitPart);

        return query.ToString().Trim();
    }

    public Dictionary<string, object> GetParameters()
    {
        return _parameters;
    }

    public (string Query, Dictionary<string, object> Parameters) Build()
    {
        if (!_hasFromClause)
            throw new InvalidOperationException("FROM clause is required");

        return (BuildQuery(), _parameters);
    }

}
