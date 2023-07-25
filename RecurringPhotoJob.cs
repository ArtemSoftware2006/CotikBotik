using Deployf.Botf;
using MongoDB.Bson;
using MongoDB.Driver;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;

namespace CotikBotik
{
    public class RecurringPhotoJob
    {
        readonly MessageBuilder Message;
        private readonly IMongoDatabase _dbClient;
        private readonly ITelegramBotClient _client;
        private readonly ILogger<RecurringPhotoJob> _logger;

        public RecurringPhotoJob( IMongoDatabase dbClient, ITelegramBotClient client, ILogger<RecurringPhotoJob> logger)
        {
            Message = new MessageBuilder();
            _dbClient = dbClient;
            _client = client;
            _logger = logger;
        }

        public async Task Exec()
        {
            try
            {
                var users = _dbClient.GetCollection<User>("users").Find(new BsonDocument()).ToList();

                if (users != null)
                {
                    _logger.LogInformation("Пользователи есть в количестве : " + users.Count);
                    foreach (var item in users)
                    {
                        Random rnd = new Random();
                        var files = Directory.GetFiles("C://Users//Artem//Desktop//Котики");
                        var photo = files[rnd.NextInt64(files.Length)].Replace("\\","//");

                        _logger.LogInformation(item.chatId + " : " + item.login);

                        using (FileStream stream = System.IO.File.OpenRead(photo))
                        {
                            InputOnlineFile inputOnlineFile = new InputOnlineFile(stream, "Котитки вперёд!");
                            await ((TelegramBotClient)_client).SendPhotoAsync(item.chatId, inputOnlineFile, caption : "Коитики вперёд!");
                        }
                    }
                }   
                else 
                    _logger.LogWarning("Нет пользователей в бд");
            }
            catch(Exception ex)
            {
                _logger.LogCritical(ex.Message, "Exception in RecurringPhoto");
            }
        }
    }
}