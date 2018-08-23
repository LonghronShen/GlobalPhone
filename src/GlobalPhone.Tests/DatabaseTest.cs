using NUnit.Framework;
using System.Linq;

namespace GlobalPhone.Tests
{

    [SetUpFixture]
    [TestFixture(typeof(DefaultDeserializer), ForData.UseHash)]
    [TestFixture(typeof(NewtonsoftDeserializer), ForData.UseHashV2)]
    [TestFixture(typeof(NewtonsoftDeserializer), ForData.UseHashV3)]
    public class DatabaseTest<TDeserializer> 
        : TestFixtureBase where TDeserializer : IDeserializer, new()
    {

        public DatabaseTest(ForData forData)
            : base(forData)
        {
        }

        [OneTimeSetUp]
        public void TestFixtureSetup()
        {
            this._deserializer = new TDeserializer();
        }

        [Test]
        public void initializing_database_manually()
        {
            var db = new Database(this.RecordData);
            Assert.That(db.Regions.Count(), Is.EqualTo(this.RecordData.Length));
        }

        [Test]
        public void finding_region_by_country_code()
        {
            var region = this.Db.TryGetRegion(1);
            Assert.That(region, Is.TypeOf<Region>());
            Assert.That(region.CountryCode, Is.EqualTo("1"));
        }

        [Test]
        public void nonexistent_region_returns_nil()
        {
            var region = this.Db.TryGetRegion(999);
            Assert.That(region, Is.Null);
        }

        [Test]
        public void finding_territory_by_name()
        {
            var territory = this.Db.TryGetTerritory("gb");
            Assert.That(territory, Is.TypeOf<Territory>());
            Assert.That(territory.Name, Is.EqualTo("GB"));
            //assert_equal "GB", territory.name
            Assert.That(territory.Region, Is.EqualTo(this.Db.TryGetRegion(44)));
        }

        [Test]
        public void nonexistent_territory_returns_nil()
        {
            var territory = this.Db.TryGetTerritory("nonexistent");
            Assert.That(territory, Is.Null);
        }

    }

}