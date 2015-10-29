using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.CustomControls
{
    /// <summary>
    /// Interaction logic for RecentsCallItem.xaml
    /// </summary>
    public partial class RecentsCallItem 
    {
        public RecentsCallItem()
        {
            InitializeComponent();
        }

        public double Duration
        {
            get { return 0; }

            set
            {
                if (Math.Abs(value - (-1)) < 1)
                {
                    lblDuration.Text = "";
                    return;
                }

                int hours = 0, minutes = 0, seconds = 0;
                if (value > 0)
                {
                    seconds = (int)(double)value % 60;
                    minutes = (int)(double)value / 60;
                    hours = (int)(double)value / 3600;
                }

                if (hours > 0)
                {
                    lblDuration.Text = string.Format("{0}h {1:00}m {2:00}s", hours, minutes, seconds);
                    return;
                }

                if (minutes > 0)
                {
                    lblDuration.Text = string.Format("{0}m {1:00}s", minutes, seconds);
                    return;
                }

                lblDuration.Text = string.Format("0m {0}s", seconds);
            }
        }

        public string CallerName
        {
            get { return lblCaller.Text; }

            set { lblCaller.Text = value; }
        }

        public string TargetNumber { get; set; }

        public string ContactId { get; set; }
        public DateTime CallTime
        {
            get { return new DateTime(0); }

            set
            {
                string dateFormat = "d.MM, HH:mm";
                var diffTime = DateTime.Now - value;
                if (diffTime.Days == 0)
                    dateFormat = "HH:mm";
                else if (diffTime.Days < 8)
                    dateFormat = "ddd, HH:mm";
                else if (diffTime.Days > 365)
                    dateFormat = "d.MM.yyyy, HH:mm";

                lblDateText.Text = value.ToString(dateFormat);
            }
        }

        public VATRPHistoryEvent.StatusType CallStatus
        {
            get { return VATRPHistoryEvent.StatusType.Incoming; }
            set
            {
               switch (value)
                {
                    case VATRPHistoryEvent.StatusType.Outgoing:
                        this.bmStatus.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/outgoing.png", UriKind.RelativeOrAbsolute));
                        lblCallType.Text = Properties.Resources.RecentsOutgoing;
                        break;
                    case VATRPHistoryEvent.StatusType.Missed:
                        this.bmStatus.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/missed.png", UriKind.RelativeOrAbsolute));
                       this.lblCallType.Foreground = new SolidColorBrush(Color.FromRgb(255,0,0));
                       lblCallType.Text = Properties.Resources.RecentsMissed;
                       this.lblDuration.Visibility = Visibility.Collapsed;
                        break;
                   default:
                        this.bmStatus.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/incoming.png", UriKind.RelativeOrAbsolute));
                        lblCallType.Text = Properties.Resources.RecentsIncoming;
                       break;
                }
            }
        }

        public string FullName
        {
            get
            {
                return Name;
            }
            set
            {
                Name = value;
            }
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            lblCaller.ToolTip = lblCaller.Text;
        }
    }
}
