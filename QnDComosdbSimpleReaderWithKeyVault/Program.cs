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
        static void Main(string[] args)
        {
            var endpointUrl = "";
            var vault = "";

            var databaseName = "ToDoList";
            var databaseCollection = "Items";

            var key = GetCosmosMasterKey(vault)
                .GetAwaiter()
                .GetResult();
                
            using (var client = new DocumentClient(new Uri(endpointUrl), key, new ConnectionPolicy{ConnectionMode = ConnectionMode.Direct, ConnectionProtocol = Protocol.Https})) {
                var query = client.CreateDocumentQuery<Item>(
                    UriFactory.CreateDocumentCollectionUri(databaseName, databaseCollection))
                        .ToList();

                foreach(var i in query) 
                {
                    Console.WriteLine($"Url - {i.Name} {i.Description}"  );  
                }
            }
     
        }

        static private async Task<string> GetCosmosMasterKey(string vaultUri) {
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
		public string Id { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public bool Completed { get; set; }
	}
}