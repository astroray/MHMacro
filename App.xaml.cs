using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WindowsInput.Native;

namespace MHMacro
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly VirtualKeyCode[] _keys =
        {
            VirtualKeyCode.LEFT,
            VirtualKeyCode.UP,
            VirtualKeyCode.RIGHT
        };

        private static readonly int[][] _permutation =
        {
            new[] { 0, 1, 2 },
            new[] { 0, 2, 1 },
            new[] { 1, 0, 2 },
            new[] { 1, 2, 0 },
            new[] { 2, 1, 0 },
            new[] { 2, 0, 1 }
        };

        private static readonly string _processName = "MonsterHunterWorld";

        public event Action<bool> StateChange;

        private KeyboardListener   _keyboardListener = new KeyboardListener();
        private RawKeyEventHandler _keyEventHandler;
        private bool               _enabled;
        private Process            _process;

        private bool enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value)
                {
                    return;
                }

                _enabled = value;
                StateChange?.Invoke(_enabled);
            }
        }

        private Process process
        {
            get
            {
                if (_process == null)
                {
                    _process = Process.GetProcessesByName(_processName).FirstOrDefault();
                }

                return _process;
            }
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            _keyEventHandler          =  OnKeydown;
            _keyboardListener.KeyDown += _keyEventHandler;
        }

        private void OnKeydown(object sender, RawKeyEventArgs args)
        {
            if (args.isControlPressed && args.Key == Key.Q)
            {
                enabled = !enabled;

                if (enabled)
                {
                    Task.Run(() =>
                    {
                        var random = new Random();
                        var input  = new InputMessenger(process);

                        Native.SetForegroundWindow(process.MainWindowHandle);

                        while (enabled && process == null)
                        {
                            var pickedPerm = _permutation[random.Next(0, _permutation.Length)];

                            for (int i = 0; i < _keys.Length; i++)
                            {
                                var key = _keys[pickedPerm[i]];

                                input.SendKey(key);
                            }

                            input.SendKey(VirtualKeyCode.SPACE);
                        }

                        enabled = false;
                    });
                }
            }
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            _keyboardListener.Dispose();
        }
    }
}