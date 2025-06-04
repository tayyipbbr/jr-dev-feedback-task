using MassTransit;
using FeedbackApp.Application.Contracts; // FeedbackReceived mesaj kontratı için
using FeedbackApp.WorkerService.Models; // FeedbackDocument için
using Microsoft.Extensions.Logging;
using MongoDB.Driver; // MongoDB Driver için
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options; // IOptions için

namespace FeedbackApp.WorkerService.Consumers
{
    // MongoDB ayarlarını tutacak sınıf
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
                // Gerçek uygulamada burada daha robust bir hata yönetimi veya başlatma hatası fırlatılabilir.
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
                ReceivedAt = message.Timestamp, // Mesajın API tarafından alındığı zaman
                ProcessedAt = DateTime.UtcNow   // Mesajın consumer tarafından işlendiği zaman
            };

            try
            {
                await _feedbackCollection.InsertOneAsync(feedbackDocument);
                _logger.LogInformation("Feedback successfully saved to MongoDB. Document ID: {DocumentId}", feedbackDocument.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving feedback to MongoDB. Message: {@FeedbackMessage}", message);
                // Burada mesajı bir error kuyruğuna gönderme veya retry mekanizması düşünülebilir.
                // MassTransit'in kendi retry ve error handling mekanizmaları da konfigüre edilebilir.
                throw; // Hatanın MassTransit tarafından yönetilmesi için tekrar fırlat
            }
        }
    }
}