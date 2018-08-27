using System;

namespace GlobalPhone
{

    /// <summary>
    /// Static class that holds a single Context
    /// </summary>
    public class GlobalPhone
    {

        private static readonly Lazy<Context> _context = new Lazy<Context>(() => DefaultContextBuilder?.Invoke() ?? new Context());

        private static Context Context => _context.Value;

        public static Func<Context> DefaultContextBuilder { get; set; } = () => new Context();

        public static string DbPath
        {
            get => Context.DbPath;
            set => Context.DbPath = value;
        }

        public static string DbText
        {
            get => Context.DbText;
            set => Context.DbText = value;
        }

        public static string DefaultTerritoryName
        {
            get => Context.DefaultTerritoryName;
            set => Context.DefaultTerritoryName = value;
        }

        public static Database Db => Context.Db;

        public static bool Validate(string number, string territoryName = null)
        {
            return Context.Validate(number, territoryName);
        }

        public static Number Parse(string number, string territoryName = null)
        {
            return Context.Parse(number, territoryName);
        }

        public static string Normalize(string number, string territoryName = null)
        {
            return Context.Normalize(number, territoryName);
        }

        public static bool TryParse(string str, out Number number, string territoryName = null)
        {
            return Context.TryParse(str, out number, territoryName);
        }

        public static bool TryNormalize(string str, out string number, string territoryName = null)
        {
            return Context.TryNormalize(str, out number, territoryName);
        }

    }

}