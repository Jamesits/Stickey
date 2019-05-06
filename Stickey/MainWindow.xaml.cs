using System;
using System.Diagnostics;
using System.Windows;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Interop;
using WindowsInput;
using WindowsInput.Native;

namespace Stickey
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private RawInput _rawinput;
        InputSimulator sim = new InputSimulator();


        const bool CaptureOnlyInForeground = false;
        public MainWindow()
        {
            InitializeComponent();

            
        }

        ~MainWindow()
        {
            DeepDarkWin32Fantasy.ResetHook();
        }

        private void OnKeyPressed(object sender, RawInputEventArg e)
        {
            Debug.WriteLine($"{e.JoystickEvent.DirectionButtons}");


            if (e.JoystickEvent.Up)
            {
                sim.Keyboard.KeyDown(VirtualKeyCode.VK_W);
            }
            else
            {
                sim.Keyboard.KeyUp(VirtualKeyCode.VK_W);
            }

            if (e.JoystickEvent.Down)
            {
                sim.Keyboard.KeyDown(VirtualKeyCode.VK_S);
            }
            else
            {
                sim.Keyboard.KeyUp(VirtualKeyCode.VK_S);
            }

            if (e.JoystickEvent.Left)
            {
                sim.Keyboard.KeyDown(VirtualKeyCode.VK_A);
            }
            else
            {
                sim.Keyboard.KeyUp(VirtualKeyCode.VK_A);
            }

            if (e.JoystickEvent.Right)
            {
                sim.Keyboard.KeyDown(VirtualKeyCode.VK_D);
            }
            else
            {
                sim.Keyboard.KeyUp(VirtualKeyCode.VK_D);
            }
        }

        private static void CurrentDomain_UnhandledException(Object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;

            if (null == ex) return;

            // Log this error. Logging the exception doesn't correct the problem but at least now
            // you may have more insight as to why the exception is being thrown.
            Debug.WriteLine("Unhandled Exception: " + ex.Message);
            Debug.WriteLine("Unhandled Exception: " + ex);
            MessageBox.Show(ex.Message);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _rawinput.JoystickEvent -= OnKeyPressed;
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            BtnStart.IsEnabled = false;
            // DeepDarkWin32Fantasy.SetHook(DeepDarkWin32Fantasy.InputHookCallback);
            var windowHandle = new WindowInteropHelper(this).EnsureHandle();
            _rawinput = new RawInput(windowHandle, CaptureOnlyInForeground);

            _rawinput.AddMessageFilter();   // Adding a message filter will cause keypresses to be handled
            Win32.DeviceAudit();            // Writes a file DeviceAudit.txt to the current directory

            _rawinput.JoystickEvent += OnKeyPressed;
        }
    }
}
