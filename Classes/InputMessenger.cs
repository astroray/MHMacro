using System.Diagnostics;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace MHMacro
{
    public class InputMessenger
    {
        public IInputSimulator _inputSimulator = new InputSimulator();

        public async Task SendKey(Process target, VirtualKeyCode key, int duration = 10)
        {
            if (target == null || target.MainWindowHandle != Native.GetForegroundWindow())
            {
                await Task.Delay(duration);

                return;
            }

            _inputSimulator.Keyboard
                           .KeyDown(key)
                           .Sleep(duration)
                           .KeyUp(key);
        }
    }
}