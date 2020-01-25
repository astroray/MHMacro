using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

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

            return Native.CallNextHookEx(hookId, nCode, wParam, lParam);
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

            return Native.CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        public event RawKeyEventHandler KeyDown;
        public event RawKeyEventHandler KeyUp;
        private LowLevelKeyboardProc    _hookCallback;

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
            Native.UnhookWindowsHookEx(hookId);
        }
        #endregion
    }
}