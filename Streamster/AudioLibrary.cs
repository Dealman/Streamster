using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Streamster
{
    [ProtoContract]
    [ProtoInclude(6, typeof(AudioFile))]
    public static class AudioLibrary
    {
        private static readonly string DataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Streamster";
        private static readonly string LibraryPath = Path.Combine(DataFolder, "AudioLibrary.bin");

        [ProtoMember(7)]
        public static ObservableCollection<AudioFile> AudioCollection = new ObservableCollection<AudioFile>();

        public static void AddFile(AudioFile audioFile)
        {
            if (AudioCollection.Count > 0)
            {
                foreach (AudioFile file in AudioCollection)
                {
                    if (file.FilePath == audioFile.FilePath)
                        return;
                }

                AudioCollection.Add(audioFile);
            }
        }

        public static void RemoveFile(AudioFile audioFile)
        {
            if (AudioCollection.Contains(audioFile))
                AudioCollection.Remove(audioFile);
        }

        public static bool Exists()
        {
            if (File.Exists(LibraryPath))
                return true;

            return false;
        }

        public static void Save()
        {
            try
            {
                using (var file = File.Create(LibraryPath))
                {
                    Serializer.Serialize(file, AudioCollection);
                }
            } catch(Exception ex) {
                MessageBox.Show($"An error occurred whilst trying to save the library!\n\nError Message:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void Load()
        {
            try
            {
                using (var file = File.OpenRead(LibraryPath))
                {
                    AudioCollection = Serializer.Deserialize<ObservableCollection<AudioFile>>(file);
                }
            } catch (Exception ex) {
                MessageBox.Show($"An error occurred whilst trying to load the library!\n\nError Message:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
