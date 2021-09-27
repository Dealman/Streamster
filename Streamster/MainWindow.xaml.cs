using MahApps.Metro.Controls;
using NAudio.CoreAudioApi;
using Streamster.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Streamster.GlobalKeyboardManager;

namespace Streamster
{
    public partial class MainWindow : MetroWindow
    {
        private ObservableCollection<SimpleDeviceItem> DeviceCollection = new ObservableCollection<SimpleDeviceItem>();
        public bool UnsavedChanges
        {
            get { return (bool)GetValue(UnsavedChangesProperty); }
            set { SetValue(UnsavedChangesProperty, value); }
        }

        public static readonly DependencyProperty UnsavedChangesProperty = DependencyProperty.Register("UnsavedChanges", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public MainWindow()
        {
            InitializeComponent();
            //KeyboardHook.KeyboardPressed += OnKeyPressed;
        }

        private void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown)
                 Debug.WriteLine($"VirtualCode {e.KeyboardData.VirtualCode} was pressed!");
            else
                Debug.WriteLine($"VirtualCode {e.KeyboardData.VirtualCode} was released!");

            e.Handled = true;
        }

        public void AudioFileChanged()
        {
            if (!UnsavedChanges)
            {
                UnsavedChanges = true;
                this.Title = "Streamster - UNSAVED CHANGES";
            }
        }

        #region Button Events & Logic
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender == GithubButton)
                Process.Start(new ProcessStartInfo("cmd", $"/c start {"http://github.com/Dealman/Streamster"}") { CreateNoWindow = true });

            if (sender == SaveButton)
            {
                AudioLibrary.Save();
                UnsavedChanges = false;
                this.Title = "Streamster";
            }
        }
        #endregion

        #region Library DataGrid Events & Logic
        private void LibraryDataGrid_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Move;
            else
                e.Effects = DragDropEffects.None;
        }

        private void LibraryDataGrid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fileList = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach(string filePath in fileList)
                {
                    if (File.Exists(filePath))
                    {
                        var audioFile = new AudioFile(filePath);
                        AudioLibrary.AddFile(audioFile);
                    }
                }
            }
        }
        #endregion

        #region Main Window Events & Logic
        private void MetroWindow_ContentRendered(object sender, EventArgs e)
        {
            if (AudioLibrary.Exists())
            {
                AudioLibrary.Load();
                LibraryDataGrid.ItemsSource = AudioLibrary.AudioCollection;
            }

            var deviceList = AudioDeviceManager.GetOutputDevices();

            if (deviceList != null && deviceList.Count > 0)
            {
                foreach(MMDevice device in deviceList)
                {
                    var item = new SimpleDeviceItem { Name = device.FriendlyName, ID = device.ID };
                    DeviceCollection.Add(item);
                }

                ListenDeviceComboBox.ItemsSource = DeviceCollection;
                TransmitDeviceComboBox.ItemsSource = DeviceCollection;

                if (!String.IsNullOrWhiteSpace(Settings.Default.LastListenDevice))
                    foreach (SimpleDeviceItem item in DeviceCollection)
                        if (item.ID == Settings.Default.LastListenDevice)
                            ListenDeviceComboBox.SelectedItem = item;

                if (!String.IsNullOrWhiteSpace(Settings.Default.LastPlaybackDevice))
                    foreach (SimpleDeviceItem item in DeviceCollection)
                        if (item.ID == Settings.Default.LastPlaybackDevice)
                            TransmitDeviceComboBox.SelectedItem = item;
            }
        }
        #endregion

        #region Slider Events & Logic
        private void Slider_Loaded(object sender, RoutedEventArgs e)
        {
            Slider slider = sender as Slider;

            if (slider != null)
            {
                if (slider.Name == "ListenSlider")
                    slider.ValueChanged += ListenSlider_ValueChanged;

                if (slider.Name == "TransmitSlider")
                    slider.ValueChanged += TransmitSlider_ValueChanged;
            }
        }

        private void ListenSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;            
            if (slider != null)
            {
                AudioFile audioFile = slider.DataContext as AudioFile;
                if (audioFile != null)
                {
                    audioFile.ListenVolume = e.NewValue;
                }
            }
        }

        private void TransmitSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                AudioFile audioFile = slider.DataContext as AudioFile;
                if (audioFile != null)
                {
                    audioFile.TransmitVolume = e.NewValue;
                }
            }
        }
        #endregion

        #region ComboBox Events & Logic
        private void DeviceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == ListenDeviceComboBox)
            {
                if (Settings.Default.LastListenDevice != ((SimpleDeviceItem)ListenDeviceComboBox.SelectedItem).ID)
                    Settings.Default.LastListenDevice = ((SimpleDeviceItem)ListenDeviceComboBox.SelectedItem).ID;

                Settings.Default.Save();
            }

            if (sender == TransmitDeviceComboBox)
            {
                if (Settings.Default.LastPlaybackDevice != ((SimpleDeviceItem)TransmitDeviceComboBox.SelectedItem).ID)
                    Settings.Default.LastPlaybackDevice = ((SimpleDeviceItem)TransmitDeviceComboBox.SelectedItem).ID;

                Settings.Default.Save();
            }
        }
        #endregion
    }
}
