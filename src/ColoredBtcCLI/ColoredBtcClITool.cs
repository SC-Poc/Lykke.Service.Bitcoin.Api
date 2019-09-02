using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using NBitcoin;
using NBitcoin.OpenAsset;
using QBitNinja.Client;

namespace ColoredBtcCLI
{
    public class ColoredBtcClITool
    {
        private readonly Network network;
        private readonly QBitNinjaClient client;
        private readonly ILogger<ColoredBtcClITool> logger;
        private readonly FeeRate feeRate;

        public ColoredBtcClITool(Settings settings, ILogger<ColoredBtcClITool> logger)
        {
            this.network = Network.GetNetwork(settings.Network);
            this.client = new QBitNinjaClient(new Uri(settings.QbitNinjaUrl), this.network)
            {
                Colored = true
            };
            this.logger = logger;
            this.feeRate = new FeeRate(settings.FeeSatoshiPerByte);
        }


        public void Execute(string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                Description = "Tool for sending retrieving colored assets",
            };

            app.Command("issue", cmd =>
            {
                cmd.Description = $"Issues colored coins for {this.network} network";
                cmd.HelpOption("-h");

                var keyOption = cmd.Option("--key <key>",
                    "Bitcoin private key in WIF-format",
                    CommandOptionType.SingleValue);

                var destinationOption = cmd.Option("--destination <destination>",
                    "Bitcoin address to send issued coins",
                    CommandOptionType.SingleValue);

                var quantityOption = cmd.Option(
                    "--quantity <quantity>",
                    "Quantity tokens to issue",
                    CommandOptionType.SingleValue);

                cmd.OnExecute(async () =>
                {
                    if (!keyOption.HasValue())
                    {
                        throw new ArgumentNullException(keyOption.Template);
                    }

                    if (!destinationOption.HasValue())
                    {
                        throw new ArgumentNullException(destinationOption.Template);
                    }

                    if (!quantityOption.HasValue())
                    {
                        throw new ArgumentNullException(quantityOption.Template);
                    }

                    var key = Key.Parse(keyOption.Value(), this.network);
                    var destionation = BitcoinAddress.Create(destinationOption.Value(), this.network);
                    var quantity = ulong.Parse(quantityOption.Value());

                    await this.IssueCoinsAsync(key, destionation, quantity);

                    return 0;
                });
            });

            app.Command("send", cmd =>
            {
                cmd.Description = $"Sends colored coins for {this.network} network";
                cmd.HelpOption("-h");

                var keyOption = cmd.Option("--key <key>",
                    "Bitcoin private key in WIF-format",
                    CommandOptionType.SingleValue);

                var destinationOption = cmd.Option("--destination <destination>",
                    "Bitcoin address to send colored coins",
                    CommandOptionType.SingleValue);

                var assetIdOption = cmd.Option("--assetId <assetId>",
                    "Bitcoin assetId",
                    CommandOptionType.SingleValue);


                var quantityOption = cmd.Option(
                    "--quantity <quantity>",
                    "Quantity tokens to issue",
                    CommandOptionType.SingleValue);

                cmd.OnExecute(async () =>
                {
                    if (!keyOption.HasValue())
                    {
                        throw new ArgumentNullException(keyOption.Template);
                    }

                    if (!destinationOption.HasValue())
                    {
                        throw new ArgumentNullException(destinationOption.Template);
                    }

                    if (!quantityOption.HasValue())
                    {
                        throw new ArgumentNullException(quantityOption.Template);
                    }

                    if (!assetIdOption.HasValue())
                    {
                        throw new ArgumentNullException(assetIdOption.Template);
                    }

                    var key = Key.Parse(keyOption.Value(), this.network);
                    var destionation = BitcoinAddress.Create(destinationOption.Value(), this.network);
                    var quantity = ulong.Parse(quantityOption.Value());
                    var assetId = new AssetId(new BitcoinAssetId(assetIdOption.Value(), this.network));

                    await this.SendCoinsAsync(key, destionation, assetId, quantity);

                    return 0;
                });
            });

            app.HelpOption("-h");
            app.Execute(args);
        }

        private async Task IssueCoinsAsync(Key sourceKey, BitcoinAddress destination, ulong quantity)
        {
            var sourceAddr = sourceKey.ScriptPubKey.GetDestinationAddress(this.network);
            var allCoins = await GetUnspentCoinsAsync(sourceAddr);

            var isssuance = new IssuanceCoin(allCoins.OfType<Coin>().First());

            var tx = network.CreateTransactionBuilder()
                .AddKeys(sourceKey)
                .AddCoins(isssuance)
                .IssueAsset(destination, new AssetMoney(isssuance.AssetId, quantity))
                .SetChange(sourceAddr)
                .SendEstimatedFees(this.feeRate)
                .BuildTransaction(true);

            await BroadcastAsync(tx);
        }

        private async Task SendCoinsAsync(Key sourceKey, BitcoinAddress destination, AssetId assetId, ulong quantity)
        {
            var sourceAddr = sourceKey.ScriptPubKey.GetDestinationAddress(this.network);
            var allCoins = await GetUnspentCoinsAsync(sourceAddr);

            var tx = network.CreateTransactionBuilder()
                .AddKeys(sourceKey)
                .AddCoins(allCoins)
                .SendAsset(destination, assetId, quantity)
                .SetChange(sourceAddr)
                .SendEstimatedFees(this.feeRate)
                .BuildTransaction(true);

            await BroadcastAsync(tx);
        }

        private async Task BroadcastAsync(Transaction tx)
        {
            this.logger.LogInformation($"Broadcasting transaction {tx.ToHex()}");

            var resp = await this.client.Broadcast(tx);
            if (!resp.Success)
            {
                throw new Exception($"Non success resp while broadcast transaction: {resp.Error.ErrorCode} : {resp.Error.Reason} : {tx}");
            }

            this.logger.LogInformation($"Transaction  {tx.GetHash()} successully broadcasted ");
        }

        private async Task<IReadOnlyCollection<ICoin>> GetUnspentCoinsAsync(BitcoinAddress address)
        {
            return (await this.client.GetBalance(address, true)).Operations.SelectMany(o => o.ReceivedCoins).ToList();
        }
    }
}
