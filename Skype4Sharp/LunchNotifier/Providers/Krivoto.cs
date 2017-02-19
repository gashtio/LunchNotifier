using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace LunchNotifier.Providers
{
    class Krivoto : ILunchProvider
    {
        public LunchInfo GetLunchInfo()
        {
            string menuPageContent = GetMenuPageContent();

            var result = ParseMenuPage(menuPageContent);

            return new LunchInfo { MenuMessage = result };
        }

        private string GetMenuPageContent()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("http://www.krivoto.com/promo.php?id=3");
            webRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            webRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.95 Safari/537.36";
            webRequest.Method = "GET";
            string rawInfo = "";
            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                rawInfo = new System.IO.StreamReader(webResponse.GetResponseStream()).ReadToEnd();
            }
            return rawInfo;
        }

        private string ParseMenuPage(string menuPageContent)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(menuPageContent);
            // Get the single expected div with class "promotion"
            HtmlNode promotion = doc.DocumentNode.Descendants().
                Where(x => (x.Name == "div" && x.Attributes["class"] != null &&
                    x.Attributes["class"].Value == "promotion1")).ToList()[0];

            // The following should really be a single node, but the page's HTML
            // groups Mon-Thu in one <h2> and Fri in another...so we need a list
            List<HtmlNode> promoTexts = promotion.Descendants().
                Where(x => (x.Name == "h2" && ContainsDayOfWeek(x.InnerText))).ToList();

            StringBuilder sb = new StringBuilder(2048);
            foreach (var promoText in promoTexts)
            {
                int promoTextXPathLevel = promoText.XPath.Count(x => (x == '/'));

                List<HtmlNode> menuItems = promoText.Descendants().
                    Where(x => (x.XPath.Count(y => (y == '/')) == promoTextXPathLevel + 1) &&
                        !string.IsNullOrWhiteSpace(x.InnerText.Replace("&nbsp;", ""))).ToList();

                foreach (var item in menuItems)
                {
                    sb.AppendLine(item.InnerText.Replace("&nbsp;", " "));
                }
            }
            var weekMenu = sb.ToString();

            var today = DateTime.Today;
            var todayName = GetDayOfWeekString(GetNormalizedDayOfWeek(today.DayOfWeek));
            var tomorrowName = GetDayOfWeekString(GetNormalizedDayOfWeek(today.DayOfWeek + 1));

            var todayIdx = weekMenu.IndexOf(todayName);
            var tomorrowIdx = weekMenu.IndexOf(tomorrowName);

            string result = "";
            if (todayIdx != -1)
            {
                if (tomorrowIdx != -1)
                {
                    result = weekMenu.Substring(todayIdx, tomorrowIdx - todayIdx);
                }
                else
                {
                    result = weekMenu.Substring(todayIdx);
                }
            }
            return result;
        }

        private DayOfWeek GetNormalizedDayOfWeek(DayOfWeek dayOfWeek)
        {
            return (DayOfWeek)((int)dayOfWeek % Enum.GetValues(typeof(DayOfWeek)).Length);
        }

        private bool ContainsDayOfWeek(string text)
        {
            return text.Contains("Понеделник") ||
                text.Contains("Вторник") ||
                text.Contains("Сряда") ||
                text.Contains("Четвъртък") ||
                text.Contains("Петък");
        }

        private string GetDayOfWeekString(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday:
                    return "Понеделник";
                case DayOfWeek.Tuesday:
                    return "Вторник";
                case DayOfWeek.Wednesday:
                    return "Сряда";
                case DayOfWeek.Thursday:
                    return "Четвъртък";
                case DayOfWeek.Friday:
                    return "Петък";
                case DayOfWeek.Saturday:
                    return "Събота";
                case DayOfWeek.Sunday:
                    return "Неделя";
                default:
                    return "Not on this planet";
            }
        }
    }
}
