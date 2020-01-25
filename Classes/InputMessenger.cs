using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace MHMacro
{
    public class InputMessenger
    {
        public IInputSimulator _inputSimulator = new InputSimulator();

        public void SendKey(VirtualKeyCode key, int duration = 10)
        {
            _inputSimulator.Keyboard
                           .KeyDown(key)
                           .Sleep(duration)
                           .KeyUp(key);
        }
    }
}