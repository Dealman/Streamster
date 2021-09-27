using NAudio.CoreAudioApi;
using NAudio.Wave;
using Streamster.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamster
{
    public static class AudioDeviceManager
    {
        public static WasapiOut ListenOutputDevice;
        public static WasapiOut PlaybackOutputDevice;
        public static IWavePlayer WavePlayer;

        public static MMDevice GetLastOutputDevice()
        {
            var deviceList = GetOutputDevices();

            if (deviceList != null && deviceList.Count > 0)
            {
                foreach (MMDevice device in deviceList)
                {
                    if (device.ID == Settings.Default.LastPlaybackDevice)
                        return device;
                }
            }

            return null;
        }

        public static MMDevice GetDeviceFromID(string ID)
        {
            var deviceList = GetOutputDevices();

            if (deviceList != null && deviceList.Count > 0)
            {
                foreach(MMDevice device in deviceList)
                {
                    if (device.ID == ID)
                        return device;
                }
            }

            return null;
        }

        public static MMDevice GetDefaultOutputDevice()
        {
            var deviceEnumerator = new MMDeviceEnumerator();

            if (deviceEnumerator.HasDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia))
                return deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            // TODO: Throw exception?
            return null;
        }

        public static int GetDeviceCount()
        {
            return WaveOut.DeviceCount;
        }

        public static MMDeviceCollection GetOutputDevices()
        {
            var deviceEnumerator = new MMDeviceEnumerator();
            MMDeviceCollection deviceList = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

            // TODO: Throw exception?
            return deviceList;
        }
    }
}
