using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Graphs;
using Microsoft.Azure.Graphs.Elements;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net;
using System.Web.Http;
using System.Threading.Tasks;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    string responseMessage = string.Empty;

    log.Info("Creating profile...");

    // Parse request body
    dynamic data = await req.Content.ReadAsAsync<object>();
    string name = data?.name;
    string email = data?.email;
    string phone = data?.phone;
    bool shareLocation = data?.shareLocation;
    bool allowPush = data?.allowPush;
    string[] situations = data?.situations.ToObject<string[]>();
    string[] skills = data?.skills.ToObject<string[]>();
    //string photoURL = data?.photoURL;

    // Create graph vertex
    string authKey = ConfigurationManager.AppSettings["AuthKey"];
    string graphUri = ConfigurationManager.AppSettings["GraphURI"];
    ConnectionPolicy connectionPolicy = new ConnectionPolicy {
        ConnectionMode = ConnectionMode.Direct,
        ConnectionProtocol = Protocol.Tcp
        };

    using (DocumentClient client = new DocumentClient(new Uri(graphUri), authKey, connectionPolicy))
    {
        DocumentCollection graph = await client.CreateDocumentCollectionIfNotExistsAsync(
            UriFactory.CreateDatabaseUri("graphdb"),
            new DocumentCollection { Id = "Persons" },
            new RequestOptions { OfferThroughput = 1000 });

        string gremlinQuery = "g.addV('person')";
        gremlinQuery += ".property('email', '" + email + "')";
        gremlinQuery += ".property('name', '" + name + "')";
        gremlinQuery += ".property('phone', '" + phone + "')";
        gremlinQuery += ".property('shareLocation', " + (shareLocation ? "true" : "false") + ")";
        gremlinQuery += ".property('allowPush', " + (allowPush ? "true" : "false") + ")";
        gremlinQuery += ".property('situations', '" + situations + "')";
        gremlinQuery += ".property('skills', '" + skills + "')";

        IDocumentQuery<dynamic> query = client.CreateGremlinQuery<dynamic>(graph, gremlinQuery);

        while (query.HasMoreResults)
        {
            foreach (dynamic result in await query.ExecuteNextAsync())
            {
                responseMessage += JsonConvert.SerializeObject(result);
            }
        }
    }

    return req.CreateResponse(HttpStatusCode.OK, responseMessage);
}
