using Google.Apis.Safebrowsing.v4.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebsiteWatcher;

public class SafeBrowsingService(IConfiguration configuration)
{
    //public (bool HasThreat, IReadOnlyList<string> Threats) Check(string url)
    //{
    //    var initializer = new Google.Apis.Services.BaseClientService.Initializer
    //    {
    //        ApiKey = configuration.GetValue<string>("GoogleSAfeBrowsingApiKey")
    //    };
    //    using var safeBrowsing = new SafeBrowsingService(initializer);
    //    var request = new GoogleSecuritySafebrowsingV4FindThreatMatchesRequest
    //    {
    //        Client = GetClientInfo(),
    //        ThreatInfo = GetThreatInfo(url)
    //    };


    //}
}
