using System;
using System.Collections.Generic;
using System.Text;
using NBitcoin;
using NBitcoin.DataEncoders;
using Stratis.Bitcoin.Networks;
using Microsoft.Extensions.Logging;



namespace GenesisMiner
{
    public class Miner
    {


        /// <summary>
        /// Mine genesis block for all networks
        /// </summary>
        /// <param name="consensusFactory">Build blocks and transactions</param>
        /// <param name="chain">Networks listed in <see cref="Stratis.Bitcoin.Networks.Networks"/> e.g Bitcoin</param>
        public void MineAllNetworks(PosConsensusFactory consensusFactory, NetworksSelector chain, string coinbaseText)
        {
            
            List<Network> networks = new List<Network>();
            networks.Add(chain.Mainnet());
            networks.Add(chain.Testnet());
            networks.Add(chain.Regtest());
            foreach (Network keyNetwork in networks)
            {
                Console.WriteLine("Looking for best hash. It may take a while... \n");
                Block genesisBlock = this.GeneterateBlock(consensusFactory, coinbaseText, new Target(keyNetwork.Consensus.PowLimit.ToUInt256()).ToUInt256(), keyNetwork.GenesisTime);
                Console.WriteLine(this.NetworkOutput(genesisBlock, keyNetwork.Name, coinbaseText));
                
            }
            Setup.OutputMenu();

        }


        private Block GeneterateBlock(PosConsensusFactory consensusFactory, string coinbaseText, uint256 target, uint nTime)
        {
            return MineGenesisBlock(consensusFactory, coinbaseText, new Target(target), nTime, Money.Zero);
        }

        private string NetworkOutput(Block genesisBlock, string networkName, string coinbaseText)
        {
            var header = (PosBlockHeader)genesisBlock.Header;

            var output = new StringBuilder();
            output.AppendLine("Network Name: "+networkName);
            output.AppendLine("nBits: " + header.Bits);
            output.AppendLine("nNonce: " + header.Nonce);
            output.AppendLine("nTime: " + header.Time);
            output.AppendLine("nVersion: " + header.Version);
            output.AppendLine("Hash: " + genesisBlock.GetHash());
            output.AppendLine("MerkleRoot: " + header.HashMerkleRoot);
            output.AppendLine("Coinbase text: " + coinbaseText);
            output.AppendLine("Target: " + header.Bits.ToUInt256());
            output.AppendLine("Use this data in your genesis parameters. ");
            return output.ToString();
        }


        public static Block MineGenesisBlock(PosConsensusFactory consensusFactory, string coinbaseText, Target target, uint nTime, Money genesisReward, int version = 1)
        {
            if (consensusFactory == null)
                throw new ArgumentException($"Parameter '{nameof(consensusFactory)}' cannot be null. Use 'new ConsensusFactory()' for Bitcoin-like proof-of-work blockchains and 'new PosConsensusFactory()' for proof-of-stake blockchains.");

            if (string.IsNullOrEmpty(coinbaseText))
                throw new ArgumentException($"Parameter '{nameof(coinbaseText)}' cannot be null. Use a news headline or any other appropriate string.");

            if (target == null)
                throw new ArgumentException($"Parameter '{nameof(target)}' cannot be null. Example use: new Target(new uint256(\"0000ffff00000000000000000000000000000000000000000000000000000000\"))");

            if (genesisReward == null)
                throw new ArgumentException($"Parameter '{nameof(genesisReward)}' cannot be null. Example use: 'Money.Coins(50m)'.");

            DateTimeOffset time = Utils.UnixTimeToDateTime(nTime);

            Transaction txNew = consensusFactory.CreateTransaction();
            txNew.Version = (uint)version;

            txNew.Time = nTime;
            txNew.AddInput(new TxIn()
            {
                ScriptSig = new Script(
                    Op.GetPushOp(0),
                    new Op()
                    {
                        Code = (OpcodeType)0x1,
                        PushData = new[] { (byte)42 }
                    },
                    Op.GetPushOp(Encoders.ASCII.DecodeData(coinbaseText)))
            });

            txNew.AddOutput(new TxOut()
            {
                Value = genesisReward,
            });

            Block genesis = consensusFactory.CreateBlock();
            genesis.Header.BlockTime = time;
            genesis.Header.Bits = target;
            genesis.Header.Nonce = 0;
            genesis.Header.Version = version;
            genesis.Transactions.Add(txNew);
            genesis.Header.HashPrevBlock = uint256.Zero;
            genesis.UpdateMerkleRoot();


            // Iterate over the nonce until the proof-of-work is valid.
            // This will mean the block header hash is under the target.
            while (!genesis.CheckProofOfWork())
            {
                genesis.Header.Nonce++;
                if (genesis.Header.Nonce == 0)
                    genesis.Header.Time++;
               
            }

            return genesis;
        }
    }
}
