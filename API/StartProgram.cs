using System.Threading.Tasks;
using static vkaudioposter_Console.Program;

namespace vkaudioposter_Console.API
{
    public class StartProgram
    {
        public static async Task Start()
        {
            Program.threadstopflag = false;
            StatusChecker.ApiStart();
        }
    }
}
