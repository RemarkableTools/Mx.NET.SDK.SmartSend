# Mx.NET.SDK.SmartSend
⚡ MultiversX SmartSend .NET SDK: Library for interacting with Smart Send contracts on MultiversX blockchain

## How to install?
The content is delivered via nuget packages:
##### RemarkableTools.Mx.SmartSend [![Package](https://img.shields.io/nuget/v/RemarkableTools.Mx.SmartSend)](https://www.nuget.org/packages/RemarkableTools.Mx.SmartSend/)

## Main Features
- Create EGLD/Token/MetaESDT/NFT/SFT transactions for Smart Send contracts

## Quick start guide
### Basic example
```csharp
IApiProvider provider = new ApiProvider(new ApiNetworkConfiguration(Network.DevNet));
var networkConfig = await NetworkConfig.GetFromNetwork(provider);

var account = Account.From(await provider.GetAccount("MY_ADDRESS"));
var mySmartSendContractAddress = "MY_CONTRACT_ADDRESS";

var smartSend = new SmartSend(account, networkConfig, mySmartSendContractAddress);
var inputTransactions = new List<TokenAmount>()
{
    new TokenAmount("ADDRESS_1", 0.1m, ESDT.EGLD()),
    new TokenAmount("ADDRESS_2", 0.03m, ESDT.EGLD())
};
var egldTxs = smartSend.CreateEGLDTransactions(inputTransactions);
//sign and send egldTxs
```

### Advanced example
*The following example is using a wallet __signer__ that should not be used in production, only in private!*
```csharp
IApiProvider provider = new ApiProvider(new ApiNetworkConfiguration(Network.DevNet));
var networkConfig = await NetworkConfig.GetFromNetwork(provider);

var filePath = "PATH/TO/KEYFILE.json";
var password = "PASSWORD";
var signer = Signer.FromKeyFile(filePath, password);

var account = Account.From(await provider.GetAccount(signer.GetAddress().Bech32));
var mySmartSendContractAddress = "MY_CONTRACT_ADDRESS";

var smartSend = new SmartSend(account, networkConfig, mySmartSendContractAddress);
var inputTransactions = new List<TokenAmount>()
{
    new TokenAmount("ADDRESS_1", 0.1m, ESDT.EGLD()),
    new TokenAmount("ADDRESS_2", 0.03m, ESDT.EGLD())
};
var egldTxs = smartSend.CreateEGLDTransactions(inputTransactions);
var signed = egldTxs.MultiSign(signer);
var response = await provider.SendTransactions(signed);
foreach (var hash in response.TxsHashes)
    Console.WriteLine(hash.Value);
```
