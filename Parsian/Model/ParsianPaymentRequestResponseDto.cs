namespace Parsian.Model
{
    public class ParsianPaymentRequestResponseDto
    {
        public string Message { get; set; }
        public short Status { get; set; }
        public long Token { get; set; }
        public string RedirectUrl { get; set; }
    }
}
