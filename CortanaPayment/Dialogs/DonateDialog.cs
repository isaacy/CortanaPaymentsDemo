using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace CortanaPayment.Dialogs 
{
    [Serializable]
    public class DonateDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            var reply = context.MakeMessage();
            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Plain;
            reply.Text = string.Format(
                   "Thanks for donating?",
                   context.Activity.From.Name);
            reply.Speak = reply.Text = string.Format(
                   "Thanks for donating.",
                   context.Activity.From.Name);
            reply.InputHint = InputHints.ExpectingInput;

            await context.PostAsync(reply);
            context.Wait(this.MessageReceivedAsync);
        }


        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result as Activity;
            activity.Text = activity.Text ?? string.Empty;
            var reply = context.MakeMessage();
            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Plain;
            reply.Text = string.Format(
                   "Debug 1?",
                   context.Activity.From.Name);
            reply.Speak = reply.Text = string.Format(
                   "Debug 1",
                   context.Activity.From.Name);
            reply.InputHint = InputHints.AcceptingInput;

            await context.PostAsync(reply);

            context.Wait(this.HandleReply);
            
        }


        private async Task HandleReply(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            activity.Text = activity.Text ?? string.Empty;
            var reply = context.MakeMessage();
            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Plain;
            reply.Text = string.Format(
                   "Debug 2?",
                   context.Activity.From.Name);
            reply.Speak = reply.Text = string.Format(
                   "Debug 2.",
                   context.Activity.From.Name);
            reply.InputHint = InputHints.ExpectingInput;

            await context.PostAsync(reply);

            context.Done("yes");

        }

    }
}