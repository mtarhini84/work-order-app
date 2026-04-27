using Azure.Data.Tables;
using Azure;
using Microsoft.Extensions.Configuration;
using WorkOrderApp.Helpers.Paging;
using WorkOrderApp.Entities;

namespace WorkOrderApp.Helpers.AST
{
    public class AzureTableService : IAzureTableService
    {
        private TableServiceClient _tableServiceClient;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);

        public AzureTableService(IConfiguration configuration)
        {
            _tableServiceClient = new TableServiceClient(configuration.GetConnectionString("AST"));
        }

        // Existing methods remain the same...
        public async Task<bool> CreateEntity(string tableName, string partitionKey, string rowKey, IDictionary<string, object> properties)
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            await tableClient.CreateIfNotExistsAsync();

            var entity = new TableEntity(partitionKey, rowKey);
            foreach (var prop in properties)
            {
                entity[prop.Key] = prop.Value;
            }

            try
            {
                await tableClient.AddEntityAsync(entity);
                return true;
            }
            catch (Exception e)
            {
                string test = e.Message;
                return false;
            }
        }

        public async Task<bool> UpdateEntity(string tableName, string partitionKey, string rowKey, IDictionary<string, object> propertiesToUpdate)
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            try
            {
                var entity = await tableClient.GetEntityAsync<TableEntity>(partitionKey, rowKey);
                foreach (var prop in propertiesToUpdate)
                {
                    entity.Value[prop.Key] = prop.Value;
                }
                await tableClient.UpdateEntityAsync(entity.Value, ETag.All, TableUpdateMode.Merge);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> CreateEntities(string tableName, string partitionKey, IDictionary<string, IDictionary<string, object>> entitiesProperties)
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            await tableClient.CreateIfNotExistsAsync();

            foreach (var entityProps in entitiesProperties)
            {
                var entity = new TableEntity(partitionKey, entityProps.Key);
                foreach (var prop in entityProps.Value)
                {
                    entity[prop.Key] = prop.Value;
                }

                try
                {
                    await tableClient.AddEntityAsync(entity);
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> DeleteEntity(string tableName, string partitionKey, string rowKey)
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            try
            {
                await tableClient.DeleteEntityAsync(partitionKey, rowKey);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteEntitiesAsync(string tableName, string partitionKey)
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            var entities = tableClient.QueryAsync<TableEntity>(entity => entity.PartitionKey == partitionKey);

            List<Task> deleteTasks = new List<Task>();

            await foreach (var entity in entities)
            {
                deleteTasks.Add(tableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey));
            }

            try
            {
                await Task.WhenAll(deleteTasks);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public async Task<IDictionary<string, object>> GetEntity(string tableName, string partitionKey, string rowKey)
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            try
            {
                var response = await tableClient.GetEntityAsync<TableEntity>(partitionKey, rowKey);
                var tableEntity = response.Value;

                var dictionary = new Dictionary<string, object>();
                foreach (var prop in tableEntity)
                {
                    dictionary[prop.Key] = prop.Value;
                }

                return dictionary;
            }
            catch
            {
                return null;
            }
        }

        public async Task<IDictionary<string, IDictionary<string, object>>> GetEntities(string tableName)
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            try
            {
                var queryResults = tableClient.QueryAsync<TableEntity>();
                var entities = new Dictionary<string, IDictionary<string, object>>();
                await foreach (var entity in queryResults)
                {
                    var propertiesDict = new Dictionary<string, object>();
                    foreach (var prop in entity)
                    {
                        propertiesDict.Add(prop.Key, prop.Value);
                    }
                    propertiesDict.TryAdd("PartitionKey", entity.PartitionKey);
                    propertiesDict.TryAdd("RowKey", entity.RowKey);
                    propertiesDict.TryAdd("Timestamp", entity.Timestamp);

                    entities.Add(entity.RowKey, propertiesDict);
                }
                return entities;
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<T>> GetEntities<T>(string tableName, Func<IDictionary<string, object>, T> mapper)
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            try
            {
                var queryResults = tableClient.QueryAsync<TableEntity>();
                var entities = new List<T>();

                await foreach (var entity in queryResults)
                {
                    var propertiesDict = new Dictionary<string, object>();
                    foreach (var prop in entity)
                    {
                        propertiesDict.Add(prop.Key, prop.Value);
                    }

                    entities.Add(mapper(propertiesDict));
                }
                return entities;
            }
            catch
            {
                return null;
            }
        }

        public async Task<IDictionary<string, IDictionary<string, object>>> GetEntities(string tableName, string partitionKey)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(tableName);
                var queryResults = tableClient.QueryAsync<TableEntity>(entity => entity.PartitionKey == partitionKey);
                var entities = new Dictionary<string, IDictionary<string, object>>();
                await foreach (var entity in queryResults)
                {
                    var propertiesDict = new Dictionary<string, object>();
                    foreach (var prop in entity)
                    {
                        propertiesDict.Add(prop.Key, prop.Value);
                    }
                    propertiesDict.TryAdd("PartitionKey", entity.PartitionKey);
                    propertiesDict.TryAdd("RowKey", entity.RowKey);
                    propertiesDict.TryAdd("Timestamp", entity.Timestamp);

                    entities.Add(entity.RowKey, propertiesDict);
                }
                return entities;
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<T>> GetEntities<T>(string tableName, string partitionKey, DateTime? minDate, Func<IDictionary<string, object>, T> mapper)
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            try
            {
                AsyncPageable<TableEntity> queryResults = null;

                if (minDate != null)
                {
                    queryResults = tableClient.QueryAsync<TableEntity>(entity => entity.PartitionKey == partitionKey && entity.Timestamp > minDate);
                }
                else
                {
                    queryResults = tableClient.QueryAsync<TableEntity>(entity => entity.PartitionKey == partitionKey);
                }

                var entities = new List<T>();

                await foreach (var entity in queryResults)
                {
                    var propertiesDict = new Dictionary<string, object>();
                    foreach (var prop in entity)
                    {
                        propertiesDict.Add(prop.Key, prop.Value);
                    }

                    entities.Add(mapper(propertiesDict));
                }
                return entities;
            }
            catch
            {
                return null;
            }
        }

        public async Task<IDictionary<string, object>> GetEntityByField(string tableName, string partitionKey, string fieldName, string fieldValue, bool isbool = false)
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            string filter;
            if (isbool)
            {
                filter = $"PartitionKey eq '{partitionKey}' and {fieldName} eq {fieldValue}";
            }
            else
            {
                filter = $"PartitionKey eq '{partitionKey}' and {fieldName} eq '{fieldValue}'";
            }

            AsyncPageable<TableEntity> queryResults = tableClient.QueryAsync<TableEntity>(filter);

            var resultList = new List<IDictionary<string, object>>();

            await foreach (TableEntity entity in queryResults)
            {
                resultList.Add(entity);
            }

            return resultList.FirstOrDefault();
        }

        public async Task<IDictionary<string, IDictionary<string, object>>> GetEntitiesByField(
            string tableName, string partitionKey, string fieldName, string fieldValue)
        {
            try
            {
                if (string.IsNullOrEmpty(tableName))
                    throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));

                if (string.IsNullOrEmpty(partitionKey))
                    return new Dictionary<string, IDictionary<string, object>>();

                if (string.IsNullOrEmpty(fieldName))
                    throw new ArgumentException("Field name cannot be null or empty", nameof(fieldName));

                if (string.IsNullOrEmpty(fieldValue))
                    return new Dictionary<string, IDictionary<string, object>>();

                if (fieldName.Contains(" ") || fieldName.Contains("'") || fieldName.Contains("\""))
                    throw new ArgumentException("Invalid field name format", nameof(fieldName));

                var tableClient = _tableServiceClient.GetTableClient(tableName);

                var escapedPartitionKey = partitionKey.Replace("'", "''");
                var escapedFieldValue = fieldValue.Replace("'", "''");

                string filter = $"PartitionKey eq '{escapedPartitionKey}' and {fieldName} eq '{escapedFieldValue}'";

                AsyncPageable<TableEntity> queryResults = tableClient.QueryAsync<TableEntity>(filter);
                var entities = new Dictionary<string, IDictionary<string, object>>();

                await foreach (var entity in queryResults)
                {
                    var propertiesDict = new Dictionary<string, object>();

                    foreach (var prop in entity)
                    {
                        propertiesDict.Add(prop.Key, prop.Value ?? string.Empty);
                    }

                    var key = entity.RowKey ?? Guid.NewGuid().ToString();
                    if (!entities.ContainsKey(key))
                    {
                        entities.Add(key, propertiesDict);
                    }
                    else
                    {
                        entities.Add($"{key}_{Guid.NewGuid().ToString("N")[..8]}", propertiesDict);
                    }
                }

                return entities;
            }
            catch (Azure.RequestFailedException azEx)
            {
                if (azEx.Status == 404)
                {
                    return new Dictionary<string, IDictionary<string, object>>();
                }
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<T>> GetEntitiesByFields<T>(string tableName, IDictionary<string, string> fields, Func<IDictionary<string, object>, T> mapper)
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);

            try
            {
                string filter = null;

                if (fields != null && fields.Any())
                {
                    var filterParts = new List<string>();

                    foreach (var field in fields)
                    {
                        if (string.IsNullOrEmpty(field.Key)) continue;

                        string fieldValue = field.Value ?? "";
                        var escapedValue = fieldValue.Replace("'", "''");

                        filterParts.Add($"{field.Key} eq '{escapedValue}'");
                    }

                    if (filterParts.Any())
                    {
                        filter = string.Join(" and ", filterParts);
                    }
                }

                AsyncPageable<TableEntity> queryResults;

                if (!string.IsNullOrEmpty(filter))
                {
                    queryResults = tableClient.QueryAsync<TableEntity>(filter);
                }
                else
                {
                    queryResults = tableClient.QueryAsync<TableEntity>();
                }

                var entities = new List<T>();

                await foreach (var entity in queryResults)
                {
                    try
                    {
                        var propertiesDict = new Dictionary<string, object>();

                        foreach (var prop in entity)
                        {
                            propertiesDict[prop.Key] = prop.Value ?? string.Empty;
                        }

                        propertiesDict["PartitionKey"] = entity.PartitionKey;
                        propertiesDict["RowKey"] = entity.RowKey;
                        propertiesDict["Timestamp"] = entity.Timestamp;

                        var mappedEntity = mapper(propertiesDict);
                        entities.Add(mappedEntity);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                return entities;
            }
            catch (Exception)
            {
                return new List<T>();
            }
        }

        public async Task<IEnumerable<T>> GetEntitiesByField<T>(string tableName, string? fieldName, string? fieldValue, Func<IDictionary<string, object>, T> mapper)
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            string filter = null;

            if (fieldName != null && fieldValue != null)
            {
                filter = $"{fieldName} eq '{fieldValue}'";
            }

            AsyncPageable<TableEntity> queryResults = tableClient.QueryAsync<TableEntity>(filter);
            var entities = new List<T>();

            await foreach (var entity in queryResults)
            {
                var propertiesDict = new Dictionary<string, object>();
                foreach (var prop in entity)
                {
                    propertiesDict.Add(prop.Key, prop.Value);
                }

                entities.Add(mapper(propertiesDict));
            }
            return entities;
        }

        public async Task<List<IDictionary<string, object>>> GetByDate(string tableName, string startDate, string endDate)
        {
            TableClient tableClient = _tableServiceClient.GetTableClient(tableName);

            string filter = $"CreatedOn ge datetime'{startDate}' and CreatedOn le datetime'{endDate}'";

            AsyncPageable<TableEntity> queryResults = tableClient.QueryAsync<TableEntity>(filter: filter);

            List<IDictionary<string, object>> entities = new List<IDictionary<string, object>>();
            await foreach (var entity in queryResults)
            {
                IDictionary<string, object> entityDict = new Dictionary<string, object>();
                foreach (var prop in entity)
                {
                    entityDict[prop.Key] = prop.Value;
                }
                entities.Add(entityDict);
            }

            return entities;
        }

        public async Task<List<IDictionary<string, object>>> GetByDateField(string tableName, string fieldName, DateTime fieldValue, bool after = true)
        {
            string filter;
            TableClient tableClient = _tableServiceClient.GetTableClient(tableName);

            if (after)
            {
                filter = $"{fieldName} ge datetime'{fieldValue.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ")}'";
            }
            else
            {
                filter = $"{fieldName} le datetime'{fieldValue.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ")}'";
            }

            AsyncPageable<TableEntity> queryResults = tableClient.QueryAsync<TableEntity>(filter: filter);

            List<IDictionary<string, object>> entities = new List<IDictionary<string, object>>();
            await foreach (var entity in queryResults)
            {
                IDictionary<string, object> entityDict = new Dictionary<string, object>();
                foreach (var prop in entity)
                {
                    entityDict[prop.Key] = prop.Value;
                }
                entities.Add(entityDict);
            }

            return entities;
        }

        // FIXED: Corrected pagination implementation
        public async Task<PagedTableResult<IDictionary<string, object>>> GetEntitiesPaginatedAsync(
            string tableName,
            string? partitionKey = null,
            QueryOptions? queryOptions = null)
        {
            return await GetEntitiesPaginated<IDictionary<string, object>>(
                tableName,
                dict => dict,
                partitionKey,
                queryOptions);
        }

        // FIXED: Main pagination method - corrected the logic flow
        public async Task<PagedTableResult<T>> GetEntitiesPaginated<T>(
            string tableName,
            Func<IDictionary<string, object>, T> mapper,
            string? partitionKey = null,
            QueryOptions? queryOptions = null,
            int maxTake = 1000)
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            queryOptions ??= new QueryOptions();

            try
            {
                string filter = BuildOptimizedFilter(partitionKey, queryOptions);

                var takeSize = queryOptions.PageSize + 1;
                var skip = (queryOptions.PageNumber - 1) * queryOptions.PageSize;

                var queryResults = tableClient.QueryAsync<TableEntity>(
                    filter: filter,
                    maxPerPage: Math.Min(maxTake, takeSize + skip)
                );

                var entities = new List<T>();
                var count = 0;

                await foreach (var entity in queryResults)
                {
                    // FIXED: Skip logic was inside wrong condition
                    if (count < skip)
                    {
                        count++;
                        continue;
                    }

                    if (entities.Count >= queryOptions.PageSize + 1)
                        break;

                    var propertiesDict = ConvertTableEntityToDictionary(entity);

                    if (!string.IsNullOrWhiteSpace(queryOptions.SearchTerm) &&
                        queryOptions.SearchColumns?.Length > 0)
                    {
                        if (!MatchesSearch(propertiesDict, queryOptions.SearchTerm, queryOptions.SearchColumns))
                            continue;
                    }

                    entities.Add(mapper(propertiesDict));
                    count++;
                }

                var hasNext = entities.Count > queryOptions.PageSize;
                if (hasNext)
                {
                    entities.RemoveAt(entities.Count - 1);
                }

                var sortedEntities = ApplySorting(entities, queryOptions);

                return new PagedTableResult<T>(
                    sortedEntities.AsReadOnly(),
                    queryOptions.PageNumber,
                    queryOptions.PageSize,
                    hasNext);
            }
            catch (Exception ex)
            {
                return new PagedTableResult<T>(
                    new List<T>().AsReadOnly(),
                    queryOptions.PageNumber,
                    queryOptions.PageSize,
                    false);
            }
        }

        // FIXED: Added missing async suffix for interface compliance
        public async Task<PagedTableResult<T>> GetEntitiesPaginatedAsync<T>(
            string tableName,
            Func<IDictionary<string, object>, T> mapper,
            string? partitionKey = null,
            QueryOptions? queryOptions = null,
            int maxTake = 1000)
        {
            return await GetEntitiesPaginated<T>(tableName, mapper, partitionKey, queryOptions, maxTake);
        }

        public async Task<PagedTableResult<T>> GetEntitiesByFieldsPaginated<T>(
            string tableName,
            IDictionary<string, string> fields,
            Func<IDictionary<string, object>, T> mapper,
            QueryOptions? queryOptions = null)
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            queryOptions ??= new QueryOptions();

            try
            {
                string filter = BuildFieldsFilterOptimized(fields, queryOptions);

                var takeSize = queryOptions.PageSize + 1;
                var skip = (queryOptions.PageNumber - 1) * queryOptions.PageSize;

                var queryResults = tableClient.QueryAsync<TableEntity>(
                    filter: filter,
                    maxPerPage: takeSize + skip
                );

                var entities = new List<T>();
                var count = 0;

                await foreach (var entity in queryResults)
                {
                    if (count < skip)
                    {
                        count++;
                        continue;
                    }

                    if (entities.Count >= queryOptions.PageSize + 1)
                        break;

                    var propertiesDict = ConvertTableEntityToDictionary(entity);

                    if (!string.IsNullOrWhiteSpace(queryOptions.SearchTerm) &&
                        queryOptions.SearchColumns?.Length > 0)
                    {
                        if (!MatchesSearch(propertiesDict, queryOptions.SearchTerm, queryOptions.SearchColumns))
                            continue;
                    }

                    entities.Add(mapper(propertiesDict));
                    count++;
                }

                var hasNext = entities.Count > queryOptions.PageSize;
                if (hasNext)
                {
                    entities.RemoveAt(entities.Count - 1);
                }

                var sortedEntities = ApplySorting(entities, queryOptions);

                return new PagedTableResult<T>(
                    sortedEntities.AsReadOnly(),
                    queryOptions.PageNumber,
                    queryOptions.PageSize,
                    hasNext);
            }
            catch (Exception ex)
            {
                return new PagedTableResult<T>(
                    new List<T>().AsReadOnly(),
                    queryOptions.PageNumber,
                    queryOptions.PageSize,
                    false);
            }
        }

        private string BuildOptimizedFilter(string? partitionKey, QueryOptions queryOptions)
        {
            var filterParts = new List<string>();

            if (!string.IsNullOrEmpty(partitionKey))
            {
                filterParts.Add($"PartitionKey eq '{partitionKey.Replace("'", "''")}'");
            }

            if (queryOptions.Filters?.Any() == true)
            {
                foreach (var filter in queryOptions.Filters)
                {
                    if (filter.Key == "Type" && filter.Value?.Length > 0)
                    {
                        var typeFilters = filter.Value
                            .Where(v => !string.IsNullOrEmpty(v))
                            .Select(v => $"Type eq '{v.Replace("'", "''")}'");

                        if (typeFilters.Any())
                        {
                            filterParts.Add($"({string.Join(" or ", typeFilters)})");
                        }
                    }
                }
            }

            return filterParts.Any() ? string.Join(" and ", filterParts) : null;
        }

        private string BuildFieldsFilterOptimized(IDictionary<string, string> fields, QueryOptions queryOptions)
        {
            var filterParts = new List<string>();

            if (fields?.Any() == true)
            {
                foreach (var field in fields)
                {
                    if (!string.IsNullOrEmpty(field.Key) && !string.IsNullOrEmpty(field.Value))
                    {
                        var escapedValue = field.Value.Replace("'", "''");
                        filterParts.Add($"{field.Key} eq '{escapedValue}'");
                    }
                }
            }

            return filterParts.Any() ? string.Join(" and ", filterParts) : null;
        }

        private Dictionary<string, object> ConvertTableEntityToDictionary(TableEntity entity)
        {
            var propertiesDict = new Dictionary<string, object>();
            foreach (var prop in entity)
            {
                propertiesDict[prop.Key] = prop.Value ?? string.Empty;
            }

            propertiesDict["PartitionKey"] = entity.PartitionKey;
            propertiesDict["RowKey"] = entity.RowKey;
            propertiesDict["Timestamp"] = entity.Timestamp;

            return propertiesDict;
        }

        private bool MatchesSearch(IDictionary<string, object> entity, string searchTerm, string[] searchColumns)
        {
            var lowerSearchTerm = searchTerm.ToLower();

            foreach (var column in searchColumns)
            {
                if (entity.TryGetValue(column, out var value) && value != null)
                {
                    if (value.ToString()?.ToLower().Contains(lowerSearchTerm) == true)
                        return true;
                }
            }

            return false;
        }

        private List<T> ApplySorting<T>(List<T> entities, QueryOptions queryOptions)
        {
            if (string.IsNullOrWhiteSpace(queryOptions.SortBy) || !entities.Any())
                return entities;

            try
            {
                var property = typeof(T).GetProperty(queryOptions.SortBy,
                    System.Reflection.BindingFlags.IgnoreCase |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance);

                if (property == null)
                    return entities;

                if (queryOptions.SortDescending)
                {
                    return entities.OrderByDescending(e => property.GetValue(e)).ToList();
                }
                else
                {
                    return entities.OrderBy(e => property.GetValue(e)).ToList();
                }
            }
            catch
            {
                return entities;
            }
        }
    }
}