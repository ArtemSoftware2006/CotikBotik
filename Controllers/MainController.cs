using Deployf.Botf;

namespace CotikBotik.Controllers
{
    public class MainController : BotController
    {
        [Action("/start","Начало работы с ботом")]
        public void Start()
        {
            PushL("Привет!");
            PushL("Этот CotikBotik будет присылать тебе фотографии милых котиков каждый час!");

            Send();
        }
        
    }
}