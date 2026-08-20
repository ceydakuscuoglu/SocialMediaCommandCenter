namespace ShakyFruits.API.DTOs
{
    public class BotConfirmRequestDto
    {
        // Artık form verilerini değil, sadece Prepare aşamasından aldığımız bileti (Oturum Kimliği) gönderiyoruz.
        public string SessionId { get; set; }
    }
}