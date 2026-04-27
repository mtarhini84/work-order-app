namespace WorkOrderApp.Helpers.AST
{
	public interface IDataStoreService
	{
		Task<string> CreateEntityAsync(string tableName, IDictionary<string, object> entity);
		Task<string> CreateEntityAsync(string tableName, string partitionKey, IDictionary<string, object> entity);
		Task<bool> UpdateEntityAsync(string tableName, string id, IDictionary<string, object> values);
		Task<bool> UpdateEntityAsync(string tableName, string partitionKey, string id, IDictionary<string, object> values);
		Task DeleteEntityAsync(string tableName, string id);
		Task DeleteEntityAsync(string tableName, string partitionKey, string id);
		Task<IDictionary<string, IDictionary<string, object>>> GetAllAsync(string tableName);
		Task<IDictionary<string, IDictionary<string, object>>> GetAllAsync(string tableName, string partitionKey);
		Task<IDictionary<string, IDictionary<string, object>>> GetByIdsAsync(string tableName, List<string> ids);
		Task<IDictionary<string, IDictionary<string, object>>> GetByIdsAsync(string tableName, string partitionKey, List<string> ids);
		Task<IDictionary<string, object>> GetByIdAsync(string tableName, string id);
		Task<IDictionary<string, object>> GetByIdAsync(string tableName, string partitionKey, string id);
		Task<IDictionary<string, IDictionary<string, object>>> SearchEntitiesAsync(string tableName, List<FilterGroup> filterGroups);
		Task<IDictionary<string, IDictionary<string, object>>> SearchEntitiesAsync(string tableName, string partitionKey, List<FilterGroup> filterGroups);
		Task<List<IDictionary<string, object>>> GetByFieldAsync(string tableName, string fieldName, object value);
		Task<List<IDictionary<string, object>>> GetByFieldAsync(string tableName, string partitionKey, string fieldName, object value);
		Task<List<IDictionary<string, object>>> GetByDate(string tableName, string startDate, string endDate);
		Task<List<IDictionary<string, object>>> GetByDate(string tableName, string partitionKey, string startDate, string endDate);
	}
}
