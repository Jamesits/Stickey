using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stickey
{
    public class JoystickEvent
    {
        public string DeviceName;       // i.e. \\?\HID#VID_045E&PID_00DD&MI_00#8&1eb402&0&0000#{884b96c3-56ef-11d1-bc8c-00a0c91405dd}
        public string DeviceType;       // KEYBOARD or HID
        public IntPtr DeviceHandle;     // Handle to the device that send the input
        public string Name;             // i.e. Microsoft USB Comfort Curve Keyboard 2000 (Mouse and Keyboard Center)
        private string _source;         // Keyboard_XX

        // raw values
        public byte MainButtons;
        public byte DirectionButtons;

        // parsed values
        public bool Up;
        public bool Down;
        public bool Left;
        public bool Right;

        public string Source
        {
            get => _source;
            set => _source = $"Keyboard_{value.PadLeft(2, '0')}";
        }

        public override string ToString()
        {
            return
                $"Device\n DeviceName: {DeviceName}\n DeviceType: {DeviceType}\n DeviceHandle: {DeviceHandle.ToInt64():X}\n Name: {Name}\n";
        }
    }
}
