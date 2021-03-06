using System.Linq;

namespace GlobalPhone
{
    internal static class NokogiriExtensions
    {
        public static string Text(this Nokogiri.Node[] self)
        {
            return string.Join("", self
                .Where(node=>node.IsText)
                .Select(node => node.Text));
        }
    }
}