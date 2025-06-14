using System.Text.Json;

namespace USDT_BEP20Transfer
{
    public class USDT_BEP20TransferConfig
    {
        private const string ConfigFileName = "appsettings.json";
        private static USDT_BEP20TransferConfig? _instance;
        
        public string FromAddress { get; set; } = "";
        public string BinanceTrAddress { get; set; } = "";
        public string PrivateKey { get; set; } = "";
        public decimal TransferAmount { get; set; } = 0;
        public bool TestMode { get; set; } = true;
        public int MinGasLimit { get; set; } = 200000;
        public int ChainId { get; set; } = 56; // BSC Mainnet
        public string BscMainnetRpc { get; set; } = "https://bsc-dataseed.binance.org/";
        public string UsdtContractAddress { get; set; } = "0x55d398326f99059fF775485246999027B3197955"; // USDT BEP20 contract address

        public static USDT_BEP20TransferConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Load();
                }
                return _instance;
            }
        }

        public static USDT_BEP20TransferConfig Load()
        {
            try
            {
                if (File.Exists(ConfigFileName))
                {
                    string jsonString = File.ReadAllText(ConfigFileName);
                    var config = JsonSerializer.Deserialize<USDT_BEP20TransferConfig>(jsonString);
                    if (config != null)
                    {
                        _instance = config;
                        return config;
                    }
                }

                // Create new config with default values if file doesn't exist
                _instance = new USDT_BEP20TransferConfig();
                Save();
                Console.WriteLine($"\n?? Created new configuration file: {ConfigFileName}");
                Console.WriteLine("Please configure your settings in the file and restart the application.");
                return _instance;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n? Error loading configuration: {ex.Message}");
                _instance = new USDT_BEP20TransferConfig();
                return _instance;
            }
        }

        public static void Save()
        {
            try
            {
                if (_instance == null) return;

                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(_instance, options);
                File.WriteAllText(ConfigFileName, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n? Error saving configuration: {ex.Message}");
            }
        }
    }
}