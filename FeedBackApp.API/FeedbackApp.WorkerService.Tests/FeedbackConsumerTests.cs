using Xunit;
using Moq;
using FeedbackApp.WorkerService.Consumers;
using FeedbackApp.Application.Contracts;
using FeedbackApp.WorkerService.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MassTransit;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace FeedbackApp.WorkerService.Tests
{
    public class FeedbackConsumerTests
    {
        private readonly Mock<ILogger<FeedbackConsumer>> _mockLogger;
        private readonly Mock<IMongoCollection<FeedbackDocument>> _mockMongoCollection;
        private readonly FeedbackConsumer _consumer;

        public FeedbackConsumerTests()
        {
            _mockLogger = new Mock<ILogger<FeedbackConsumer>>();
            _mockMongoCollection = new Mock<IMongoCollection<FeedbackDocument>>();

            _consumer = new FeedbackConsumer(_mockLogger.Object, _mockMongoCollection.Object);
        }

        private Mock<ConsumeContext<FeedbackReceived>> CreateMockConsumeContext(FeedbackReceived message)
        {
            var mockContext = new Mock<ConsumeContext<FeedbackReceived>>();
            mockContext.Setup(x => x.Message).Returns(message);
            return mockContext;
        }

        [Fact]
        public async Task Consume_ValidMessage_InsertsDocumentToMongoDb()
        {
            // Arrange
            var feedbackMessage = new FeedbackReceived { Name = "Test", Email = "test@test.com", Message = "Msg", Timestamp = DateTime.UtcNow };
            var consumeContext = CreateMockConsumeContext(feedbackMessage);

            _mockMongoCollection.Setup(x => x.InsertOneAsync(
                It.IsAny<FeedbackDocument>(),
                null,
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _consumer.Consume(consumeContext.Object);

            // Assert
            _mockMongoCollection.Verify(c => c.InsertOneAsync(
                It.Is<FeedbackDocument>(doc =>
                    doc.Name == feedbackMessage.Name &&
                    doc.Email == feedbackMessage.Email &&
                    doc.Message == feedbackMessage.Message &&
                    doc.ReceivedAt == feedbackMessage.Timestamp),
                null,
                It.IsAny<CancellationToken>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Feedback successfully saved to MongoDB")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Consume_MongoDbError_ThrowsExceptionAndLogsError()
        {
            // Arrange
            var feedbackMessage = new FeedbackReceived { Name = "Test Error", Email = "error@test.com", Message = "Error Msg" };
            var consumeContext = CreateMockConsumeContext(feedbackMessage);

            // MongoWriteException yerine daha genel ve public constructor'ı olan MongoException kullanalım.
            var mongoException = new MongoException("Simulated MongoDB connection error for testing.");

            _mockMongoCollection.Setup(x => x.InsertOneAsync(
                It.IsAny<FeedbackDocument>(),
                null,
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(mongoException); // Simüle edilmiş MongoException'ı fırlat

            // Act & Assert
            // Artık MongoException bekliyoruz.
            await Assert.ThrowsAsync<MongoException>(() => _consumer.Consume(consumeContext.Object));

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error saving feedback to MongoDB")),
                    mongoException, // Loglanan exception'ın bizim fırlattığımızla aynı türde olduğunu (veya onu içerdiğini) doğrula
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}