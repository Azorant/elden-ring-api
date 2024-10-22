﻿using EldenRingAPI.Models;
using MongoDB.Driver;

namespace EldenRingAPI
{
    public class Database
    {
        private readonly MongoClient client;
        private const string dbName = "EldenRingAPI";
        private const string weapons = "weapons";

        public Database()
        {
            client = new MongoClient(Environment.GetEnvironmentVariable("MONGO_URI"));
        }

        private IMongoCollection<T> getCollection<T>(in string collection)
        {
            var database = client.GetDatabase(dbName);
            return database.GetCollection<T>(collection);
        }

        public async Task<Weapon?> getWeaponByURL(string url)
        {
            var collection = getCollection<Weapon>(weapons);
            var weapon = await collection.Find(w => w.link == url).FirstOrDefaultAsync();
            return weapon;
        }

        public async Task<Weapon?> getWeaponByID(string id)
        {
            var collection = getCollection<Weapon>(weapons);
            var weapon = await collection.Find(w => w.id == id).FirstOrDefaultAsync();
            return weapon;
        }

        public async Task<Results<Weapon>> getWeapons(int page, int perPage)
        {
            var collection = getCollection<Weapon>(weapons);
            var countFacet = AggregateFacet.Create("count", PipelineDefinition<Weapon, AggregateCountResult>.Create(new[] {
                PipelineStageDefinitionBuilder.Count<Weapon>()
            }));
            var dataFacet = AggregateFacet.Create("data", PipelineDefinition<Weapon, Weapon>.Create(new[] {
                PipelineStageDefinitionBuilder.Sort(Builders<Weapon>.Sort.Ascending(x => x.name)),
                PipelineStageDefinitionBuilder.Skip<Weapon>((page - 1) * perPage),
                PipelineStageDefinitionBuilder.Limit<Weapon>(perPage),
            }));

            var filter = Builders<Weapon>.Filter.Empty;
            var aggregation = await collection.Aggregate()
                .Match(filter)
                .Facet(countFacet, dataFacet)
                .ToListAsync();

            var count = aggregation.First()
                .Facets.First(x => x.Name == "count")
                .Output<AggregateCountResult>()
                ?.FirstOrDefault()
                ?.Count ?? 0;

            var pages = Math.Ceiling((decimal)count / perPage);

            var data = aggregation.First()
                .Facets.First(x => x.Name == "data")
                .Output<Weapon>().ToList();

            return new Results<Weapon> { count = count, pages = pages, page = page, items = data };
        }

        public async Task<bool> setWeapon(Weapon document)
        {
            var collection = getCollection<Weapon>(weapons);
            var result = await collection.ReplaceOneAsync(w => w.id == document.id, document, new ReplaceOptions { IsUpsert = true });
            return result.IsAcknowledged;
        }
    }
}
