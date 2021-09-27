using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    public partial class GridButton : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsToggled
        {
            get { return (bool)GetValue(IsToggledProperty); }
            set { SetValue(IsToggledProperty, value); NotifyPropertyChanged("Icon"); }
        }
        public static readonly DependencyProperty IsToggledProperty =
            DependencyProperty.Register(nameof(IsToggled), typeof(bool), typeof(GridButton));


        #region Opacity Properties
        public double ActiveOpacity { get; set; } = 0.25;
        public double InactiveOpacity { get; set; } = 0.25;
        public double HoverOpacity { get; set; } = 0.50;
        #endregion

        #region Icon & Colour Properties
        public PackIconFontAwesomeKind Icon { get { return (IsToggled ? ActiveIcon : InactiveIcon); } }
        private PackIconFontAwesomeKind activeIcon = PackIconFontAwesomeKind.QuestionSolid;
        private PackIconFontAwesomeKind inactiveIcon = PackIconFontAwesomeKind.ExclamationSolid;
        public PackIconFontAwesomeKind ActiveIcon { get { return activeIcon; } set { activeIcon = value; NotifyPropertyChanged("Icon"); } }
        public PackIconFontAwesomeKind InactiveIcon { get { return inactiveIcon; } set { inactiveIcon = value; NotifyPropertyChanged("Icon"); } }
        private Color activeColour = Color.FromRgb(0, 255, 0);
        public Color ActiveColour { get { return activeColour; } set { activeColour = value; NotifyPropertyChanged("Icon"); } }
        private Color inactiveColour = Color.FromRgb(255, 255, 255);
        public Color InactiveColour { get { return inactiveColour; } set { inactiveColour = value; NotifyPropertyChanged("Icon"); } }
        private Color hoverColour = Color.FromRgb(255, 255, 255);
        public Color HoverColour { get { return hoverColour; } set { hoverColour = value; NotifyPropertyChanged("Icon"); } }
        #endregion

        public GridButton()
        {
            InitializeComponent();
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                switch (propertyName)
                {
                    case "Icon":
                        UpdateColours();
                        break;
                }

                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void UpdateColours()
        {
            if (IsToggled)
            {
                ButtonBackground.Background = new SolidColorBrush(ActiveColour);
                this.BorderBrush = new SolidColorBrush(ActiveColour);
                ButtonBackground.Background.Opacity = ActiveOpacity;
            } else {
                ButtonBackground.Background = new SolidColorBrush(InactiveColour);
                this.BorderBrush = new SolidColorBrush(InactiveColour);
                ButtonBackground.Background.Opacity = InactiveOpacity;
            }
        }

        private void GridButton_MouseEnter(object sender, MouseEventArgs e)
        {
            ButtonBackground.Background.Opacity = HoverOpacity;
        }

        private void GridButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (IsToggled)
                ButtonBackground.Background.Opacity = ActiveOpacity;
            else
                ButtonBackground.Background.Opacity = InactiveOpacity;
        }

        private void GridButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            IsToggled = !IsToggled;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateColours();
        }
    }
}
