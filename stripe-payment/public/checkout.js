
// We'll initialize these on page load and mount the
// Stripe Element for the card input. We need access to
// `stripe` and `cardElement` later when we checkout.
let stripe, cardElement, elements;

async function registerElements(pubKey) {
    stripe = Stripe(pubKey);
    //const elements = stripe.elements();
    //cardElement = elements.create("card", {
    //    style: {
    //        base: {
    //            color: '#495057',
    //            fontWeight: 400,
    //        },
    //    },
    //    hidePostalCode: true
    //});

    //cardElement.mount("#card-element");
}

//async function checkout(paymentIntentClientSecret) {
//    console.log(`Checkout called with: ${paymentIntentClientSecret}`);

//    const cardholderNameInput = "Wassim";

//    const { paymentIntent, error } = await stripe.confirmCardPayment(
//        paymentIntentClientSecret, {
//        payment_method: {
//            card: cardElement,
//            billing_details: {
//                name: cardholderNameInput,
//            }
//        },
//        // return_url: `${window.location.origin}/checkout/success`
//    }
//    )


//    if (error) {
//        alert(error.message);
//    } else {
//        window.location.href = "/success.html";
//    }
//}


async function registerPaymentElement(paymentIntentClientSecret) {


    const options = {
        clientSecret: paymentIntentClientSecret,
        // Fully customizable with appearance API.
        appearance: { theme: 'stripe' },
    };

    // Set up Stripe.js and Elements to use in checkout form, passing the client secret obtained in step 2
    elements = stripe.elements(options);
    // Create and mount the Payment Element
    const paymentElement = elements.create('payment',
        {
            fields: {
                billingDetails: {
                    name: 'auto',
                    address: {
                        country: 'never'
                    }
                }
            }
        });
    paymentElement.mount('#payment-element');
}
