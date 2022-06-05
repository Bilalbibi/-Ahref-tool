using System;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Ahref_tool.Services
{
    public class ExcelService
    {
        public void Export(string path)
        {
            try
            {
                Reporter.Log($"Exporting result to {path}");
                #region Write headers of DomainsToCheck and Ahrefs Data sheets
                var package = new ExcelPackage(new FileInfo(path));
                var sheet1 = package.Workbook.Worksheets.Add("DomainsToCheck");
                var sheet2 = package.Workbook.Worksheets.Add("Ahrefs_Data");

                var row = 1;

                sheet1.Protection.IsProtected = false;
                sheet1.Protection.AllowSelectLockedCells = false;
                sheet1.Row(1).Height = 20;
                sheet1.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet1.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                sheet1.Row(1).Style.Font.Bold = true;
                sheet1.Row(1).Style.Font.Size = 8;
                sheet1.Row(1).Style.WrapText = true;

                sheet1.Cells[row, 1].Value = "Domain Name";
                sheet1.Cells[row, 2].Value = "TLD";
                sheet1.Cells[row, 3].Value = "DA";
                sheet1.Cells[row, 4].Value = "PA";
                sheet1.Cells[row, 5].Value = "Links";
                sheet1.Cells[row, 6].Value = "Equity";
                sheet1.Cells[row, 7].Value = "TF";
                sheet1.Cells[row, 8].Value = "CF";
                sheet1.Cells[row, 9].Value = "TF/CE Ratio";
                sheet1.Cells[row, 10].Value = "Ahrefs Rank";
                sheet1.Cells[row, 11].Value = "hrefs DR";
                sheet1.Cells[row, 12].Value = "Ahrefs UR";
                sheet1.Cells[row, 13].Value = "Backlinks";
                sheet1.Cells[row, 14].Value = "Backlinks" + "\r\n" + " (Recent)";
                sheet1.Cells[row, 15].Value = "Backlinks" + "\r\n" + " (Historical)";
                sheet1.Cells[row, 16].Value = "RD";
                sheet1.Cells[row, 17].Value = "RD " + "\r\n" + " (Recent)";
                sheet1.Cells[row, 18].Value = "RD"  + "\r\n" + "(Historical)";
                sheet1.Cells[row, 19].Value = "Ahrefs RD's";
                sheet1.Cells[row, 20].Value = "Ahrefs RD"+ "\r\n" + " (1 Yr Ago)";
                sheet1.Cells[row, 21].Value = "Ahrefs RD" + "\r\n" + " (2 Yr Ago)";
                sheet1.Cells[row, 22].Value = "Ahref RD Inc?";
                sheet1.Cells[row, 23].Value = "Ahrefs Traffic";
                sheet1.Cells[row, 24].Value = "Ahrefs Traffic" + "\r\n" + " (1 Yr Ago)";
                sheet1.Cells[row, 25].Value = "Ahrefs Traffic" + "\r\n" + " (2 Yr Ago)";
                sheet1.Cells[row, 26].Value = "Ahrefs Traffic Increasing?";
                sheet1.Cells[row, 27].Value = "Ahrefs Traffic Val.";
                sheet1.Cells[row, 28].Value = "# of Dofollow Links with UR > 20";
                sheet1.Cells[row, 29].Value = "Reg. Date";
                sheet1.Cells[row, 30].Value = "Older Than 1 Year?";
                sheet1.Cells[row, 31].Value = "Indexed";
                sheet1.Cells[row, 32].Value = "# of Pages Indexed";

                var range = sheet1.Cells[$"A1:AF{Singleton.Domains.Count + 1}"];
                var tab = sheet1.Tables.Add(range, "DomainsToCheck");

                sheet2.Protection.IsProtected = false;
                sheet2.Protection.AllowSelectLockedCells = false;
                sheet2.Row(1).Height = 20;
                sheet2.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet2.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                sheet2.Row(1).Style.Font.Bold = true;
                sheet2.Row(1).Style.Font.Size = 8;
                sheet2.Row(1).Style.WrapText = true;

                sheet2.Cells[row, 1].Value = "Domain Name";
                sheet2.Cells[row, 2].Value = "Total Traffic";
                sheet2.Cells[row, 3].Value = "Ahrefs Rank";
                sheet2.Cells[row, 4].Value = "DR";
                sheet2.Cells[row, 5].Value = "Domains";
                sheet2.Cells[row, 6].Value = "RD (Dofollow)";
                sheet2.Cells[row, 7].Value = "Backlinks";
                sheet2.Cells[row, 8].Value = "Backlinks Text";

                var range2 = sheet2.Cells[$"A1:H{Singleton.Domains.Count + 1}"];
                var ta2b = sheet2.Tables.Add(range2, "Ahrefs_Data");
                row = 2;
                #endregion
                foreach (var domain in Singleton.Domains)
                {
                    #region Save data in DomainsToCheck sheet

                    sheet1.Cells[row, 1].Value = domain.Name;
                    sheet1.Cells[row, 2].Value = domain.Name.Substring(domain.Name.LastIndexOf(".", StringComparison.Ordinal) + 1);
                    sheet1.Cells[row, 3].Value = domain.Da;
                    sheet1.Cells[row, 4].Value = domain.Pa;
                    sheet1.Cells[row, 5].Value = domain.Links;
                    sheet1.Cells[row, 6].Value = domain.Equity;
                    sheet1.Cells[row, 7].Value = domain.Tf;
                    sheet1.Cells[row, 8].Value = domain.Cf;
                    sheet1.Cells[row, 9].Value = (int)((domain.Tf / domain.Cf) * 100) + " %";
                    sheet1.Cells[row, 10].Value = domain.AhrefsRank;
                    sheet1.Cells[row, 11].Value = domain.DomainRating;
                    //sheet1.Cells[row, 12].Value = domain.Ur;
                    sheet1.Cells[row, 13].Value = domain.BackLinksValue;
                    sheet1.Cells[row, 14].Value = domain.BackLinksRecent;
                    sheet1.Cells[row, 15].Value = domain.BackLinksHistorical;
                    sheet1.Cells[row, 16].Value = domain.Rd;
                    sheet1.Cells[row, 17].Value = domain.RdRecent;
                    sheet1.Cells[row, 18].Value = domain.RdHistorical;
                    sheet1.Cells[row, 19].Value = domain.AhrefRd;
                    sheet1.Cells[row, 20].Value = domain.AhrefRD1YearsAgo;
                    sheet1.Cells[row, 21].Value = domain.AhrefRD2YearsAgo;
                    sheet1.Cells[row, 22].Value = domain.AhrefRD2YearsAgo < domain.AhrefRD1YearsAgo && domain.AhrefRD1YearsAgo < domain.AhrefRd ? "Yes" : "No";
                    sheet1.Cells[row, 23].Value = Math.Round(domain.OrganicTraffic, 0);
                    sheet1.Cells[row, 24].Value = Math.Round(domain.OrganicTraffic1YearsAgo, 0);
                    sheet1.Cells[row, 25].Value = Math.Round(domain.OrganicTraffic2YearsAgo, 0);
                    sheet1.Cells[row, 26].Value = domain.OrganicTraffic2YearsAgo < domain.OrganicTraffic1YearsAgo && domain.OrganicTraffic1YearsAgo < domain.OrganicTraffic ? "Yes" : "No";
                    sheet1.Cells[row, 27].Value = domain.OrganicTrafficCost;
                    sheet1.Cells[row, 28].Value = domain.UrGreaterThan20;
                    sheet1.Cells[row, 29].Value = domain.RegisteredDate == default(DateTime) ? "NA" : domain.RegisteredDate.ToString("dd-MMM-yyyy");
                    sheet1.Cells[row, 30].Value = domain.RegisteredDate < DateTime.Now.AddYears(-1) ? "Yes" : "No";
                    sheet1.Cells[row, 31].Value = domain.GoogleResults > 0 ? "Yes" : "No";
                    sheet1.Cells[row, 32].Value = domain.GoogleResults;

                    #endregion

                    #region Save data in Ahrefs Data sheet
                    sheet2.Cells[row, 1].Value = domain.Name;
                    sheet2.Cells[row, 2].Value = domain.TotalTraffic;
                    sheet2.Cells[row, 3].Value = domain.AhrefsRank;
                    sheet2.Cells[row, 4].Value = domain.DomainRating;
                    sheet2.Cells[row, 5].Value = domain.Domains;
                    sheet2.Cells[row, 6].Value = domain.RefDoaminsDofollow;
                    sheet2.Cells[row, 7].Value = domain.BackLinks;
                    sheet2.Cells[row, 8].Value = domain.BackLinksText;
                    #endregion

                    row++;
                }
                for (var i = 1; i <= 25; i++) sheet1.Column(i).AutoFit();
                for (var i = 1; i <= 8; i++) sheet2.Column(i).AutoFit();


                sheet1.Cells[$"A2:B{Singleton.Domains.Count + 1}"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                sheet1.Cells[$"A2:B{Singleton.Domains.Count + 1}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                sheet1.Cells[$"C2:H{Singleton.Domains.Count + 1}"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                sheet1.Cells[$"C2:H{Singleton.Domains.Count + 1}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                sheet1.Cells[$"Z2:Z{Singleton.Domains.Count + 1}"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                sheet1.Cells[$"Z2:Z{Singleton.Domains.Count + 1}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                sheet1.Cells[$"AD2:AE{Singleton.Domains.Count + 1}"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                sheet1.Cells[$"AD2:AE{Singleton.Domains.Count + 1}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                #region set borders and it's style
                var borders1 = sheet1.Cells[$"C1:C{Singleton.Domains.Count + 1}"];
                var borders2 = sheet1.Cells[$"F1:F{ Singleton.Domains.Count + 1}"];
                borders1.Style.Border.Left.Style = ExcelBorderStyle.Thick;
                borders2.Style.Border.Right.Style = ExcelBorderStyle.Thick;

                var borders3 = sheet1.Cells[$"G1:G{Singleton.Domains.Count + 1}"];
                var borders4 = sheet1.Cells[$"I1:I{Singleton.Domains.Count + 1}"];
                borders3.Style.Border.Left.Style = ExcelBorderStyle.Thick;
                borders4.Style.Border.Right.Style = ExcelBorderStyle.Thick;


                var borders5 = sheet1.Cells[$"P1:P{Singleton.Domains.Count + 1}"];
                var borders6 = sheet1.Cells[$"V1:V{ Singleton.Domains.Count + 1}"];
                borders5.Style.Border.Left.Style = ExcelBorderStyle.Medium;
                borders6.Style.Border.Right.Style = ExcelBorderStyle.Medium;

                var borders7 = sheet1.Cells[$"M1:M{Singleton.Domains.Count + 1}"];
                var borders8 = sheet1.Cells[$"O1:O{Singleton.Domains.Count + 1}"];
                borders7.Style.Border.Left.Style = ExcelBorderStyle.Medium;
                borders8.Style.Border.Right.Style = ExcelBorderStyle.Medium;

                var borders9 = sheet1.Cells[$"W1:W{Singleton.Domains.Count + 1}"];
                var borders10 = sheet1.Cells[$"AA1:AA{Singleton.Domains.Count + 1}"];
                borders9.Style.Border.Left.Style = ExcelBorderStyle.Medium;
                borders10.Style.Border.Right.Style = ExcelBorderStyle.Medium;


                var borders11 = sheet1.Cells[$"AC1:AC{Singleton.Domains.Count + 1}"];
                var borders12 = sheet1.Cells[$"AF1:AF{Singleton.Domains.Count + 1}"];
                borders11.Style.Border.Left.Style = ExcelBorderStyle.Medium;
                borders12.Style.Border.Right.Style = ExcelBorderStyle.Medium;
                #endregion

                sheet1.Cells[$"A1:AF{Singleton.Domains.Count + 1}"].Style.Font.Size = 8;


                sheet1.Cells[$"A1:AF{Singleton.Domains.Count + 1}"].Style.Font.Size = 8;

                foreach (var cell in sheet1.Cells["C2:F" + row])
                {
                    cell.Style.Numberformat.Format = "0";
                    cell.Value = Convert.ToDecimal(cell.Value);
                }
                foreach (var cell in sheet1.Cells["J2:K" + row])
                {
                    cell.Style.Numberformat.Format = "0";
                    cell.Value = Convert.ToDecimal(cell.Value);
                }
                foreach (var cell in sheet1.Cells["AA2:AA" + row])
                {
                    cell.Style.Numberformat.Format = "$* #0";
                    cell.Value = Convert.ToDecimal(cell.Value);
                }

                for (int i = 7; i <= 20; i++) sheet2.Column(i).Width = 8;

                sheet1.Column(3).Width = 5;
                sheet1.Column(4).Width = 5;
                sheet1.Column(5).Width = 5;
                sheet1.Column(6).Width = 8;
                sheet1.Column(21).Width = 10;
                sheet1.Column(22).Width = 8;
                sheet1.Column(23).Width = 10;
                sheet1.Column(24).Width = 8;
                sheet1.Column(25).Width = 8;

                sheet1.View.FreezePanes(2, 1);
                sheet1.View.FreezePanes(2, 2);


                sheet2.Cells["A1:H"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                sheet2.Cells["A1:H"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                for (var i = 1; i <= 8; i++) sheet2.Column(i).AutoFit();

                sheet2.View.FreezePanes(2, 1);
                sheet2.View.FreezePanes(2, 2);

                package.Save();
                Reporter.Log($"Export completed");
            }
            catch (Exception e)
            {
                Reporter.Error($"Failed to export {e.Message}");
            }

        }
    }
}