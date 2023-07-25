using Deployf.Botf;
using Hangfire;
using MongoDB.Bson;
using MongoDB.Driver;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;

namespace CotikBotik.Controllers
{
    public class MainController : BotController
    {
        private readonly IMongoDatabase _dbClient;

        [Obsolete]
        public MainController(IMongoDatabase dbClient)
        {
            this._dbClient = dbClient;
            // RecurringJob.AddOrUpdate<RecurringPhotoJob>(x => x.Exec(), Cron.MinuteInterval(1));

        }

        [Action("/start","Начало работы с ботом")]
        public async void Start()
        {
            try
            {
                PushL("Котики вперёд!");
                RecurringJob.AddOrUpdate(() => Console.WriteLine(1), Cron.MinuteInterval(1));

                Random rnd = new Random();
                var files = Directory.GetFiles("C://Users//Artem//Desktop//Котики");
                var photo = files[rnd.NextInt64(files.Length)].Replace("\\","//");

                User userNow = new User() {login = Context.GetUsername(), chatId = this.ChatId};

                var user = await _dbClient.GetCollection<User>("users").Find(x => x.login == userNow.login).FirstOrDefaultAsync();


                if(user == null)
                {
                    await _dbClient.GetCollection<User>("users").InsertOneAsync(userNow);
                }

                using (FileStream stream = System.IO.File.OpenRead(photo))
                {
                    InputOnlineFile inputOnlineFile = new InputOnlineFile(stream, "Котик");
                    await ((TelegramBotClient)Client).SendPhotoAsync(this.ChatId, inputOnlineFile);
                }

                Send();
            }
            catch (System.Exception ex)
            {
                PushL(ex.Message );
                Send();
            }
        }

         [Action("/info","Что делает этот бот?")]
        public void Info()
        {
            Console.WriteLine(this.ChatId);

            PushL("Привет!");
            PushL("Этот CotikBotik будет присылать тебе фотографии милых котиков каждый час!");
            PushL("Вызвав команду /start вы подпишитесь на отправку картинок каждый час!");

            Send();
        }
        [Action("/stop","Прекращение работы с ботом")]
        public void Stop()
        {
            Console.WriteLine("/stop " + this.ChatId);

            _dbClient.GetCollection<User>("users").DeleteOneAsync(x => x.chatId == this.ChatId);
            PushL("Пока!");

            Send();
        }

    }
}