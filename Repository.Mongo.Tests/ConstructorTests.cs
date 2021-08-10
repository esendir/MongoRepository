using AutoFixture;
using AutoFixture.AutoMoq;
using MongoDB.Driver;
using System.Diagnostics;
using Xunit;

namespace Repository.Mongo.Tests
{
    public class ConstructorTests
    {
        [Fact]
        public void Constructor_IMongoDatabase_ShouldBe_LessThen5ms()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var param1 = fixture.Create<IMongoDatabase>();
            var param2 = fixture.Create<string>();

            var watch = Stopwatch.StartNew();

            new Repository<IEntity>(param1, param2);

            var duration = watch.ElapsedMilliseconds;
            Assert.True(duration < 5);
        }

        [Fact]
        public void Constructor_IMongoClient_ShouldBe_LessThen5ms()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var param1 = fixture.Create<IMongoClient>();
            var param2 = fixture.Create<string>();
            var watch = Stopwatch.StartNew();

            new Repository<IEntity>(param1, param2, param2);

            var duration = watch.ElapsedMilliseconds;
            Assert.True(duration < 5);
        }

        [Fact]
        public void Constructor_ConnectionString_ShouldBe_LessThen5ms()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var param1 = fixture.Create<string>();
            var watch = Stopwatch.StartNew();

            new Repository<IEntity>(param1, param1);

            var duration = watch.ElapsedMilliseconds;
            Assert.True(duration < 5);
        }
    }
}
