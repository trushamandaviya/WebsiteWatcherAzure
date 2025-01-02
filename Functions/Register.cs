using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Data.SqlClient;

namespace WebsiteWatcher.Functions;

public class Register(ILogger<Register> logger)
{
    [Function(nameof(Register))]
    [SqlOutput("dbo.Websites", "WebsiteWatcher")]
    public async Task<Website> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        try
        {
            logger.LogInformation("New registration has begin");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            Website newWebsite = JsonSerializer.Deserialize<Website>(requestBody, options);
            newWebsite.Id = Guid.NewGuid();
            return newWebsite;
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }

    public class Website
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public string? XPathExpression { get; set; }
    }
}

