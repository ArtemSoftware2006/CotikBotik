
using Deployf.Botf;

namespace CotikBorik
{
    public  class Program
    {
        public static void Main(string[] args)
        {
           BotfProgram.StartBot(args,
           onConfigure : (svc, config) => {
            //Конфигкрация сервисов (builder.Services)
           },
           onRun : (app, config) => {
            //app (Как в Asp.Net)
           });
        }
    }
}