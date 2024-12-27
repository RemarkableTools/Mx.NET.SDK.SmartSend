using Mx.NET.SDK.Core.Domain.Values;
using Mx.NET.SDK.Core.Domain;
using Mx.NET.SDK.Domain;
using Mx.NET.SDK.Domain.Data.Accounts;
using Mx.NET.SDK.Domain.Data.Network;
using Mx.NET.SDK.SmartSend.Models;
using static Mx.NET.SDK.TransactionsManager.EGLDTransactionRequest;
using static Mx.NET.SDK.TransactionsManager.TokenTransactionRequest;
using static Mx.NET.SDK.TransactionsManager.ESDTTransactionRequest;
using static Mx.NET.SDK.SmartSend.Helper.ScMethods;
using Mx.NET.SDK.Core.Domain.Helper;

namespace Mx.NET.SDK.SmartSend
{
    public class SmartSend
    {
        private readonly Account _account;
        private readonly NetworkConfig _networkConfig;
        private string SmartSendContract { get; set; } = string.Empty;
        private int ChunkLimit { get; set; } = 100;

        public SmartSend(
            Account account,
            NetworkConfig networkConfig)
        {
            _account = account;
            _networkConfig = networkConfig;
        }

        public SmartSend(
            Account account,
            NetworkConfig networkConfig,
            string smartSendContract,
            int chunkLimit = 100)
        {
            _account = account;
            _networkConfig = networkConfig;
            SmartSendContract = smartSendContract;
            ChunkLimit = chunkLimit;
        }

        public void SetChunkLimit(int limit)
        {
            ChunkLimit = limit;
        }

        public void SetSmartSendContractAddress(string address)
        {
            SmartSendContract = address;
        }

        public TransactionRequest[] CreateEGLDTransactions(
            List<TokenAmount> inputTransactions,
            int gasPerTx = 600000,
            string? contractAddress = null)
        {
            Address smartSendContractAddress;
            if (contractAddress != null)
            {
                smartSendContractAddress = Address.FromBech32(contractAddress);
            }
            else
            {
                if (!string.IsNullOrEmpty(SmartSendContract))
                    smartSendContractAddress = Address.FromBech32(SmartSendContract);
                else
                    throw new Exception("Smart Send Contract address is not set");
            }

            var transactionsChunks = inputTransactions.Chunk(ChunkLimit);
            List<TransactionRequest> transactionRequests = new();
            foreach (var chunk in transactionsChunks)
            {
                var gasLimit = chunk.Length < 7 ? new GasLimit(4000000) : new GasLimit(chunk.Length * gasPerTx);

                List<IBinaryType> arguments = new();
                foreach (var tx in chunk)
                {
                    arguments.Add(tx.Address);
                    arguments.Add(NumericValue.BigUintValue(tx.Amount.Value));
                }

                var amounts = chunk.Select(tx => tx.Amount.Value);
                var amount = ESDTAmount.From(amounts.Aggregate((currentSum, item) => currentSum + item).ToString());

                var transactionRequest = EGLDTransferToSmartContract(
                    _networkConfig,
                    _account,
                    smartSendContractAddress,
                    gasLimit,
                    amount,
                    SmartSendMethod,
                    arguments.ToArray()
                );

                _account.IncrementNonce();
                transactionRequests.Add(transactionRequest);
            }

            return transactionRequests.ToArray();
        }

        public TransactionRequest[] CreateTokenTransactions(
            AccountToken token,
            List<TokenAmount> inputTransactions,
            int gasPerTx = 900000,
            string? contractAddress = null)
        {
            Address smartSendContractAddress;
            if (contractAddress != null)
            {
                smartSendContractAddress = Address.FromBech32(contractAddress);
            }
            else
            {
                if (!string.IsNullOrEmpty(SmartSendContract))
                    smartSendContractAddress = Address.FromBech32(SmartSendContract);
                else
                    throw new Exception("Smart Send Contract address is not set");
            }

            var transactionsChunks = inputTransactions.Chunk(ChunkLimit);
            List<TransactionRequest> transactionRequests = new();
            foreach (var chunk in transactionsChunks)
            {
                var gasLimit = chunk.Length < 7 ? new GasLimit(6000000) : new GasLimit(chunk.Length * gasPerTx);

                List<IBinaryType> arguments = new();
                foreach (var tx in chunk)
                {
                    arguments.Add(tx.Address);
                    arguments.Add(NumericValue.BigUintValue(tx.Amount.Value));
                }

                var amounts = chunk.Select(tx => tx.Amount.Value);
                var amount = ESDTAmount.From(
                    amounts.Aggregate((currentSum, item) => currentSum + item).ToString(),
                    token.GetESDT()
                );

                var transactionRequest = TokenTransferToSmartContract(
                    _networkConfig,
                    _account,
                    smartSendContractAddress,
                    gasLimit,
                    token.Identifier,
                    amount,
                    SmartSendMethod,
                    arguments.ToArray()
                );

                _account.IncrementNonce();
                transactionRequests.Add(transactionRequest);
            }

            return transactionRequests.ToArray();
        }

        public TransactionRequest[] CreateMetaESDTTransactions(
            AccountMetaESDT metaESDT,
            List<TokenAmount> inputTransactions,
            int gasPerTx = 900000,
            string? contractAddress = null)
        {
            Address smartSendContractAddress;
            if (contractAddress != null)
            {
                smartSendContractAddress = Address.FromBech32(contractAddress);
            }
            else
            {
                if (!string.IsNullOrEmpty(SmartSendContract))
                    smartSendContractAddress = Address.FromBech32(SmartSendContract);
                else
                    throw new Exception("Smart Send Contract address is not set");
            }

            var transactionsChunks = inputTransactions.Chunk(ChunkLimit);
            List<TransactionRequest> transactionRequests = new();
            foreach (var chunk in transactionsChunks)
            {
                var gasLimit = chunk.Length < 7 ? new GasLimit(6000000) : new GasLimit(chunk.Length * gasPerTx);

                List<IBinaryType> arguments = new();
                foreach (var tx in chunk)
                {
                    arguments.Add(tx.Address);
                    arguments.Add(NumericValue.BigUintValue(tx.Amount.Value));
                }

                var amounts = chunk.Select(tx => tx.Amount.Value);
                var amount = ESDTAmount.From(
                    amounts.Aggregate((currentSum, item) => currentSum + item).ToString(),
                    metaESDT.GetESDT()
                );

                var transactionRequest = NFTTransferToSmartContract(
                    _networkConfig,
                    _account,
                    smartSendContractAddress,
                    gasLimit,
                    metaESDT.Collection,
                    metaESDT.Nonce,
                    amount,
                    SmartSendMethod,
                    arguments.ToArray()
                );

                _account.IncrementNonce();
                transactionRequests.Add(transactionRequest);
            }

            return transactionRequests.ToArray();
        }

        public TransactionRequest[] CreateNFTTransactions(
            List<TokenAmount> inputTransactions,
            int gasPerTx = 900000,
            string? contractAddress = null)
        {
            Address smartSendContractAddress;
            if (contractAddress != null)
            {
                smartSendContractAddress = Address.FromBech32(contractAddress);
            }
            else
            {
                if (!string.IsNullOrEmpty(SmartSendContract))
                    smartSendContractAddress = Address.FromBech32(SmartSendContract);
                else
                    throw new Exception("Smart Send Contract address is not set");
            }

            var transactionsChunks = inputTransactions.Chunk(ChunkLimit);
            List<TransactionRequest> transactionRequests = new();
            foreach (var chunk in transactionsChunks)
            {
                var gasLimit = chunk.Length < 7 ? new GasLimit(6000000) : new GasLimit(chunk.Length * gasPerTx);

                List<Tuple<ESDTIdentifierValue, ulong, ESDTAmount>> nfts = new();
                List<IBinaryType> arguments = new();
                foreach (var tx in chunk)
                {
                    var collection = ESDTIdentifierValue.From(tx.Amount.Esdt.Collection);
                    var nonce = tx.Amount.Esdt.Identifier.GetNonce();
                    var amount = tx.Amount;
                    nfts.Add(new Tuple<ESDTIdentifierValue, ulong, ESDTAmount>(collection, nonce, amount));

                    arguments.Add(tx.Address);
                    arguments.Add(collection);
                    arguments.Add(NumericValue.U64Value(nonce));
                }

                var transactionRequest = MultiNFTTransferToSmartContract(
                    _networkConfig,
                    _account,
                    smartSendContractAddress,
                    gasLimit,
                    nfts.ToArray(),
                    SmartSendNftMethod,
                    arguments.ToArray()
                );

                _account.IncrementNonce();
                transactionRequests.Add(transactionRequest);
            }

            return transactionRequests.ToArray();
        }

        public TransactionRequest[] CreateSFTTransactions(
            AccountNFT sft,
            List<TokenAmount> inputTransactions,
            int gasPerTx = 900000,
            string? contractAddress = null)
        {
            Address smartSendContractAddress;
            if (contractAddress != null)
            {
                smartSendContractAddress = Address.FromBech32(contractAddress);
            }
            else
            {
                if (!string.IsNullOrEmpty(SmartSendContract))
                    smartSendContractAddress = Address.FromBech32(SmartSendContract);
                else
                    throw new Exception("Smart Send Contract address is not set");
            }

            var sftCollection = sft.Collection;
            var sftNonce = sft.Nonce;

            var transactionsChunks = inputTransactions.Chunk(ChunkLimit);
            List<TransactionRequest> transactionRequests = new();
            foreach (var chunk in transactionsChunks)
            {
                var gasLimit = chunk.Length < 7 ? new GasLimit(6000000) : new GasLimit(chunk.Length * gasPerTx);

                List<IBinaryType> arguments = new();
                foreach (var tx in chunk)
                {
                    arguments.Add(tx.Address);
                    arguments.Add(NumericValue.BigUintValue(tx.Amount.Value));
                }

                var amounts = chunk.Select(tx => tx.Amount.Value);
                var amount = ESDTAmount.From(
                    amounts.Aggregate((currentSum, item) => currentSum + item).ToString(),
                    sft.GetESDT()
                );

                var transactionRequest = NFTTransferToSmartContract(
                    _networkConfig,
                    _account,
                    smartSendContractAddress,
                    gasLimit,
                    sftCollection,
                    sftNonce,
                    amount,
                    SmartSendMethod,
                    arguments.ToArray()
                );

                _account.IncrementNonce();
                transactionRequests.Add(transactionRequest);
            }

            return transactionRequests.ToArray();
        }
    }
}
