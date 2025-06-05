using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MassTransit;
using FeedbackApp.Application.Contracts; 
using FeedbackApp.WorkerService.Models;
using System;
using System.Threading.Tasks;

namespace FeedbackApp.WorkerService.Consumers
{
    public class FeedbackConsumer : IConsumer<FeedbackReceived>
    {
        private readonly ILogger<FeedbackConsumer> _logger;
        private readonly IMongoCollection<FeedbackDocument> _feedbackCollection;

        public FeedbackConsumer(ILogger<FeedbackConsumer> logger, IMongoCollection<FeedbackDocument> feedbackCollection)
        {
            _logger = logger;
            _feedbackCollection = feedbackCollection;
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
                throw;
            }
        }
    }
}