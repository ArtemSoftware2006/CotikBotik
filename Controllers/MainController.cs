using Amazon.S3;
using Amazon.S3.Model;
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
        private readonly IConfigurationRoot config;
        private readonly AmazonS3Config configsS3;

        [Obsolete]
        public MainController(IMongoDatabase dbClient)
        {
            this._dbClient = dbClient;

            var conf_builder = new ConfigurationBuilder();

            conf_builder.SetBasePath(Directory.GetCurrentDirectory());
            conf_builder.AddJsonFile("security.json");
            config = conf_builder.Build();

            configsS3 = new  AmazonS3Config() {
                ServiceURL="https://storage.yandexcloud.net"
            };

            RecurringJob.AddOrUpdate<RecurringPhotoJob>(x => x.Exec(), Cron.MinuteInterval(1));

        }

        [Action("/start","Начало работы с ботом")]
        public async void Start()
        {
            
            try
            {

                User userNow = new User() {login = Context.GetUsername(), chatId = this.ChatId};

                var user = await _dbClient.GetCollection<User>("users").Find(x => x.login == userNow.login).FirstOrDefaultAsync();

                if(user == null)
                {
                    await _dbClient.GetCollection<User>("users").InsertOneAsync(userNow);
                }
                using (var client = new AmazonS3Client(config.GetSection("accessKey").Value, config.GetSection("secretKey").Value, configsS3))
                {
                    var requestKeys = new ListObjectsV2Request
                    {
                        BucketName = config.GetSection("bucketName").Value
                    };

                    ListObjectsV2Response response = await client.ListObjectsV2Async(requestKeys);

                    Random rnd = new Random();
                    var keyToPhoto = response.S3Objects[(int)rnd.NextInt64(10L)].Key;

                    var requestPhoto = new GetObjectRequest();

                    var responsePhoto = await client.GetObjectAsync(config.GetSection("bucketName").Value, keyToPhoto);

                    var stream = responsePhoto.ResponseStream;
                    
                    InputOnlineFile inputOnlineFile = new InputOnlineFile(stream, "Котитки вперёд!");
                    await ((TelegramBotClient)this.Client).SendPhotoAsync(userNow.chatId, inputOnlineFile, caption : "Коитики вперёд!");

                    await stream.DisposeAsync();
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
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