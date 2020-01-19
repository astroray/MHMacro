using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MHMacro
{
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

            return InterceptKeys.CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        private IntPtr HookCallbackInner(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == (IntPtr) InterceptKeys.WM_KEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);

                    if (KeyDown != null) KeyDown(this, new RawKeyEventArgs(vkCode, false));
                }
                else if (wParam == (IntPtr) InterceptKeys.WM_KEYUP)
                {
                    int vkCode = Marshal.ReadInt32(lParam);

                    if (KeyUp != null) KeyUp(this, new RawKeyEventArgs(vkCode, false));
                }
            }

            return InterceptKeys.CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        public event RawKeyEventHandler            KeyDown;
        public event RawKeyEventHandler            KeyUp;
        private InterceptKeys.LowLevelKeyboardProc _hookCallback;

        public KeyboardListener()
        {
            _hookCallback = HookCallback;
            hookId        = InterceptKeys.SetHook(_hookCallback);
        }

        ~KeyboardListener()
        {
            Dispose();
        }

        #region IDisposable Members
        public void Dispose()
        {
            _hookCallback = null;
            InterceptKeys.UnhookWindowsHookEx(hookId);
        }
        #endregion
    }

    internal static class InterceptKeys
    {
        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public static int  WH_KEYBOARD_LL = 13;
        public static uint WM_KEYDOWN     = 0x0100;
        public static uint WM_KEYUP       = 0x0101;

        public static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL,    proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook,
                                                     LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
                                                   IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState(int keyCode);
    }

    public class RawKeyEventArgs : EventArgs
    {
        private const int VK_SHIFT   = 0x10;
        private const int VK_CONTROL = 0x11;
        private const int VK_MENU    = 0x12;
        private const int VK_CAPITAL = 0x14;

        public int  VKCode            { get; }
        public Key  Key               { get; }
        public bool IsSysKey          { get; }
        public bool isControlPressed  { get; }
        public bool isAltPressed      { get; }
        public bool isShiftPressed    { get; }
        public bool isCapslockPressed { get; }

        public RawKeyEventArgs(int vkCode, bool isSysKey)
        {
            VKCode   = vkCode;
            IsSysKey = isSysKey;
            Key      = KeyInterop.KeyFromVirtualKey(vkCode);

            if ((InterceptKeys.GetKeyState(VK_CAPITAL) & 0x0001) != 0)
            {
                isCapslockPressed = true;
            }

            if ((InterceptKeys.GetKeyState(VK_SHIFT) & 0x8000) != 0)
            {
                isShiftPressed = true;
            }

            if ((InterceptKeys.GetKeyState(VK_CONTROL) & 0x8000) != 0)
            {
                isControlPressed = true;
            }

            if ((InterceptKeys.GetKeyState(VK_MENU) & 0x8000) != 0)
            {
                isAltPressed = true;
            }
        }
    }

    public delegate void RawKeyEventHandler(object sender, RawKeyEventArgs args);
}