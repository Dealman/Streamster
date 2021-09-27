using NAudio.CoreAudioApi;
using NAudio.Wave;
using ProtoBuf;
using Streamster.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Streamster
{
    public enum AudioState
    {
        Stopped = 0,
        Playing = 1,
        Paused = 2
    }

    public enum VolumeType
    {
        Listen = 0,
        Transmit = 1
    }

    [ProtoContract]
    public class AudioFile : INotifyPropertyChanged, IDisposable
    {
        // Events
        public event PropertyChangedEventHandler PropertyChanged;
        public delegate void StateChangedHandler(object sender, AudioState newState);
        public event StateChangedHandler OnStateChanged;

        // Non-Serialized Properties
        public string Name { get; set; }
        public AudioState CurrentState { get { return currentState; } set { currentState = value; AudioStateChanged(value); } }
        public AudioFileReader ListenFileReader;
        public AudioFileReader PlaybackFileReader;
        private AudioState currentState = AudioState.Stopped;
        private bool loaded;
        private bool disposed;
        private MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

        // Serialized Properties
        #region FilePath
        private string filePath;
        [ProtoMember(1)]
        public string FilePath { get { return filePath; } set { filePath = value; NotifyPropertyChanged(); } }
        #endregion
        #region TransmitVolume
        private double transmitVolume = 75.0;
        [ProtoMember(2)]
        public double TransmitVolume { get { return transmitVolume; } set { transmitVolume = value; NotifyPropertyChanged(); } }
        #endregion
        #region ListenVolume
        private double listenVolume = 50.0;
        [ProtoMember(3)]
        public double ListenVolume { get { return listenVolume; } set { listenVolume = value; NotifyPropertyChanged("ListenVolume"); } }
        #endregion
        #region Listen
        private bool listen = true;
        [ProtoMember(4), DefaultValue(true)]
        public bool Listen { get { return listen; } set { listen = value; NotifyPropertyChanged(); } }
        #endregion
        #region Repeat
        private bool repeat;
        [ProtoMember(5), DefaultValue(false)]
        public bool Repeat { get { return repeat; } set { repeat = value; NotifyPropertyChanged(); } }
        #endregion

        private AudioFile()
        {

        }

        public AudioFile(string filePath)
        {
            FilePath = filePath;
            Name = Path.GetFileName(FilePath);
            ListenFileReader = new AudioFileReader(FilePath);
            PlaybackFileReader = new AudioFileReader(FilePath);
            loaded = true;
        }

        [ProtoAfterDeserialization]
        private void Deserialized()
        {
            // TODO: Need to account for if the file has been deleted/renamed.
            Name = Path.GetFileName(FilePath);
            ListenFileReader = new AudioFileReader(FilePath);
            PlaybackFileReader = new AudioFileReader(FilePath);
            loaded = true;
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

            if (propertyName == "ListenVolume")
                SetVolume(VolumeType.Listen);

            if (propertyName == "TransmitVolume")
                SetVolume(VolumeType.Transmit);

            if (loaded)
                mainWindow.AudioFileChanged();
        }

        private void SetVolume(VolumeType type)
        {
            if (type == VolumeType.Listen)
            {
                if (ListenFileReader != null)
                {
                    float fixedValue = (float)Math.Clamp((ListenVolume / 100.0), 0.0, 1.0);
                    ListenFileReader.Volume = fixedValue;
                }
            }

            if (type == VolumeType.Transmit)
            {
                if (PlaybackFileReader != null)
                {
                    float fixedValue = (float)Math.Clamp((transmitVolume / 100.0), 0.0, 1.0);
                    PlaybackFileReader.Volume = fixedValue;
                }
            }
        }

        private void AudioStateChanged(AudioState state)
        {
            if (OnStateChanged != null)
                OnStateChanged(this, state);
        }

        public bool Exists()
        {
            if (!String.IsNullOrWhiteSpace(FilePath))
                if (File.Exists(FilePath))
                    return true;

            return false;
        }

        public void Open()
        {
            if (Exists())
                Process.Start("explorer.exe", $"/select,{FilePath}");
        }

        public void Stop()
        {
            AudioDeviceManager.ListenOutputDevice?.Dispose();
            AudioDeviceManager.PlaybackOutputDevice?.Dispose();
            ListenFileReader.Position = 0;
            PlaybackFileReader.Position = 0;
            CurrentState = AudioState.Stopped;
        }

        public void Preview()
        {
            Stop();

            var listenDevice = AudioDeviceManager.GetDeviceFromID(Settings.Default.LastListenDevice);

            if (listenDevice != null)
            {
                AudioDeviceManager.ListenOutputDevice = new WasapiOut(listenDevice, AudioClientShareMode.Shared, true, 300);
                AudioDeviceManager.ListenOutputDevice.Init(ListenFileReader);
                AudioDeviceManager.ListenOutputDevice.Play();

                if (AudioDeviceManager.ListenOutputDevice.PlaybackState == PlaybackState.Playing)
                    CurrentState = AudioState.Playing;
            }
        }

        public void Play()
        {
            Stop();

            var listenDevice = AudioDeviceManager.GetDeviceFromID(Settings.Default.LastListenDevice);
            var playbackDevice = AudioDeviceManager.GetDeviceFromID(Settings.Default.LastPlaybackDevice);

            if (listenDevice != null)
                AudioDeviceManager.ListenOutputDevice = new WasapiOut(listenDevice, AudioClientShareMode.Shared, true, 300);

            if (playbackDevice != null)
                AudioDeviceManager.PlaybackOutputDevice = new WasapiOut(playbackDevice, AudioClientShareMode.Shared, true, 300);

            if (AudioDeviceManager.ListenOutputDevice != null && AudioDeviceManager.PlaybackOutputDevice != null)
            {
                AudioDeviceManager.ListenOutputDevice.Init(ListenFileReader);
                AudioDeviceManager.PlaybackOutputDevice.Init(PlaybackFileReader);
                AudioDeviceManager.ListenOutputDevice.Play();
                AudioDeviceManager.PlaybackOutputDevice.Play();
            }
        }

        #region Disposal
        public void Dispose()
        {
            AudioLibrary.RemoveFile(this);
            mainWindow.AudioFileChanged();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing) { }

            disposed = true;
        }
        #endregion
    }
}
