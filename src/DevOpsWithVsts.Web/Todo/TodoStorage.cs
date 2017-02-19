namespace DevOpsWithVsts.Web.Todo
{
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;

    public class TodoStorage : ITodoStorage
    {
        private readonly string tableName = typeof(TodoItem).Name;
        private readonly CloudTable table;

        public TodoStorage(string storageConnectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            this.table = tableClient.GetTableReference(tableName);

            // Create the table if it doesn't exist.
            table.CreateIfNotExists();
        }

        /// <summary>
        /// Inserts the specified item in the table.
        /// </summary>
        /// <returns>
        /// a boolean indiciating if the insert succeeded
        /// </returns>
        public Task<bool> InsertAsync(string userId, string text, bool isClosed)
        {
            var insertOperation = TableOperation.Insert(new TodoItem
            {
                Id = DateTimeOffset.UtcNow.Ticks,
                UserId = userId,
                Text = text,
                IsClosed = isClosed
            });

            return this.ExecuteOperationAsync(tableName, insertOperation);
        }

        /// <summary>
        /// Updates the specified item in the table.
        /// </summary>
        /// <returns>
        /// a boolean indiciating if the insert succeeded
        /// </returns>
        public Task<bool> UpdateAsync(string userId, long id, string text, bool isClosed)
        {
            var replaceOperation = TableOperation.Replace(new TodoItem
            {
                ETag = "*",
                Id = id,
                UserId = userId,
                Text = text,
                IsClosed = isClosed
            });

            return this.ExecuteOperationAsync(tableName, replaceOperation);
        }

        /// <summary>
        /// Retrieves the item with specified key from the table.
        /// </summary>
        /// <returns>
        /// The entity retrieved from the table storage
        /// </returns>
        public async Task<TodoItem> RetrieveAsync(string userId, long id)
        {
            var retrieveOperation = TableOperation.Retrieve<TodoItem>(userId, id.ToString());
            var retrieveResult = await this.table.ExecuteAsync(retrieveOperation);

            return retrieveResult.Result as TodoItem;
        }

        /// <summary>
        /// Retrieves all items from the table based on query
        /// </summary>
        /// <returns>
        /// The entities retrieved from the table storage
        /// </returns>
        public async Task<IEnumerable<TodoItem>> RetrieveAsync(string userId)
        {
            var list = new List<TodoItem>(3000);
            var tableQuery = new TableQuery<TodoItem>().Where(TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal,
                    userId
                ));
            TableContinuationToken continuationToken = null;
            do
            {
                // Retrieve a segment (up to 1000 entities).
                var tableQueryResult = await table.ExecuteQuerySegmentedAsync(tableQuery, continuationToken);
                list.AddRange(tableQueryResult);
                continuationToken = tableQueryResult.ContinuationToken;
            }
            while (continuationToken != null);
            return list;
        }

        /// <summary>
        /// Removes the item with specified key from the table.
        /// </summary>
        /// <returns>
        /// a boolean indiciating if the removal succeeded
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Throws when an item with the specified key is not found in the table with the specified name.</exception>
        public async Task<bool> RemoveAsync(string userId, long id)
        {
            var retrieveOperation = TableOperation.Retrieve<TodoItem>(userId, id.ToString());
            var retrieveResult = await this.table.ExecuteAsync(retrieveOperation);

            var result = retrieveResult.Result as TodoItem;

            if (result == null)
            {
                return true;
            }
            else
            {
                result.ETag = "*";

                var deleteOperation = TableOperation.Delete(result);

                var deleteResult = await table.ExecuteAsync(deleteOperation);

                return deleteResult.HttpStatusCode == (int)HttpStatusCode.OK;
            }
        }

        private async Task<bool> ExecuteOperationAsync(string tableName, TableOperation insertOperation)
        {
            var insertResult = await this.table.ExecuteAsync(insertOperation);
            return insertResult.HttpStatusCode == (int)HttpStatusCode.NoContent;
        }
    }
}