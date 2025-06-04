using System.Threading.Tasks;
using FeedbackApp.Application.Dtos;

namespace FeedbackApp.Application.Interfaces
{
    public interface IFeedbackService
    {
        Task ProcessFeedbackAsync(FeedbackDto feedbackDto);

    }
}