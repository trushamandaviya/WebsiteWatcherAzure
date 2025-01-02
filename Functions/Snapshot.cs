using HtmlAgilityPack;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using Microsoft.Extensions.Logging;
using System.Net;
using static WebsiteWatcher.Functions.Register;

namespace WebsiteWatcher.Functions
{
    public class Snapshot(ILogger<Snapshot> logger)
    {
        [Function(nameof(Snapshot))]
        [SqlOutput("dbo.Snapshots", "WebsiteWatcher")]
        public SnapshotRecord? Run(
            [SqlTrigger("dbo.Websites", "WebsiteWatcher")] IReadOnlyList<SqlChange<Website>> changes)
        {
            SnapshotRecord? result = null;
            foreach (var change in changes)
            {
                logger.LogInformation($"SQL Operation : {change.Operation}");
                logger.LogInformation($"Id: {change.Item.Id} Url: {change.Item.Url}");

                if (change.Operation != SqlChangeOperation.Insert)
                {
                    continue;
                }
                HtmlWeb htmlWeb = new();
                HtmlDocument doc = htmlWeb.Load(change.Item.Url);

                var divWithContent = doc.DocumentNode.SelectSingleNode(change.Item.XPathExpression);

                var content = divWithContent != null ? divWithContent.InnerText.Trim() : "No Content";

                //logger.LogInformation(content);

                result = new SnapshotRecord(change.Item.Id, content);

            }
            return result;
        }
    }

}
public record SnapshotRecord(Guid Id, string Content);
