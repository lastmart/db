using System;
using MongoDB.Driver;

namespace Game.Domain
{
    public class MongoUserRepository : IUserRepository
    {
        private readonly IMongoCollection<UserEntity> userCollection;
        public const string CollectionName = "users";

        public MongoUserRepository(IMongoDatabase database)
        {
            userCollection = database.GetCollection<UserEntity>(CollectionName);
            var loginIndexModel = new CreateIndexModel<UserEntity>(
                Builders<UserEntity>.IndexKeys.Ascending(userEntity => userEntity.Login),
                new CreateIndexOptions { Unique = true });
            userCollection.Indexes.CreateOne(loginIndexModel);
        }

        public UserEntity Insert(UserEntity user)
        {
            userCollection.InsertOne(user);
            return user;
        }

        public UserEntity FindById(Guid id)
        {
            var findResult = userCollection.Find(userEntity => userEntity.Id == id);
            return findResult.SingleOrDefault();
        }

        public UserEntity GetOrCreateByLogin(string login)
        {
            try
            {
                return Insert(new UserEntity { Login = login });
            }
            catch (MongoWriteException)
            {
                var findResult = userCollection.Find(userEntity => userEntity.Login == login);
                return findResult.SingleOrDefault();
            }
        }

        public void Update(UserEntity user)
        {
            userCollection.ReplaceOne(userEntity => userEntity.Id == user.Id, user);
        }

        public void Delete(Guid id)
        {
            userCollection.DeleteOne(userEntity => userEntity.Id == id);
        }

        // Для вывода списка всех пользователей (упорядоченных по логину)
        // страницы нумеруются с единицы
        public PageList<UserEntity> GetPage(int pageNumber, int pageSize)
        {
            var totalCount = userCollection.CountDocuments(FilterDefinition<UserEntity>.Empty);
            var foundUsers = userCollection
                .Find(FilterDefinition<UserEntity>.Empty)
                .SortBy(userEntity => userEntity.Login)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToList();
            return new PageList<UserEntity>(foundUsers, totalCount, pageNumber, pageSize);
        }

        // Не нужно реализовывать этот метод
        public void UpdateOrInsert(UserEntity user, out bool isInserted)
        {
            throw new NotImplementedException();
        }
    }
}