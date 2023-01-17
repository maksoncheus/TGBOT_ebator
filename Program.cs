using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Exceptions;
using HtmlAgilityPack;
using System.Text;
namespace TGBOT_ebator
{
    class Program
    {
        static ITelegramBotClient bot = new TelegramBotClient(""); //токен из бота BotFather в тг
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (message.Text.ToLower() == "/start")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Привет!");
                    //return;
                }
                if (message.Text == "Хочу гигачада")
                {
                    await botClient.SendPhotoAsync
                        (
                        chatId: message.Chat.Id,
                        photo: GetRandomPhoto(),
                        replyMarkup: GetButtons()
                        );
                }
                if (message.Text == "АУФ цитата")
                {
                    string[] text = System.IO.File.ReadAllLines("citates-images.txt");
                    Random random = new Random();
                    int choosenCitate = random.Next(text.Length);
                    string url = text[choosenCitate];
                    await botClient.SendPhotoAsync(
                        chatId: message.Chat.Id,
                        photo: url,
                        replyMarkup: GetButtons()
                        );
                }
                await botClient.SendTextMessageAsync(message.Chat, "Выбери одну из команд ниже:", replyMarkup: GetButtons());
            }
        }

        public static Telegram.Bot.Types.InputFiles.InputOnlineFile GetRandomPhoto()
        {
            var directory = new DirectoryInfo(@"../../../gigachads/");
            if (directory.Exists)
            {
                FileInfo[] files = directory.GetFiles();
                if (files.Length > 0)
                {
                    Random random = new Random();
                    int getRand = random.Next(files.Length);
                    Stream stream = System.IO.File.OpenRead(files[getRand].FullName);
                    return new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream);
                }
            }
            return "https://thumbs.dreamstime.com/z/error-rubber-stamp-word-error-inside-illustration-109026446.jpg";
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        private static IReplyMarkup GetButtons()
        {
            return new ReplyKeyboardMarkup(new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton>{ new KeyboardButton("АУФ цитата"), new KeyboardButton("Хочу гигачада") }
                });
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            string url = "https://statusas.ru/citaty-i-aforizmy/citaty-pro-zhivotnyx-i-zverej/citaty-i-memy-volka-auf.html";
            var response = CallUrl(url).Result;
            var linkList = GetWolfCitateImage(response);
            WriteToText(linkList, "citates-images.txt");
            Console.ReadLine();
        }

        private static async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetStringAsync(fullUrl);
            return response;
        }
        private static List<string> GetWolfCitateImage(string html)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(System.Web.HttpUtility.HtmlDecode(html));

            var wolfImages = htmlDoc.DocumentNode.Descendants("p")
                .Where(node => node.ParentNode.GetAttributeValue("class","").Contains("entry-content"))
                .ToList();

            List<string> imageSrc = new List<string>();

            foreach (var str in wolfImages)
            {
                if (str.FirstChild.Attributes.Count > 0 && str.FirstChild.Name != "span")
                {
                    imageSrc.Add(str.FirstChild.Attributes["data-src"].Value);
                }
            }

            return imageSrc;
        }
        private static void WriteToText(List<string> links, string output)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var link in links)
            {
                sb.AppendLine(link);
            }

            System.IO.File.WriteAllText(output, sb.ToString());
        }
    }
}