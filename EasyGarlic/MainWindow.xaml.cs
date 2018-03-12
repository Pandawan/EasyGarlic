using NLog;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EasyGarlic {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {

        private static Logger logger = LogManager.GetLogger("MainLogger");
        private static Logger headerLogger = LogManager.GetLogger("HeaderLogger");
        public Linker linker;
        private SettingsWindow settingsWindow;
        public OutputWindow outputWindow;

        private bool initializing;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            initializing = true;
            headerLogger.Info("");

            // Setup Loading text
            logger.Info("Loading...");
            LoadingText = "Loading...";

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

            bool skipAPI = false;

            // Loading progress for the Linker
            Progress<ProgressReport> loadingProgress = new Progress<ProgressReport>((data) =>
            {
                LoadingText = data.message;

                // If there was an error
                if (data.error != null)
                {
                    // Problem while fetching API data
                    if (data.error.GetType() == typeof(System.Net.WebException))
                    {
                        logger.Error(data.message);
                        logger.Error(data.error);
                        skipAPI = true;
                        ConnectionErrors = true;
                    }
                }
            });

            // Setup Managers & Linkers
            linker = new Linker();
            await linker.Setup(loadingProgress);

            // Start Output Window
            if (linker.minerManager.data.openConsole)
            {
                OpenDebugConsole();
            }

            // Get Pool List
            logger.Info("Loading Pool List...");
            PoolList = new List<PoolData>();
            // If there were no issues contacting Online Data
            if (!skipAPI)
            {
                // Fetch Pool List
                try
                {
                    PoolList = new List<PoolData>(await linker.networkManager.GetPoolData(linker.networkManager.data.pools));
                }
                // If there were issues contacting Pool List
                catch (System.Net.WebException err)
                {
                    // Report error
                    ((IProgress<ProgressReport>)loadingProgress).Report(new ProgressReport("Could not load Pool Data at " + linker.networkManager.data.pools, err));

                    // Create an empty list 
                    PoolList = new List<PoolData>();
                }
            }
            // Add Custom item to the Pool List
            PoolList.Add(PoolData.Custom);

            // Select Pool List Item if LocalData.savedPool is set
            if (linker.minerManager.data.savedPool != null)
            {
                PoolData saved = linker.minerManager.data.savedPool;
                int index = saved.id;

                // If that object is actually a custom
                if (saved.name == "Custom")
                {
                    // Use it as a custom (last index)
                    index = PoolList.Count - 1;
                    PoolList[index].stratum = saved.stratum;
                }

                PoolListIndex = index;
                logger.Info("Using saved pool: " + saved);
            }

            // Check Saved Address
            AddressInput = linker.minerManager.GetSavedAddress();
            logger.Info("Using saved address: " + AddressInput);

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
            initializing = false;
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

            if (outputWindow != null)
            {
                // Close Output Window
                outputWindow.Close();
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

        private bool connectionErrors;
        public bool ConnectionErrors
        {
            get
            {
                return connectionErrors;
            }
            set
            {
                connectionErrors = value;

                OnPropertyChanged(nameof(ConnectionErrors));
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

        private int poolListIndex;
        public int PoolListIndex
        {
            get
            {
                return poolListIndex;
            }
            set
            {
                poolListIndex = value;

                OnPropertyChanged(nameof(PoolListIndex));
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

                // If this is just an overall status rather than a miner status
                if (data.id == "NONE")
                {
                    return;
                }

                MiningTab tab = MiningTabs.FirstOrDefault(x => (x.id.Contains(data.id) || data.id.Contains(x.id)));

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
            logger.Info("Starting miners on " + poolInfo);

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

        // Use a dictionary to show multiple steps happening at once
        private Dictionary<string, string> progressSteps = new Dictionary<string, string>();

        private async Task CheckMiner(string id)
        {
            Progress<bool> progress = new Progress<bool>((data) =>
            {
                ReadyToStart = data;
            });
            Progress<string> installingProgress = new Progress<string>((data) =>
            {
                // Set its data
                progressSteps[id] = data;

                // Change info text to update with every steps' info
                InfoText = String.Join(", ", progressSteps.Values);
            });

            ReadyToStart = false;
            EnableAdvanced = false;

            // Add the current step
            progressSteps.Add(id, "");

            // Add to tab list
            MiningTab tab = new MiningTab() { Header = Utilities.IDToTitle(id), id = id, Data = new MiningTabData() };
            MiningTabs.Add(tab);

            // Enable And/Or Download the miner
            await linker.minerManager.EnableMiner(id, progress, installingProgress);

            // Remove from the steps
            progressSteps.Remove(id);


            ReadyToStart = true;
            EnableAdvanced = true;
        }

        private void UncheckMiner(string id)
        {
            Progress<bool> progress = new Progress<bool>((data) =>
            {
                ReadyToStart = data;
            });
            linker.minerManager.DisableMiner(id, progress);

            // Remove from tab list
            MiningTab tab = MiningTabs.FirstOrDefault(x => x.id.Contains(id));
            MiningTabs.Remove(tab);
        }

        private async void MiningNvidia_Checked(object sender, RoutedEventArgs e)
        {
            await CheckMiner("nvidia");
        }

        private void MiningNvidia_Unchecked(object sender, RoutedEventArgs e)
        {
            UncheckMiner("nvidia");
        }

        private async void MiningAMD_Checked(object sender, RoutedEventArgs e)
        {
            await CheckMiner("amd");
        }

        private void MiningAMD_Unchecked(object sender, RoutedEventArgs e)
        {
            UncheckMiner("amd");
        }

        private async void MiningCPU_Checked(object sender, RoutedEventArgs e)
        {
            await CheckMiner("cpu");
        }

        private void MiningCPU_Unchecked(object sender, RoutedEventArgs e)
        {
            UncheckMiner("cpu");
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

                // Don't want to override the saved pool on initialization because it can't be changed  by user at that point
                if (!initializing)
                {
                    logger.Info("Using pool: " + poolData);
                    // Save the selected pool as the savedPool for auto-select
                    linker.minerManager.data.savedPool = poolData;
                }
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

        // Open the debug console and manage it here
        public void OpenDebugConsole()
        {
            if (outputWindow == null)
            {
                outputWindow = new OutputWindow();
                outputWindow.Show();

                outputWindow.Closed += (s, e) =>
                {
                    outputWindow = null;
                };
            }
        }
    }
}
