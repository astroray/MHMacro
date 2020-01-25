using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace MHMacro
{
    public delegate void RawKeyEventHandler(object sender, RawKeyEventArgs args);

    public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    internal static class InterceptKeys
    {
        public static int WH_KEYBOARD_LL = 13;
        public static uint WM_KEYDOWN = 0x0100;
        public static uint WM_KEYUP = 0x0101;

        public static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return Native.SetWindowsHookEx(WH_KEYBOARD_LL, proc, Native.GetModuleHandle(curModule.ModuleName), 0);
            }
        }
    }

    public class RawKeyEventArgs : EventArgs
    {
        private const int VK_SHIFT = 0x10;
        private const int VK_CONTROL = 0x11;
        private const int VK_MENU = 0x12;
        private const int VK_CAPITAL = 0x14;

        public int VKCode { get; }
        public Key Key { get; }
        public bool IsSysKey { get; }
        public bool isControlPressed { get; }
        public bool isAltPressed { get; }
        public bool isShiftPressed { get; }
        public bool isCapslockPressed { get; }

        public RawKeyEventArgs(int vkCode, bool isSysKey)
        {
            VKCode = vkCode;
            IsSysKey = isSysKey;
            Key = KeyInterop.KeyFromVirtualKey(vkCode);

            if ((Native.GetKeyState(VK_CAPITAL) & 0x0001) != 0)
            {
                isCapslockPressed = true;
            }

            if ((Native.GetKeyState(VK_SHIFT) & 0x8000) != 0)
            {
                isShiftPressed = true;
            }

            if ((Native.GetKeyState(VK_CONTROL) & 0x8000) != 0)
            {
                isControlPressed = true;
            }

            if ((Native.GetKeyState(VK_MENU) & 0x8000) != 0)
            {
                isAltPressed = true;
            }
        }
    }

    public class KeyboardListener : IDisposable
    {
        private static IntPtr hookId = IntPtr.Zero;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                return HookCallbackInner(nCode, wParam, lParam);
            }
            catch
            {
                Console.WriteLine("There was some error somewhere...");
            }

            return Native.CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        private IntPtr HookCallbackInner(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == (IntPtr)InterceptKeys.WM_KEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);

                    KeyDown?.Invoke(this, new RawKeyEventArgs(vkCode, false));
                }
                else if (wParam == (IntPtr)InterceptKeys.WM_KEYUP)
                {
                    int vkCode = Marshal.ReadInt32(lParam);

                    KeyUp?.Invoke(this, new RawKeyEventArgs(vkCode, false));
                }
            }

            return Native.CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        public event RawKeyEventHandler KeyDown;
        public event RawKeyEventHandler KeyUp;
        private LowLevelKeyboardProc _hookCallback;

        public KeyboardListener()
        {
            _hookCallback = HookCallback;
            hookId = InterceptKeys.SetHook(_hookCallback);
        }

        ~KeyboardListener()
        {
            Dispose();
        }

        #region IDisposable Members
        public void Dispose()
        {
            _hookCallback = null;
            Native.UnhookWindowsHookEx(hookId);
        }
        #endregion
    }
}