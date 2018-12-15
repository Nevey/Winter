using Game.Services;
using Game.UserInput.Services;

namespace Game.Console.Services
{
    public class ConsoleService : Service<ConsoleService>
    {
        protected override void OnInitialize()
        {
            base.OnInitialize();

            InputService.Instance.ToggleConsoleEvent += OnToggleConsole;
        }

        private void OnToggleConsole(float obj)
        {
            
        }
    }
}
