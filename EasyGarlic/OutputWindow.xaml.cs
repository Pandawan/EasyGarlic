using NLog;
using System;
using System.Windows;
using System.Windows.Media;

namespace EasyGarlic {
    /// <summary>
    /// Interaction logic for OutputWindow.xaml
    /// </summary>
    public partial class OutputWindow : Window {
        private NLogText _Target;


        public OutputWindow()
        {
            InitializeComponent();

            Loaded += OutputWindow_Loaded;
        }

        private void OutputWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _Target = new NLogText("DebugOutput", LogLevel.Info);
            _Target.Log += log => LogText(log);
        }

        private void LogText(LogEventInfo log)
        {
            this.Dispatcher.Invoke((Action)delegate () {
                RichTextBoxExtensions.AppendText(MessageView, log.FormattedMessage + "\n", ColorFromEvent(log).ToString());
                //this.MessageView.AppendText(message + "\n", Brushes.Green);
                this.MessageView.ScrollToEnd();
            });
        }

        private SolidColorBrush ColorFromEvent(LogEventInfo e)
        {
            if (e.Level.Name == LogLevel.Debug.Name)
            {
                return Brushes.White;
            }

            if (e.Level.Name == LogLevel.Error.Name)
            {
                return Brushes.Red;
            }

            if (e.Level.Name == LogLevel.Warn.Name)
            {
                return Brushes.Orange;
            }

            return Brushes.White;
        }
    }
}