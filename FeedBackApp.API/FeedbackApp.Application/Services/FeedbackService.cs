using FeedbackApp.Application.Interfaces;
using FeedbackApp.Application.Dtos;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FeedbackApp.Application.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<FeedbackService> _logger;

        public FeedbackService(
            IPublishEndpoint publishEndpoint,
            ILogger<FeedbackService> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task ProcessFeedbackAsync(FeedbackDto feedbackDto)
        {
            if (feedbackDto == null)
            {
                _logger.LogWarning("İşlenecek geri bildirim verisi (FeedbackDto) null geldi.");

                throw new ArgumentNullException(nameof(feedbackDto));

                return;
            }

            try
            {
                _logger.LogInformation("Geri bildirim işleniyor (MassTransit ile): {@FeedbackDto}", feedbackDto);

                await _publishEndpoint.Publish<FeedbackDto>(feedbackDto);

                _logger.LogInformation("Geri bildirim başarıyla MassTransit üzerinden yayınlandı: {Name}, {Email}", feedbackDto.Name, feedbackDto.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Geri bildirim MassTransit üzerinden yayınlanırken hata oluştu: {@FeedbackDto}", feedbackDto);

                throw;
            }
        }
    }
}