using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace GlobalPhone
{

    /// <summary>
    /// Database of phone number information.
    /// </summary>
    public class Database
        : Parsing
    {

        public IEnumerable<Region> Regions { get; }

        private readonly Dictionary<string, Territory> _territoriesByName;

        public Database(object[] recordData)
        {
            this._territoriesByName = new Dictionary<string, Territory>(StringComparer.OrdinalIgnoreCase);
            this.Regions = recordData.Select(data => new Region(data)).ToList();
        }

        public static Database LoadFile(string filename, IDeserializer serializer)
        {
            return Load(File.ReadAllText(filename), serializer);
        }

        public static Database Load(string text, IDeserializer serializer)
        {
            return new Database(serializer.Deserialize(text));
        }

        public static Database Load(Stream stream, IDeserializer serializer)
        {
            return new Database(serializer.Deserialize<Dictionary<string, object>[]>(stream)
                .Select(x => (object)x).ToArray());
        }

        public bool TryGetRegion(int countryCode, out Region value)
        {
            return this.TryGetRegion(countryCode.ToString(CultureInfo.InvariantCulture), out value);
        }

        public Region TryGetRegion(int countryCode)
        {
            return this.TryGetRegion(countryCode.ToString(CultureInfo.InvariantCulture), out var value) ? value : null;
        }

        public override bool TryGetRegion(string countryCode, out Region value)
        {
            return this.RegionsByCountryCode.TryGetValue(countryCode, out value);
        }

        private Dictionary<string, Region> _regionsByCountryCode;

        protected Dictionary<string, Region> RegionsByCountryCode
        {
            get { return this._regionsByCountryCode ?? (this._regionsByCountryCode = this.Regions.ToDictionary(r => r.CountryCode)); }
        }


        public override bool TryGetTerritory(string name, out Territory territory)
        {
            if (this._territoriesByName.TryGetValue(name, out var value))
            {
                territory = value;
                return true;
            }

            Region region;
            if ((region = this.RegionForTerritory(name)) != null
                && (territory = region.Territory(name)) != null)
            {
                this._territoriesByName.Add(name, territory);
                return true;
            }
            territory = null;
            return false;
        }

        private Region RegionForTerritory(string name)
        {
            return this.Regions.SingleOrDefault(r => r.HasTerritory(name));
        }

    }

}