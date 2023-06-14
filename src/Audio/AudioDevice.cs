//  Author:
//       Noah Ablaseau <nablaseau@hotmail.com>
//
//  Copyright (c) 2017 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using OpenTK;
using OpenTK.Audio.OpenAL;
using System;

namespace linerider.Audio
{
    internal class AudioDevice : IDisposable
    {
        private readonly IntPtr _hDevice;
        private ContextHandle _hContext;
        private bool disposed = false;
        private static int devicecount = 0;

        public AudioDevice()
        {
            try
            {
                _hDevice = Alc.OpenDevice(null);
                if (_hDevice != IntPtr.Zero)
                {
                    _hContext = Alc.CreateContext(_hDevice, (int[])null);
                    if (_hContext.Handle != IntPtr.Zero)
                    {
                        _ = Alc.MakeContextCurrent(_hContext);
                        devicecount++;
                        Check();
                    }
                }
            }
            catch
            {
                devicecount--;
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                devicecount--;
                disposed = true;
                GC.SuppressFinalize(this);
                try
                {
                    _ = Alc.MakeContextCurrent(new ContextHandle(IntPtr.Zero));
                }
                catch
                {
                }
                if (_hContext.Handle != IntPtr.Zero)
                {
                    Alc.DestroyContext(_hContext);
                }
                if (_hDevice != IntPtr.Zero)
                {
                    _ = Alc.CloseDevice(_hDevice);
                }
            }
        }

        public static void Check()
        {
            if (devicecount != 0)
            {
                ALError error = AL.GetError();
                if (error != ALError.NoError)
                    throw new OpenTK.Audio.AudioException("OpenAL error " + AL.GetErrorString(error));
            }
            else
            {
                throw new OpenTK.Audio.AudioDeviceException("Audio device was disposed");
            }
        }
    }
}