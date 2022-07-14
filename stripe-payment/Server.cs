using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stripe;
using Stripe.Checkout;
using StripeExample;

public class StripeOptions
{
    public string option { get; set; }
}

namespace server.Controllers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebHost.CreateDefaultBuilder(args)
              .UseUrls("http://0.0.0.0:4242")
              .UseWebRoot("public")
              .UseStartup<Startup>()
              .Build()
              .Run();
        }
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddNewtonsoftJson();
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // This is your test secret API key.
            StripeConfiguration.ApiKey = "sk_test_51LAKSVIj6R8IWHe4AOZtq7KcDNGl8hR8XLIxMULKZmIJkhqMIgi2sRAdLjIPLw80CIlpAvN1KpuVT827VLxJVM6X00rN0oIHg8";
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
            app.UseRouting();
            app.UseStaticFiles();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }

    [Route("checkout")]
    [ApiController]
    public class CheckoutApiController : Controller
    {
        [HttpPost("create")]
        public ActionResult Create()
        {
            string transferGrp = "TR0002";
            var options = new PaymentIntentCreateOptions
            {
                Amount = 10000,
                Currency = "aud",
                PaymentMethodTypes = new List<string>
                {
                    "card",
                    "au_becs_debit",
                    "afterpay_clearpay"
                },
                TransferGroup=transferGrp,

                //TransferData = new PaymentIntentTransferDataOptions
                //{
                //    Destination = "acct_1LJ35xRFiwFctZgD",
                //}
            };

            var service = new PaymentIntentService();
            var paymentIntent = service.Create(options);

            return Ok(paymentIntent.ClientSecret);
        }

        [HttpPost("transfer/{paymentIntentId}")]
        public ActionResult Transfer(string paymentIntentId)
        {

            var payIntentService = new Stripe.PaymentIntentService();
            var paymentIntent = payIntentService.Get(paymentIntentId);

            var charge = paymentIntent.Charges.Data.SingleOrDefault();
            
            // Split charges for two different accounts
            // Create a Transfer to the clinic account (later):
            var transferOptions = new TransferCreateOptions
            {
                Amount = 7000,
                Currency = "aud",
                Destination = "acct_1LEwkrRJohca2bym",
                TransferGroup = paymentIntent.TransferGroup,
                SourceTransaction = charge.Id
            };

            var transferService = new TransferService();
            var firstTransfer = transferService.Create(transferOptions);

            // Create a second Transfer to dr. account (later):
            var secondTransferOptions = new TransferCreateOptions
            {
                Amount = 2000,
                Currency = "aud",
                Destination = "acct_1LEwgZRK3EgITr3u",
                TransferGroup = paymentIntent.TransferGroup,
                SourceTransaction = charge.Id
            };
            var secondTransfer = transferService.Create(secondTransferOptions);

            return Ok();
        }
    }

    [Route("create-stripe-account")]
    [ApiController]
    public class AccountApiController : Controller
    {
        [HttpPost]
        public ActionResult Create()
        {

            string redirect_uri = "http://localhost:4242/token";
            string clientId = "ca_Ls4hdT0Ard53YNfcPmrjZqaMtC8TdEkS";
            string state = "123";
            string queryString = $"redirect_uri={redirect_uri}&" +
                $"stripe_user[business_type]=individual&stripe_user[first_name]=Dr.&stripe_user[last_name]=Strange&stripe_user[email]=strange@test.com" +
                $"&stripe_user[country]=au&state={state}&client_id={clientId}";

            string authURL = "https://connect.stripe.com/express/oauth/authorize?" + queryString;

            Response.Headers.Add("Location", authURL);

            return new StatusCodeResult(303);
        }
    }

    [Route("token")]
    [ApiController]
    public class TokenApiController : Controller
    {
        [HttpGet]
        public ActionResult getToken()
        {
            // Get account id after registration
            string accessCode = Request.Query["code"].ToString();
            string state = Request.Query["state"].ToString();
            if (accessCode != "")
            {
                APIClient stripeConnect = new("https://connect.stripe.com/");

                var body =
                new
                {
                    grant_type = "authorization_code",
                    client_id = "ca_Ls4hdT0Ard53YNfcPmrjZqaMtC8TdEkS",
                    client_secret = "sk_test_51LAKSVIj6R8IWHe4AOZtq7KcDNGl8hR8XLIxMULKZmIJkhqMIgi2sRAdLjIPLw80CIlpAvN1KpuVT827VLxJVM6X00rN0oIHg8",
                    code = accessCode
                };

                var tokenAuthResultTask = stripeConnect.PostAsJsonAsync(body, "oauth/token");

                tokenAuthResultTask.Wait();
            }

            return new StatusCodeResult(303);
        }
    }
}