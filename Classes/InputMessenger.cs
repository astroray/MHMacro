using System.Diagnostics;
using WindowsInput;
using WindowsInput.Native;

namespace MHMacro
{
    public class InputMessenger
    {
        private Process         _process;
        private IInputSimulator _inputSimulator = new InputSimulator();

        public InputMessenger(Process process)
        {
            _process = process;
        }

        public void SendKey(VirtualKeyCode key, int duration = 10)
        {
            if (_process == null
                || _process.MainWindowHandle != Native.GetForegroundWindow())
            {
                return;
            }

            _inputSimulator.Keyboard
                           .KeyDown(key)
                           .Sleep(duration)
                           .KeyUp(key);
        }
    }
}