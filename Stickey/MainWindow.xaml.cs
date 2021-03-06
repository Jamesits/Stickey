﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Interop;
using WindowsInput;
using WindowsInput.Native;
using MahApps.Metro.Controls;
using Microsoft.ApplicationInsights;
using NLog;

namespace Stickey
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        private RawInput _rawinput;
        private readonly InputSimulator _sim = new InputSimulator();
        private readonly TelemetryClient client = new TelemetryClient();

        const bool CaptureOnlyInForeground = false;
        public MainWindow()
        {
            AppDomain.CurrentDomain.UnhandledException += GenericLogging.CurrentDomain_UnhandledException;
            InitializeComponent();
        }

        ~MainWindow()
        {
            // DeepDarkWin32Fantasy.ResetHook();
        }

        private void OnKeyPressed(object sender, RawInputEventArg e)
        {
            Debug.WriteLine($"{e.JoystickEvent.DirectionButtons}");

            // WASD
            if (e.JoystickEvent.Up)
            {
                _sim.Keyboard.KeyDown(VirtualKeyCode.VK_W);
            }
            else
            {
                _sim.Keyboard.KeyUp(VirtualKeyCode.VK_W);
            }

            if (e.JoystickEvent.Down)
            {
                _sim.Keyboard.KeyDown(VirtualKeyCode.VK_S);
            }
            else
            {
                _sim.Keyboard.KeyUp(VirtualKeyCode.VK_S);
            }

            if (e.JoystickEvent.Left)
            {
                _sim.Keyboard.KeyDown(VirtualKeyCode.VK_A);
            }
            else
            {
                _sim.Keyboard.KeyUp(VirtualKeyCode.VK_A);
            }

            if (e.JoystickEvent.Right)
            {
                _sim.Keyboard.KeyDown(VirtualKeyCode.VK_D);
            }
            else
            {
                _sim.Keyboard.KeyUp(VirtualKeyCode.VK_D);
            }

            if (e.JoystickEvent.A)
            {
                _sim.Keyboard.KeyDown(VirtualKeyCode.DOWN);
            }
            else
            {
                _sim.Keyboard.KeyUp(VirtualKeyCode.DOWN);
            }

            if (e.JoystickEvent.B)
            {
                _sim.Keyboard.KeyDown(VirtualKeyCode.RIGHT);
            }
            else
            {
                _sim.Keyboard.KeyUp(VirtualKeyCode.RIGHT);
            }

            if (e.JoystickEvent.X)
            {
                _sim.Keyboard.KeyDown(VirtualKeyCode.LEFT);
            }
            else
            {
                _sim.Keyboard.KeyUp(VirtualKeyCode.LEFT);
            }

            if (e.JoystickEvent.Y)
            {
                _sim.Keyboard.KeyDown(VirtualKeyCode.UP);
            }
            else
            {
                _sim.Keyboard.KeyUp(VirtualKeyCode.UP);
            }

            if (e.JoystickEvent.LB)
            {
                _sim.Keyboard.KeyDown(VirtualKeyCode.VK_E);
            }
            else
            {
                _sim.Keyboard.KeyUp(VirtualKeyCode.VK_E);
            }

            if (e.JoystickEvent.RB)
            {
                _sim.Keyboard.KeyDown(VirtualKeyCode.SPACE);
            }
            else
            {
                _sim.Keyboard.KeyUp(VirtualKeyCode.SPACE);
            }

            if (e.JoystickEvent.View)
            {
                _sim.Keyboard.KeyDown(VirtualKeyCode.VK_Q);
            }
            else
            {
                _sim.Keyboard.KeyUp(VirtualKeyCode.VK_Q);
            }
        }

        private void CbGlobalSwitch_Checked(object sender, RoutedEventArgs e)
        {
            // DeepDarkWin32Fantasy.SetHook(DeepDarkWin32Fantasy.InputHookCallback);
            var windowHandle = new WindowInteropHelper(this).EnsureHandle();
            _rawinput = new RawInput(windowHandle, CaptureOnlyInForeground);

            _rawinput.AddMessageFilter();   // Adding a message filter will cause keypresses to be handled
            Win32.DeviceAudit();            // Writes a file DeviceAudit.txt to the current directory

            _rawinput.JoystickEvent += OnKeyPressed;
        }

        private void CbGlobalSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            _rawinput.JoystickEvent -= OnKeyPressed;
            _rawinput = null;
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            client.Flush();
        }
    }
}
