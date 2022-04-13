// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OTIFBot
{
    public class EmptyBot : ActivityHandler
    {
        //public const string WelcomeText = "This bot will introduce you to suggestedActions. Please answer the question:";
        private BotState _conversationState;
        private BotState _userState;

        

        public EmptyBot(ConversationState conversationState, UserState userState)
        {
            _conversationState = conversationState;
            _userState = userState;
        }


        private static IList<Choice> GetChoices()
        {
            var cardOptions = new List<Choice>()
            {
                new Choice() { Value = "Adaptive Card", Synonyms = new List<string>() { "adaptive" } },
                new Choice() { Value = "Animation Card", Synonyms = new List<string>() { "animation" } },
                new Choice() { Value = "Audio Card", Synonyms = new List<string>() { "audio" } },
                new Choice() { Value = "Hero Card", Synonyms = new List<string>() { "hero" } },
                new Choice() { Value = "OAuth Card", Synonyms = new List<string>() { "oauth" } },
                new Choice() { Value = "Receipt Card", Synonyms = new List<string>() { "receipt" } },
                new Choice() { Value = "Signin Card", Synonyms = new List<string>() { "signin" } },
                new Choice() { Value = "Thumbnail Card", Synonyms = new List<string>() { "thumbnail", "thumb" } },
                new Choice() { Value = "Video Card", Synonyms = new List<string>() { "video" } },
                new Choice() { Value = "All cards", Synonyms = new List<string>() { "all" } },
            };

            return cardOptions;
        }
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            
            await TypingConversation(turnContext,cancellationToken,2000);
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var WelcomeText = MessageFactory.Text("Hi , I am Botspy.");
            
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {

                    await turnContext.SendActivityAsync(WelcomeText, cancellationToken);
                    await TypingConversation(turnContext, cancellationToken, 2000);
                    var greetingText = greeting();
                    await turnContext.SendActivityAsync(greetingText, cancellationToken: cancellationToken);
                    await TypingConversation(turnContext, cancellationToken, 2000);

                    //await turnContext.SendActivityAsync(startQuestionText, cancellationToken: cancellationToken);


                   /* var card = new HeroCard
                    {
                        Title = "How may I help you?",
                        Images = new List<CardImage>() { new CardImage("https://aka.ms/bf-welcome-card-image") },
                        Buttons = new List<CardAction>()
                        {
                            new CardAction(ActionTypes.MessageBack,"Enquiry realted to upcoming production date.", null, "Enquiry realted to upcoming production date.", "Enquiry realted to upcoming production date.", value: "PD"),
                            new CardAction(ActionTypes.MessageBack, "Current Production update -Plantwise", null, "Current Production update -Plantwise", "Current Production update -Plantwise", value: "CP"),
                            new CardAction(ActionTypes.MessageBack, "Sales Support", null, "Sales Support", "Sales Support", value: "SS"),
                            new CardAction(ActionTypes.MessageBack, "Talk to CSD Team", null, "Talk to CSD Team", "Talk to CSD Team", value: "CSD"),
                        }
                    };*/
                    var response = MessageFactory.Attachment(GetMainMenu());

                    await turnContext.SendActivityAsync(response, cancellationToken);
                }
            }
        }

        
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var conversationStateAccessors = _conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
            var conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationData());
            if (turnContext.Activity.Value != null)
            {
                var text = turnContext.Activity.Value.ToString();
                if (!conversationData.PromptedUserForName)
                {
                    if (text.Contains("PD"))
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(GetTicknessCard()), cancellationToken);
                    }
                    else if (text.Contains("CP"))
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(GetPlantWiseCard()), cancellationToken);
                    }
                    else { await SendSuggestedActionsAsync(turnContext, cancellationToken); }

                    conversationData.PromptedUserForName = true;
                }
                else
                {
                    if (text.Contains("Exit"))
                    {
                        await EndConversation(turnContext, cancellationToken);
                        await TypingConversation(turnContext, cancellationToken, 2000);
                        await turnContext.SendActivityAsync(MessageFactory.Text("Have a nice day !!"), cancellationToken);
                        await TypingConversation(turnContext, cancellationToken, 2000);
                        await turnContext.SendActivityAsync(MessageFactory.Text("Hope to serve you again!!!"), cancellationToken).ConfigureAwait(false);
                    }
                    else if (text.Contains("Menu"))
                    {
                        var response = MessageFactory.Attachment(GetMainMenu());
                        await turnContext.SendActivityAsync(response, cancellationToken);
                        conversationData.PromptedUserForName = true;
                    }
                    else if (text.Contains("PD"))
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(GetTicknessCard()), cancellationToken);
                    }
                    else if (text.Contains("CP"))
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(GetPlantWiseCard()), cancellationToken);
                    }
                    else if (text.Contains("MM"))
                    {
                        await GetThicknessAsync(turnContext, cancellationToken);
                    }
                    else if (text.Contains("PW"))
                    {
                        await GetPlantWiseAsync(turnContext, cancellationToken);
                    }
                    else if(text.Contains("CSD") || text.Contains("SS"))
                    { 
                        await SendSuggestedActionsAsync(turnContext, cancellationToken); 
                    }

                }
            }
            else
            {
                if (turnContext.Activity.Text!=null)
                {
                    var text = turnContext.Activity.Text;
                    if(text.ToLower().Contains("thick"))
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(GetTicknessCard()), cancellationToken);
                    }
                    else if (text.ToLower().Contains("plant"))
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(GetPlantWiseCard()), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Thank you !!!"), cancellationToken).ConfigureAwait(false);
                        await TypingConversation(turnContext, cancellationToken, 2000);

                        var heroCard = GetHeroCard();
                        var res = MessageFactory.Attachment(heroCard);
                        await turnContext.SendActivityAsync(res, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
           
        }
        
        private static Attachment GetHeroCard()
        {
            var heroCard = new HeroCard
            {                              
                Text = "Would you like to End the chat or proceed to the main menu?",                
                Buttons = new List<CardAction> {
                    new CardAction(ActionTypes.MessageBack, "Main Menu", value: "Menu"),
                    new CardAction(ActionTypes.MessageBack, "End Chat", value: "Exit")
                },
            };

            return heroCard.ToAttachment();
        }
        private static Attachment GetConfirmationCard(string titleStr)
        {
            var heroCard = new HeroCard
            {
                Text = titleStr,
                Buttons = new List<CardAction> {
                    new CardAction(ActionTypes.MessageBack, "Yes", value: "Yes"),
                    new CardAction(ActionTypes.MessageBack, "No", value: "No")
                },
            };

            return heroCard.ToAttachment();
        }
        private static Attachment GetMainMenu()
        {
            var card = new HeroCard
            {
                Title = "How may I help you?",
                Images = new List<CardImage>() { new CardImage("https://aka.ms/bf-welcome-card-image") },
                Buttons = new List<CardAction>()
                        {
                            new CardAction(ActionTypes.MessageBack,"Enquiry realted to upcoming production date.", null, "Enquiry realted to upcoming production date.", "Enquiry realted to upcoming production date.", value: "PD"),
                            new CardAction(ActionTypes.MessageBack, "Current Production update -Plantwise", null, "Current Production update -Plantwise", "Current Production update -Plantwise", value: "CP"),
                            new CardAction(ActionTypes.MessageBack, "Sales Support", null, "Sales Support", "Sales Support", value: "SS"),
                            new CardAction(ActionTypes.MessageBack, "Talk to CSD Team", null, "Talk to CSD Team", "Talk to CSD Team", value: "CSD"),
                        }
            };
            return card.ToAttachment();
        }
        private static Attachment GetTicknessCard()
        {
            var thicknessCard = new HeroCard
            {
                Title = "Which thickness you want to enquire about?",
                Buttons = new List<CardAction>()
                        {
                            new CardAction(ActionTypes.MessageBack,"3MM", null, "3MM", "3MM", value: "3MM"),
                            new CardAction(ActionTypes.MessageBack,"4MM", null, "4MM", "4MM", value: "4MM"),
                            new CardAction(ActionTypes.MessageBack,"5MM", null, "5MM", "5MM", value: "5MM"),
                            new CardAction(ActionTypes.MessageBack,"6MM", null, "6MM", "6MM", value: "6MM"),
                            new CardAction(ActionTypes.MessageBack,"8MM", null, "8MM", "8MM", value: "8MM"),
                            new CardAction(ActionTypes.MessageBack,"10MM", null, "10MM", "10MM", value: "10MM"),
                            new CardAction(ActionTypes.MessageBack,"12MM", null, "12MM", "12MM", value: "12MM")
                        }
            };
            return thicknessCard.ToAttachment();
        }

        private static Attachment GetPlantWiseCard()
        {
            var plantWiseCard = new HeroCard
            {
                Title = "Which plant update you want?",
                Buttons = new List<CardAction>()
                        {
                            new CardAction(ActionTypes.MessageBack,"Bhiwadi", null, "Bhiwadi", "Bhiwadi", value: "PW"),
                            new CardAction(ActionTypes.MessageBack,"Jhagadia", null, "Jhagadia", "Jhagadia", value: "PW"),
                            new CardAction(ActionTypes.MessageBack,"Chennai", null, "Chennai", "Chennai", value: "PW")
                        }
            };
            return plantWiseCard.ToAttachment();
        }

        private static Attachment GetProfileThumbnailCard()
        {
            var imagePath = Path.Combine(Environment.CurrentDirectory, @"imgs\MyImage.jpg");
            var imageData = Convert.ToBase64String(File.ReadAllBytes(imagePath));

            var thumbnailCard = new ThumbnailCard
            {
                Title = "Vanitha",
                Subtitle = "Microsoft certified solution developer",
                Tap = new CardAction(ActionTypes.OpenUrl, "Learn More", value: "http://www.devenvexe.com"),
                Text = "Vanitha is a Technical Lead and C# Corner MVP. He has extensive 10+ years of experience working on different technologies, mostly in Microsoft space. Her focus areas are SFDC, Azure,Mobile Application, Web and ChatBot.\n\n\n\nEmail:- vanitha.a@saint-gobain.com\n\n\n\n‌‌Contact Number:- +91 9787512351",
                Images = new List<CardImage> { new CardImage($"data:image/png;base64,{imageData}") }
            };
            return thumbnailCard.ToAttachment();
        }

        private static async Task SendSuggestedActionsAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Value != null) {
                // Extract the text from the message activity the user sent.
                var text = turnContext.Activity.Value.ToString();

                //// Take the input from the user and create the appropriate response.
                var responseText = QuickProcessInput(text);

                // Respond to the user.
                // 
                if (responseText != "A" && responseText != "MM" && responseText != "PW")
                {

                    await SendGladMessageAsync(turnContext, cancellationToken);
                    await TypingConversation(turnContext, cancellationToken, 2000);

                    await turnContext.SendActivityAsync(responseText, cancellationToken: cancellationToken);
                    
                    await SendWelcomeMessageAsync(turnContext,cancellationToken);
                }
                
                    /*PromptOptions promptOptions = new PromptOptions
                    {
                        Prompt = MessageFactory.Text("What card would you like to see? You can click or type the card name"),
                        RetryPrompt = MessageFactory.Text("That was not a valid choice, please select a card or number from 1 to 9."),
                        Choices = GetChoices(),
                        Style = ListStyle.HeroCard
                    };
                    var steps = new WaterfallStep[]
                    {
                        ChoiceCardStepAsync

                    };*/
                   
                    /*return await stepContext.PromptAsync((nameof(ChoicePrompt), options, cancellationToken);*/
              
                

            }
        }
        private async static Task<DialogTurnResult> ChoiceCardStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("What card would you like to see? You can click or type the card name"),
                RetryPrompt = MessageFactory.Text("That was not a valid choice, please select a card or number from 1 to 9."),
                Choices = GetChoices(),
            };
            
            // Prompt the user with the configured PromptOptions.
            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }
        private static async Task SendGladMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var replyText = "Glad to serve you!!";
            await turnContext.SendActivityAsync(replyText, cancellationToken: cancellationToken);

        }
        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {

            await TypingConversation(turnContext, cancellationToken, 10000);

            if (turnContext.Activity.Value != null)
            {
                if (turnContext.Activity.Value.ToString() == "CSD" || turnContext.Activity.Value.ToString() == "SS")
                {
                    var replyText = "Thank you for waiting.We are assigned Vanitha to assist you";
                    await turnContext.SendActivityAsync(replyText, cancellationToken: cancellationToken);
                    await TypingConversation(turnContext, cancellationToken, 2000);

                    var thumbCard = GetProfileThumbnailCard();
                    var response = MessageFactory.Attachment(thumbCard);
                    await turnContext.SendActivityAsync(response, cancellationToken);
                    await TypingConversation(turnContext, cancellationToken, 2000);

                    await turnContext.SendActivityAsync(MessageFactory.Text("Thank you !!!"), cancellationToken).ConfigureAwait(false);
                    await TypingConversation(turnContext, cancellationToken, 2000);

                    var heroCard = GetHeroCard();
                    var res = MessageFactory.Attachment(heroCard);
                    await turnContext.SendActivityAsync(res, cancellationToken).ConfigureAwait(false);

                }

            }

            
        }
        private static async Task GetThicknessAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Value != null)
            {   

                await SendGladMessageAsync(turnContext, cancellationToken);
                await TypingConversation(turnContext, cancellationToken, 2000);

                await turnContext.SendActivityAsync(MessageFactory.Text("Wait for 20 sec . We are procedding with the information"), cancellationToken).ConfigureAwait(false);
                await TypingConversation(turnContext, cancellationToken, 2000);

                var thicknessDate = GetRandomDate().ToString();
                await turnContext.SendActivityAsync(thicknessDate, cancellationToken: cancellationToken);
                await TypingConversation(turnContext, cancellationToken, 2000);

                await turnContext.SendActivityAsync(MessageFactory.Text("Any other thickness you want the info?"), cancellationToken);
                await TypingConversation(turnContext, cancellationToken, 2000);
                await turnContext.SendActivityAsync(MessageFactory.Text("If Yes, Please type More Thickness to get started."), cancellationToken);
                
            }


        }

        private static async Task GetPlantWiseAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Value != null)
            {

                await SendGladMessageAsync(turnContext, cancellationToken);
                await TypingConversation(turnContext, cancellationToken, 2000);

                await turnContext.SendActivityAsync(MessageFactory.Text("Wait for 20 sec . We are procedding with the information"), cancellationToken).ConfigureAwait(false);
                await TypingConversation(turnContext, cancellationToken, 2000);

                var plantWiseProduction = "Currently No production";
                await turnContext.SendActivityAsync(plantWiseProduction, cancellationToken: cancellationToken);
                await TypingConversation(turnContext, cancellationToken, 2000);

                await turnContext.SendActivityAsync(MessageFactory.Text("Any other plant updation you want to enquire?"), cancellationToken);
                await TypingConversation(turnContext, cancellationToken, 2000);
                await turnContext.SendActivityAsync(MessageFactory.Text("If Yes, Please type More Plant to get started."), cancellationToken);

            }


        }
        private static async Task EndConversation(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            string code = EndOfConversationCodes.CompletedSuccessfully;
            var endOfConversation = Activity.CreateEndOfConversationActivity();
            endOfConversation.Code = code;
            await turnContext.SendActivityAsync(endOfConversation, cancellationToken);
            
        }
        private static async Task TypingConversation(ITurnContext turnContext,CancellationToken cancellationToken,int delay)
        {
            var typing = new Activity() { Type = ActivityTypes.Typing };
            await turnContext.SendActivityAsync(typing);
            await Task.Delay(delay);

            /*ITypingActivity replyActivity = Activity.CreateTypingActivity();
            await turnContext.SendActivityAsync((Activity)replyActivity);
            await Task.Delay(2000);*/
        }

        private static DateTime GetRandomDate()
        {
            var random = new Random();
            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddYears(20);
            var range = Convert.ToInt32(endDate.Subtract(startDate).TotalDays);
            return startDate.AddDays(random.Next(range));
        }
        private static string greeting()
        {
            var hour = DateTime.Now.Hour;
            if (hour < 12)
            {
                return "Good Morning";
            }
            if (hour < 17)
            {
                return "Good Afternoon";
            }
            return "Good Evening";
        }

        private static string QuickProcessInput(string text)
        {
            const string waitText = "Wait for 20 sec . We are procedding with the list of";
            switch (text)
            {
                case "PD":
                {
                    return $"MM";
                }
                case "CP":
                {
                    return $"PW";
                }
                case "SS":
                {
                    return $"{waitText} RM & AM!!!";
                }

                case "CSD":
                {
                    return $"{waitText} CSD Team!!!";
                }
                default:
                {
                    return "A";
                }
            }
        }
    }


}
