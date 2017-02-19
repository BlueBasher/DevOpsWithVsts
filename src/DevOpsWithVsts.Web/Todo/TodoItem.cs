namespace DevOpsWithVsts.Web.Todo
{
    using Microsoft.WindowsAzure.Storage.Table;
    using System;

    public class TodoItem : TableEntity
    {
        public long Id
        {
            get
            {
                long result = 0;
                long.TryParse(RowKey, out result);
                return result;
            }
            set
            {
                RowKey = value.ToString();
            }
        }

        public string UserId
        {
            get
            {
                return PartitionKey;
            }
            set
            {
                PartitionKey = value;
            }
        }

        public string Text { get; set; }

        public bool IsClosed { get; set; }
    }
}