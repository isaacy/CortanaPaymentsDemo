
namespace CortanaPayment.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Helpers;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Connector.Payments;
    using Models;
    using Properties;
    using Services;

    [Serializable]
    public class PaymentDialog : IDialog<string>
    {
        private Donation donation;

        public const string CARTKEY = "CART_ID";

        public PaymentDialog(Donation donation)
        {
            this.donation = donation;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await Task.FromResult(true);
            context.Wait(this.MessageReceivedAsync);
        }


        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {

            await context.SayAsync("confirming your payment...");

            var cartId = donation.Id.ToString();
            context.ConversationData.SetValue(CARTKEY, cartId);
            context.ConversationData.SetValue(cartId, context.Activity.From.Id);

            var replyMessage = context.MakeMessage();
            replyMessage.Attachments = new List<Attachment>();
            replyMessage.Speak = "Please confirm payment.";
            replyMessage.Attachments.Add(await BuildPaymentCardAsync(cartId, donation));
            replyMessage.InputHint = InputHints.ExpectingInput;

            await context.PostAsync(replyMessage);

            context.Wait(this.AfterPurchaseAsync);
        }

        private static PaymentRequest BuildPaymentRequest(string cartId, Donation item, MicrosoftPayMethodData methodData)
        {
            return new PaymentRequest
            {
                Id = cartId,
                Expires = TimeSpan.FromDays(1).ToString(),
                MethodData = new List<PaymentMethodData>
                {
                    methodData.ToPaymentMethodData()
                },
                Details = new PaymentDetails
                {
                    Total = new PaymentItem
                    {
                        Label = Resources.Wallet_Label_Total,
                        Amount = new PaymentCurrencyAmount
                        {
                            Currency = item.Currency,
                            Value = Convert.ToString(item.Amount, CultureInfo.InvariantCulture)
                        },
                        Pending = true
                    },
                    DisplayItems = new List<PaymentItem>
                    {
                        new PaymentItem
                        {
                            Label = item.ToString(),
                            Amount = new PaymentCurrencyAmount
                            {
                                Currency = item.Currency,
                                Value = item.Amount.ToString(CultureInfo.InvariantCulture)
                            }
                        },
                        /*
                        new PaymentItem
                        {
                            Label = Resources.Wallet_Label_Shipping,
                            Amount = new PaymentCurrencyAmount
                            {
                                Currency = item.Currency,
                                Value = "0.00"
                            },
                            Pending = true
                        },
                        new PaymentItem
                        {
                            Label = Resources.Wallet_Label_Tax,
                            Amount = new PaymentCurrencyAmount
                            {
                                Currency = item.Currency,
                                Value = "0.00"
                            },
                            Pending = true
                        }
                        */
                    }
                },
                Options = new PaymentOptions
                {
                    RequestShipping = false,
                    RequestPayerEmail = true,
                    RequestPayerName = false,
                    RequestPayerPhone = false,
                    //ShippingType = PaymentShippingTypes.Shipping
                }
            };
        }

        private static Task<Attachment> BuildPaymentCardAsync(string cartId, Donation item)
        {
            var heroCard = new HeroCard
            {
                Title = item.ToString(),
                Subtitle = $"{item.Currency} {item.Amount.ToString("F")}",
                Text = item.DonorName,
                Images = new List<CardImage>
                {
                    new CardImage
                    {
                        Url = item.Recipient.ImageUrl
                    }
                },
                Buttons = new List<CardAction>
                {
                    new CardAction
                    {
                        Title = "Confirm Payment",
                        Type = PaymentRequest.PaymentActionType,
                        Value = BuildPaymentRequest(cartId, item, PaymentService.GetAllowedPaymentMethods())
                    }
                }
            };

            return Task.FromResult(heroCard.ToAttachment());
        }

        private static ReceiptItem BuildReceiptItem(string title, string subtitle, string price, string imageUrl)
        {
            return new ReceiptItem(
                title: title,
                subtitle: subtitle,
                price: price,
                image: new CardImage(imageUrl));
        }

        private static async Task<Attachment> BuildReceiptCardAsync(PaymentRecord paymentRecord, Donation donationItem)
        {
            
           
            var receiptItems = new List<ReceiptItem>();

            receiptItems.AddRange(paymentRecord.Items.Select<PaymentItem, ReceiptItem>(item =>
            {
                if (donationItem.ToString().Equals(item.Label))
                {
                    return PaymentDialog.BuildReceiptItem(
                        donationItem.ToString(),
                        donationItem.Description,
                        $"{donationItem.Currency} {donationItem.Amount.ToString("F")}",
                        donationItem.Recipient.ImageUrl);
                }
                else
                {
                    return PaymentDialog.BuildReceiptItem(
                        item.Label,

                        null,
                        $"{item.Amount.Currency} {item.Amount.Value}",
                        null);
                }
            }));

            var receiptCard = new ReceiptCard
            {
                Title = Resources.RootDialog_Receipt_Title,
                Facts = new List<Fact>
                {
                    new Fact(Resources.RootDialog_Receipt_OrderID, paymentRecord.OrderId.ToString()),
                    new Fact(Resources.RootDialog_Receipt_PaymentMethod, paymentRecord.MethodName)
                },
                Items = receiptItems,
                Tax = null, // Sales Tax is a displayed line item, leave this blank
                Total = $"{paymentRecord.Total.Amount.Currency} {paymentRecord.Total.Amount.Value}"
            };

            return receiptCard.ToAttachment();
        }

        private async Task AfterPurchaseAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            // clean up state store after completion
            var cartId = context.ConversationData.Get<string>(CARTKEY);
            context.ConversationData.RemoveValue(CARTKEY);
            context.ConversationData.RemoveValue(cartId);

            var activity = await argument as Activity;
            var paymentRecord = activity?.Value as PaymentRecord;

            if (paymentRecord == null)
            {
                // show error
                var errorMessage = activity.Text;
                var message = context.MakeMessage();
                message.Text = errorMessage;
                message.InputHint = InputHints.IgnoringInput;

                await context.PostAsync(message);

                context.Wait(this.MessageReceivedAsync);
            }
            else
            {
                // show receipt
                var message = context.MakeMessage();
                message.Text = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.RootDialog_Receipt_Text,
                    paymentRecord.OrderId,
                    paymentRecord.PaymentProcessor);

                message.Attachments.Add(await BuildReceiptCardAsync(paymentRecord, donation));
                message.InputHint = InputHints.IgnoringInput;
                await context.PostAsync(message);

            }
        }
        



    }
}