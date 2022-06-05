using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahref_tool.Models
{
    public class Domain
    {
        public string Name { get; set; }
        public string TotalTraffic { get; set; }
        public string AhrefsRank { get; set; }
        public string DomainRating { get; set; }
        public string Domains { get; set; }
        public string RefDoaminsDofollow { get; set; }
        public int AhrefRd { get; set; }
        public int AhrefRD1YearsAgo { get; set; }
        public int AhrefRD2YearsAgo { get; set; }

        public decimal OrganicTraffic { get; set; }
        public decimal OrganicTraffic1YearsAgo { get; set; }
        public decimal OrganicTraffic2YearsAgo { get; set; }

        public string BackLinks { get; set; }
        public int BackLinksValue { get; set; }
        public int BackLinksRecent { get; set; }
        public int BackLinksHistorical { get; set; }


        public string Ur { get; set; }

        public int Rd { get; set; }
        public int RdRecent { get; set; }
        public int RdHistorical { get; set; }

        public string BackLinksText { get; set; }
        public double Tf { get; set; }
        public double Cf { get; set; }
        public string Da { get; set; }
        public string Pa { get; set; }
        public string Links { get; set; }
        public string Equity { get; set; }
        public DateTime RegisteredDate { get; set; }

        public int UrGreaterThan20 { get; set; }
        public decimal OrganicTrafficCost { get; set; }

        public int GoogleResults { get; set; }
    }
}
