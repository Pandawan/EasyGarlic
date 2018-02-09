using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

        private InstallManager installManager;
        private MiningManager miningManager;
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadingText = "Loading...";
            Progress<string> loadingProgress = new Progress<string>((data) =>
            {
                LoadingText = data;
            });

            // Create a new InstallManager & Setup
            installManager = new InstallManager();
            await installManager.Setup(loadingProgress);
            
            // Create a MiningManager
            miningManager = new MiningManager(installManager);

            // Add a Test Miner & Download it
            //installManager.AddMiner("nvidia", new LocalData.Miner() { id = "nvidia", path = @"D:\Projects\VS Projects\EasyGarlic\EasyGarlic\EasyGarlic\bin\Debug\data\nvidia.zip", platform = "win", version = "0" });
            //await installManager.UpdateMiners();

            // Tell it we're done
            Console.WriteLine("Finished Downloading!");
            ReadyToShow = true;
            InfoText = "Ready!";
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

        private string loadingText;
        public string LoadingText
        {
            get
            {
                return loadingText;
            }
            set
            {
                loadingText = value;

                OnPropertyChanged(nameof(LoadingText));
            }
        }

        private string infoText;
        public string InfoText
        {
            get
            {
                return infoText;
            }
            set
            {
                infoText = value;

                OnPropertyChanged(nameof(InfoText));
            }
        }

        private string addressInput;
        public string AddressInput
        {
            get
            {
                return addressInput;
            }
            set
            {
                addressInput = value;

                OnPropertyChanged(nameof(AddressInput));
            }
        }

        private string poolInput;
        public string PoolInput
        {
            get
            {
                return poolInput;
            }
            set
            {
                poolInput = value;

                OnPropertyChanged(nameof(PoolInput));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            Progress<string> startingProgress = new Progress<string>((data) =>
            {
                InfoText = data;
            });

           await miningManager.StartMining(AddressInput, PoolInput, startingProgress);
        }

        #region Mining Buttons

        private void MiningNvidia_Checked(object sender, RoutedEventArgs e)
        {
            miningManager.ToggleMiner("nvidia");
        }

        private void MiningNvidia_Unchecked(object sender, RoutedEventArgs e)
        {
            miningManager.ToggleMiner("nvidia");
        }

        private void MiningAMD_Checked(object sender, RoutedEventArgs e)
        {
            miningManager.ToggleMiner("amd");
        }

        private void MiningAMD_Unchecked(object sender, RoutedEventArgs e)
        {
            miningManager.ToggleMiner("amd");
        }

        private void MiningCPU_Checked(object sender, RoutedEventArgs e)
        {
            miningManager.ToggleMiner("cpu");
        }

        private void MiningCPU_Unchecked(object sender, RoutedEventArgs e)
        {
            miningManager.ToggleMiner("cpu");
        }

        #endregion
    }
}
