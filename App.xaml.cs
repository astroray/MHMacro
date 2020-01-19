using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WindowsInput;
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

        private static readonly int[][] _permutaion =
        {
            new[] { 0, 1, 2 },
            new[] { 0, 2, 1 },
            new[] { 1, 0, 2 },
            new[] { 1, 2, 0 },
            new[] { 2, 1, 0 },
            new[] { 2, 0, 1 }
        };

        public event Action<bool> StateChange;

        private KeyboardListener   _keyboardListener = new KeyboardListener();
        private RawKeyEventHandler _keyEventHandler;
        private bool               _enabled;
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

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            _keyEventHandler          =  OnKeydown;
            _keyboardListener.KeyDown += _keyEventHandler;
        }

        private void OnKeydown(object sender, RawKeyEventArgs args)
        {
            if (args.isControlPressed && args.Key == Key.Tab)
            {
                enabled = !enabled;

                if (enabled)
                {
                    Task.Run(() =>
                    {
                        var random      = new Random();
                        var inputHelper = new InputSimulator();

                        while (enabled)
                        {
                            var pickedPerm = _permutaion[random.Next(0, _permutaion.Length)];

                            for (int i = 0; i < _keys.Length; i++)
                            {
                                var key = _keys[pickedPerm[i]];

                                inputHelper.Keyboard
                                           .KeyDown(key)
                                           .Sleep(10)
                                           .KeyUp(key);
                            }

                            inputHelper.Keyboard
                                       .KeyDown(VirtualKeyCode.SPACE)
                                       .Sleep(10)
                                       .KeyUp(VirtualKeyCode.SPACE);
                        }
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