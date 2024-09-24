using Parsian.Model;
using System;
using System.Text;
using System.Xml.Linq;

namespace Parsian.Service
{
    public class SoapClient
    {
        private readonly HttpClient _httpClient;

        public SoapClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ConfirmResponseData> ConfirmPaymentAsync(string loginAccount, long token)
        {
            var soapRequest = $@"
    <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:web=""https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService"">
        <soapenv:Header/>
        <soapenv:Body>
            <web:ConfirmPayment>
                <web:requestData>
                    <web:LoginAccount>{loginAccount}</web:LoginAccount>
                    <web:Token>{token}</web:Token>
                </web:requestData>
            </web:ConfirmPayment>
        </soapenv:Body>
    </soapenv:Envelope>";

            // ارسال درخواست به سرویس SOAP
            //var content = new StringContent(soapRequest, Encoding.UTF8, "application/soap+xml");
            var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");

            var response = await _httpClient.PostAsync("https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService.asmx", content);

            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            // تجزیه XML پاسخ برای استخراج اطلاعات لازم
            var confirmResponseData = ParseSoapResponse(responseContent);

            return confirmResponseData;
        }

        private ConfirmResponseData ParseSoapResponse(string xmlContent)
        {
            var xDoc = XDocument.Parse(xmlContent);
            XNamespace ns = "https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService";

            var responseData = new ConfirmResponseData
            {
                Status = (short)xDoc.Descendants(ns + "Status").FirstOrDefault(),
                CardNumberMasked = (string)xDoc.Descendants(ns + "CardNumberMasked").FirstOrDefault(),
                RRN = (long)xDoc.Descendants(ns + "RRN").FirstOrDefault(),
                Token = (long)xDoc.Descendants(ns + "Token").FirstOrDefault()
            };

            return responseData;
        }

        public async Task<ConfirmResponseData> ConfirmPaymentAsync2(ParsianRequestDto req, string loginAccount)
        {
            var soapRequest = $@"
<soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
    <soap12:Body>
        <web:SalePaymentRequest xmlns:web=""https://pec.shaparak.ir/NewIPGServices/Sale/SaleService"">
            <web:requestData>
                <web:LoginAccount>{loginAccount}</web:LoginAccount>
                <web:Amount>{req.Amount}</web:Amount>
                <web:OrderId>{req.OrderId}</web:OrderId>
                <web:CallBackUrl>{req.CallBackUrl}</web:CallBackUrl>
                <web:AdditionalData>{req.AdditionalData}</web:AdditionalData>
                <web:Originator>{req.Originator}</web:Originator>
            </web:requestData>
        </web:SalePaymentRequest>
    </soap12:Body>
</soap12:Envelope>";

            // ارسال درخواست به سرویس SOAP
            //var content = new StringContent(soapRequest, Encoding.UTF8, "application/soap+xml");
            var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
        
            var response = await _httpClient.PostAsync("https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService", content);

            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            // تجزیه XML پاسخ برای استخراج اطلاعات لازم
            var confirmResponseData = ParseSoapResponse(responseContent);

            return confirmResponseData;
        }

        public async Task<ParsianPaymentRequestResponseDto> SalePaymentManualAsync(ParsianRequestDto req, string loginAccount)
        {
            var soapRequest = $@"
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Body>
        <SalePaymentRequest xmlns=""https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService"">
            <requestData>
                <LoginAccount>{loginAccount}</LoginAccount>
                <Amount>{req.Amount}</Amount>
                <OrderId>{req.OrderId}</OrderId>
                <CallBackUrl>{req.CallBackUrl}</CallBackUrl>
                <AdditionalData>{req.AdditionalData}</AdditionalData>
                <Originator>{req.Originator}</Originator>
            </requestData>
        </SalePaymentRequest>
    </soap:Body>
</soap:Envelope>";

            var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");

            // ایجاد HttpRequestMessage
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://pec.shaparak.ir/NewIPGServices/Sale/SaleService.asmx");
            requestMessage.Content = content;
            requestMessage.Headers.Add("SOAPAction", "https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService/SalePaymentRequest");

            // ارسال درخواست
            var response = await _httpClient.SendAsync(requestMessage);

            // بررسی موفقیت ارسال درخواست
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            // تجزیه XML پاسخ
            var paymentResponseData = ParseSoapSaleResponse(responseContent);

            if (paymentResponseData.Status == 0 && paymentResponseData.Token > 0)
            {
                return new ParsianPaymentRequestResponseDto
                {
                    Message = "Success",
                    Status = 0,
                    Token = paymentResponseData.Token,
                    RedirectUrl = "https://pec.shaparak.ir/NewIPG/?token=" + paymentResponseData.Token
                };
            }
            else
            {
                return new ParsianPaymentRequestResponseDto
                {
                    Message = paymentResponseData.Message,
                    Status = paymentResponseData.Status,
                    Token = paymentResponseData.Token
                };
            }


        }

        private ParsianPaymentResponseData ParseSoapSaleResponse(string responseContent)
        {
            var doc = XDocument.Parse(responseContent);
            XNamespace ns = "https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService";

            // استخراج داده‌ها با استفاده از Linq to XML
            var token = (long?)doc.Descendants(ns + "Token").FirstOrDefault();
            var Status = (int?)doc.Descendants(ns + "Status").FirstOrDefault();
            var message = (string)doc.Descendants(ns + "Message").FirstOrDefault();

            // بررسی null بودن داده‌ها
            if (token == null || Status == null || message == null)
            {
                throw new ArgumentNullException("element", "One of the required elements is missing in the response XML.");
            }

            return new ParsianPaymentResponseData
            {
                Token = token.Value,
                Status = (short)Status.Value,
                Message = message
            };
        }



    }

}
