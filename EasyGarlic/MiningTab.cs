using System.ComponentModel;

namespace EasyGarlic {
    public class MiningTab {
        public string Header { get; set; }

        public string id;

        public MiningTabData Data { get; set; }
    }
    public class MiningTabData : INotifyPropertyChanged {

        private string hashrateText;
        public string HashrateText
        {
            get
            {
                return hashrateText;
            }
            set
            {
                hashrateText = value;

                OnPropertyChanged(nameof(HashrateText));
            }
        }

        private string lastBlockText;
        public string LastBlockText
        {
            get
            {
                return lastBlockText;
            }
            set
            {
                lastBlockText = value;

                OnPropertyChanged(nameof(LastBlockText));
            }
        }

        private string acceptedSharesText;
        public string AcceptedSharesText
        {
            get
            {
                return acceptedSharesText;
            }
            set
            {
                acceptedSharesText = value;

                OnPropertyChanged(nameof(AcceptedSharesText));
            }
        }

        private string rejectedSharesText;
        public string RejectedSharesText
        {
            get
            {
                return rejectedSharesText;
            }
            set
            {
                rejectedSharesText = value;

                OnPropertyChanged(nameof(RejectedSharesText));
            }
        }

        private string temperatureText;
        public string TemperatureText
        {
            get
            {
                return temperatureText;
            }
            set
            {
                temperatureText = value;

                OnPropertyChanged(nameof(TemperatureText));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
