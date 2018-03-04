using NLog;
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
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace EasyGarlic {
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window, INotifyPropertyChanged {

        private static Logger logger = LogManager.GetLogger("SettingsLogger");

        private MainWindow parentWindow;

        #region WPF Properties

        private bool enableOptions;
        public bool EnableOptions
        {
            get
            {
                return enableOptions;
            }
            set
            {
                enableOptions = value;

                OnPropertyChanged(nameof(EnableOptions));
            }
        }

        private string versionText;
        public string VersionText
        {
            get
            {
                return versionText;
            }
            set
            {
                versionText = value;

                OnPropertyChanged(nameof(VersionText));
            }
        }

        private bool showCPUOptions;
        public bool ShowCPUOptions
        {
            get
            {
                return showCPUOptions;
            }
            set
            {
                showCPUOptions = value;

                OnPropertyChanged(nameof(ShowCPUOptions));
            }
        }

        private bool showALTOptions;
        public bool ShowALTOptions
        {
            get
            {
                return showALTOptions;
            }
            set
            {
                showALTOptions = value;

                OnPropertyChanged(nameof(ShowALTOptions));
            }
        }

        private bool showMiningTab;
        public bool ShowMiningTab
        {
            get
            {
                return showMiningTab;
            }
            set
            {
                showMiningTab = value;

                OnPropertyChanged(nameof(ShowMiningTab));
            }
        }

        private KeyValuePair<int, Miner> selectedMiner;
        private List<Miner> minerList;
        public List<Miner> MinerList
        {
            get
            {
                return minerList;
            }
            set
            {
                minerList = value;

                OnPropertyChanged(nameof(MinerList));
            }
        }

        private int intensityInput;
        public int IntensityInput
        {
            get
            {
                return intensityInput;
            }
            set
            {
                intensityInput = value;

                OnPropertyChanged(nameof(IntensityInput));
            }
        }

        private string customParameters;
        public string CustomParameters
        {
            get
            {
                return customParameters;
            }
            set
            {
                customParameters = value;

                OnPropertyChanged(nameof(CustomParameters));
            }
        }
        
        private bool useAlternateMiner;
        public bool UseAlternateMiner
        {
            get
            {
                return useAlternateMiner;
            }
            set
            {
                useAlternateMiner = value;

                OnPropertyChanged(nameof(UseAlternateMiner));
            }
        }


        private bool showConsoleOnStart;
        public bool ShowConsoleOnStart
        {
            get
            {
                return showConsoleOnStart;
            }
            set
            {
                showConsoleOnStart = value;

                OnPropertyChanged(nameof(ShowConsoleOnStart));
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion


        public SettingsWindow(MainWindow mainWindow)
        {
            parentWindow = mainWindow;

            InitializeComponent();
            DataContext = this;
            Loaded += SettingsWindow_Loaded;
            Closing += SettingsWindow_Closing;
        }

        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Load different Miner items in ComboBox
            LoadMiningView(0);

            // TODO: Make it so that you can run a different .exe instead of the default miner (if there are more than one available)
            // TODO: Clean up the _alt mode so that you can't edit regular & alt in settings at same time

            // Load Version Number
            VersionText = "v" + Config.VERSION;

            // Load Console Settings
            ShowConsoleOnStart = parentWindow.linker.minerManager.data.openConsole;

            // Hide CPU Options
            ShowCPUOptions = false;
            EnableOptions = true;
        }

        private void LoadMiningView(int index)
        {
            // Load different Miner items in ComboBox
            MinerList = new List<Miner>(parentWindow.linker.minerManager.data.installed.Values);
            if (MinerList.Count > 0 && index >= 0)
            {
                // Show the Mining tabs
                ShowMiningTab = true;

                // Load default miner data
                selectedMiner = new KeyValuePair<int, Miner>(index, MinerList[index]);
                minerComboBox.SelectedIndex = index;
                IntensityInput = selectedMiner.Value.customIntensity;
                CustomParameters = selectedMiner.Value.customParameters;
                UseAlternateMiner = selectedMiner.Value.usingAlt;

                // Use CPU view if cpu
                ShowCPUOptions = (selectedMiner.Value.type == "cpu");

                // Use ALT view if alt
                ShowALTOptions = selectedMiner.Value.alt;
            }
            else
            {
                ShowMiningTab = false;
                EnableOptions = false;
                selectedMiner = new KeyValuePair<int, Miner>(-1, null);
                minerComboBox.SelectedIndex = -1;
            }
        }

        private async void SettingsWindow_Closing(object sender, CancelEventArgs e)
        {
            // Apply Miner changes
            Dictionary<string, Miner> installed = parentWindow.linker.minerManager.data.installed;
            for (int i = 0; i < MinerList.Count; i++)
            {
                installed[MinerList[i].GetID()] = MinerList[i];
            }

            // Save Console settings
            parentWindow.linker.minerManager.data.openConsole = ShowConsoleOnStart;

            // Save Data
            await parentWindow.linker.minerManager.data.SaveAsync();

            // TODO: Perhaps change it so you don't have to reload EVERYTHING
            // Reload data on main window
            parentWindow.ReloadData();

        }

        #region Data Changed
        private void MinerListCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check if miners are installed
            int selectedIndex = (sender as ComboBox).SelectedIndex;
            if (selectedIndex >= 0 && MinerList.Count > selectedIndex)
            {
                // Set selected miner
                selectedMiner = new KeyValuePair<int, Miner>((sender as ComboBox).SelectedIndex, MinerList[selectedIndex]);
            }
            else
            {
                // Disable everything
                (sender as ComboBox).SelectedIndex = -1;
                ShowCPUOptions = false;
            }

            LoadMiningView(selectedIndex);
        }

        private void IntensityInput_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (selectedMiner.Value != null)
            {
                selectedMiner.Value.customIntensity = IntensityInput;
            }
        }

        private void CustomParametersInput_ValueChanged(object sender, TextChangedEventArgs e)
        {
            if (selectedMiner.Value != null)
            {
                selectedMiner.Value.customParameters = CustomParameters;
            }
        }
        
        private void UseAlternateMiner_Checked(object sender, RoutedEventArgs e)
        {
            selectedMiner.Value.usingAlt = UseAlternateMiner;
        }

        private void UseAlternateMiner_Unchecked(object sender, RoutedEventArgs e)
        {
            selectedMiner.Value.usingAlt = UseAlternateMiner;
        }

        #endregion

        private async void UninstallMinerButton_Click(object sender, RoutedEventArgs e)
        {
            Progress<string> uninstallProgress = new Progress<string>((data) =>
            {
                InfoText = data;
            });

            // Uninstall
            await parentWindow.linker.minerManager.UninstallMiner(selectedMiner.Value, uninstallProgress);

            // Refresh View
            LoadMiningView(0);
        }

        private void ResetDefaultsButton_Click(object sender, RoutedEventArgs e)
        {
            IntensityInput = 0;
            CustomParameters = "";
            UseAlternateMiner = false;
            InfoText = "Reset settings for \"" + selectedMiner.Value.GetID() + "\" to default";
        }

        private void DebugButton_Click(object sender, RoutedEventArgs e)
        {
            parentWindow.OpenDebugConsole();
        }
    }
}
