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

public class Mapping
{
    public string Intent { get; set; }
    public string Skill { get; set; }
    public float Theta { get; set; }

    public static string GetQuery(Mapping mapping)
    {
        string gremlinQuery = "g.addV('mapping')";
        gremlinQuery += ".property('intent', '" + mapping.Intent + "')";
        gremlinQuery += ".property('skill', '" + mapping.Skill + "')";
        gremlinQuery += ".property('theta', " + mapping.Theta + ")";
        return gremlinQuery;
    }
}

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

    public static string GetQuery(Person person)
    {
        string gremlinQuery = "g.addV('person')";
        gremlinQuery += ".property('email', '" + person.Email + "')";
        gremlinQuery += ".property('name', '" + person.Name + "')";
        gremlinQuery += ".property('phone', '" + person.Phone + "')";
        gremlinQuery += ".property('allowPush', " + (person.AllowPush ? "true" : "false") + ")";
        gremlinQuery += ".property('shareLocation', " + (person.ShareLocation ? "true" : "false") + ")";
        gremlinQuery += ".property('skills', '" + person.Skills + "')";
        gremlinQuery += ".property('situations', '" + person.Situations + "')";
        gremlinQuery += ".property('latitude', " + person.Latitude + ")";
        gremlinQuery += ".property('longitude', " + person.Longitude + ")";
        return gremlinQuery;
    }
}

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    List<string> mappings = new List<string>();
    List<string> persons = new List<string>();
    string responseMessage = string.Empty;

    log.Info("Creating sample data...");

    // Queries
    mappings.Add(Mapping.GetQuery(new Mapping {
        Intent = "Short of breath",
        Skill = "Cardiac arrest management",
        Theta = 0.587F
    }));
    persons.Add(Person.GetQuery(new Person {
        Name = "Barry Howard",
        Email = "barry.howard@ge.com",
        Phone = "770-519-2683",
        AllowPush = true,
        ShareLocation = true,
        Situations = new string[1] {
            "Medical"
        },
        Skills = new string[1] {
            "Retired Nurse"
        },
        Latitude = 0.0D,
        Longitude = 0.0D
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
        IDocumentQuery<dynamic> query;

        await client.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri("graphdb", "Mappings"));

        DocumentCollection mappingCollection = await client.CreateDocumentCollectionIfNotExistsAsync(
            UriFactory.CreateDatabaseUri("graphdb"),
            new DocumentCollection { Id = "Mappings" },
            new RequestOptions { OfferThroughput = 1000 });

        foreach (string mappingQuery in mappings)
        {
            query = client.CreateGremlinQuery<dynamic>(mappingCollection, mappingQuery);
            while (query.HasMoreResults)
            {
                foreach (dynamic result in await query.ExecuteNextAsync())
                {
                    responseMessage += JsonConvert.SerializeObject(result);
                }
            }
        }

        await client.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri("graphdb", "Persons"));

        DocumentCollection personCollection = await client.CreateDocumentCollectionIfNotExistsAsync(
            UriFactory.CreateDatabaseUri("graphdb"),
            new DocumentCollection { Id = "Persons" },
            new RequestOptions { OfferThroughput = 1000 });

        foreach (string personQuery in persons)
        {
            query = client.CreateGremlinQuery<dynamic>(personCollection, personQuery);
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
