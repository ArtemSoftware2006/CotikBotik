using Amazon.S3;
using Amazon.S3.Model;
using Deployf.Botf;
using MongoDB.Bson;
using MongoDB.Driver;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;

namespace CotikBotik
{
    /// <summary>
    /// Повторяющийся процесс отправки подписчикам фотографий  
    /// </summary>
    public class RecurringPhotoJob
    {
        readonly MessageBuilder Message;
        private readonly IMongoDatabase _dbClient;
        private readonly ITelegramBotClient _client;
        private readonly ILogger<RecurringPhotoJob> _logger;
        private readonly AmazonS3Config configsS3;
        private readonly IConfigurationRoot config;
        /// <summary>
        /// Конструктор процесса со всеми используемыми зависимостями
        /// </summary>
        /// <param name="dbClient">Клиент связи с базой данных</param>
        /// <param name="client">Клиент телеграма</param>
        /// <param name="logger">Логгирование</param>
        public RecurringPhotoJob( IMongoDatabase dbClient, ITelegramBotClient client, ILogger<RecurringPhotoJob> logger)
        {
            Message = new MessageBuilder();
            _dbClient = dbClient;
            _client = client;
            _logger = logger;

            var conf_builder = new ConfigurationBuilder();

            conf_builder.SetBasePath(Directory.GetCurrentDirectory());
            conf_builder.AddJsonFile("security.json");
            config = conf_builder.Build();

            configsS3 = new  AmazonS3Config() {
                ServiceURL="https://storage.yandexcloud.net"
            };
        }
        /// <summary>
        /// Выполняет поиск пользователей по бд и отправляет им фотографии
        /// </summary>
        /// <returns></returns>
        public async Task Exec()
        {
            try
            {
                var users = _dbClient.GetCollection<User>("users").Find(new BsonDocument()).ToList();

                if (users != null)
                {
                    using (var client = new AmazonS3Client(config.GetSection("accessKey").Value, config.GetSection("secretKey").Value, configsS3))
                    {
                        var requestKeys = new ListObjectsV2Request
                        {
                            BucketName = config.GetSection("bucketName").Value,
                            MaxKeys = 12,
                        };

                        ListObjectsV2Response response = await client.ListObjectsV2Async(requestKeys);

                        Random rnd = new Random();
                        var keyToPhoto = response.S3Objects[(int)rnd.NextInt64(12L)].Key;

                        var requestPhoto = new GetObjectRequest();

                        var responsePhoto = await client.GetObjectAsync(config.GetSection("bucketName").Value, keyToPhoto);

                        var stream = responsePhoto.ResponseStream;
                        foreach (var item in users)
                        {
                            InputOnlineFile inputOnlineFile = new InputOnlineFile(stream, "Котитки вперёд!");
                            await ((TelegramBotClient)_client).SendPhotoAsync(item.chatId, inputOnlineFile, caption : "Котики вперёд!");
                            
                        }

                        await stream.DisposeAsync();
                    }
                }   
                else 
                    _logger.LogWarning("Нет пользователей в бд");
            }
            catch(AmazonS3Exception ex)
            {
                _logger.LogCritical(ex.ErrorCode);
                _logger.LogCritical(ex.StatusCode.ToString());
                _logger.LogCritical(ex.Message);
                _logger.LogCritical(ex.StackTrace);
            }
            catch(Exception ex)
            {
                _logger.LogCritical(ex.Message, "Exception in RecurringPhoto");
                _logger.LogCritical(ex.InnerException, "Exception in RecurringPhoto");
            }
        }
    }
}