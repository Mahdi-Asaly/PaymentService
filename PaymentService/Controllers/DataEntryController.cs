using PaymentService.Classes;
using PaymentService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Net;
namespace PaymentService.Controllers
{
    public class DataEntryController : Controller
    {
        //Payment Page
        public ActionResult Index()
        {
            return View(new transactions());
        }
        /*
         * this function recieves client details (it should be encrypted)
         * then it's send it to the Preview page as object.
        */
        [HttpPost]
        public ActionResult Index(transactions trans)
        {
            return RedirectToAction("PreviewPage", trans);
        }
        /*
         *the preview page are the page moment before the payment 
         *the client preview the details then if they willing to pay 
         *he/she should press to the pay button 
        */
        public ActionResult PreviewPage(transactions trans)
        {
            return View(trans);
        }

        /*
         * We should get the Encrypted Information and server should send post request
         * to the company server , there the server checks weither the details of specific
         * client are correct, the local server receives from the company server 
         * response code if the response was success ,the local server stores the information to the database
         * and send it to the final page result .
        */
        [HttpPost]
        public ActionResult PreviewPage(transactions trans, int id=0)
        {
            //first we decrypt the data
            var card_number = AESEncryption.DecryptStringAES(trans.card_number);
            var cvv = AESEncryption.DecryptStringAES(trans.cvv);
            var expdate_month = AESEncryption.DecryptStringAES(trans.expdate_month);
            var expdate_year = AESEncryption.DecryptStringAES(trans.expdate_year);
            //var card_holder_name = AESEncryption.DecryptStringAES(trans.card_holder_name);
            //var card_holder_name = "TEST";

            //this is they key that provided by the company.
            string hash_key = "7ZIQHB7YYN";
            string merchant_id = "3355796"; //change that to your private merchant id
            /*              ##Signature building##
            *   We build the signature by concrating user information one by one 
            *   in addation we add the hash key that provided by the service company.
            */
            string sSignature = Signature.GenerateSHA256("33557961" + "0" + "1" +
                trans.amount + "0" + card_number + hash_key);

            //now we creating a post request url that contains paramaters that suitable for company.
            String sendStr;
            sendStr = "https://process.coriunder.cloud/member/remote_charge.asp?";
            sendStr += "CompanyNum=" + Server.UrlEncode(merchant_id) + "&";
            sendStr += "TransType=" + Server.UrlEncode("0") + "&";
            sendStr += "CardNum=" + Server.UrlEncode(card_number) + "&";
            sendStr += "ExpMonth=" + Server.UrlEncode(expdate_month) + "&";
            sendStr += "ExpYear=" + Server.UrlEncode(expdate_year) + "&";
            sendStr += "Member=" + Server.UrlEncode(trans.card_holder_name) + "&";
            sendStr += "TypeCredit=" + Server.UrlEncode("1") + "&";
            sendStr += "Payments=" + Server.UrlEncode("1") + "&";
            sendStr += "Amount=" + Server.UrlEncode(trans.amount.ToString()) + "&";
            sendStr += "Currency=" + Server.UrlEncode("0") + "&";
            sendStr += "CVV2=" + Server.UrlEncode(cvv) + "&";
            sendStr += "Email=" + Server.UrlEncode(trans.email) + "&";
            sendStr += "ClientIP=" + Server.UrlEncode("222.11.545.166") + "&";
            sendStr += "PhoneNumber=" + Server.UrlEncode(trans.phone) + "&";
            sendStr += "Signature=" + Server.UrlEncode(sSignature);


            //fixing the issue of denied access / ssl
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            //------------- creating the request
            HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(sendStr);
            webReq.Method = "GET";
            //------------- checking the response
            try
            {
                HttpWebResponse webRes = (HttpWebResponse)webReq.GetResponse();
                StreamReader sr = new StreamReader(webRes.GetResponseStream());
                String result = sr.ReadToEnd();
                //UpdateLocalDB(trans, result); //updating the local database with the encrypted informations.
                return RedirectToAction("ResultPage",new { result = result });
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
            }
            //if (card_number == "keyError" && cvv == "keyError")
            return View();
        }

        [HttpGet]
        //this function receive the final result (Accepted/Declined).
        public ActionResult ResultPage(string result)
        {
            if (result.Contains("Reply=663"))
            {
                //Transaction still in processing mode
                TempData["Result"] = "STILL";
                TempData["Code"] = 663;
            }
            else if (result.Contains("Reply=525"))
            {
                //Daily volume limit exceeded
                TempData["Result"] = "EXCEED";
                TempData["Code"] = 525;
            }
            else if (result.Contains("Reply=000"))
            {
                TempData["Result"] = "SUCCESS";
                TempData["Code"] = 000;
                //sucess
            }
            else if (result.Contains("Reply=507"))
            {
                //invalid card
                TempData["Result"] = "INVALIDCARD";
                TempData["Code"] = 507;
            }
            else if (result.Contains("Reply=001"))
            {
                //pending
                TempData["Result"] = "PENDING";
                TempData["Code"] = 001;
            }
            else
            {
                //other case (not included)
                TempData["Result"] = "OTHER";
                TempData["Code"] = 999;
            }
            return View();
        }

        //function that stores information to local db.
        public void UpdateLocalDB(transactions trans, string result)
        {
            using (Model1 db = new Model1())
            {
                try
                {
                    var rand = new Random();
                    var obj = new transactions();
                    obj.card_holder_name = trans.card_holder_name;
                    obj.amount = trans.amount;
                    obj.address = trans.address;
                    obj.card_number = trans.card_number;
                    obj.cvv = trans.cvv;
                    obj.email = trans.email;
                    obj.expdate_month = trans.expdate_month;
                    obj.expdate_year = trans.expdate_year;
                    obj.phone = trans.phone;
                    obj.zip = trans.zip;
                    obj.country = trans.country;
                    obj.city = trans.city;
                    obj.response_cod = getCode(result).ToString();
                    obj.transaction_cod = rand.Next().ToString();
                    obj.status = "";
                    db.transactions.Add(obj);
                    db.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    Exception raise = dbEx;
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            string message = string.Format("{0}:{1}",
                                validationErrors.Entry.Entity.ToString(),
                                validationError.ErrorMessage);
                            raise = new InvalidOperationException(message, raise);
                        }
                    }
                    throw raise;
                }
            }
        }
        public int getCode(string result)
        {
            if (result.Contains("Reply=663"))
            {
                //Transaction still in processing mode
                return 663;
            }
            else if (result.Contains("Reply=525"))
            {
                //Daily volume limit exceeded
                return 525;
            }
            else if (result.Contains("Reply=000"))
            {
                return 000;
                //sucess
            }
            else if (result.Contains("Reply=507"))
            {
                //invalid card
                return 507;
            }
            else if (result.Contains("Reply=001"))
            {
                //pending
                return 001;
            }
            else
            {
                return 999; ///unknown (didnt mentioned all the cases)
            }
        }
    }
}