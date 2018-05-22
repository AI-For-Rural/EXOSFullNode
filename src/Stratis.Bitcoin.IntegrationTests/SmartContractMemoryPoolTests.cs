﻿using System.Linq;
using System.Threading;
using NBitcoin;
using Stratis.Bitcoin.IntegrationTests.EnvironmentMockUpHelpers;
using Stratis.SmartContracts;
using Stratis.SmartContracts.Core;
using Xunit;

namespace Stratis.Bitcoin.IntegrationTests.SmartContracts
{
    public class SmartContractMemoryPoolTests
    {
        [Fact]
        public void SmartContracts_AddToMempool_Success()
        {
            using (NodeBuilder builder = NodeBuilder.Create())
            {
                var stratisNodeSync = builder.CreateSmartContractNode();
                builder.StartAll();

                stratisNodeSync.SetDummyMinerSecret(new BitcoinSecret(new Key(), stratisNodeSync.FullNode.Network));
                stratisNodeSync.GenerateSmartContractStratis(105); // coinbase maturity = 100
                TestHelper.WaitLoop(() => stratisNodeSync.FullNode.ConsensusLoop().Tip.HashBlock == stratisNodeSync.FullNode.Chain.Tip.HashBlock);
                TestHelper.WaitLoop(() => stratisNodeSync.FullNode.HighestPersistedBlock().HashBlock == stratisNodeSync.FullNode.Chain.Tip.HashBlock);

                var block = stratisNodeSync.FullNode.BlockStoreManager().BlockRepository.GetAsync(stratisNodeSync.FullNode.Chain.GetBlock(4).HashBlock).Result;
                var prevTrx = block.Transactions.First();
                var dest = new BitcoinSecret(new Key(), stratisNodeSync.FullNode.Network);

                Transaction tx = new Transaction();
                tx.AddInput(new TxIn(new OutPoint(prevTrx.GetHash(), 0), PayToPubkeyHashTemplate.Instance.GenerateScriptPubKey(stratisNodeSync.MinerSecret.PubKey)));
                tx.AddOutput(new TxOut("25", dest.PubKey.Hash));
                tx.AddOutput(new TxOut("24", new Key().PubKey.Hash)); // 1 btc fee
                tx.Sign(stratisNodeSync.FullNode.Network, stratisNodeSync.MinerSecret, false);

                stratisNodeSync.Broadcast(tx);

                TestHelper.WaitLoop(() => stratisNodeSync.CreateRPCClient().GetRawMempool().Length == 1);
            }
        }

        [Fact]
        public void SmartContracts_AddToMempool_OnlyValid()
        {
            using (NodeBuilder builder = NodeBuilder.Create())
            {
                var stratisNodeSync = builder.CreateSmartContractNode();
                builder.StartAll();

                stratisNodeSync.SetDummyMinerSecret(new BitcoinSecret(new Key(), stratisNodeSync.FullNode.Network));
                stratisNodeSync.GenerateSmartContractStratis(105); // coinbase maturity = 100
                TestHelper.WaitLoop(() => stratisNodeSync.FullNode.ConsensusLoop().Tip.HashBlock == stratisNodeSync.FullNode.Chain.Tip.HashBlock);
                TestHelper.WaitLoop(() => stratisNodeSync.FullNode.HighestPersistedBlock().HashBlock == stratisNodeSync.FullNode.Chain.Tip.HashBlock);

                var block = stratisNodeSync.FullNode.BlockStoreManager().BlockRepository.GetAsync(stratisNodeSync.FullNode.Chain.GetBlock(4).HashBlock).Result;
                var prevTrx = block.Transactions.First();
                var dest = new BitcoinSecret(new Key(), stratisNodeSync.FullNode.Network);

                // OP_CREATE with value > 0
                Transaction tx = new Transaction();
                tx.AddInput(new TxIn(new OutPoint(prevTrx.GetHash(), 0), PayToPubkeyHashTemplate.Instance.GenerateScriptPubKey(stratisNodeSync.MinerSecret.PubKey)));
                SmartContractCarrier smartContractCarrier = SmartContractCarrier.CreateContract(1, new byte[0], 1, new Gas(100_000));
                tx.AddOutput(new TxOut(1, new Script(smartContractCarrier.Serialize())));
                tx.Sign(stratisNodeSync.FullNode.Network, stratisNodeSync.MinerSecret, false);
                stratisNodeSync.Broadcast(tx);

                // Gas higher than allowed limit
                tx = new Transaction();
                tx.AddInput(new TxIn(new OutPoint(prevTrx.GetHash(), 0), PayToPubkeyHashTemplate.Instance.GenerateScriptPubKey(stratisNodeSync.MinerSecret.PubKey)));
                smartContractCarrier = SmartContractCarrier.CallContract(1, new uint160(0), "Test", 1, new Gas(10_000_000));
                tx.AddOutput(new TxOut(1, new Script(smartContractCarrier.Serialize())));
                tx.Sign(stratisNodeSync.FullNode.Network, stratisNodeSync.MinerSecret, false);
                stratisNodeSync.Broadcast(tx);

                // OP_SPEND in user's tx - we can't sign this because the TransactionBuilder recognises the ScriptPubKey is invalid.
                tx = new Transaction();
                tx.AddInput(new TxIn(new OutPoint(prevTrx.GetHash(), 0), new Script(OpcodeType.OP_SPEND)));
                smartContractCarrier = SmartContractCarrier.CallContract(1, new uint160(0), "Test", 1, new Gas(100_000));
                tx.AddOutput(new TxOut(1, new Script(smartContractCarrier.Serialize())));
                stratisNodeSync.Broadcast(tx);

                // 2 smart contract outputs
                tx = new Transaction();
                tx.AddInput(new TxIn(new OutPoint(prevTrx.GetHash(), 0), PayToPubkeyHashTemplate.Instance.GenerateScriptPubKey(stratisNodeSync.MinerSecret.PubKey)));
                smartContractCarrier = SmartContractCarrier.CallContract(1, new uint160(0), "Test", 1, new Gas(100_000));
                tx.AddOutput(new TxOut(1, new Script(smartContractCarrier.Serialize())));
                tx.AddOutput(new TxOut(1, new Script(smartContractCarrier.Serialize())));
                tx.Sign(stratisNodeSync.FullNode.Network, stratisNodeSync.MinerSecret, false);
                stratisNodeSync.Broadcast(tx);

                // After 5 seconds (plenty of time but ideally we would have a more accurate measure) no txs in mempool. All failed validation.
                Thread.Sleep(5000);
                Assert.Empty(stratisNodeSync.CreateRPCClient().GetRawMempool());

                // Valid tx still works
                tx = new Transaction();
                tx.AddInput(new TxIn(new OutPoint(prevTrx.GetHash(), 0), PayToPubkeyHashTemplate.Instance.GenerateScriptPubKey(stratisNodeSync.MinerSecret.PubKey)));
                tx.AddOutput(new TxOut("25", dest.PubKey.Hash));
                tx.AddOutput(new TxOut("24", new Key().PubKey.Hash)); // 1 btc fee
                tx.Sign(stratisNodeSync.FullNode.Network, stratisNodeSync.MinerSecret, false);
                stratisNodeSync.Broadcast(tx);
                TestHelper.WaitLoop(() => stratisNodeSync.CreateRPCClient().GetRawMempool().Length == 1);
            }
        }
    }
}