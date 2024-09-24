namespace Parsian.Model
{
    public class ConfirmResponseData
    {
        public short Status { get; set; }             // وضعیت تراکنش (مثلاً 0 به معنی موفقیت)
        public string CardNumberMasked { get; set; }   // شماره کارت به صورت ماسک‌شده
        public long RRN { get; set; }                 // شماره مرجع تراکنش
        public long Token { get; set; }               // توکن تراکنش
        public int TerminalNumber { get; set; }
        public long OrderId { get; set; }
        public string? Amount { get; set; }
        public string? DiscountAmount { get; set; }
    }
}
