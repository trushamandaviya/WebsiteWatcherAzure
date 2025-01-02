using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using Microsoft.Extensions.Logging;
using static WebsiteWatcher.Functions.Register;
using PuppeteerSharp;
using Azure.Storage.Blobs;
using WebsiteWatcher.Services;

namespace WebsiteWatcher.Functions;

public class PdfCreator(ILogger<PdfCreator> logger, PdfCreatorService pdfCreatorService)
{
    // Visit https://aka.ms/sqltrigger to learn how to use this trigger binding
    [Function("PdfCreator")]
    public async Task Run(
        [SqlTrigger("[dbo].[Websites]", "WebsiteWatcher")] SqlChange<Website>[] changes)
    {
        foreach (var change in changes)
        {
            if (change.Operation == SqlChangeOperation.Insert)
            {
                var result = await pdfCreatorService.ConvertPageToPdfAsync(change.Item.Url);

                var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings:WebsiteWatcherStorage");
                var blobClient = new BlobClient(connectionString, "pdfs", $"{change.Item.Id}.pdf");
                await blobClient.UploadAsync(result);

                logger.LogInformation($"PDF strength is : {result.Length}");
            }
        }
    }
}
