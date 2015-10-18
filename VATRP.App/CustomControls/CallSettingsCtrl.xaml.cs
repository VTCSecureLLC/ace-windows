using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VATRP.App.Interfaces;
using VATRP.App.Services;
using VATRP.Core.Model;

namespace VATRP.App.CustomControls
{
    /// <summary>
    /// Interaction logic for CallSettingsCtrl.xaml
    /// </summary>
    public partial class CallSettingsCtrl : ISettings
    {
        public CallSettingsCtrl()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            EnableAutoAnswerBox.IsChecked = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AUTO_ANSWER, false);
            AnswerTimeoutTextBox.Text = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AUTO_ANSWER_AFTER, "2");
            EnableAVPFMode.IsChecked = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AVPF_ON, true);
            SendDtmfInfo.IsChecked = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                 Configuration.ConfEntry.DTMF_SIP_INFO, false);
        }

        #region ISettings

        public bool IsChanged()
        {
            var enabled = EnableAutoAnswerBox.IsChecked ?? false;
            var cfgTimeout = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AUTO_ANSWER_AFTER, 2);

            if (enabled != ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AUTO_ANSWER, false))
                return true;

            if (AnswerTimeoutTextBox.Text != cfgTimeout.ToString())
                return true;

            enabled = EnableAVPFMode.IsChecked ?? false;

            if (enabled != ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AVPF_ON, true))
                return true;

            enabled = SendDtmfInfo.IsChecked ?? false;

            if (enabled != ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.DTMF_SIP_INFO, false))
                return true;

            return false;
        }

        public bool Save()
        {
            bool enabled = EnableAutoAnswerBox.IsChecked ?? false;

            if (enabled)
            {
                if (string.IsNullOrWhiteSpace(AnswerTimeoutTextBox.Text))
                {
                    MessageBox.Show("Please enter value between 0 and 60", "ACE", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }

                int val;

                if (!int.TryParse(AnswerTimeoutTextBox.Text, out val))
                    return false;

                if (val < 0 || val > 60)
                {
                    MessageBox.Show("Please enter value between 0 and 60", "ACE", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }

                ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                    Configuration.ConfEntry.AUTO_ANSWER_AFTER, val);
            }

            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AUTO_ANSWER, enabled);

            enabled = EnableAVPFMode.IsChecked ?? false;
            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AVPF_ON, enabled);

            enabled = SendDtmfInfo.IsChecked ?? false;
            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.DTMF_SIP_INFO, enabled);

            ServiceManager.Instance.ConfigurationService.SaveConfig();
            return true;
        }

        #endregion

        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var allowed = IsTextAllowed(e.Text);
            var handled = true;
            do
            {
                if (!allowed)
                    break;
                handled = false;
            } while (false);

            e.Handled = handled;
        }

        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                var text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
