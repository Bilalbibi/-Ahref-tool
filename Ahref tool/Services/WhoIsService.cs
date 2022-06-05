using Ahref_tool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahref_tool.Services
{
    public class WhoIsService
    {
        private readonly HttpCaller _httpCaller = new HttpCaller();
        public async Task GetRegisteredDate()
        {
            Reporter.Log("Getting registered date for all domains");
            for (var i = 0; i < Singleton.Domains.Count; i++)
            {
                Reporter.Progress((i + 1), Singleton.Domains.Count, "Getting registered date ");
                var domain = Singleton.Domains[i];
                var doc = await _httpCaller.GetDoc($"https://who.is/whois/{domain.Name}");
                var s = doc.DocumentNode.SelectSingleNode("//div[text()='Registered On']/following-sibling::div")?.InnerText.Trim();
                if (s == null)
                {
                    Reporter.Error($"Could not find registered date for {domain.Name}");
                    continue;
                }
                try
                {
                    domain.RegisteredDate = DateTime.Parse(s);
                }
                catch (Exception)
                {
                    Reporter.Error($"Could not parse date : {s} on {domain.Name}");
                }
            }
            Reporter.Log("completed Getting registered date for all domains");
        }
    }
}
