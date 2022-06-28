// See https://aka.ms/new-console-template for more information
using Stripe;


StripeConfiguration.ApiKey = "sk_test_51LAKSVIj6R8IWHe4AOZtq7KcDNGl8hR8XLIxMULKZmIJkhqMIgi2sRAdLjIPLw80CIlpAvN1KpuVT827VLxJVM6X00rN0oIHg8";

string transferGroup = "TRAN0004";
// Create a PaymentIntent:
var paymentIntentOptions = new PaymentIntentCreateOptions
{
    Amount = 2000,
    Currency = "aud",
    TransferGroup = transferGroup,
};
var paymentIntentService = new PaymentIntentService();
var paymentIntent = paymentIntentService.Create(paymentIntentOptions);

// Create a Transfer to the connected account
var transferOptions = new TransferCreateOptions
{
    Amount = 1000,
    Currency = "aud",
    Destination = "acct_1LEwgZRK3EgITr3u",
    TransferGroup = transferGroup,
};

var transferService = new TransferService();
var transfer = transferService.Create(transferOptions);

// Create a second Transfer to another connected account
var secondTransferOptions = new TransferCreateOptions
{
    Amount = 500,
    Currency = "aud",
    Destination = "acct_1LEtgARLQXlwaSAi",
    TransferGroup = transferGroup,
};
var secondTransfer = transferService.Create(secondTransferOptions);

// Create a third Transfer to another connected account
var thirdTransferOptions = new TransferCreateOptions
{
    Amount = 300,
    Currency = "aud",
    Destination = "acct_1LAhpnRAf7Tw3tEG",
    TransferGroup = transferGroup,
};
var thirdTransfer = transferService.Create(secondTransferOptions);


Console.WriteLine("Hello, World!");
