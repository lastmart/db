using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Game.Domain
{
    public class MongoGameRepository : IGameRepository
    {
        private readonly IMongoCollection<GameEntity> gameCollection;
        public const string CollectionName = "games";

        public MongoGameRepository(IMongoDatabase db)
        {
            gameCollection = db.GetCollection<GameEntity>(CollectionName);
        }

        public GameEntity Insert(GameEntity game)
        {
            gameCollection.InsertOne(game);
            return game;
        }

        public GameEntity FindById(Guid gameId)
        {
            return gameCollection
                .Find(game => game.Id == gameId)
                .SingleOrDefault();
        }

        public void Update(GameEntity game)
        {
            var filter = new BsonDocument("_id", new BsonBinaryData(game.Id, GuidRepresentation.Standard));
            gameCollection.ReplaceOne(filter, game);
        }

        // Возвращает не более чем limit игр со статусом GameStatus.WaitingToStart
        public IList<GameEntity> FindWaitingToStart(int limit)
        {
            return gameCollection
                .Find(game => game.Status == GameStatus.WaitingToStart)
                .Limit(limit)
                .ToList();
        }

        // Обновляет игру, если она находится в статусе GameStatus.WaitingToStart
        public bool TryUpdateWaitingToStart(GameEntity game)
        {
            var filter = Builders<GameEntity>.Filter.And(
                Builders<GameEntity>.Filter.Eq(gameEntity => gameEntity.Id, game.Id),
                Builders<GameEntity>.Filter.Eq(gameEntity => gameEntity.Status, GameStatus.WaitingToStart)
            );
            return gameCollection.ReplaceOne(filter, game).ModifiedCount > 0;
        }
    }
}