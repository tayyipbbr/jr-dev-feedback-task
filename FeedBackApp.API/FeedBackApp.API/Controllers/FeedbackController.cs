using FeedbackApp.Application.Dtos;
using FeedbackApp.Application.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FeedBackApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly ILogger<FeedbackController> _logger;
        private readonly IPublishEndpoint _publishEndpoint; // masstransit publish endpointi

        public FeedbackController(ILogger<FeedbackController> logger, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> PostFeedback([FromBody] FeedbackDto feedbackDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Geçersiz model durumu: {@ModelStateErerors}",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Yeni geri bildirim isteği alındı: {@FeedbackData}", feedbackDto);

            //dto->received kontratı
            var feedbackMessage = new FeedbackReceived
            {
                Name = feedbackDto.Name,
                Email = feedbackDto.Email,
                Message = feedbackDto.Message,
                Timestamp = DateTime.UtcNow
            };

            await _publishEndpoint.Publish(feedbackMessage);
            _logger.LogInformation("Geri bildirim mesajı RabbitMQ'ya başarıyla yayınlandı: {@FeedbackMessage}", feedbackMessage);

            return Ok(new { message = "Geri bildiriminiz başarıyla alındı ve işlenmek üzere sıraya eklendi." });


            // rabbitmq bağlantı kontrolünü manuel yapmıyorum.
            // masstransit bu görevi üstleniyor, ben fırlatılan exleri yakalıyorum.


            /* //// TODO: Middleweare çalışması
            try
            {
                await _publishEndpoint.Publish(feedbackMessage);
                _logger.LogInformation("Geri bildirim mesajı RabbitMQ'ya başarıyla yayınlandı: {@FeedbackMessage}", feedbackMessage);

                return Ok(new { message = "Geri bildiriminiz başarıyla alındı ve işlenmek üzere sıraya eklendi." });
            }
            catch (MassTransitException mtEx)
            {
                _logger.LogError(mtEx, "MassTransit ile mesaj yayınlanırken bir hata oluştu (Mesaj Kuyruğu Hatası): {@FeedbackMessage}", feedbackMessage);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Mesaj kuyruğuna erişimde geçici bir sorun yaşanıyor. Lütfen daha sonra tekrar deneyin." });
            }
            catch (TimeoutException timeoutEx)
            {
                _logger.LogError(timeoutEx, "Mesaj yayınlama işlemi zaman aşımına uğradı: {@FeedbackMessage}", feedbackMessage);
                return StatusCode(StatusCodes.Status504GatewayTimeout, new { message = "İşleminiz zaman aşımına uğradı. Lütfen daha sonra tekrar deneyin." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Geri bildirim mesajı RabbitMQ'ya yayınlanırken bir hata oluştu: {@FeedbackMessage}", feedbackMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Geri bildiriminiz işlenirken bir sunucu hatası oluştu. Lütfen daha sonra tekrar deneyin." });
            }

            */
        }
    }
}
