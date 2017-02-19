namespace DevOpsWithVsts.Web.Todo
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ITodoStorage
    {
        /// <summary>
        /// Inserts the specified item in the table.
        /// </summary>
        /// <returns>
        /// a boolean indiciating if the insert succeeded
        /// </returns>
        Task<bool> InsertAsync(string userId, string text, bool isClosed);

        /// <summary>
        /// Updates the specified item in the table.
        /// </summary>
        /// <returns>
        /// a boolean indiciating if the insert succeeded
        /// </returns>
        Task<bool> UpdateAsync(string userId, long id, string text, bool isClosed);

        /// <summary>
        /// Removes the item with specified key from the table.
        /// </summary>
        /// <returns>
        /// a boolean indiciating if the removal succeeded
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Throws when an item with the specified key is not found in the table with the specified name.</exception>
        Task<bool> RemoveAsync(string userId, long id);

        /// <summary>
        /// Retrieves all items from the table based on query
        /// </summary>
        /// <returns>
        /// The entities retrieved from the table storage
        /// </returns>
        Task<IEnumerable<TodoItem>> RetrieveAsync(string userId);

        /// <summary>
        /// Retrieves the item with specified key from the table.
        /// </summary>
        /// <returns>
        /// The entity retrieved from the table storage
        /// </returns>
        Task<TodoItem> RetrieveAsync(string userId, long id);
    }
}