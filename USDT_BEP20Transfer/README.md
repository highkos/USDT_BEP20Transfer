# ?? USDT BEP20 Transfer Tool

A .NET 6 console application for easily transferring USDT (BEP20) tokens from your BSC wallet to Binance TR. Never accidentally send your tokens to the wrong network again! ??

## ?? Features

- ?? Secure local private key management
- ?? Real-time BNB and USDT balance checking
- ? Automatic gas estimation and validation
- ?? Test mode for safe configuration testing
- ?? Transaction status tracking with BscScan links
- ?? Robust error handling and validation
- ? Support for BSC network (Binance Smart Chain)

## ?? Getting Started

### Prerequisites

- ?? .NET 6.0 SDK or later
- ?? BSC wallet with:
  - USDT (BEP20) tokens
  - BNB for gas fees
- ?? Binance TR account

### ?? Installation

1. Clone the repository or download the latest release
2. Build the project:
   ```bash
   dotnet build
   ```
3. Run the application:
   ```bash
   dotnet run
   ```

## ?? Configuration

On first run, the application will create an `appsettings.json` file. You can configure:

- ?? **From Address**: Your BSC wallet address
- ?? **Binance TR Address**: Your Binance TR USDT (BEP20) deposit address
- ?? **Private Key**: Your BSC wallet private key
- ?? **Transfer Amount**: Amount of USDT to transfer
- ?? **Test Mode**: Enable/disable test mode
- ? **Min Gas Limit**: Minimum gas limit for transactions
- ?? **USDT Contract**: USDT BEP20 contract address (pre-configured)

?? **IMPORTANT**: Never share your private key with anyone!

## ?? Usage

1. Launch the application
2. Choose "Configuration Settings" to set up your wallet and transfer details
3. Enable/disable test mode as needed
4. Select "Make Transfer" to initiate the USDT transfer
5. Follow the prompts and confirm the transaction
6. Track your transaction using the provided BscScan link

## ?? Validation Checks

The application performs several checks before executing a transfer:

- ? BSC address format validation
- ? Sufficient USDT balance
- ? Sufficient BNB for gas fees
- ? Contract address validation
- ? Gas price estimation

## ??? Security

- Private keys are stored locally in the configuration file
- No external API dependencies except BSC node
- All transactions require manual confirmation
- Test mode available for safe configuration testing

## ?? Disclaimer

This tool is provided as-is. Always verify transaction details before confirming. The developers are not responsible for any lost funds due to user error or network issues.

## ?? Contributing

Contributions, issues, and feature requests are welcome! Feel free to check the issues page.

## ?? License

This project is licensed under the MIT License - see the LICENSE file for details.

## ?? Acknowledgments

- Nethereum library for Ethereum/BSC interaction
- BSC network for affordable transactions
- Binance TR for providing BEP20 deposit support

Remember: Always double-check addresses and amounts before confirming transactions! ??