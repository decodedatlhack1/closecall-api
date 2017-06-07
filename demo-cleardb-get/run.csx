using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Graphs;
using Microsoft.Azure.Graphs.Elements;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Web.Http;
using System.Threading.Tasks;

public class Person
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public bool AllowPush { get; set; }
    public bool ShareLocation { get; set; }
    public string[] Skills { get; set; }
    public string[] Situations { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public static string GetCreatePersonQuery(Person person)
{
    string gremlinQuery = "g.addV('person')";
    gremlinQuery += ".property('email', '" + person.Email + "')";
    gremlinQuery += ".property('name', '" + person.Name + "')";
    gremlinQuery += ".property('phone', '" + person.Phone + "')";
    gremlinQuery += ".property('allowPush', " + (person.AllowPush ? "true" : "false") + ")";
    gremlinQuery += ".property('shareLocation', " + (person.shareLocation ? "true" : "false") + ")";
    gremlinQuery += ".property('skills', '" + person.Skills + "')";
    gremlinQuery += ".property('situations', '" + person.Situations + "')";
    gremlinQuery += ".property('situations', " + person.Latitude + ")";
    gremlinQuery += ".property('situations', " + person.Longitude + ")";
}

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    List<string> queries = new List<string>();
    string responseMessage = string.Empty;

    log.Info("Creating sample data...");

    // Queries
    queries.Add("g.V.remove()");
    queries.Add(GetCreatePersonQuery(new Person {
        Name = "Barry Howard",
        Email = "barry.howard@ge.com",
        Phone = "770-519-2683",
        AllowPush = true,
        ShareLocation = true,
        Skills = new string[1] {
            "A skill"
        },
        Situations = new string[1] {
            "A situation"
        }
    }));

    // Create graph vertex
    string authKey = ConfigurationManager.AppSettings["AuthKey"];
    string graphUri = ConfigurationManager.AppSettings["GraphURI"];

    using (DocumentClient client = new DocumentClient(
        new Uri(graphUri),
        authKey,
        new ConnectionPolicy {
            ConnectionMode = ConnectionMode.Direct,
            ConnectionProtocol = Protocol.Tcp
            }))
    {
        DocumentCollection graph = await client.CreateDocumentCollectionIfNotExistsAsync(
            UriFactory.CreateDatabaseUri("graphdb"),
            new DocumentCollection { Id = "Persons" },
            new RequestOptions { OfferThroughput = 1000 });

        foreach (string gremlinQuery in queries)
        {
            IDocumentQuery<dynamic> query = client.CreateGremlinQuery<dynamic>(graph, gremlinQuery);
            while (query.HasMoreResults)
            {
                foreach (dynamic result in await query.ExecuteNextAsync())
                {
                    responseMessage += JsonConvert.SerializeObject(result);
                }
            }
        }
    }

    return req.CreateResponse(HttpStatusCode.OK, responseMessage);
}
