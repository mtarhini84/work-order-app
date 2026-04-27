using WorkOrderApp.Helpers.Paging;

namespace WorkOrderApp.Helpers.AST
{
	public interface IAzureTableService
	{
		Task<bool> CreateEntity(string tableName, string partitionKey, string rowKey, IDictionary<string, object> properties);
		Task<bool> UpdateEntity(string tableName, string partitionKey, string rowKey, IDictionary<string, object> propertiesToUpdate);
		Task<bool> CreateEntities(string tableName, string partitionKey, IDictionary<string, IDictionary<string, object>> entitiesProperties);
		Task<bool> DeleteEntity(string tableName, string partitionKey, string rowKey);
		Task<bool> DeleteEntitiesAsync(string tableName, string partitionKey);
		Task<IDictionary<string, object>> GetEntity(string tableName, string partitionKey, string rowKey);
		Task<IDictionary<string, IDictionary<string, object>>> GetEntities(string tableName);
		Task<IEnumerable<T>> GetEntities<T>(string tableName, Func<IDictionary<string, object>, T> mapper);
		Task<IDictionary<string, IDictionary<string, object>>> GetEntities(string tableName, string partitionKey);
		Task<IEnumerable<T>> GetEntities<T>(string tableName, string partitionKey, DateTime? minDate, Func<IDictionary<string, object>, T> mapper);
		Task<IDictionary<string, object>> GetEntityByField(string tableName, string partitionKey, string fieldName, string fieldValue, bool isbool = false);
		Task<IDictionary<string, IDictionary<string, object>>> GetEntitiesByField(string tableName, string partitionKey, string fieldName, string fieldValue);
		Task<IEnumerable<T>> GetEntitiesByField<T>(string tableName, string? fieldName, string? fieldValue, Func<IDictionary<string, object>, T> mapper);
		Task<IEnumerable<T>> GetEntitiesByFields<T>(string tableName, IDictionary<string, string> fields, Func<IDictionary<string, object>, T> mapper);
		Task<List<IDictionary<string, object>>> GetByDate(string tableName, string startDate, string endDate);
		Task<List<IDictionary<string, object>>> GetByDateField(string tableName, string fieldName, DateTime fieldValue, bool after = true);

		Task<PagedTableResult<IDictionary<string, object>>> GetEntitiesPaginatedAsync(string tableName, string? partitionKey = null, QueryOptions? queryOptions = null);
		Task<PagedTableResult<T>> GetEntitiesPaginated<T>(string tableName, Func<IDictionary<string, object>, T> mapper, string? partitionKey = null, QueryOptions? queryOptions = null, int maxTake = 1000);
		Task<PagedTableResult<T>> GetEntitiesPaginatedAsync<T>(string tableName, Func<IDictionary<string, object>, T> mapper, string? partitionKey = null, QueryOptions? queryOptions = null, int maxTake = 1000);
		Task<PagedTableResult<T>> GetEntitiesByFieldsPaginated<T>(string tableName, IDictionary<string, string> fields, Func<IDictionary<string, object>, T> mapper, QueryOptions? queryOptions = null);
	}
}
