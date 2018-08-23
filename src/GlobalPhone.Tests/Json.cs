
using NUnit.Framework.Constraints;

namespace GlobalPhone.Tests
{

    public class Json
    {

        private class JsonResolveConstraint
            : IResolveConstraint
        {

            private readonly object _match;

            public JsonResolveConstraint(object match)
            {
                this._match = match;
            }

            public IConstraint Resolve()
            {
                return new JsonEqualConstraint(this._match);
            }

        }

        internal class JsonEqualConstraint
            : ComparisonConstraint
        {

            public JsonEqualConstraint(object match)
                : base(match)
            {
            }

            protected override bool PerformComparison(ComparisonAdapter comparer, object actual, object expected, Tolerance tolerance)
            {
                var makrillConvert = new Makrill.JsonConvert();
                return expected.Equals(makrillConvert.Serialize(actual));
            }

        }

        public static IResolveConstraint EqualTo(object match)
        {
            return new JsonResolveConstraint(match);
        }

    }

}