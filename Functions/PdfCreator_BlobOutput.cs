using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using Microsoft.Extensions.Logging;
using static WebsiteWatcher.Functions.Register;
using PuppeteerSharp;

namespace WebsiteWatcher;

public class PdfCreator_BlobOutput(ILogger<PdfCreator_BlobOutput> logger)
{
    // Visit https://aka.ms/sqltrigger to learn how to use this trigger binding
    [Function("PdfCreator_BlobOutput")]
    [BlobOutput("pdfs/new.pdf", Connection = "WebsiteWatcherStorage")]
    public async Task<byte[]?> Run(
        [SqlTrigger("[dbo].[Websites]", "WebsiteWatcher")] SqlChange<Website>[] changes)
    {
        byte[]? buffer = null; 
        foreach (var change in changes)
        {
            if(change.Operation == SqlChangeOperation.Insert)
            {
                var result = await ConvertPageToPdfAsync(change.Item.Url);
                buffer = new byte[result.Length];
                await result.ReadAsync(buffer);

                logger.LogInformation($"PDF strength is : {result.Length}");
            }
        }
        return buffer;
    }

    private async Task<Stream> ConvertPageToPdfAsync(string url)
    {
        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();
        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
        await using var page = await browser.NewPageAsync();
        await page.GoToAsync(url);
        await page.EvaluateExpressionHandleAsync("document.fonts.ready");
        var result = await page.PdfStreamAsync();
        result.Position = 0;

        return result;
    }
}
