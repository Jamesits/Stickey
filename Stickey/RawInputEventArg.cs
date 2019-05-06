using System;

namespace Stickey
{
    public class RawInputEventArg : EventArgs
    {
        public RawInputEventArg(JoystickEvent arg)
        {
            JoystickEvent = arg;
        }
        
        public JoystickEvent JoystickEvent { get; private set; }
    }
}
