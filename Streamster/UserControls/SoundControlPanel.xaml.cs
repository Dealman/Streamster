using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace Streamster.UserControls
{
    public partial class SoundControlPanel : UserControl, INotifyPropertyChanged
    {
        public AudioFile Audio { get { return (AudioFile)GetValue(AudioProperty); } set { SetValue(AudioProperty, value); NotifyPropertyChanged(); } }
        public static readonly DependencyProperty AudioProperty = DependencyProperty.Register(nameof(AudioFile), typeof(AudioFile), typeof(SoundControlPanel));

        public event PropertyChangedEventHandler PropertyChanged;

        public SoundControlPanel()
        {
            InitializeComponent();
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void GridButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender == RepeatButton)
                if (Audio != null)
                    Audio.Repeat = !Audio.Repeat;

            if (sender == PreviewButton)
                if (Audio != null)
                    if (Audio.CurrentState == AudioState.Stopped)
                        Audio.Preview();
                    else
                        Audio.Stop();

            if (sender == OpenButton)
                if (Audio != null)
                    Audio.Open();

            if (sender == DeleteButton)
                if (Audio != null)
                    Audio.Dispose();
        }

        private void SoundController_Loaded(object sender, RoutedEventArgs e)
        {
            AudioFile audioFile = this.DataContext as AudioFile;

            if (audioFile != null)
                Audio = audioFile;
        }
    }
}
