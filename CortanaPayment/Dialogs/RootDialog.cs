

namespace CortanaPayment.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Properties;

    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private int count;
        public const string CARTKEY = "CART_ID";

        public async Task StartAsync(IDialogContext context)
        {
            //this.count = 0;
            await Task.FromResult(true);
            context.Wait(MessageReceivedAsync);
            
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {

            var reply = context.MakeMessage();

            reply.Text = string.Format(
                    
                    Resources.RootDialog_Welcome_Msg,
                    context.Activity.From.Name);

            reply.Speak = string.Format(
                    Resources.RootDialog_Welcome_Msg,
                    context.Activity.From.Name);
            reply.InputHint = InputHints.IgnoringInput;

            await context.PostAsync(reply);

            await ReplyWithOptions(context);

            context.Wait(this.HandleReply);

            var activity = await result as Activity;
            activity.Text = activity.Text ?? string.Empty;

            

        }

        private static async Task ReplyWithOptions(IDialogContext context)
        {
            var reply = context.MakeMessage();
            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Plain;
            reply.Text = string.Format(
                   "How can I help you now?",
                   context.Activity.From.Name);
            reply.Speak = reply.Text = string.Format(
                   "How can I help you now?",
                   context.Activity.From.Name);

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction(){ Title = "Donate", Type=ActionTypes.PostBack, Value="donate" },
                    new CardAction(){ Title = "Search", Type=ActionTypes.PostBack, Value="search" },
                    new CardAction(){ Title = "Reset", Type=ActionTypes.PostBack, Value="reset" }
                }
            };
            reply.InputHint = InputHints.ExpectingInput;

            await context.PostAsync(reply);
        }

        public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirm = await argument;
            // check if user wants to reset the counter or not
            if (confirm)
            {
                this.count = 1;
                await context.SayAsync("Reset count.", "I reset the counter for you!");
            }
            else
            {
                await context.SayAsync("Did not reset count.", $"Counter is not reset. Current value: {this.count}");
            }
            context.Wait(MessageReceivedAsync);
        }



        public async Task HandleReply(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result as Activity;
            activity.Text = activity.Text ?? string.Empty;
            // check if the user said reset
            if (activity.Text.ToLowerInvariant().StartsWith("reset"))
            {
                // ask the user to confirm if they want to reset the counter
                var options = new PromptOptions<string>(prompt: "Are you sure you want to reset the count?",
                    retry: "Didn't get that!", speak: "Do you want me to reset the counter?",
                    retrySpeak: "You can say yes or no!",
                    options: PromptDialog.PromptConfirm.Options,
                    promptStyler: new PromptStyler());

                PromptDialog.Confirm(context, AfterResetAsync, options);

            }
            else if (activity.Text.ToLowerInvariant().StartsWith("donate"))
            {
                
                context.Call(new DonateDialog(), this.HandleDonateComplete);
            }
            else
            {
                // calculate something for us to return
                int length = activity.Text.Length;

                // increment the counter
                this.count++;

                // say reply to the user
                await context.SayAsync($"{count}: You sent {activity.Text} which was {length} characters", $"{count}: You said {activity.Text}", new MessageOptions() { InputHint = InputHints.AcceptingInput });
                context.Wait(MessageReceivedAsync);
            }

            /*
            var activity = await result as Activity;
            activity.Text = activity.Text ?? string.Empty;

            if (activity.Text.ToLowerInvariant().StartsWith("yes"))
            {
                // check if user wants to reset the counter or not
                
               
                
                // go into the donation mode
                //context.Wait(this.MessageReceivedAsync);
                context.Call(new DonateDialog(), this.HandleDonateComplete);
            }
            else
            {

                // check if user wants to reset the counter or not
                var reply = context.MakeMessage();
                reply.Type = ActivityTypes.Message;
                reply.TextFormat = TextFormatTypes.Plain;
                reply.Text = string.Format(
                       "OK you don't care.",
                       context.Activity.From.Name);
                reply.Speak = reply.Text = string.Format(
                       "OK you don't care.",
                       context.Activity.From.Name);
                reply.InputHint = InputHints.IgnoringInput;

                await context.PostAsync(reply);
                context.Wait(this.MessageReceivedAsync);
            }
            */


        }


        public async Task HandleDonateComplete(IDialogContext context, IAwaitable<object> result)
        {
            var message = await result as string;



            var reply = context.MakeMessage();
            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Plain;
            reply.Text = string.Format(
                   message!=null? message: "Haha.",
                   context.Activity.From.Name);
            reply.Speak = reply.Text = string.Format(
                   message != null ? message : "Haha.",
                   context.Activity.From.Name);
            reply.InputHint = InputHints.IgnoringInput;

            await context.PostAsync(reply);

            /*
            var exitReply = context.MakeMessage();
            exitReply.Type = ActivityTypes.EndOfConversation;
            exitReply.Code = EndOfConversationCodes.CompletedSuccessfully;
            exitReply.AsEndOfConversationActivity();
            await context.PostAsync(exitReply);
            */
            context.Done<object>(null);
        }

    }
}