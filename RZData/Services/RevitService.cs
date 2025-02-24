using Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace Services
{
    public class RevitService
    {
        [DllImport("user32.dll")]
        private static extern int EnableWindow(IntPtr handle, bool enable);

        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);
        public static void SetWindowTop(Window window)
        {
            IntPtr applicationWindow = ComponentManager.ApplicationWindow;
            new WindowInteropHelper(window).Owner = applicationWindow;
        }
        public static void RevitActive()
        {
            SendMessage(ComponentManager.ApplicationWindow, 7, IntPtr.Zero, IntPtr.Zero);
        }
    }
}
