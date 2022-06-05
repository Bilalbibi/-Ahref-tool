using Ahref_tool.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Ahref_tool.Services
{
    public class SeoService
    {
        public HttpCaller HttpCaller = new HttpCaller();
        public async Task PopulateTfAndCfData()
        {
            Reporter.Log("Start populating TF and CF data");
            var tpl = new TransformBlock<Domain, (double tf, double cf, Domain domain)>(async x => await GetTfAndCfData(x).ConfigureAwait(false),
               new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 40 });

            foreach (var domain in Singleton.Domains)
                tpl.Post(domain);

            for (var i = 0; i < Singleton.Domains.Count; i++)
            {
                Reporter.Progress((i + 1), Singleton.Domains.Count, "Scraping TF/CF ");
                var (tf, cf, domain) = await tpl.ReceiveAsync().ConfigureAwait(false);
            }

            Reporter.Log("Completed populating TF and CF data");
        }

        private async Task<(double tf, double cf, Domain domain)> GetTfAndCfData(Domain domain)
        {
            do
            {
                var html = await HttpCaller.GetHtml($"https://seo-rank.my-addr.com/api3/F1EF5461AEE11BE918A459EA5204150F/{domain.Name}");
                if (!html.Contains("status"))
                {
                    await Task.Delay(1000 * 60 * 2);
                    continue;
                }
                var obj = JObject.Parse(html);
                var status = (string)obj.SelectToken("status");
                if (status != "Found")
                    return (0, 0, domain);

                var tf = (double)obj.SelectToken("tf");
                var cf = (double)obj.SelectToken("cf");
                domain.Tf = tf;
                domain.Cf = cf;
                return (tf, cf, domain);
            } while (true);
        }
        public async Task PopulateDaStatus(Domain domain)
        {
            var json = await HttpCaller.GetHtml($"https://seo-rank.my-addr.com/api2/+moz/F1EF5461AEE11BE918A459EA5204150F/{domain.Name}");
            var obj = JObject.Parse(json);
            domain.Da = (string)obj.SelectToken("da");
            domain.Pa = (string)obj.SelectToken("pa");
            domain.Links = (string)obj.SelectToken("links");
            domain.Equity = (string)obj.SelectToken("equity");
        }

        public async Task PopulateDaStatus()
        {
            Reporter.Log("Start populating Da, Pa, Links and Equity data");

            for (int i = 0; i < Singleton.Domains.Count; i++)
            {
                Reporter.Progress((i + 1), Singleton.Domains.Count, "Scraping Da, Pa, Links and Equity ");
                try
                {
                    await PopulateDaStatus(Singleton.Domains[i]);
                }
                catch (Exception e)
                {
                    Reporter.Error($"Error scraping da,pa {Singleton.Domains[i]} : {e.Message}");
                }
            }
            Reporter.Log("Completed populating Da, Pa, Links and Equity data");
        }
    }
}
