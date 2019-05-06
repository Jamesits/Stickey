using System;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Stickey
{
    // last 4
    [Flags]
    public enum XInputJoystickButtons {
        A = 0x01,
        B = 0x02,
        X = 0x04,
        Y = 0x08,
        LB = 0x10,
        RB = 0x20,
        View = 0x40,
        Menu = 0x80,
    }

    // last 3
    [Flags]
    public enum XInputJoystickDirectionButtons
    {
        L3 = 0x01,
        R3 = 0x02,
        Up = 0x04,
        Right = 0x0c,
        Down = 0x14,
        Left = 0x1c,
    }

    public sealed class RawJoystick
    {
        private readonly Dictionary<IntPtr, JoystickEvent> _deviceList = new Dictionary<IntPtr, JoystickEvent>();
        public delegate void DeviceEventHandler(object sender, RawInputEventArg e);
        public event DeviceEventHandler JoystickPressed;
        private readonly object _padLock = new object();
        public int NumberOfKeyboards { get; private set; }
        private static InputData _rawBuffer;

        public RawJoystick(IntPtr hwnd, bool captureOnlyInForeground)
        {
            var rid = new RawInputDevice[2];

            rid[0].UsagePage = HidUsagePage.GENERIC;
            rid[0].Usage = HidUsage.Gamepad;
            rid[0].Flags = (captureOnlyInForeground ? RawInputDeviceFlags.NONE : RawInputDeviceFlags.INPUTSINK) | RawInputDeviceFlags.DEVNOTIFY;
            rid[0].Target = hwnd;

            rid[1].UsagePage = HidUsagePage.GENERIC;
            rid[1].Usage = HidUsage.Joystick;
            rid[1].Flags = (captureOnlyInForeground ? RawInputDeviceFlags.NONE : RawInputDeviceFlags.INPUTSINK) | RawInputDeviceFlags.DEVNOTIFY;
            rid[1].Target = hwnd;

            if (!Win32.RegisterRawInputDevices(rid, (uint)rid.Length, (uint)Marshal.SizeOf(rid[0])))
            {
                throw new ApplicationException("Failed to register raw input device(s).");
            }
        }

        public void EnumerateDevices()
        {
            lock (_padLock)
            {
                _deviceList.Clear();

                var keyboardNumber = 0;

                var globalDevice = new JoystickEvent
                {
                    DeviceName = "Global Joystick",
                    DeviceHandle = IntPtr.Zero,
                    DeviceType = Win32.GetDeviceType(DeviceType.RimTypeHid),
                    Name = "Fake Joystick?",
                    Source = keyboardNumber++.ToString(CultureInfo.InvariantCulture)
                };

                _deviceList.Add(globalDevice.DeviceHandle, globalDevice);

                var numberOfDevices = 0;
                uint deviceCount = 0;
                var dwSize = (Marshal.SizeOf(typeof(Rawinputdevicelist)));

                if (Win32.GetRawInputDeviceList(IntPtr.Zero, ref deviceCount, (uint)dwSize) == 0)
                {
                    var pRawInputDeviceList = Marshal.AllocHGlobal((int)(dwSize * deviceCount));
                    Win32.GetRawInputDeviceList(pRawInputDeviceList, ref deviceCount, (uint)dwSize);

                    for (var i = 0; i < deviceCount; i++)
                    {
                        uint pcbSize = 0;

                        // On Window 8 64bit when compiling against .Net > 3.5 using .ToInt32 you will generate an arithmetic overflow. Leave as it is for 32bit/64bit applications
                        var rid = (Rawinputdevicelist)Marshal.PtrToStructure(new IntPtr((pRawInputDeviceList.ToInt64() + (dwSize * i))), typeof(Rawinputdevicelist));

                        Win32.GetRawInputDeviceInfo(rid.hDevice, RawInputDeviceInfo.RIDI_DEVICENAME, IntPtr.Zero, ref pcbSize);

                        if (pcbSize <= 0) continue;

                        var pData = Marshal.AllocHGlobal((int)pcbSize);
                        Win32.GetRawInputDeviceInfo(rid.hDevice, RawInputDeviceInfo.RIDI_DEVICENAME, pData, ref pcbSize);
                        var deviceName = Marshal.PtrToStringAnsi(pData);

                        // Debug.WriteLine(rid.dwType);

                        if (rid.dwType == DeviceType.RimTypeHid)
                        {
                            var deviceDesc = Win32.GetDeviceDescription(deviceName);

                            var dInfo = new JoystickEvent
                            {
                                DeviceName = Marshal.PtrToStringAnsi(pData),
                                DeviceHandle = rid.hDevice,
                                DeviceType = Win32.GetDeviceType(rid.dwType),
                                Name = deviceDesc,
                                Source = keyboardNumber++.ToString(CultureInfo.InvariantCulture)
                            };

                            if (!_deviceList.ContainsKey(rid.hDevice))
                            {
                                numberOfDevices++;
                                _deviceList.Add(rid.hDevice, dInfo);
                            }
                        }

                        Marshal.FreeHGlobal(pData);
                    }

                    Marshal.FreeHGlobal(pRawInputDeviceList);

                    NumberOfKeyboards = numberOfDevices;
                    Debug.WriteLine("EnumerateDevices() found {0} Joystick(s)", NumberOfKeyboards);
                    return;
                }
            }

            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public void ProcessRawInput(IntPtr hdevice)
        {
            // Debug.WriteLine(_rawBuffer.data.keyboard.ToString());
            // Debug.WriteLine(_rawBuffer.data.hid.ToString());
            // Debug.WriteLine(_rawBuffer.header.ToString());

            if (_deviceList.Count == 0) return;

            var dwSize = 0;
            Win32.GetRawInputData(hdevice, DataCommand.RID_INPUT, IntPtr.Zero, ref dwSize, Marshal.SizeOf(typeof(Rawinputheader)));

            var unmanagedPointer = Marshal.AllocHGlobal(dwSize);

            if (dwSize != Win32.GetRawInputData(hdevice, DataCommand.RID_INPUT, unmanagedPointer, ref dwSize, Marshal.SizeOf(typeof(Rawinputheader))))
            {
                Debug.WriteLine("Error getting the rawinput buffer");
                return;
            }

            var inputData = (InputData)Marshal.PtrToStructure(unmanagedPointer, typeof(InputData));

            var size = (int)(inputData.data.hid.dwCount * inputData.data.hid.dwSizHid);

            if (_deviceList.ContainsKey(_rawBuffer.header.hDevice))
            {
                lock (_padLock)
                {
                    Debug.WriteLine(_deviceList[_rawBuffer.header.hDevice]);
                }

                byte[] managedArray = new byte[dwSize];
                Marshal.Copy(unmanagedPointer, managedArray, 0, dwSize);

                var sb = new StringBuilder();
                sb.AppendFormat("Size: {0}/{1} ", dwSize, size);
                for (int i = Marshal.SizeOf(typeof(Rawinputheader)) + Marshal.SizeOf(typeof(byte)) * 2; i < dwSize; ++i)
                {
                    sb.AppendFormat("{0,3:X}", managedArray[i]);
                }

                Debug.WriteLine(sb);

                JoystickEvent e = new JoystickEvent()
                {
                    MainButtons = managedArray[managedArray.Length - 4],
                    DirectionButtons = managedArray[managedArray.Length - 3]
                };

                // Direction buttons
                var directionBtn = (byte)(e.DirectionButtons & 0x3c);
                switch (directionBtn)
                {
                    case 0x04:
                        e.Up = true;
                        break;
                    case 0x0c:
                        e.Right = true;
                        break;
                    case 0x14:
                        e.Down = true;
                        break;
                    case 0x1c:
                        e.Left = true;
                        break;
                    case 0x18:
                        e.Down = true;
                        e.Left = true;
                        break;
                    case 0x20:
                        e.Up = true;
                        e.Left = true;
                        break;
                    case 0x10:
                        e.Down = true;
                        e.Right = true;
                        break;
                    case 0x08:
                        e.Up = true;
                        e.Right = true;
                        break;
                }

                // L3 & R3
                if ((e.DirectionButtons & 0x01) != 0)
                {
                    e.L3 = true;
                }

                if ((e.DirectionButtons & 0x02) != 0)
                {
                    e.R3 = true;
                }

                // normal buttons
                if ((e.MainButtons & (byte)XInputJoystickButtons.A) != 0)
                {
                    e.A = true;
                }
                if ((e.MainButtons & (byte)XInputJoystickButtons.B) != 0)
                {
                    e.B = true;
                }
                if ((e.MainButtons & (byte)XInputJoystickButtons.X) != 0)
                {
                    e.X = true;
                }
                if ((e.MainButtons & (byte)XInputJoystickButtons.Y) != 0)
                {
                    e.Y = true;
                }
                if ((e.MainButtons & (byte)XInputJoystickButtons.LB) != 0)
                {
                    e.LB = true;
                }
                if ((e.MainButtons & (byte)XInputJoystickButtons.RB) != 0)
                {
                    e.RB = true;
                }
                if ((e.MainButtons & (byte)XInputJoystickButtons.View) != 0)
                {
                    e.View = true;
                }
                if ((e.MainButtons & (byte)XInputJoystickButtons.Menu) != 0)
                {
                    e.Menu = true;
                }

                JoystickPressed?.Invoke(this, new RawInputEventArg(e));
            }
            else
            {
                Debug.WriteLine("Handle: {0} was not in the device list.", _rawBuffer.header.hDevice);
                return;
            }

            Marshal.FreeHGlobal(unmanagedPointer);
        }
    }
}
