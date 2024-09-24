namespace Parsian.Model
{
    public class ParsianRequestDto
    {
        public long? Amount { get; set; }
        public long? OrderId { get; set; }
        public string CallBackUrl { get; set; }
        public string AdditionalData { get; set; }
        public string Originator { get; set; }
    }
}
