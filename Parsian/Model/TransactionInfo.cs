namespace Parsian.Model
{
    public class TransactionInfo
    {
        public string Amount { get; set; }           // مبلغ تراکنش
        public int TerminalNumber { get; set; }      // شماره ترمینال
        public string DiscountAmount { get; set; }   // مقدار تخفیف (در صورت وجود)
        public long OrderId { get; set; }            // شناسه سفارش
    }
}
