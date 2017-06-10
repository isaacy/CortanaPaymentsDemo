namespace CortanaPayment.Dialogs 
{
    using System;
    using System.Globalization;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Models;
    using Services;

    [Serializable]
    public class DonateDialog : IDialog<object>
    {
        private Charity selectedCharity;

        public async Task StartAsync(IDialogContext context)
        {
            var reply = context.MakeMessage();
            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Plain;
            reply.Text = string.Format(
                   "Tell me the name of the charity you want to give, or type a 4-digits code of the charity.",
                   context.Activity.From.Name);
            reply.Speak = reply.Text = string.Format(
                   "Tell me the name of the charity you want to give",
                   context.Activity.From.Name);
            reply.InputHint = InputHints.ExpectingInput;

            await context.PostAsync(reply);
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result as Activity;
            activity.Text = activity.Text ?? string.Empty;

            var listingItem = await new CharityListingService().GetListingByEventCodeAsync(1111);

            if (listingItem != null)
            {
                selectedCharity = listingItem;
                var reply = context.MakeMessage();
                reply.Speak = listingItem.Name;
                //reply.Summary = listingItem.Name;
                var attachment = GetCharityHeroCard(listingItem);
                reply.Attachments.Add(attachment);

                await context.PostAsync(reply);

                context.Wait(this.HandleReply);
            }
            else
            {
                var reply = context.MakeMessage();
                reply.Type = ActivityTypes.Message;
                reply.TextFormat = TextFormatTypes.Plain;
                reply.Text = string.Format(
                       "Debug 1?",
                       context.Activity.From.Name);
                reply.Speak = reply.Text = string.Format(
                       "Debug 1",
                       context.Activity.From.Name);
                reply.InputHint = InputHints.ExpectingInput;

                await context.PostAsync(reply);

                context.Wait(this.HandleFallbackReply);
            }
        }


        private async Task HandleReply(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            activity.Text = activity.Text ?? string.Empty;

            var reply = context.MakeMessage();
            reply.Type = ActivityTypes.Message;
            reply.Text = string.Format(
                   $"Thanks for choosing to donate to {selectedCharity.Name}.  How much would you like to give today?");
            reply.Speak = reply.Text = string.Format(
                   $"Thanks for choosing to donate to {selectedCharity.Name}.  How much would you like to give today?");

            // NOTE: Bot Framework does not support Suggested Actions
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction(){ Title = "$10", Type=ActionTypes.PostBack, Value="10" },
                    new CardAction(){ Title = "$20", Type=ActionTypes.PostBack, Value="20" },
                    new CardAction(){ Title = "$50", Type=ActionTypes.PostBack, Value="50" },
                    new CardAction(){ Title = "$100", Type=ActionTypes.PostBack, Value="100" }
                }
            };
            reply.InputHint = InputHints.ExpectingInput;

            await context.PostAsync(reply);

            context.Wait(this.HandleDonationSelection);

        }


        private async Task HandleDonationSelection(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            activity.Text = activity.Text ?? string.Empty;

           
            await context.SayAsync("thank you");

            Donation donation = new Donation
            {
                Amount = 10,
                Recipient = selectedCharity,
                Currency = "USD",
                Description = "Charitable donation",
                DonorName = context.Activity.From.Name
            };

            context.Call(new PaymentDialog(donation), this.DonationCompleted);
        }

        private async Task DonationCompleted(IDialogContext context, IAwaitable<string> result)
        {
            var reply = context.MakeMessage();

            reply.Text = string.Format(
                    CultureInfo.CurrentCulture,
                    "Thank you for donating to {0}",
                    selectedCharity.Name);

            await context.PostAsync(reply);

            context.Done("completed");
        }

        private async Task HandleFallbackReply(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            activity.Text = activity.Text ?? string.Empty;
            var reply = context.MakeMessage();
            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Plain;
            reply.Text = string.Format(
                   "I don't support this operation for now.",
                   context.Activity.From.Name);
            reply.Speak = reply.Text = string.Format(
                    "I don't support this operation for now.",
                   context.Activity.From.Name);
            reply.InputHint = InputHints.IgnoringInput;

            await context.PostAsync(reply);
            context.Done("Dialog Completed");

        }

        private Attachment GetCharityHeroCard(Charity charity)
        {
            List<CardImage> cardImages = new List<CardImage>();
            cardImages.Add(new CardImage(charity.ImageUrl));

            List<CardAction> cardButtons = new List<CardAction>();

            CardAction donateButton = new CardAction()
            {
                Value = $"donate|{charity.EventCode}",
                Type = "postBack", // note: cortana does not support 'imback'
                Title = "donate"
            };

            CardAction learnMoreButton = new CardAction()
            {
                Value = charity.Website,
                Type = "openUrl",
                Title = "learn more"
            };

            cardButtons.Add(donateButton);
            cardButtons.Add(learnMoreButton);

            HeroCard plCard = new HeroCard()
            {
                Title = charity.Name,
                Subtitle = string.Join(" ", charity.Causes),
                Images = cardImages,
                Buttons = cardButtons
            };

            return plCard.ToAttachment();
        }
        
    }
}