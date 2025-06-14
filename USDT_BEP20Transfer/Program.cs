using System;
using System.Threading.Tasks;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Hex.HexTypes;
using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Text.Json;

namespace USDT_BEP20Transfer
{
    // USDT BEP20 Token contract function definitions
    [Function("transfer", "bool")]
    public class TransferFunction : FunctionMessage
    {
        [Parameter("address", "_to", 1)]
        public string? To { get; set; }

        [Parameter("uint256", "_value", 2)]
        public BigInteger Value { get; set; }
    }

    [Function("balanceOf", "uint256")]
    public class BalanceOfFunction : FunctionMessage
    {
        [Parameter("address", "_owner", 1)]
        public string? Owner { get; set; }
    }

    [Function("decimals", "uint8")]
    public class DecimalsFunction : FunctionMessage
    {
    }

    [Function("symbol", "string")]
    public class SymbolFunction : FunctionMessage
    {
    }

    internal class Program
    {
        private static Web3? _web3;
        private static Account? _account;
        private static Contract? _usdtContract;
        private static BigInteger _lastNonce = -1;

        static async Task Main(string[] args)
        {
            try
            {
                // Set invariant culture for consistent decimal handling
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

                while (true)
                {
                    Console.Clear();
                    ShowMainMenu();

                    var key = Console.ReadKey(true);
                    Console.WriteLine();

                    switch (key.KeyChar)
                    {
                        case '1':
                            await ShowConfigurationMenu();
                            break;
                        case '2':
                            await InitializeAndTransfer();
                            break;
                        case '3':
                            Environment.Exit(0);
                            return;
                        default:
                            Console.WriteLine("❌ Invalid option selected");
                            await Task.Delay(1000);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ An error occurred: {ex.Message}");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }

        private static void ShowMainMenu()
        {
            Console.WriteLine("💰 USDT BEP20 Transfer Tool 💰");
            Console.WriteLine("==============================");
            Console.WriteLine("\nMain Menu:");
            Console.WriteLine("1. ⚙️  Configuration Settings");
            Console.WriteLine("2. 🚀 Make Transfer");
            Console.WriteLine("3. 🚪 Exit");
            Console.Write("\nSelect an option: ");
        }

        private static async Task ShowConfigurationMenu()
        {
            var config = USDT_BEP20TransferConfig.Instance;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("⚙️  Configuration Settings");
                Console.WriteLine("========================");
                Console.WriteLine($"\n1. 📤 From Address: {MaskAddress(config.FromAddress)}");
                Console.WriteLine($"2. 📥 To Address (Binance TR): {MaskAddress(config.BinanceTrAddress)}");
                Console.WriteLine($"3. 🔐 Private Key: {MaskPrivateKey(config.PrivateKey)}");
                Console.WriteLine($"4. 💵 Transfer Amount: {config.TransferAmount} USDT");
                Console.WriteLine($"5. 🧪 Test Mode: {(config.TestMode ? "Enabled" : "Disabled")}");
                Console.WriteLine($"6. ⛽ Min Gas Limit: {config.MinGasLimit}");
                Console.WriteLine($"7. 🔗 USDT Contract: {MaskAddress(config.UsdtContractAddress)}");
                Console.WriteLine("8. 💾 Save and Return to Main Menu");
                Console.Write("\nSelect an option: ");

                var key = Console.ReadKey(true);
                Console.WriteLine();

                switch (key.KeyChar)
                {
                    case '1':
                        await UpdateFromAddress();
                        break;
                    case '2':
                        await UpdateBinanceTrAddress();
                        break;
                    case '3':
                        UpdatePrivateKey();
                        break;
                    case '4':
                        UpdateTransferAmount();
                        break;
                    case '5':
                        ToggleTestMode();
                        break;
                    case '6':
                        UpdateGasLimit();
                        break;
                    case '7':
                        UpdateUsdtContract();
                        break;
                    case '8':
                        USDT_BEP20TransferConfig.Save();
                        return;
                    default:
                        Console.WriteLine("❌ Invalid option selected");
                        await Task.Delay(1000);
                        break;
                }
            }
        }

        private static string MaskAddress(string address)
        {
            if (string.IsNullOrEmpty(address)) return "Not Set";
            if (address.Length <= 10) return address;
            return $"{address[..6]}...{address[^4..]}";
        }

        private static string MaskPrivateKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return "Not Set";
            return "**********************";
        }

        private static async Task UpdateFromAddress()
        {
            var config = USDT_BEP20TransferConfig.Instance;
            Console.Write("\n📤 Enter your BSC wallet address (starting with 0x): ");
            string input = Console.ReadLine()?.Trim() ?? "";
            if (IsValidBscAddress(input))
            {
                config.FromAddress = input;
                Console.WriteLine("✅ From address updated successfully!");
            }
            else
            {
                Console.WriteLine("❌ Invalid BSC address format! Address should start with '0x' followed by 40 hexadecimal characters.");
            }
            await Task.Delay(1500);
        }

        private static async Task UpdateBinanceTrAddress()
        {
            var config = USDT_BEP20TransferConfig.Instance;
            Console.WriteLine("\n📥 Enter your Binance TR BEP20 deposit address:");
            Console.WriteLine("(You can find this in Binance TR > Wallet > Deposit > USDT > BEP20 network)");
            string input = Console.ReadLine()?.Trim() ?? "";
            if (IsValidBscAddress(input))
            {
                config.BinanceTrAddress = input;
                Console.WriteLine("✅ Binance TR address updated successfully!");
            }
            else
            {
                Console.WriteLine("❌ Invalid BSC address format! Address should start with '0x' followed by 40 hexadecimal characters.");
            }
            await Task.Delay(1500);
        }

        private static void UpdatePrivateKey()
        {
            var config = USDT_BEP20TransferConfig.Instance;
            Console.WriteLine("\n🔐 Enter your wallet's private key:");
            Console.WriteLine("⚠️  WARNING: Never share your private key with anyone!");
            string input = Console.ReadLine()?.Trim() ?? "";
            if (!string.IsNullOrEmpty(input) && (input.Length >= 64 || input.StartsWith("0x")))
            {
                config.PrivateKey = input;
                Console.WriteLine("✅ Private key updated successfully!");
            }
            else
            {
                Console.WriteLine("❌ Invalid private key format!");
            }
            Task.Delay(1500).Wait();
        }

        private static void UpdateTransferAmount()
        {
            var config = USDT_BEP20TransferConfig.Instance;
            Console.Write("\n💵 Enter transfer amount in USDT: ");
            if (decimal.TryParse(Console.ReadLine()?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount) && amount > 0)
            {
                config.TransferAmount = amount;
                Console.WriteLine("✅ Transfer amount updated successfully!");
            }
            else
            {
                Console.WriteLine("❌ Invalid amount! Please enter a valid number greater than 0.");
            }
            Task.Delay(1500).Wait();
        }

        private static void ToggleTestMode()
        {
            var config = USDT_BEP20TransferConfig.Instance;
            config.TestMode = !config.TestMode;
            Console.WriteLine($"\n🧪 Test mode {(config.TestMode ? "enabled" : "disabled")}!");
            if (!config.TestMode)
            {
                Console.WriteLine("⚠️  WARNING: Real transactions will be made!");
            }
            Task.Delay(1500).Wait();
        }

        private static void UpdateGasLimit()
        {
            var config = USDT_BEP20TransferConfig.Instance;
            Console.Write("\n⛽ Enter minimum gas limit (default 60000 for token transfers): ");
            if (int.TryParse(Console.ReadLine()?.Trim(), out int limit) && limit >= 21000)
            {
                config.MinGasLimit = limit;
                Console.WriteLine("✅ Gas limit updated successfully!");
            }
            else
            {
                Console.WriteLine("❌ Invalid gas limit! Must be at least 21000.");
            }
            Task.Delay(1500).Wait();
        }

        private static void UpdateUsdtContract()
        {
            var config = USDT_BEP20TransferConfig.Instance;
            Console.WriteLine("\n🔗 Current USDT BEP20 contract address:");
            Console.WriteLine($"   {config.UsdtContractAddress}");
            Console.Write("\nEnter new USDT contract address (or press Enter to keep current): ");
            string input = Console.ReadLine()?.Trim() ?? "";
            if (!string.IsNullOrEmpty(input) && IsValidBscAddress(input))
            {
                config.UsdtContractAddress = input;
                Console.WriteLine("✅ USDT contract address updated successfully!");
            }
            else if (!string.IsNullOrEmpty(input))
            {
                Console.WriteLine("❌ Invalid contract address format!");
            }
            Task.Delay(1500).Wait();
        }

        private static async Task InitializeAndTransfer()
        {
            var config = USDT_BEP20TransferConfig.Instance;

            // Validate configuration
            if (string.IsNullOrEmpty(config.FromAddress) ||
                string.IsNullOrEmpty(config.BinanceTrAddress) ||
                string.IsNullOrEmpty(config.PrivateKey))
            {
                Console.WriteLine("\n❌ Error: Please configure all required settings first!");
                await Task.Delay(2000);
                return;
            }

            try
            {
                // Initialize Web3 and Account
                _account = new Account(config.PrivateKey.Replace("0x", ""), config.ChainId);
                _web3 = new Web3(_account, config.BscMainnetRpc);

                if (_web3 == null || _account == null)
                {
                    Console.WriteLine("\n❌ Error: Failed to initialize Web3 or Account");
                    await Task.Delay(2000);
                    return;
                }

                // Initialize USDT contract
                _usdtContract = _web3.Eth.GetContract(@"[{""constant"":true,""inputs"":[{""name"":""_owner"",""type"":""address""}],""name"":""balanceOf"",""outputs"":[{""name"":""balance"",""type"":""uint256""}],""type"":""function""},{""constant"":false,""inputs"":[{""name"":""_to"",""type"":""address""},{""name"":""_value"",""type"":""uint256""}],""name"":""transfer"",""outputs"":[{""name"":"""",""type"":""bool""}],""type"":""function""}]", config.UsdtContractAddress);

                // Check BNB balance for gas fees
                decimal bnbBalance = await CheckBnbBalance();
                if (bnbBalance <= 0)
                {
                    Console.WriteLine("Failed to retrieve BNB balance. Please check your connection.");
                    await Task.Delay(2000);
                    return;
                }

                // Check USDT balance
                decimal usdtBalance = await CheckUsdtBalance();
                if (usdtBalance < 0)
                {
                    Console.WriteLine("Failed to retrieve USDT balance. Please check your connection.");
                    await Task.Delay(2000);
                    return;
                }

                bool isValid = await ValidateAddress();
                if (!isValid)
                {
                    Console.WriteLine("Please provide a valid Binance TR deposit address before continuing.");
                    await Task.Delay(2000);
                    return;
                }

                await Transfer(bnbBalance, usdtBalance);

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error: {ex.Message}");
                await Task.Delay(2000);
            }
        }

        private static async Task<decimal> CheckBnbBalance()
        {
            var config = USDT_BEP20TransferConfig.Instance;

            if (_web3 == null)
            {
                Console.WriteLine("\n❌ Error: Web3 not initialized");
                return 0;
            }

            try
            {
                var balance = await _web3.Eth.GetBalance.SendRequestAsync(config.FromAddress);
                var bnb = Web3.Convert.FromWei(balance.Value);

                Console.WriteLine($"\n💰 Current BNB Balance: {bnb.ToString("F8", CultureInfo.InvariantCulture)} BNB");
                Console.WriteLine($"📤 Address: {config.FromAddress}");

                return bnb;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error checking BNB balance: {ex.Message}");
                return 0;
            }
        }

        private static async Task<decimal> CheckUsdtBalance()
        {
            var config = USDT_BEP20TransferConfig.Instance;

            if (_web3 == null || _usdtContract == null)
            {
                Console.WriteLine("\n❌ Error: Web3 or USDT contract not initialized");
                return -1;
            }

            try
            {
                var balanceOfHandler = _web3.Eth.GetContractQueryHandler<BalanceOfFunction>();
                var balance = await balanceOfHandler.QueryAsync<BigInteger>(config.UsdtContractAddress, new BalanceOfFunction { Owner = config.FromAddress });

                // USDT has 18 decimals on BEP20
                var usdt = (decimal)balance / (decimal)Math.Pow(10, 18);

                Console.WriteLine($"💵 Current USDT Balance: {usdt.ToString("F6", CultureInfo.InvariantCulture)} USDT");

                return usdt;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error checking USDT balance: {ex.Message}");
                return -1;
            }
        }

        private static async Task Transfer(decimal bnbBalance, decimal usdtBalance)
        {
            var config = USDT_BEP20TransferConfig.Instance;

            if (_web3 == null || _account == null || _usdtContract == null)
            {
                Console.WriteLine("\n❌ Error: Web3, Account, or USDT contract not initialized");
                return;
            }

            Console.WriteLine("\n🚀 Initiating USDT transfer...");

            // Show transfer details
            Console.WriteLine($"\n📋 Transfer Details:");
            Console.WriteLine($"From: {config.FromAddress}");
            Console.WriteLine($"To: {config.BinanceTrAddress}");
            Console.WriteLine($"Amount to Transfer: {config.TransferAmount.ToString("F6", CultureInfo.InvariantCulture)} USDT");
            Console.WriteLine($"Current USDT Balance: {usdtBalance.ToString("F6", CultureInfo.InvariantCulture)} USDT");
            Console.WriteLine($"Current BNB Balance: {bnbBalance.ToString("F8", CultureInfo.InvariantCulture)} BNB");

            if (config.TestMode)
            {
                Console.WriteLine("\n🧪 TEST MODE is enabled - No actual transfer will be made");
                return;
            }

            // Real transfer mode checks
            if (decimal.Compare(usdtBalance, config.TransferAmount) < 0)
            {
                Console.WriteLine($"\n❌ Insufficient USDT balance. Need {config.TransferAmount.ToString("F6", CultureInfo.InvariantCulture)} USDT but only have {usdtBalance.ToString("F6", CultureInfo.InvariantCulture)} USDT");
                return;
            }

            try
            {
                // Get current gas price
                var gasPrice = await _web3.Eth.GasPrice.SendRequestAsync();
                var gasPriceGwei = Web3.Convert.FromWei(gasPrice.Value, Nethereum.Util.UnitConversion.EthUnit.Gwei);

                // Get nonce
                var nonce = await GetNextNonce();

                // Convert USDT amount to Wei (18 decimals)
                var amountInWei = new BigInteger(config.TransferAmount * (decimal)Math.Pow(10, 18));

                // Create transfer function
                var transferFunction = new TransferFunction
                {
                    To = config.BinanceTrAddress,
                    Value = amountInWei,
                    FromAddress = _account.Address,
                    Gas = config.MinGasLimit,
                    GasPrice = gasPrice.Value,
                    Nonce = nonce
                };

                // Estimate gas for the transfer
                var transferHandler = _web3.Eth.GetContractTransactionHandler<TransferFunction>();
                var estimatedGas = await transferHandler.EstimateGasAsync(config.UsdtContractAddress, transferFunction);
                var gasLimit = Math.Max((int)estimatedGas.Value, config.MinGasLimit);

                // Update gas limit
                transferFunction.Gas = gasLimit;

                // Calculate gas costs
                var gasCost = Web3.Convert.FromWei(gasPrice.Value * gasLimit);

                if (decimal.Compare(bnbBalance, gasCost) < 0)
                {
                    Console.WriteLine($"\n❌ Insufficient BNB balance for gas fees. Need {gasCost.ToString("F8", CultureInfo.InvariantCulture)} BNB but only have {bnbBalance.ToString("F8", CultureInfo.InvariantCulture)} BNB");
                    return;
                }

                Console.WriteLine($"\n⛽ Gas Details:");
                Console.WriteLine($"Gas price: {gasPriceGwei.ToString("F2", CultureInfo.InvariantCulture)} Gwei");
                Console.WriteLine($"Estimated gas limit: {gasLimit}");
                Console.WriteLine($"Gas cost: {gasCost.ToString("F8", CultureInfo.InvariantCulture)} BNB");
                Console.WriteLine($"Remaining BNB after transfer: {(bnbBalance - gasCost).ToString("F8", CultureInfo.InvariantCulture)} BNB");
                Console.WriteLine($"Nonce: {nonce}");

                Console.WriteLine("\n🎯 Transaction ready to send!");
                Console.Write("Press Y to confirm or any other key to cancel: ");

                if (char.ToUpper(Console.ReadKey().KeyChar) != 'Y')
                {
                    Console.WriteLine("\n❌ Transaction cancelled by user");
                    return;
                }

                Console.WriteLine("\n\n🔄 Preparing transaction...");

                // Send the transaction
                var txHash = await transferHandler.SendRequestAsync(config.UsdtContractAddress, transferFunction);

                Console.WriteLine($"\n✅ Transaction sent successfully!");
                Console.WriteLine($"Transaction Hash: {txHash}");
                Console.WriteLine("\nYou can track the transaction status on BscScan:");
                Console.WriteLine($"https://bscscan.com/tx/{txHash}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error sending transaction: {ex.Message}");
            }
        }

        private static bool IsValidBscAddress(string address)
        {
            var pattern = @"^0x[0-9a-fA-F]{40}$";
            return Regex.IsMatch(address, pattern);
        }

        private static async Task<bool> IsValidAddress(string address)
        {
            if (_web3 == null)
            {
                Console.WriteLine("\n❌ Error: Web3 not initialized");
                return false;
            }

            if (!IsValidBscAddress(address)) return false;
            var code = await _web3.Eth.GetCode.SendRequestAsync(address);
            return code == "0x"; // Not a contract address
        }

        private static async Task<bool> ValidateAddress()
        {
            var config = USDT_BEP20TransferConfig.Instance;

            if (_web3 == null)
            {
                Console.WriteLine("\n❌ Error: Web3 not initialized");
                return false;
            }

            if (string.IsNullOrEmpty(config.BinanceTrAddress))
            {
                Console.WriteLine("\n❌ Error: Binance TR deposit address is not set!");
                Console.WriteLine("\n📋 Please follow these steps:");
                Console.WriteLine("1. Log in to Binance TR");
                Console.WriteLine("2. Go to Wallet > Deposit");
                Console.WriteLine("3. Select USDT");
                Console.WriteLine("4. Choose Network: BEP20 (BSC)");
                Console.WriteLine("5. Copy the deposit address");
                Console.WriteLine("6. Enter the deposit address when prompted");
                return false;
            }

            if (!await IsValidAddress(config.BinanceTrAddress))
            {
                Console.WriteLine("\n❌ Error: Invalid BSC address or contract address detected");
                Console.WriteLine("Please ensure you're using a valid BSC wallet address (not a contract address)");
                return false;
            }

            Console.WriteLine("\n✅ Address validation successful");
            return true;
        }

        private static async Task<BigInteger> GetNextNonce()
        {
            if (_web3 == null || _account == null)
            {
                throw new InvalidOperationException("Web3 or Account not initialized");
            }

            if (_lastNonce == -1)
            {
                _lastNonce = await _web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(_account.Address);
            }
            else
            {
                _lastNonce++;
            }

            return _lastNonce;
        }
    }
}