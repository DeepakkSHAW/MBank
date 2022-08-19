using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Documents;
using MBank.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Collections.Specialized;

namespace MBank.Lib
{
    public interface IAccountServices
    {
        public Task<bool> AddAccountAsync(AccountEntity accountEntity);
    }
    public class AccountServices : IAccountServices
    {
        //private readonly string _connection = "DefaultEndpointsProtocol=https;AccountName=storagepocfunction;AccountKey=t7LcEs0ZOBHPm/Eb2xqYfMTY6kwxMdlHCRFem7iGLFpJJcBkGp/r6xxjs/YUYqOelCbAcf3onSBYYIwV79A0yQ==;EndpointSuffix=core.windows.net";
        //private readonly string _table = "TMBankAccount";
        
        private readonly CloudTable _stroragetable;
        private readonly ILogger<AccountServices> _logger;
        private readonly IConfiguration _config;
        public AccountServices(ILogger<AccountServices> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            string connection = _config.GetSection("AzureStorage:TableConnectionString").Value;
            string table = _config.GetSection("AzureStorage:Table").Value;
            if (string.IsNullOrEmpty(connection) || string.IsNullOrEmpty(table)) throw new Exception("connection details was not found in configuration file");

            CloudStorageAccount storageAccount;
            storageAccount = CloudStorageAccount.Parse(connection);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            _stroragetable = tableClient.GetTableReference(table);
        }
        public async Task<bool> AddAccountAsync(AccountEntity accountEntity)
        {
            _logger.LogInformation("AddAccountAsync:: Started");
            bool operationStatus = false;
            try
            {
                //Check whether Account already Exist
                TableOperation toFindAccount = TableOperation.Retrieve<AccountEntity>(accountEntity.PartitionKey, accountEntity.RowKey);
                //TableOperation toFindAccount = TableOperation.Retrieve(account.PartitionKey, account.RowKey);
                TableResult trAccountExist = _stroragetable.Execute(toFindAccount);
                if (trAccountExist != null)
                {
                    //TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(accountEntity);
                    TableOperation insertOrMergeOperation = TableOperation.Insert(accountEntity);
                    TableResult result = await _stroragetable.ExecuteAsync(insertOrMergeOperation);
                    var v = result.Result;
                    operationStatus = true;
                }
             }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred {ex.Message}");
//                vResult = false;
            }
            return operationStatus;
        }


        //public async Task<string> AddAccountAsync_TEST()
        //{
        //    CloudStorageAccount storageAccount;
        //    storageAccount = CloudStorageAccount.Parse(_connection);

        //    CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
        //    CloudTable table = tableClient.GetTableReference(_table);

        //    CustomerEntity customer = new CustomerEntity("Harp", "Walter")
        //    {
        //        Email = "Walter@contoso.com",
        //        PhoneNumber = "425-555-0101"
        //    };

        //    TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(customer);
        //    TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
        //    CustomerEntity insertedCustomer = result.Result as CustomerEntity;
        //    Console.WriteLine("Added user.");
        //    return "done";
        //}

        //public class CustomerEntity : TableEntity
        //{
        //    public CustomerEntity() { }
        //    public CustomerEntity(string lastName, string firstName)
        //    {
        //        PartitionKey = lastName;
        //        RowKey = firstName;
        //    }

        //    public string Email { get; set; }
        //    public string PhoneNumber { get; set; }
        //}
    }
}