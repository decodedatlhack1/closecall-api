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

public class Person
{
    public string Name { get; set; }
    public string Phone { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("Getting help...");

    Person respondant = new Person {
        Name = "Christopher Bridges",
        Phone = "404-226-7531",
        Latitude = 33.7779679,
        Longitude = -84.391745
    };

    return req.CreateResponse(HttpStatusCode.OK, respondant);
}
