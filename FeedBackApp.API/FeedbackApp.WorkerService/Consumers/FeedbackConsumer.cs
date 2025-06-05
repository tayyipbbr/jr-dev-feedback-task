using MassTransit;
using FeedbackApp.Application.Contracts;
using FeedbackApp.WorkerService.Models; 
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace FeedbackApp.WorkerService.Consumers
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string CollectionName { get; set; } = string.Empty;
    }

    public class FeedbackConsumer : IConsumer<FeedbackReceived>
    {
        private readonly ILogger<FeedbackConsumer> _logger;
        private readonly IMongoCollection<FeedbackDocument> _feedbackCollection;

        public FeedbackConsumer(ILogger<FeedbackConsumer> logger, IOptions<MongoDbSettings> mongoDbSettings)
        {
            _logger = logger;
            var settings = mongoDbSettings.Value;
            if (string.IsNullOrEmpty(settings.ConnectionString) ||
                string.IsNullOrEmpty(settings.DatabaseName) ||
                string.IsNullOrEmpty(settings.CollectionName))
            {
                _logger.LogError("MongoDB settings are not configured properly.");
                // TODO: detaylı catch throw yapılacak / veya mw
                throw new InvalidOperationException("MongoDB settings are not configured properly.");
            }

            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _feedbackCollection = database.GetCollection<FeedbackDocument>(settings.CollectionName);
            _logger.LogInformation("MongoDB client initialized. Database: {DatabaseName}, Collection: {CollectionName}", settings.DatabaseName, settings.CollectionName);
        }

        public async Task Consume(ConsumeContext<FeedbackReceived> context)
        {
            var message = context.Message;
            _logger.LogInformation("Received Feedback: Name='{Name}', Email='{Email}', Timestamp='{Timestamp}'",
                                   message.Name, message.Email, message.Timestamp);

            var feedbackDocument = new FeedbackDocument
            {
                Name = message.Name,
                Email = message.Email,
                Message = message.Message,
                ReceivedAt = message.Timestamp,
                ProcessedAt = DateTime.UtcNow
            };

            try
            {
                await _feedbackCollection.InsertOneAsync(feedbackDocument);
                _logger.LogInformation("Feedback successfully saved to MongoDB. Document ID: {DocumentId}", feedbackDocument.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving feedback to MongoDB. Message: {@FeedbackMessage}", message);
                // masstransitin kendi handlingi var , eklenecek
                throw;
            }
        }
    }
}