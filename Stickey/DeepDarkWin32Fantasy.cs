using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Stickey
{
    public class DeepDarkWin32Fantasy
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WH_MOUSE_LL = 14;

        #region structs

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT

        {
            public int x;
            public int y;
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT

        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        #endregion

        #region enums

        private enum MouseMessages

        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }

        #endregion

        #region dllimport

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelInputProc lpfn, IntPtr hMod, uint dwThreadId);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        #endregion

        public delegate IntPtr LowLevelInputProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static LowLevelInputProc lowLevelInputProcDelegate;
        private static readonly Dictionary<int, IntPtr> HookPtrs = new Dictionary<int, IntPtr>();

        #region helper functions

        public static void SetHook(LowLevelInputProc proc)

        {
            // fuck GC
            // https://stackoverflow.com/a/6193914/2646069
            lowLevelInputProcDelegate = proc;
            foreach (var hook in new List<int>
            {
                WH_KEYBOARD_LL,
                WH_MOUSE_LL,
            })
            {
                HookPtrs.Add(hook, SetWindowsHookEx(hook, lowLevelInputProcDelegate, Marshal.GetHINSTANCE(typeof(DeepDarkWin32Fantasy).Module), 0));
            }
        }

        public static void ResetHook()
        {
            foreach (var hook in HookPtrs)
            {
                UnhookWindowsHookEx(hook.Value);
            }
        }

        // https://blogs.msdn.microsoft.com/toub/2006/05/03/low-level-keyboard-hook-in-c/
        public static IntPtr InputHookCallback(int nCode, IntPtr wParam, IntPtr lParam)

        {
            if (nCode >= 0 &&
                MouseMessages.WM_LBUTTONDOWN == (MouseMessages) wParam)

            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                Console.WriteLine(hookStruct.pt.x + ", " + hookStruct.pt.y);
            }

            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)

            {
                int vkCode = Marshal.ReadInt32(lParam);

                Console.WriteLine((Keys)vkCode);
            }

            // https://stackoverflow.com/a/26031829/2646069
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        #endregion
    }
}