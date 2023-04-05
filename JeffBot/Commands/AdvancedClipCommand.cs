using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using TwitchLib.Api.Helix.Models.Clips.CreateClip;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.PubSub;

namespace JeffBot
{
    public class AdvancedClipCommand : BotCommandBase<AdvancedClipCommandSettings>
    {
        #region NoobHunterFormUrl
        public string NoobHunterFormUrl { get; set; } = "http://bit.ly/NHClips";
        #endregion
        #region MostRecentClips
        public Dictionary<string, (string url, DateTime dateTime)> MostRecentClips { get; set; } = new Dictionary<string, (string url, DateTime dateTime)>();
        #endregion

        #region Constructor
        public AdvancedClipCommand(BotCommandSettings<AdvancedClipCommandSettings> botCommandSettings, ManagedTwitchApi twitchApiClient, TwitchClient twitchChatClient, TwitchPubSub twitchPubSub, StreamerSettings streamerSettings, ILogger<JeffBot> logger) : base(botCommandSettings, twitchApiClient, twitchChatClient, twitchPubSub, streamerSettings, logger)
        {
        }
        #endregion

        #region ProcessMessage - Override
        public override async Task<bool> ProcessMessage(ChatMessage chatMessage)
        {
            if (StreamerSettings.BotFeatures.Any(a => a.Name == BotFeatureName.Clip.ToString() || a.Name == BotFeatureName.AdvancedClip.ToString()))
            {
                #region Clip
                var isClipMessage = Regex.Match(chatMessage.Message.ToLower(), @$"^!{BotCommandSettings.TriggerWord}$");
                if (isClipMessage.Captures.Count > 0)
                {
                    await CreateTwitchClip(chatMessage, StreamerSettings.BotFeatures.Any(a => a.Name == "AdvancedClip"));
                }
                #endregion
            }

            if (StreamerSettings.BotFeatures.Any(a => a.Name == BotFeatureName.AdvancedClip.ToString()))
            {
                #region Clip Noobhunter
                var isPostNoobHunter = Regex.Match(chatMessage.Message.ToLower(), @$"^!{BotCommandSettings.TriggerWord} noobhunter$");
                if (isPostNoobHunter.Captures.Count > 0)
                {
                    ValidateAndPostToNoobHuner(chatMessage);
                }
                #endregion
            }

            return false;
        }
        #endregion
        #region Initialize - Override
        public override void Initialize()
        {
        }
        #endregion

        #region CreateTwitchClip
        private async Task CreateTwitchClip(ChatMessage chatMessage, bool canPerformAdvancedClip)
        {
            CreatedClipResponse clip = null;
            try
            {
                if (!await IsStreamLive())
                {
                    TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"Cannot create clip for an offline stream.");
                    return;
                }
                clip = await TwitchApiClient.ExecuteRequest(async api => await api.Helix.Clips.CreateClipAsync(StreamerSettings.StreamerId));

                if (clip != null && clip.CreatedClips.Any())
                {
                    TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"Clip created successfully {clip.CreatedClips[0].EditUrl.Replace("/edit", string.Empty)}");
                    MostRecentClips[chatMessage.Username] = (clip.CreatedClips[0].EditUrl.Replace("/edit", string.Empty), DateTime.UtcNow);
                    if (canPerformAdvancedClip)
                        TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"You can submit this clip to NoobHunter for consideration by typing \"!clip noobhunter\" in chat.");
                }
                else
                {
                    TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"Stream NOT successfully clipped.");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.Source == "Newtonsoft.Json")
                {
                    if (clip != null && clip.CreatedClips.Any())
                    {
                        TwitchChatClient.SendMessage(chatMessage.Channel, $"Stream successfully clipped: ");
                        TwitchChatClient.SendMessage(chatMessage.Channel, $"Clip created successfully {clip.CreatedClips[0].EditUrl.Replace("/edit", string.Empty)}");
                        MostRecentClips[chatMessage.Username] = (clip.CreatedClips[0].EditUrl.Replace("/edit", string.Empty), DateTime.UtcNow);
                        if (canPerformAdvancedClip)
                            TwitchChatClient.SendMessage(chatMessage.Channel, $"@{chatMessage.DisplayName} you can submit this clip to NoobHunter for consideration by typing \"!clip noobhunter\" in chat.");
                    }
                    else
                    {
                        TwitchChatClient.SendMessage(chatMessage.Channel, $"Stream NOT successfully clipped.");
                    }
                }
                else
                {
                    TwitchChatClient.SendMessage(chatMessage.Channel, "Stream was NOT successfully clipped.. Someone tell Jeff..");
                }
            }
        }
        #endregion
        #region ValidateAndPostToNoobHuner
        private void ValidateAndPostToNoobHuner(ChatMessage chatMessage)
        {
            string url = string.Empty;
            KeyValuePair<string, (string url, DateTime dateTime)> recentClip = new KeyValuePair<string, (string url, DateTime dateTime)>("default user", (string.Empty, DateTime.Now));

            if (MostRecentClips.TryGetValue(chatMessage.Username, out (string url, DateTime dateTime) clip))
            {
                url = clip.url;
            }
            else if (chatMessage.IsModerator)
            {
                if (MostRecentClips.Count > 0)
                {
                    recentClip = MostRecentClips.FirstOrDefault(a => a.Value.dateTime == MostRecentClips.Max(b => b.Value.dateTime));
                    url = recentClip.Value.url;
                }
            }
            else
            {
                TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"Sorry {chatMessage.DisplayName}, there are currently no clips you can submit to NoobHunter, please use !clip and then try again.");
            }
            if (url != string.Empty)
            {
                var result = FillOutNoobHunterFormAndSubmit(url);
                if (result.success)
                {
                    MostRecentClips.Remove(chatMessage.Username);
                    if (recentClip.Key != "default user")
                    {
                        TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"{chatMessage.DisplayName}, {recentClip.Key}'s clip has been successfully submitted to NoobHunter!");
                    }
                    else
                    {
                        TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"{chatMessage.DisplayName}, your clip has been successfully submitted to NoobHunter!");
                    }
                }
                else
                {
                    TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"An error occurred submitting your clip to NoobHunter, you can try again, or just yell at Jeff to fix it.");
                }
            }
        }
        #endregion
        #region FillOutNoobHunterFormAndSubmit
        private (bool success, string message) FillOutNoobHunterFormAndSubmit(string url)
        {
            ChromeDriver driver = null;
            try
            {
                var chromeOptions = new ChromeOptions();
                chromeOptions.AddArguments("headless", "no-sandbox", "disable-dev-shm-usage");
                driver = new ChromeDriver(chromeOptions);
                driver.Navigate().GoToUrl(NoobHunterFormUrl);
                var firstQuestion = WaitAndFindElementByXpath(driver, "//div[contains(@data-params, 'Clip Link')]");
                var firstQuestionInput = firstQuestion.FindElement(By.TagName("textarea"));
                firstQuestionInput.SendKeys(url);
                var secondQuestion = WaitAndFindElementByXpath(driver, "//div[contains(@data-params, 'Featured Name')]");
                var secondQuestionInput = secondQuestion.FindElement(By.TagName("input"));
                secondQuestionInput.SendKeys(StreamerSettings.StreamerName);
                var submitButton = WaitAndFindElementByXpath(driver, "//span[text()='Submit']");
                submitButton.Click();

                try
                {
                    var waitForSubmit = new WebDriverWait(driver, TimeSpan.FromSeconds(15)).Until(a => a.FindElement(By.PartialLinkText("Submit another response")));
                    return (true, "lol");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                    return (false, ex.Message);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                return (false, ex.Message);
            }
            finally
            {
                if (driver != null)
                {
                    try
                    {
                        Logger.LogInformation("Closing Chrome Driver");
                        driver.Close();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex.ToString());
                        // Swallow
                    }
                }
            }
        }
        #endregion
        #region WaitAndFindElementByXpath
        private IWebElement WaitAndFindElementByXpath(IWebDriver driver, string xpath)
        {
            return new WebDriverWait(driver, TimeSpan.FromSeconds(15)).Until(a => a.FindElement(By.XPath(xpath)));
        }
        #endregion
    }
}