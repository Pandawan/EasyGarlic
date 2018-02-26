using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Squirrel;

namespace EasyGarlic {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {

        private static Logger logger = LogManager.GetLogger("MainLogger");
        private static Logger headerLogger = LogManager.GetLogger("HeaderLogger");
        public Linker linker;
        private SettingsWindow settingsWindow;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            headerLogger.Info("");
            logger.Info("Loading...");
            // Setup Loading text
            LoadingText = "Loading...";
            Progress<string> loadingProgress = new Progress<string>((data) =>
            {
                LoadingText = data;
            });

            // Check for Updates
            LoadingText = "Checking for Updates...";
            logger.Info("Checking for Updates...");
            try
            {
                // TODO: Make it so it applies major updates immediately
                // TODO: Change this to use a URL instead
                using (UpdateManager manager = new UpdateManager(@"D:\Projects\VS Projects\EasyGarlic\EasyGarlic\Releases"))
                {
                    await manager.UpdateApp();
                }

            }
            catch (Exception error)
            {
                // Don't show this error in debug mode because it's always gonna happen
                if (error.Message == "Update.exe not found, not a Squirrel-installed app?")
                {
#if !DEBUG
                logger.Error("Updater: " + error);
                LoadingText = "Could not check for updates.";
#endif
                }
                else
                {
                    logger.Error("Updater: " + error);
                    LoadingText = "Could not check for updates.";
                }
            }

            LoadingText = "Loading...";

            // Setup Managers & Linkers
            linker = new Linker();
            await linker.Setup(loadingProgress);

            // Get Pool List
            logger.Info("Loading Pool List...");
            PoolList = new List<PoolData>(await linker.networkManager.GetPoolData(linker.networkManager.data.pools));
            PoolList.Add(PoolData.Custom);

            // Check Saved Address
            AddressInput = linker.minerManager.GetSavedAddress();
            logger.Info("Using saved address: " + AddressInput);

            // Load different mining tabs

            //tabDynamic.DataContext = MiningTabs;
            //MiningTabs.Add(new MiningTab() { Header = "HEADER", id = "Nvidia", Data = new MiningTabData() { HashrateText = "100 AYY" } });

            logger.Info("Miners Installed: " + String.Join(", ", linker.minerManager.data.installed.Keys.ToArray()));

            // Set Default values
            ReadyToShow = true;
            ShowStats = false;
            ShowStop = false;
            EnableAdvanced = true;
            ShowCustomPool = false;
            InfoText = "Ready!";

            // Tell it we're done
            logger.Info("Finished Loading.");
        }

        private async void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            logger.Info("Closing...");
            
            Progress<string> progress = new Progress<string>((data) =>
            {
                InfoText = data;
            });

            // Stop Mining
            await linker.minerManager.StopMining(progress);

            if (settingsWindow != null)
            {
                // Close Settings Window
                settingsWindow.Close();
            }
            
            // Save Data
            await linker.minerManager.data.SaveAsync();

            logger.Info("Closed all processes...");
            headerLogger.Info("");
        }

        public async void ReloadData()
        {
            await Task.Run(() => MainWindow_Loaded(this, null));
        }

#region WPF Properties

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

        private bool readyToStart;
        public bool ReadyToStart
        {
            get
            {
                return readyToStart;
            }
            set
            {
                readyToStart = value;

                OnPropertyChanged(nameof(ReadyToStart));
            }
        }

        private bool showStop;
        public bool ShowStop
        {
            get
            {
                return showStop;
            }
            set
            {
                showStop = value;

                OnPropertyChanged(nameof(ShowStop));
            }
        }

        private bool showStats;
        public bool ShowStats
        {
            get
            {
                return showStats;
            }
            set
            {
                showStats = value;

                OnPropertyChanged(nameof(ShowStats));
            }
        }

        private PoolData lastPoolDataValue;
        private List<PoolData> poolList;
        public List<PoolData> PoolList
        {
            get
            {
                return poolList;
            }
            set
            {
                poolList = value;

                OnPropertyChanged(nameof(PoolList));
            }
        }

        private bool showCustomPool;
        public bool ShowCustomPool
        {
            get
            {
                return showCustomPool;
            }
            set
            {
                showCustomPool = value;

                OnPropertyChanged(nameof(ShowCustomPool));
            }
        }

        private string miningInfoText;
        public string MiningInfoText
        {
            get
            {
                return miningInfoText;
            }
            set
            {
                miningInfoText = value;

                OnPropertyChanged(nameof(MiningInfoText));
            }
        }
        
        private bool enableAdvanced;
        public bool EnableAdvanced
        {
            get
            {
                return enableAdvanced;
            }
            set
            {
                enableAdvanced = value;

                OnPropertyChanged(nameof(EnableAdvanced));
            }
        }

        private ObservableCollection<MiningTab> miningTabs = new ObservableCollection<MiningTab>();
        public ObservableCollection<MiningTab> MiningTabs
        {
            get
            {
                return miningTabs;
            }
            set
            {
                miningTabs = value;

                OnPropertyChanged(nameof(MiningTabs));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

#endregion

#region Start & Stop

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            Progress<MiningStatus> startingProgress = new Progress<MiningStatus>((data) =>
            {
                // TODO: Add an Amount Earned feature
                InfoText = data.info;
                MiningTab tab = MiningTabs.FirstOrDefault(x => x.id.Contains(data.id) || data.id.Contains(x.id));
                
                tab.Data.HashrateText = "Hashrate: " + data.hashRate;
                tab.Data.LastBlockText = "Block: " + data.lastBlock;
                tab.Data.AcceptedSharesText = "Accepted Shares: " + data.acceptedShares;
                tab.Data.RejectedSharesText = "Rejected Shares: " + data.rejectedShares;
                tab.Data.TemperatureText = "Temperature: " + data.temperature;

            });

            // Select first tab
            tabDynamic.SelectedIndex = 0;

            // Show & Hide screens
            ReadyToStart = false;
            ShowStop = true;
            ShowStats = true;
            EnableAdvanced = false;

            // Log Pool Info
            string poolInfo = (lastPoolDataValue.id == -1 ? "Custom Pool (" + lastPoolDataValue.stratum + ")" : lastPoolDataValue.name + " (" + PoolInput.Trim() + ")");
            MiningInfoText = "Mining on " + poolInfo;
            logger.Info("Strating miners on " + poolInfo);

            await linker.minerManager.StartMining(AddressInput.Trim(), PoolInput.Trim(), startingProgress);

            EnableAdvanced = true;
            ShowStats = false;
            ShowStop = false;
            ReadyToStart = true;
        }

        private async void Stop_Click(object sender, RoutedEventArgs e)
        {
            Progress<string> stoppingProgress = new Progress<string>((data) =>
            {
                InfoText = data;
            });

            ReadyToStart = false;
            ShowStop = false;
            EnableAdvanced = false;

            await linker.minerManager.StopMining(stoppingProgress);

            EnableAdvanced = true;
            ShowStop = false;
            ReadyToStart = true;
        }

#endregion

#region Mining Buttons

        private void MiningNvidia_Checked(object sender, RoutedEventArgs e)
        {
            Progress<bool> progress = new Progress<bool>((data) =>
            {
                ReadyToStart = data;
            });
            Progress<string> installingProgress = new Progress<string>((data) =>
            {
                InfoText = data;
            });

            // Add to tab list
            MiningTab tab = new MiningTab() { Header = Utilities.IDToTitle("nvidia"), id = "nvidia" };
            tab.Data = new MiningTabData();
            MiningTabs.Add(tab);

#pragma warning disable 4014
            linker.minerManager.EnableMiner("nvidia", progress, installingProgress);
#pragma warning restore 4014

        }

        private void MiningNvidia_Unchecked(object sender, RoutedEventArgs e)
        {
            Progress<bool> progress = new Progress<bool>((data) =>
            {
                ReadyToStart = data;
            });
            linker.minerManager.DisableMiner("nvidia", progress);

            // Remove from tab list
            MiningTab tab = MiningTabs.FirstOrDefault(x => x.id.Contains("nvidia"));
            MiningTabs.Remove(tab);
        }

        private void MiningAMD_Checked(object sender, RoutedEventArgs e)
        {
            Progress<bool> progress = new Progress<bool>((data) =>
            {
                ReadyToStart = data;
            });
            Progress<string> installingProgress = new Progress<string>((data) =>
            {
                InfoText = data;
            });

            // Add to tab list
            MiningTab tab = new MiningTab() { Header = Utilities.IDToTitle("amd"), id = "amd" };
            tab.Data = new MiningTabData();
            MiningTabs.Add(tab);

#pragma warning disable 4014
            linker.minerManager.EnableMiner("amd", progress, installingProgress);
#pragma warning restore 4014
        }

        private void MiningAMD_Unchecked(object sender, RoutedEventArgs e)
        {
            Progress<bool> progress = new Progress<bool>((data) =>
            {
                ReadyToStart = data;
            });
            linker.minerManager.DisableMiner("amd", progress);

            // Remove from tab list
            MiningTab tab = MiningTabs.FirstOrDefault(x => x.id.Contains("amd"));
            MiningTabs.Remove(tab);
        }

        private void MiningCPU_Checked(object sender, RoutedEventArgs e)
        {
            Progress<bool> progress = new Progress<bool>((data) =>
            {
                ReadyToStart = data;
            });
            Progress<string> installingProgress = new Progress<string>((data) =>
            {
                InfoText = data;
            });

            // Add to tab list
            MiningTab tab = new MiningTab() { Header = Utilities.IDToTitle("cpu"), id = "cpu" };
            tab.Data = new MiningTabData();
            MiningTabs.Add(tab);

#pragma warning disable 4014
            linker.minerManager.EnableMiner("cpu", progress, installingProgress);
#pragma warning restore 4014
        }

        private void MiningCPU_Unchecked(object sender, RoutedEventArgs e)
        {
            Progress<bool> progress = new Progress<bool>((data) =>
            {
                ReadyToStart = data;
            });
            linker.minerManager.DisableMiner("cpu", progress);

            // Remove from tab list
            MiningTab tab = MiningTabs.FirstOrDefault(x => x.id.Contains("cpu"));
            MiningTabs.Remove(tab);
        }

#endregion

#region Input Value Change

        private void PoolListCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count != 0)
            {
                PoolData poolData = e.AddedItems[0] as PoolData;

                // If it's the same value, don't change, it might override custom pool input
                if (lastPoolDataValue != null && lastPoolDataValue.id == poolData.id) return;

                logger.Info("Using pool: " + poolData);

                // If it's a custom, show the pool input
                if (poolData.name == "Custom")
                {
                    PoolInput = "";
                    ShowCustomPool = true;
                }
                // If it's not, set pool input to the first stratum
                else
                {
                    PoolInput = poolData.Value();
                    ShowCustomPool = false;
                }

                lastPoolDataValue = poolData;
            }
        }

        private void AddressInputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            linker.minerManager.SaveAddress((sender as TextBox).Text.Trim());
        }

#endregion

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Change this so it only does that when ACTUALLY uninstalling
            // Uncheck buttons, might be uninstalling
            mineNvidiaButton.IsChecked = false;
            mineCPUButton.IsChecked = false;
            mineAMDButton.IsChecked = false;
            
            settingsWindow = new SettingsWindow(this);
            settingsWindow.ShowDialog();
        }
    }
}
