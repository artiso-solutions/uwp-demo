using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Linq;
using System.Threading.Tasks;

namespace MyWhiteboard.Service.Controllers
{
    public class StorageAccess
    {
        private readonly CloudTable backgroundsTable;

        public StorageAccess()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            var tableClient = storageAccount.CreateCloudTableClient();
            backgroundsTable = tableClient.GetTableReference("Backgrounds");
        }

        public async Task SaveCurrentBackgroundAsync(BackgroundImageDescription imageDescription)
        {
            var existingEntity = LoadCurrentBackgroundImageEntity();

            if (existingEntity != null)
            {
                if (existingEntity.RowKey == imageDescription.Id.ToString())
                {
                    return;
                }

                var deleteOperation = TableOperation.Delete(existingEntity);
                await backgroundsTable.ExecuteAsync(deleteOperation);
            }

            var newEntity = new BackgroundImageEntity(imageDescription);
            var insertOperation = TableOperation.Insert(newEntity);

            await backgroundsTable.ExecuteAsync(insertOperation);
        }

        public BackgroundImageDescription LoadCurrentBackgroundImageDescription()
        {
            return LoadCurrentBackgroundImageEntity()?.ToBackgroundImageDescription();
        }

        private BackgroundImageEntity LoadCurrentBackgroundImageEntity()
        {
            var rangeQuery = new TableQuery<BackgroundImageEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, BackgroundImageEntity.AnonymousPartitionKey));

            var existingEntity = backgroundsTable.ExecuteQuery(rangeQuery).FirstOrDefault();
            return existingEntity;
        }
    }
}