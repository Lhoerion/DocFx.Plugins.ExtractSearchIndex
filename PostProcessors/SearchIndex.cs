using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.DocAsCode.MarkdownLite;
using Microsoft.DocAsCode.Plugins;

namespace DocFx.Plugins.ExtractSearchIndex
{
    public class SearchIndex
    {
        private static readonly Regex RegexWhiteSpace = new Regex(@"\s+", RegexOptions.Compiled);

        public static SearchIndexItem ExtractItem(HtmlDocument html, string href, ManifestItem item)
        {
            var contentBuilder = new StringBuilder();

            if (html.DocumentNode.SelectNodes("/html/head/meta[@name='searchOption' and @content='noindex']") != null)
            {
                return null;
            }

            var nodes = html.DocumentNode.SelectNodes("//*[contains(@class,'data-searchable')]") ?? Enumerable.Empty<HtmlNode>();
            nodes = nodes.Union(html.DocumentNode.SelectNodes("//article") ?? Enumerable.Empty<HtmlNode>());
            foreach (var node in nodes)
            {
                ExtractTextFromNode(node, contentBuilder);
            }

            var content = NormalizeContent(contentBuilder.ToString());
            var title = ExtractTitleFromHtml(html);
            var langs = ExtractLanguagesFromHtml(item);
            var type = item.DocumentType == "Conceptual" ? "article" : "api";

            return new SearchIndexItem { Type = type, Href = href, Title = title, Keywords = content, Languages = langs};
        }

        private static string ExtractTitleFromHtml(HtmlDocument html)
        {
            var titleNode = html.DocumentNode.SelectSingleNode("//head/title");
            var originalTitle = titleNode?.InnerText;
            return NormalizeContent(originalTitle);
        }

        private static string ExtractLanguagesFromHtml(ManifestItem item)
        {
            return item.Metadata.TryGetValue("langs", out var langs) ? NormalizeLanguages((List<string>)langs) : string.Empty;
        }

        private static string NormalizeLanguages(IEnumerable<string> list)
        {
            var newList = new List<string>();
            foreach (var lang in list)
            {
                switch (lang.ToLower())
                {
                    case "csharp":
                    case "cs":
                        newList.Add("csharp");
                        newList.Add("cs");
                        break;
                    case "typescript":
                    case "ts":
                        newList.Add("typescript");
                        newList.Add("ts");
                        newList.Add("javascript");
                        newList.Add("js");
                        break;
                    default:
                        newList.Add(lang);
                        break;
                }
            }

            return string.Join(" ", newList.Distinct());
        }

        private static string NormalizeContent(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }
            str = StringHelper.HtmlDecode(str);
            return RegexWhiteSpace.Replace(str, " ").Trim();
        }

        private static void ExtractTextFromNode(HtmlNode root, StringBuilder contentBuilder)
        {
            if (root == null)
            {
                return;
            }

            if (root.Attributes.Contains("data-noindex"))
            {
                return;
            }

            if (!root.HasChildNodes)
            {
                contentBuilder.Append(root.InnerText);
                contentBuilder.Append(" ");
            }
            else
            {
                foreach (var node in root.ChildNodes)
                {
                    ExtractTextFromNode(node, contentBuilder);
                }
            }
        }
    }
}