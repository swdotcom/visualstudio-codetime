using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CodeTime
{
    /// <summary>
    /// Interaction logic for StatusBarButton.xaml
    /// </summary>
    public partial class StatusBarManager : UserControl
    {

        public static Boolean showingStatusbarMetrics = true;

        private Image ClockImage = null;
        private Image PawImage = null;
        private Image RocketImage = null;

        public StatusBarManager()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public async Task UpdateDisplayAsync(string label, string iconName)
        {
            Image statusImg = null;
            if (PawImage == null)
            {
                PawImage = ImageManager.CreateImage("cpaw.png");
            }

            // 3 types of images: clock, rocket, and paw
            if (!showingStatusbarMetrics)
            {
                label = "";
                iconName = "clock.png";
                if (ClockImage == null)
                {
                    ClockImage = ImageManager.CreateImage(iconName);
                }
                statusImg = ClockImage;
            }
            else if (iconName == "rocket.png")
            {
                if (RocketImage == null)
                {
                    RocketImage = ImageManager.CreateImage(iconName);
                }
                statusImg = RocketImage;
            }
            else
            {
                statusImg = PawImage;
            }

            await Dispatcher.BeginInvoke(new Action(() =>
            {
                string tooltip = "Active code time today. Click to see more from Code Time.";
                string email = FileManager.getItemAsString("name");
                if (email != null)
                {
                    tooltip += " Logged in as " + email;
                }
                TimeLabel.Content = label;
                TimeLabel.ToolTip = "Code time today";

                TimeIcon.Source = statusImg.Source;
                TimeIcon.ToolTip = tooltip;

                if (FileManager.IsInFlow())
                {
                    string msg = "Exit Flow";
                    Image dotImg = ImageManager.CreateImage("dot.png");
                    FlowModeLabel.ToolTip = msg;
                    FlowModeIcon.Source = dotImg.Source;
                    FlowModeIcon.ToolTip = msg;
                }
                else
                {
                    string msg = "Enter Flow";
                    Image dotOutlinedImg = ImageManager.CreateImage("dot-outlined.png");
                    FlowModeLabel.ToolTip = msg;
                    FlowModeIcon.Source = dotOutlinedImg.Source;
                    FlowModeIcon.ToolTip = msg;
                }

            }));
        }

        private void ToggleFlowMode(object sender, RoutedEventArgs args)
        {
            if (FileManager.IsInFlow())
            {
                FlowManager.DisableFlow();
            }
            else
            {
                FlowManager.EnableFlow(false);
            }
        }

        private void LaunchCodeMetricsView(object sender, RoutedEventArgs args)
        {
            try
            {
                _ = CodeTimePackage.OpenCodeMetricsPaneAsync();
            }
            catch (Exception e)
            {
                LogManager.Error("Error launching the code metrics view", e);
            }

        }
    }
}
