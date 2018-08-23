using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GlobalPhone
{

    /// <summary>
    /// Database generator.
    /// </summary>
    public class DatabaseGenerator
    {

        private IDictionary[] _recordDataHash;
        private string[][] _testCases;

        private Nokogiri.XmlDoc Doc { get; set; }

        private DatabaseGenerator(Nokogiri.XmlDoc doc)
        {
            this.Doc = doc;
        }

        /// <summary>
        /// Load the specified text as xml.
        /// </summary>
        public static DatabaseGenerator Load(string text)
        {
            return new DatabaseGenerator(Nokogiri.Xml(text));
        }

        /// <summary>
        /// Load the specified text as xml.
        /// </summary>
        public static DatabaseGenerator Load(Stream stream)
        {
            return new DatabaseGenerator(Nokogiri.Xml(stream));
        }

        /// <summary>
        /// Loads the file as a file containing xml.
        /// </summary>
        public static DatabaseGenerator LoadFile(string filename)
        {
            return Load(File.ReadAllText(filename));
        }

        /// <summary>
        /// The records in the data.
        /// </summary>
        public IDictionary[] RecordData()
        {
            return this._recordDataHash ?? (this._recordDataHash = this.TerritoryNodesByRegion().Select(kv =>
            {
                var countryCode = kv.Key;
                var territoryNodes = kv.ToArray();
                return
                    Truncate(this.CompileRegion(
                        territoryNodes,
                        countryCode));
            }).ToArray());
        }

        /// <summary>
        /// Return example numbers for territories.
        /// </summary>
        public string[][] TestCases()
        {
            return this._testCases ?? (this._testCases = this.TerritoryNodes().Select(this.ExampleNumbersForTerritoryNode)
                .Flatten(1).Cast<string[]>().Where(arr => arr.Length > 0).ToArray());
        }

        private IEnumerable<Nokogiri.Node> TerritoryNodes()
        {
            return this.Doc.Search("territory");
        }

        private static string TerritoryName(Nokogiri.Node node)
        {
            return node["id"];
        }

        private IEnumerable<string[]> ExampleNumbersForTerritoryNode(Nokogiri.Node node)
        {
            var name = TerritoryName(node);
            if (name == "001") return new[] { new string[0] };
            return node.Search(this.example_numbers_selector())
                .Select(node1 => new[] { node1.Text, name })
                .ToArray();
        }

        private IEnumerable<IGrouping<string, Nokogiri.Node>> TerritoryNodesByRegion()
        {
            return this.TerritoryNodes().GroupBy(node => node["countryCode"]);
        }

        private string example_numbers_selector()
        {
            return "./*[not(" + String.Join(" or ", this.ExampleNumberTypesToExclude().Select(type =>
                                                                                           "self::" + type)) +
                   ")]/exampleNumber";
        }

        private string[] ExampleNumberTypesToExclude()
        {
            return "emergency shortCode".Split(new[] { ' ' });
        }

        private IDictionary CompileRegion(IEnumerable<Nokogiri.Node> territoryNodes, string countryCode)
        {
            var nodes = territoryNodes.ToArray();
            var kv = this.CompileTerritories(nodes);
            var territories = kv.Item1;
            var mainTerritoryNode = kv.Item2;
            var formats = this.CompileFormats(nodes);

            return new Dictionary<string, object>
                     {
                         {"countryCode",countryCode},
                         {"formats",formats},
                         {"territories",territories},
                         {"interPrefix", mainTerritoryNode["internationalPrefix"]},
                         {"prefix",mainTerritoryNode["nationalPrefix"]},
                         {"prefixParse",Squish(mainTerritoryNode["nationalPrefixForParsing"])},
                         {"prefixTRule",Squish(mainTerritoryNode["nationalPrefixTransformRule"])}
                     };
        }

        private Tuple<object[], Nokogiri.Node> CompileTerritories(IEnumerable<Nokogiri.Node> territoryNodes)
        {
            var territories = new List<object>();
            var nodes = territoryNodes.ToArray();
            var mainTerritoryNode = nodes.First();
            foreach (var node in nodes)
            {
                var territory = Truncate(this.CompileTerritory(node));
                if (node["mainCountryForCode"] != null)
                {
                    mainTerritoryNode = node;
                    territories.Insert(0, territory);
                }
                else
                {
                    territories.Add(territory);
                }
            }

            return new Tuple<object[], Nokogiri.Node>(territories.ToArray(), mainTerritoryNode);
        }

        private IDictionary CompileTerritory(Nokogiri.Node node)
        {
            var possibleNumberPattern = this.Pattern(node, "generalDesc possibleNumberPattern");
            var nationalNumberPattern = this.Pattern(node, "generalDesc nationalNumberPattern");
            var d = new Dictionary<string, object>
            {
                {"name",TerritoryName(node)},
                {"possibleNumber",possibleNumberPattern.FirstOrDefault()},
                {"nationalNumber",nationalNumberPattern.FirstOrDefault()},
                {"formattingRule",Squish(node["nationalPrefixFormattingRule"])},
                {"nationalPrefix",Squish(node["nationalPrefix"])},
            };
            if ("true".Equals(Squish(node["nationalPrefixOptionalWhenFormatting"])))
            {
                d.Add("nationalPrefixOptionalWhenFormatting", true);
            }
            return d;
        }

        private IEnumerable<IDictionary> CompileFormats(IEnumerable<Nokogiri.Node> territoryNodes)
        {
            return Truncate(this.FormatNodesFor(territoryNodes).Select(node => Truncate(this.CompileFormat(node))));
        }

        private IDictionary CompileFormat(Nokogiri.Node node)
        {
            var format = new Dictionary<string, object>
                                    {
                                        {"pattern",node["pattern"]},
                                        {"format", TextOrEmpty(node, "format").FirstOrDefault()},
                                        {"leadingDigits",this.Pattern(node, "leadingDigits")},
                                        {"formatRule",node["nationalPrefixFormattingRule"]},
                                        {"intlFormat",TextOrEmpty(node, "intlFormat").FirstOrDefault()},
                                    };
            return format;
        }

        private IEnumerable<Nokogiri.Node> FormatNodesFor(IEnumerable<Nokogiri.Node> territoryNodes)
        {
            return territoryNodes.Select(node =>
                                       node.Search("availableFormats numberFormat").ToArray()).Flatten<Nokogiri.Node>();
        }
        private static readonly Regex WhiteSpace = new Regex(@"\s+");
        private static string Squish(string @string)
        {
            return !String.IsNullOrEmpty(@string) ? WhiteSpace.Replace(@string, "") : @string;
        }

        private string[] Pattern(Nokogiri.Node node, string selector)
        {
            return TextOrEmpty(node, selector)
                .Select(Squish)
                .Where(this.NotNullOrEmpty)
                .ToArray();
        }

        private bool NotNullOrEmpty(string arg)
        {
            return !string.IsNullOrEmpty(arg);
        }

        private static string[] TextOrEmpty(Nokogiri.Node node, string selector)
        {
            var nodes = node.Search(selector);
            return nodes == null || !nodes.Any()
                ? new string[0]
                : nodes.Select(n => n.Text).ToArray();

        }

        private static IDictionary Truncate(IDictionary self)
        {
            var truncated = new Dictionary<string, object>();
            foreach (string key in self.Keys)
            {
                var value = self[key];
                if (value != null)
                {
                    truncated.Add(key, value);
                }
            }
            return truncated;
        }

        private static T[] Truncate<T>(IEnumerable<T> self)
        {
            return Truncate(self.ToArray());
        }

        private static T[] Truncate<T>(T[] self)
        {
            /*     def truncate(array)
array.dup.tap do |result|
result.pop while result.any? && result.last.nil?
end
end
*/
            var found = -1;
            for (var i = self.Length - 1; i >= 0; i--)
            {
                if (self[i] != null)
                {
                    found = i;
                    break;
                }
            }
            if (found >= 0 && found != self.Length - 1)
            {
                return self.Take(found + 1).ToArray();
            }
            return self.ToArray();
        }

    }

}