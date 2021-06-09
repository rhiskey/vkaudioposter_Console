using System.Threading.Tasks;
using static vkaudioposter_Console.Program;

namespace vkaudioposter_Console.API
{
    public class StartProgram
    {
        public static Task Start()
        {
            Program.threadstopflag = false;
            //Thread rabbitReciever = new(new ThreadStart(StatusChecker.ApiStart))
            //{
            //    IsBackground = false
            //};

            StatusChecker.ApiStart();
            return Task.CompletedTask;
        }
    }
}
