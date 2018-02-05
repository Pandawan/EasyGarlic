using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace EasyGarlic {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {

        private InstallManager manager;
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            manager = new InstallManager();
            await manager.Setup();
            manager.data.installed.Add("nvidia", new LocalData.Miner() { id = "nvidia", path = @"D:\Projects\VS Projects\EasyGarlic\EasyGarlic\EasyGarlic\bin\Debug\data\nvidia.zip", platform = "win", version = "0" });
            await manager.UpdateMiners();
            Console.WriteLine("Finished Downloading!");
            ReadyToShow = true;
        }

        private bool readyToShow;
        public bool ReadyToShow
        {
            get
            {
                return readyToShow;
            }
            set
            {
                readyToShow = value;

                OnPropertyChanged(nameof(ReadyToShow));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }


        private void Start_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MiningType_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
