using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ahref_tool.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ahref_tool.Services
{
    public class AhrefService
    {
        private readonly HttpCaller _httpCaller = new HttpCaller();
        public async Task LogIn(string user, string pass)
        {
            //var json = "{\"remember_me\":true,\"auth\":{\"password\":\"5e\\\\.Xq5<(-QK.}r7\",\"login\":\"mei@alw.org.uk\"}}"; 5raaaaaa
            var jsonObj = new
            {
                remember_me = true,
                auth = new
                {
                    password = pass,
                    login = user
                }
            };
            var json = JsonConvert.SerializeObject(jsonObj);
            await _httpCaller.PostJson("https://auth.ahrefs.com/auth/login", json);
            var html = await _httpCaller.GetHtml("https://ahrefs.com/dashboard");
            var token = html.Substring(html.IndexOf("-Token", StringComparison.Ordinal) + 10);
            var x = token.IndexOf("\"", StringComparison.Ordinal);
            token = token.Substring(0, x);
            _httpCaller.CsrfToken = token;
        }

        public async Task PopulateOrganicTraffic()
        {
            Reporter.Log($"Start getting organic traffic for {Singleton.Domains.Count} domains");
            for (var i = 0; i < Singleton.Domains.Count; i++)
            {
                var domain = Singleton.Domains[i];
                Reporter.Progress((i + 1), Singleton.Domains.Count, "Scraping organic traffic ");
                try
                {
                    await GetOrganicTraffic(domain);
                }
                catch (Exception e)
                {
                    Reporter.Error($"Error on getting organic traffic {domain.Name} : {e.Message}");
                }
            }
            Reporter.Log($"completed getting organic traffic for {Singleton.Domains.Count} domains");
        }

        public async Task PopulateOrganicTrafficCost()
        {
            Reporter.Log($"Start getting organic traffic cost for {Singleton.Domains.Count} domains");
            for (var i = 0; i < Singleton.Domains.Count; i++)
            {
                var domain = Singleton.Domains[i];
                Reporter.Progress((i + 1), Singleton.Domains.Count, "Scraping organic traffic cost ");
                try
                {
                    await GetOrganicTrafficCost(domain);
                }
                catch (Exception e)
                {
                    Reporter.Error($"Error on getting organic traffic cost {domain.Name} : {e.Message}");
                }
            }
            Reporter.Log($"completed getting organic traffic cost for {Singleton.Domains.Count} domains");
        }

        public async Task PopulateReferringTraffic()
        {
            Reporter.Log($"Start getting referring traffic for {Singleton.Domains.Count} domains");
            for (var i = 0; i < Singleton.Domains.Count; i++)
            {
                var domain = Singleton.Domains[i];
                Reporter.Progress((i + 1), Singleton.Domains.Count, "Scraping referring traffic ");
                try
                {
                    await GetReferringTraffic(domain);
                }
                catch (Exception e)
                {
                    Reporter.Error($"Error on getting referring traffic {domain.Name} : {e.Message}");
                }
            }
            Reporter.Log($"completed getting referring traffic for {Singleton.Domains.Count} domains");
        }

        public async Task PopulateUr()
        {
            Reporter.Log($"Start getting UR for {Singleton.Domains.Count} domains");
            for (var i = 0; i < Singleton.Domains.Count; i++)
            {
                var domain = Singleton.Domains[i];
                Reporter.Progress((i + 1), Singleton.Domains.Count, "Scraping UR ");
                try
                {
                    await GetBacklinksCount(domain);
                }
                catch (Exception e)
                {
                    Reporter.Error($"Error on getting UR {domain.Name} : {e.Message}");
                }
            }
            Reporter.Log($"completed getting UR for {Singleton.Domains.Count} domains");
        }
        public async Task PopulateBackLinks()
        {
            Reporter.Log($"Start getting BackLinks data for {Singleton.Domains.Count} domains");
            for (var i = 0; i < Singleton.Domains.Count; i++)
            {
                var domain = Singleton.Domains[i];
                Reporter.Progress((i + 1), Singleton.Domains.Count, "Scraping BackLinks data ");
                try
                {
                    await GetBacklinksData(domain);
                }
                catch (Exception e)
                {
                    Reporter.Error($"Error on getting BackLinks data {domain.Name} : {e.Message}");
                }
            }
            Reporter.Log($"completed getting BackLinks data for {Singleton.Domains.Count} domains");
        }
        private async Task GetBacklinksData(Domain domain)
        {
            var csHash = await GetCsHash(domain);
            var json = await _httpCaller.GetHtml($"https://ahrefs.com/site-explorer/ajax/overview/backlinks-stats/{csHash}", 3, true);
            var obj = JObject.Parse(json);
            domain.BackLinksValue = (int) obj.SelectToken("total_backlinks");
            domain.BackLinksRecent = (int) obj.SelectToken("opposite_recent_mode_total_backlinks");
            domain.BackLinksHistorical = (int) obj.SelectToken("opposite_historical_mode_total_backlinks");
        }

        public async Task PopulateReferringDomains()
        {
            Reporter.Log($"Start getting Referring domains data for {Singleton.Domains.Count} domains");
            for (var i = 0; i < Singleton.Domains.Count; i++)
            {
                var domain = Singleton.Domains[i];
                Reporter.Progress((i + 1), Singleton.Domains.Count, "Scraping BackLinks data ");
                try
                {
                    await GetReferringDomainsData(domain);
                }
                catch (Exception e)
                {
                    Reporter.Error($"Error on getting Referring domains data {domain.Name} : {e.Message}");
                }
            }
            Reporter.Log($"completed getting Referring domains data for {Singleton.Domains.Count} domains");
        }
        private async Task GetReferringDomainsData(Domain domain)
        {
            var csHash = await GetCsHash(domain);
            var json = await _httpCaller.GetHtml($"https://ahrefs.com/site-explorer/ajax/overview/referring-domains-stats/{csHash}", 3, true);
            var obj = JObject.Parse(json);
            domain.Rd = (int)obj.SelectToken("total_referring_domains");
            domain.RdRecent = (int)obj.SelectToken("opposite_recent_mode_total_referring_domains");
            domain.RdHistorical = (int)obj.SelectToken("opposite_historical_mode_total_referring_domains");
        }

        private async Task GetOrganicTraffic(Domain domain)
        {
            var json = await _httpCaller.PostJson("https://ahrefs.com/v3/api-adaptor/seGetOrganicChartDataPhpCompat?pretty=1", "{\"select\":\"traffic\",\"mode\":\"subdomains\",\"url\":\"" + domain.Name + "\"}");
            var obj = JArray.Parse(json);
            var data = (JArray)obj[1][1];
            Dictionary<DateTime, decimal> values = new Dictionary<DateTime, decimal>();
            foreach (var d in data)
            {
                var date = DateTime.Parse((string)d.SelectToken("date"));
                var traffic = (decimal)d.SelectToken("traffic");
                values.Add(date, traffic);
            }

            var lastPoint = values.Last().Value;
            var yearAgo = values.ContainsKey(values.Last().Key.AddYears(-1)) ? values[values.Last().Key.AddYears(-1)] : 0;
            var twoYearAgo = values.ContainsKey(values.Last().Key.AddYears(-2)) ? values[values.Last().Key.AddYears(-2)] : 0;
            domain.OrganicTraffic = lastPoint;
            domain.OrganicTraffic1YearsAgo = yearAgo;
            domain.OrganicTraffic2YearsAgo = twoYearAgo;
        }

        private async Task<string> GetCsHash(Domain domain)
        {
            var html = await _httpCaller.GetHtml($"https://ahrefs.com/site-explorer/overview/v2/subdomains/live?target={domain.Name}", 3, false);
            var csHash = html.Substring(html.IndexOf("CSHash = ", StringComparison.Ordinal) + 10);
            var x = csHash.IndexOf(";", StringComparison.Ordinal);
            csHash = csHash.Substring(0, x - 1);
            var Ur = html.Substring(html.IndexOf("ahrefs_rank = ", StringComparison.Ordinal) + 15);
             x = Ur.IndexOf(";", StringComparison.Ordinal);
            Ur = Ur.Substring(0, x - 1);
            domain.Ur = Ur;
            return csHash;
        }

        private async Task GetReferringTraffic(Domain domain)
        {
            var csHash = await GetCsHash(domain);
            var json = await _httpCaller.GetHtml("https://ahrefs.com/site-explorer/ajax/overview/main-chart/" + csHash, 3, true);
            var obj = JObject.Parse(json);
            var series = (JArray)obj.SelectTokens("..Series[?(@.name == 'Referring Domains')].data").First();
            var startDate = DateTime.Parse((string)obj.SelectToken("pointStartText"));
            var days = 0;
            domain.AhrefRd = (int)series.Last;
            foreach (var p in series)
            {
                days++;
                if (startDate.AddDays(days) == DateTime.Now.Date.AddYears(-1))
                {
                    domain.AhrefRD1YearsAgo = (int)p;
                }
                if (startDate.AddDays(days) == DateTime.Now.Date.AddYears(-2))
                {
                    domain.AhrefRD2YearsAgo = (int)p;
                }
            }
        }

        private async Task GetBacklinksCount(Domain domain)
        {
            var json = await _httpCaller.GetHtml($"https://ahrefs.com/site-explorer/backlinks/v7/external-similar-links/subdomains/live/all/all/dofollow/1/ahrefs_rank_desc?target={domain.Name}", 3, true);
            var obj = JObject.Parse(json);
            var results = (JArray)obj.SelectToken("result");
            var count = 0;
            var froms = new HashSet<string>();
            foreach (var result in results)
            {
                var rank = (int)result.SelectToken("ahrefs_rank");
                var from = (string)result.SelectToken("url_from");
                if (froms.Contains(from)) continue;
                froms.Add(from);
                if (rank >= 20)
                    count++;
            }
            domain.UrGreaterThan20 = count;
        }

        public async Task GetOrganicTrafficCost(Domain domain)
        {
            var csHash = await GetCsHash(domain);
            var json = await _httpCaller.GetHtml($"https://ahrefs.com/site-explorer/ajax/overview/PE-stats/{csHash}?source=overview", 3, true);
            var obj = JObject.Parse(json);
            var cost = (decimal)obj.SelectToken("organicTrafficCost");
            domain.OrganicTrafficCost = cost;
        }

        public async Task DownloadAhrefCsv()
        {
            StringBuilder convertedDataDomains = new StringBuilder();
            foreach (var target in Singleton.Domains)
            {
                convertedDataDomains.Append(target.Name + "\r\n");
            }
            var batchFormData = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("batch_requests",convertedDataDomains.ToString()),
                new KeyValuePair<string, string>("protocol","http + https"),
                new KeyValuePair<string, string>("mode","auto"),
                new KeyValuePair<string, string>("history_mode","live"),
                new KeyValuePair<string, string>("sort_by",""),
                new KeyValuePair<string, string>("need_submit_and_export",""),
                new KeyValuePair<string, string>("charset","utf-16")
            };
            var stream = await _httpCaller.GetStream("https://ahrefs.com/batch-analysis?export=1", batchFormData);
            using (var fileStream = File.Create("Ahref.csv"))
                await stream.CopyToAsync(fileStream);
            // File.WriteAllText("Ahref.csv", response);
            var lines = File.ReadAllLines("Ahref.csv").ToList();
            lines.RemoveAt(0);
            Singleton.Domains = new List<Domain>();
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                var cells = line.Split('\t');
                Singleton.Domains.Add(new Domain
                {
                    Name = cells[1].Replace("\"", ""),
                    TotalTraffic = cells[24],
                    AhrefsRank = cells[6],
                    DomainRating = cells[5],
                    Domains = cells[7],
                    RefDoaminsDofollow = cells[8],
                    BackLinks = cells[14],
                    BackLinksText = cells[15]
                });
            }

            File.Delete("Ahref.csv");
        }
    }
}