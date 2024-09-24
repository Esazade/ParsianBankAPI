using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Parsian.Model;
using Parsian.Service;

namespace Parsian.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : Controller
    {
        private readonly SoapClient _soapClient;

        public HomeController(SoapClient soapClient)
        {
            _soapClient = soapClient;
        }

        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmPayment1([FromBody] ConfirmPaymentRequest request)
        {
            try
            {
                var response = await _soapClient.ConfirmPaymentAsync("c6l3E0RVL86weuVISl3Q", request.Token);

                // تجزیه پاسخ و بازگشت نتیجه نهایی به View یا API
                if (response.Status == 0)
                {
                    return Ok(response);  // پرداخت موفقیت‌آمیز بود
                    Callback(response.Status, response.Token, response.OrderId);
                }
                else
                {
                    return BadRequest(response);  // پرداخت ناموفق بود
                }

                
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Internal server error", Details = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Callback(short status,long token,long orderId)
        {
            try
            {
                // دریافت وضعیت و اطلاعات کلیدی از فرم
                //var status = Convert.ToInt16(Request.Form["status"]);
                //var token = Convert.ToInt64(Request.Form["Token"]);
                //var orderId = Convert.ToInt64(Request.Form["OrderId"]);

                // اگر کاربر از پرداخت انصراف داده باشد
                if (status == -138)
                {
                    ViewBag.Error = $"status: {status} - کاربر از پرداخت انصراف داده است! Token: {token}, شماره سفارش: {orderId}";
                    return View("Error");
                }

                // بررسی وضعیت تراکنش موفق و داشتن شماره مرجع معتبر
                if (status == 0 && Convert.ToInt64(Request.Form["RRN"]) > 0L)
                {
                    // دریافت اطلاعات تکمیلی
                    var terminalNumber = Convert.ToInt32(Request.Form["TerminalNo"]);
                    var rrn = Convert.ToInt64(Request.Form["RRN"]);
                    var amountAsString = Request.Form["Amount"];
                    var discountAmount = Request.Form["SwAmount"];
                    var cardNumberHashed = Request.Form["HashCardNumber"];

                    // ایجاد نمونه‌ای از مدل ConfirmResponseData
                    ConfirmResponseData data = new ConfirmResponseData
                    {
                        Amount = amountAsString,
                        TerminalNumber = terminalNumber,
                        DiscountAmount = discountAmount,
                        OrderId = orderId
                    };

                    // به صورت دستی درخواست SOAP را ارسال کنید
                    var loginAccount = "c6l3E0RVL86weuVISl3Q";
                    var confirm = await _soapClient.ConfirmPaymentAsync(loginAccount, token);

                    // پر کردن داده‌ها از پاسخ سرویس
                    data.CardNumberMasked = confirm.CardNumberMasked;
                    data.RRN = confirm.RRN;
                    data.Token = confirm.Token;
                    data.Status = confirm.Status;

                    // بررسی وضعیت تراکنش و بازگشت پاسخ مناسب
                    if (data.Status == 0)
                    {
                        // تراکنش موفقیت آمیز بود
                        return View("CallBackResponse", data);
                    }
                    else
                    {
                        // عملیات پرداخت انجام نشد
                        ViewBag.Error = $"تراکنش ناموفق بود: {data.Status}";
                        return View("CallBackResponse", data);
                    }
                }
                else
                {
                    // سایر حالت‌ها برای ناموفق بودن تراکنش
                    ViewBag.Error = $"status : {status} - Token : {token} - شماره سفارش : {orderId}";
                    return View("Error");
                }
            }
            catch (Exception e)
            {
                // مدیریت خطاها و نمایش پیام خطا
                ViewBag.Error = $"خطا رخ داد: {e.Message}";
                return View("Error");
            }
        }


        [HttpPost("SalePayment")]
        public async Task<IActionResult> SalePayment()
        {
            try
            {
                var request = new ParsianRequestDto()
                {
                    AdditionalData = "2354",
                    Amount = 10000,
                    CallBackUrl = "https://www.toorangco.com/shopping/irankishverify",
                    OrderId = 2586,
                    Originator = "تورنگ"
                };

                //var response = await _soapClient.ConfirmPaymentAsync2(request, "c6l3E0RVL86weuVISl3Q");

                var report = await _soapClient.SalePaymentManualAsync(request, "c6l3E0RVL86weuVISl3Q");
                return Ok(report);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
