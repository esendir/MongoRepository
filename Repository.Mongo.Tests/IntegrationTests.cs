using Repository.Mongo.Tests.Util;
using System.Linq;
using Xunit;

namespace Repository.Mongo.Tests
{
    public class IntegrationTests : IClassFixture<MongoDaemon>
    {
        private readonly MongoDaemon deamon;
        private readonly Repository<Sample> repository;

        public IntegrationTests(MongoDaemon deamon)
        {
            this.deamon = deamon;
            this.repository = new Repository<Sample>(MongoDaemon.ConnectionString);
        }

        [Fact]
        public void Insert_Entity()
        {
            var entity = new Sample();

            repository.Insert(entity);

            var dbEntity = repository.Get(entity.Id);

            Assert.Equal(entity.Id, dbEntity.Id);
        }

        [Fact]
        public void Update_Entity()
        {
            var entity = new Sample();
            repository.Insert(entity);

            var dbEntity = repository.Get(entity.Id);
            Assert.Null(dbEntity.Name);

            var update = repository.Updater.Set(i => i.Name, "Update");
            repository.Update(entity.Id, update);

            dbEntity = repository.Get(entity.Id);
            Assert.Equal("Update", dbEntity.Name);
        }

        [Fact]
        public void Delete_Entity()
        {
            var entity = new Sample();
            repository.Insert(entity);

            var dbEntity = repository.Get(entity.Id);
            Assert.Equal(entity.Id, dbEntity.Id);

            repository.Delete(entity.Id);

            dbEntity = repository.Get(entity.Id);
            Assert.Null(dbEntity);
        }

        [Fact]
        public void Find_Entity()
        {
            var entity = new Sample();

            repository.Insert(entity);

            var dbEntity = repository.Find(i => i.Id == entity.Id);

            Assert.Equal(entity.Id, dbEntity.First().Id);
        }
    }
}
