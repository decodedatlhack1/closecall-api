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

public class Edge
{
    public string Email1 { get; set; }
    public string Email2 { get; set; }
    public float Weight { get; set; }

    public static string GetQuery(Edge edge)
    {
        string gremlinQuery = "g.V().hasLabel('person').has('email','" + edge.Email1 + "')";
        gremlinQuery += ".addE('distance')";
        gremlinQuery += ".to(g.V().hasLabel('person').has('email','" + edge.Email2 + "'))";
        return gremlinQuery;
    }
}

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
    List<string> edges = new List<string>();
    List<string> mappings = new List<string>();
    List<string> persons = new List<string>();
    string responseMessage = string.Empty;

    log.Info("Creating sample data...");

    // Mappings
    mappings.Add(Mapping.GetQuery(new Mapping {
        Intent = "Fall Or Trauma",
        Skill = "Retired Nurse",
        Theta = 0.95F
    }));
    mappings.Add(Mapping.GetQuery(new Mapping {
        Intent = "Fall Or Trauma",
        Skill = "Doctor",
        Theta = 1.00F
    }));
    mappings.Add(Mapping.GetQuery(new Mapping {
        Intent = "Fall Or Trauma",
        Skill = "Basic Triage",
        Theta = 0.80F
    }));
    mappings.Add(Mapping.GetQuery(new Mapping {
        Intent = "Bleeding",
        Skill = "EMT",
        Theta = 0.75F
    }));
    mappings.Add(Mapping.GetQuery(new Mapping {
        Intent = "Chest Pain",
        Skill = "CPR",
        Theta = 0.85F
    }));
    mappings.Add(Mapping.GetQuery(new Mapping {
        Intent = "Chest Pain",
        Skill = "Retired Nurse",
        Theta = 0.75F
    }));
    mappings.Add(Mapping.GetQuery(new Mapping {
        Intent = "Fall Or Trauma",
        Skill = "Emotional Therapist",
        Theta = 0.15F
    }));

    // Persons
    persons.Add(Person.GetQuery(new Person {
        Name = "Molly Percocet",
        Email = "molly.percocet@live.com",
        Phone = "404-226-7528",
        AllowPush = true,
        ShareLocation = true,
        Skills = new string[1] { "Retired Nurse" },
        Situations = new string[1] { "Medical" },
        Latitude = 33.778959D,
        Longitude = -84.389540D
    }));
    persons.Add(Person.GetQuery(new Person {
        Name = "Kendrick Lamar",
        Email = "humble@live.com",
        Phone = "404-226-7529",
        AllowPush = true,
        ShareLocation = true,
        Skills = new string[1] { "CPR" },
        Situations = new string[1] { "Medical" },
        Latitude = 33.776427D,
        Longitude = -84.391171D
    }));
    persons.Add(Person.GetQuery(new Person {
        Name = "Gucci Mane",
        Email = "guccigucci@live.com",
        Phone = "404-226-7530",
        AllowPush = true,
        ShareLocation = true,
        Skills = new string[1] { "CPR" },
        Situations = new string[1] { "Medical" },
        Latitude = 33.772696D,
        Longitude = -84.386247D
    }));
    persons.Add(Person.GetQuery(new Person {
        Name = "Christopher Bridges",
        Email = "luddddaaaaa@live.com",
        Phone = "404-226-7531",
        AllowPush = true,
        ShareLocation = true,
        Skills = new string[1] { "EMT" },
        Situations = new string[1] { "Medical" },
        Latitude = 33.763583D,
        Longitude = -84.394293D
    }));
    persons.Add(Person.GetQuery(new Person {
        Name = "John Mayer",
        Email = "ilovecountrymusic@live.com",
        Phone = "404-226-7532",
        AllowPush = true,
        ShareLocation = true,
        Skills = new string[1] { "Emotional Therapist" },
        Situations = new string[1] { "Medical" },
        Latitude = 33.791485D,
        Longitude = -84.371095D
    }));
    persons.Add(Person.GetQuery(new Person {
        Name = "Trinidad James",
        Email = "allgoldinmywatch@live.com",
        Phone = "404-226-7533",
        AllowPush = true,
        ShareLocation = true,
        Skills = new string[1] { "Basic Triage" },
        Situations = new string[1] { "Medical" },
        Latitude = 33.775972D,
        Longitude = -84.399886D
    }));
    persons.Add(Person.GetQuery(new Person {
        Name = "Soulja Boy",
        Email = "superman@live.com",
        Phone = "404-226-7534",
        AllowPush = true,
        ShareLocation = true,
        Skills = new string[1] { "Doctor" },
        Situations = new string[1] { "Medical" },
        Latitude = 33.779452D,
        Longitude = -84.384594D
    }));

    // Edges
    Edge edge;
    string[] emails = new string[7] {
        "molly.percocet@live.com",
        "humble@live.com",
        "guccigucci@live.com",
        "luddddaaaaa@live.com",
        "ilovecountrymusic@live.com",
        "allgoldinmywatch@live.com",
        "superman@live.com"
    };
    Random random = new Random();
    for (int i = 0; i < emails.Length; i++)
    {
        for (int j = 0; j < emails.Length; j++)
        {
            edge = new Edge();
            edge.Email1 = emails[i];
            edge.Email2 = emails[j];
            edge.Weight = (float)random.NextDouble();
            edges.Add(Edge.GetQuery(edge));
        }
    }

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

        foreach (string edgeQuery in edges)
        {
            query = client.CreateGremlinQuery<dynamic>(personCollection, edgeQuery);
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
