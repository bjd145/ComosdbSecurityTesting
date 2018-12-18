using System;
using System.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using System.Threading.Tasks;
using System.Threading;

namespace cosmosdb
{
    class Program
    {
        private static readonly string endpointUrl = "";
        private static readonly string databaseName = "ToDoList";
        private static readonly string databaseCollection = "Items";

        static void Main(string[] args)
        {
            var masterKeyUri = "";
            var tokenUri = "";

            Console.Out.WriteLine("----------------------------------");
            Console.Out.WriteLine("Using Master Key - Write New Entry");
            WriteNewItem(masterKeyUri);
            Console.Out.WriteLine("----------------------------------");
            Thread.Sleep(1000);

            Console.Out.WriteLine("----------------------------------");
            Console.Out.WriteLine("Using Master Key - Query Entries");
            QueryCosmos(masterKeyUri);
            Console.Out.WriteLine("----------------------------------");
            Thread.Sleep(1000);

            Console.Out.WriteLine("----------------------------------");
            Console.Out.WriteLine("Using Token - Write New Entry");
            WriteNewItem(tokenUri);
            Console.Out.WriteLine("----------------------------------");
            Thread.Sleep(1000);

            Console.Out.WriteLine("----------------------------------");
            Console.Out.WriteLine("Using Token - Query Entries");
            QueryCosmos(tokenUri);
            Console.Out.WriteLine("----------------------------------");
            Thread.Sleep(1000);
        }

        static private void QueryCosmos(string vaultUri) 
        {
            var key = GetCosmosMasterKey(vaultUri)
                .GetAwaiter()
                .GetResult();

            using (var client = new DocumentClient(new Uri(endpointUrl), key, new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct, ConnectionProtocol = Protocol.Https }))
            {
                var query = client.CreateDocumentQuery<Item>(
                    UriFactory.CreateDocumentCollectionUri(databaseName, databaseCollection))
                        .ToList();

                foreach (var i in query)
                {
                    Console.WriteLine($"Url - {i.name} {i.description}");
                }
            }
        }

        static private async void WriteNewItem(string vaultUri)
        {
            var key = GetCosmosMasterKey(vaultUri)
                .GetAwaiter()
                .GetResult();

            var client = new DocumentClient(new Uri(endpointUrl), key);
            var collectionLink = UriFactory.CreateDocumentCollectionUri(databaseName, databaseCollection);

            var i = new Item
            {
                id = Guid.NewGuid().ToString(),
                name = $"Update Permissions - {DateTime.Now}",
                description = "What is the outcome",
                isComplete = false
            };

            try {
                Document results = await client.CreateDocumentAsync(collectionLink, i);
                Console.Out.WriteLine($"Write successed with {results.Id}");
            } 
            catch (DocumentClientException e ) {
                Console.Out.WriteLine($"Failed in Write with {e.Message}");
            }
            catch {
                Console.Out.WriteLine("General Failed in Write");    
            }
        }

        static private async Task<string> GetCosmosMasterKey(string vaultUri)
        {
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(
                new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            var secret = await keyVaultClient.GetSecretAsync(vaultUri)
                .ConfigureAwait(false);
            return secret.Value;
        }
    }

	public class Item
	{
		public string id { get; set; }
		public string name { get; set; }
		public string description { get; set; }
		public bool isComplete { get; set; }
	}
}