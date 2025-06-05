using Xunit;
using Moq;
using FeedBackApp.API.Controllers;
using FeedbackApp.Application.Dtos;
using FeedbackApp.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MassTransit;
using System.Threading.Tasks;
using System.Threading;

namespace FeedbackApp.Api.Tests
{
    public class FeedbackControllerTests
    {
        private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
        private readonly Mock<ILogger<FeedbackController>> _mockLogger;
        private readonly FeedbackController _controller;

        public FeedbackControllerTests()
        {
            _mockPublishEndpoint = new Mock<IPublishEndpoint>();
            _mockLogger = new Mock<ILogger<FeedbackController>>();
            _controller = new FeedbackController(_mockLogger.Object, _mockPublishEndpoint.Object);
        }

        [Fact]
        public async Task PostFeedback_WithValidModel_ReturnsOkResultAndPublishesMessage()
        {
            // Arrange
            var feedbackDto = new FeedbackDto { Name = "Test User", Email = "test@example.com", Message = "Test message" };

            // Act
            var result = await _controller.PostFeedback(feedbackDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            dynamic responseValue = okResult.Value;
            Assert.Equal("Geri bildiriminiz başarıyla alındı ve işlenmek üzere sıraya eklendi.", responseValue.GetType().GetProperty("message").GetValue(responseValue, null));


            _mockPublishEndpoint.Verify(p => p.Publish(
                It.Is<FeedbackReceived>(msg =>
                    msg.Name == feedbackDto.Name &&
                    msg.Email == feedbackDto.Email &&
                    msg.Message == feedbackDto.Message),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task PostFeedback_WithInvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var feedbackDto = new FeedbackDto { Name = "Test User" };
            _controller.ModelState.AddModelError("Email", "Email alanı zorunludur.");
            _controller.ModelState.AddModelError("Message", "Mesaj alanı zorunludur.");

            // Act
            var result = await _controller.PostFeedback(feedbackDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsAssignableFrom<SerializableError>(badRequestResult.Value);

            _mockPublishEndpoint.Verify(p => p.Publish(
                It.IsAny<FeedbackReceived>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}