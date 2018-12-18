using System;
using System.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using System.Threading.Tasks;

namespace cosmosdb
{
    class Program
    {
        private static DocumentClient client;
        private static DateTime BeginningOfTime = new DateTime(2017, 1, 1);

        static void Main(string[] args)
        {
            var endpointUrl = "";
            var key = "";
            var token = CreateToken(endpointUrl, key)
                .GetAwaiter()
                .GetResult();

            Console.Out.WriteLine($"Token - {token}");
        }

        static private async Task<string> CreateToken(string endpointUrl, string key) 
        {
            var databaseName = "ToDoList";
            var databaseCollection = "Items";

            var userName = "brian001";

            client = new DocumentClient(new Uri(endpointUrl), key);
            var db = await client.GetDatabaseAccountAsync();

            Database database =  await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseName));
            User user = await CreateUserIfNotExistAsync(databaseName, userName);

            Permission docPermission = new Permission
            {
                PermissionMode = PermissionMode.Read,
                ResourceLink = UriFactory.CreateDocumentCollectionUri(databaseName,  databaseCollection).ToString(),
                Id = "readperm"
            };

            var requestOptions = new RequestOptions() { ResourceTokenExpirySeconds = 8*3600 };
            try {
                docPermission = await client.ReadPermissionAsync(UriFactory.CreatePermissionUri( databaseName, userName, docPermission.Id),  requestOptions);
            } 
            catch {
                docPermission = await client.CreatePermissionAsync(UriFactory.CreateUserUri( databaseName, userName ), docPermission, requestOptions);
            }
            
            return docPermission.Token.ToString();
        }

        static private async Task<User> CreateUserIfNotExistAsync(string databaseId, string userId )
        {
            try
            {
                return await client.ReadUserAsync(UriFactory.CreateUserUri(databaseId, userId));
            }
            catch
            {
                var user = await client.CreateUserAsync(UriFactory.CreateDatabaseUri(databaseId), new User { Id = userId });
                return user;
            }
        }
    }
}