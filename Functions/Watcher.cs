using System;
using HtmlAgilityPack;
using System.Threading.Channels;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using Azure.Storage.Blobs;
using WebsiteWatcher.Services;

namespace WebsiteWatcher.Functions
{
    public class Watcher(ILogger<Watcher> logger, PdfCreatorService pdfCreatorService)
    {
        private const string SqlInputQuery = @"SELECT w.Id, w.Url, w.XPathExpression , s.Content as LatestContent
                                        FROM dbo.Websites w
                                        LEFT JOIN dbo.Snapshots s ON w.Id = s.Id
                                        WHERE s.Timestamp = (SELECT MAX(Timestamp) FROM dbo.Snapshots WHERE Id = w.Id )";
        [Function(nameof(Watcher))]
        [SqlOutput("dbo.Snapshots", "WebsiteWatcher")]
        public async Task<SnapshotRecord?> RunAsync([TimerTrigger("*/20 * * * * *")] TimerInfo myTimer, [SqlInput(SqlInputQuery, "WebsiteWatcher")] IReadOnlyList<WebsiteModel> websites)
        {
            logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            SnapshotRecord? result = null;

            foreach (WebsiteModel website in websites)
            {
                HtmlWeb htmlWeb = new();
                HtmlDocument doc = htmlWeb.Load(website.Url);

                var divWithContent = doc.DocumentNode.SelectSingleNode(website.XPathExpression);

                var content = divWithContent != null ? divWithContent.InnerText.Trim() : "No Content";
                var contentHasChanged = content != website.LatestContent;

                if (contentHasChanged)
                {
                    logger.LogInformation("Content has changed");
                    var newPdf = await pdfCreatorService.ConvertPageToPdfAsync(website.Url);

                    var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings:WebsiteWatcherStorage");
                    var blobClient = new BlobClient(connectionString, "pdfs", $"{website.Id}-{DateTime.UtcNow:MMddyyyyhhmmss}.pdf");
                    await blobClient.UploadAsync(newPdf);
                    logger.LogInformation("New pdf uploaded");

                    result = new SnapshotRecord(website.Id, content);
                }

            }
            return result;
        }

        public class WebsiteModel
        {
            public Guid Id { get; set; }
            public string Url { get; set; }
            public string? XPathExpression { get; set; }
            public string LatestContent { get; set; }
        }
    }
}
