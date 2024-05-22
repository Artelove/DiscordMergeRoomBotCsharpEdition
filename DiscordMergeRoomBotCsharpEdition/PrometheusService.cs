using MongoDB.Bson;
using MongoDB.Driver;
using Prometheus;

namespace DiscordMergeRoomBotCsharpEdition
{

    public class PrometheusService : IHostedService
    {
        private readonly IMongoClient _mongoClient;

        public PrometheusService(IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var counter = Metrics.CreateCounter("example_counter", "An example counter");
            counter.Inc();

            // Начало сбора метрик MongoDB и других необходимых метрик
            CollectMongoMetrics();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void CollectMongoMetrics()
        {
            var database = _mongoClient.GetDatabase("mergeRoomBot");
            var collection = database.GetCollection<BsonDocument>("projects");
            var count = collection.CountDocuments(FilterDefinition<BsonDocument>.Empty);
            var gauge = Metrics.CreateGauge("mongo_collection_count", "Number of documents in MongoDB collection");
            gauge.Set(count);
        }
    }
}
