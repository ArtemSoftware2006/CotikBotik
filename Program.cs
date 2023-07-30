
using CotikBotik;
using Deployf.Botf;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using MongoDB.Driver;

namespace CotikBorik
{
    public  class Program
    {
        [Obsolete]
        public static void Main(string[] args)
        {   
            BotfProgram.StartBot(args,
            onConfigure : (svc, config) => {
                var conf_builder = new ConfigurationBuilder();

                conf_builder.SetBasePath(Directory.GetCurrentDirectory());
                conf_builder.AddJsonFile("security.json");
                config = conf_builder.Build();

                svc.AddSingleton(new MongoClient(config.GetConnectionString("ConnectStrMongoDocker")).GetDatabase("CotikBotik"));

                var mongoConnection = config.GetConnectionString("ConnectStrMongoDocker");
                var migrationOptions = new MongoMigrationOptions
                {
                    MigrationStrategy = new DropMongoMigrationStrategy(),
                    BackupStrategy = new CollectionMongoBackupStrategy(),
                };

                svc.AddHangfire(config =>
                {
                    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
                    config.UseSimpleAssemblyNameTypeSerializer();
                    config.UseRecommendedSerializerSettings();
                    config.UseMongoStorage(mongoConnection, "Hangfire",new MongoStorageOptions 
                    {   
                        MigrationOptions = migrationOptions, 
                        CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection 
                    });

                });
                svc.AddHangfireServer();
           },
           onRun : (app, config) => {
              app.UseHangfireServer();
              app.UseHangfireDashboard("/dashboard");
              RecurringJob.AddOrUpdate<RecurringPhotoJob>(x => x.Exec(), Cron.MinuteInterval(1));
           });

        }
    }
}