using System.ComponentModel.DataAnnotations;

namespace FeedbackApp.Application.Dtos
{
    public class FeedbackDto
    {
        [Required(ErrorMessage = "İsim alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "İsim alanı en fazla 50 karakter olabilir.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Email alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        [StringLength(50, ErrorMessage = "Email alanı en fazla 50 karakter olabilir.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Mesaj alanı zorunludur.")]
        [StringLength(400, ErrorMessage = "Mesaj alanı en fazla 400 karakter olabilir.")]
        public string? Message { get; set; }
    }
}


// req+nullable kullanım sebebi: doldurulmasını zorunlu kılar ve olası nullable hatalarının önüne geçecektir.