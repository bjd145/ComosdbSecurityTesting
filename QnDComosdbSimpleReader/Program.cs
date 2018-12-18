using System;
using System.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace cosmosdb
{
    class Program
    {
        static void Main(string[] args)
        {
            var endpointUrl = "";
            var key = "";
            var databaseName = "ToDoList";
            var databaseCollection = "Items";

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
    }
	public class Item
	{
		public string Id { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public bool Completed { get; set; }
	}
}