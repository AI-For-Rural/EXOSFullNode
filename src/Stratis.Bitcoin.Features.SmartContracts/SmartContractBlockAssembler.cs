﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Stratis.Bitcoin.Features.Consensus;
using Stratis.Bitcoin.Features.Consensus.CoinViews;
using Stratis.Bitcoin.Features.Consensus.Interfaces;
using Stratis.Bitcoin.Features.MemoryPool;
using Stratis.Bitcoin.Features.MemoryPool.Interfaces;
using Stratis.Bitcoin.Features.Miner;
using Stratis.Bitcoin.Utilities;
using Stratis.SmartContracts.Core;
using Stratis.SmartContracts.Core.Backend;
using Stratis.SmartContracts.Core.State;
using Stratis.SmartContracts.Core.Util;

namespace Stratis.Bitcoin.Features.SmartContracts
{
    public class SmartContractBlockAssembler : PowBlockAssembler
    {
        private List<TxOut> refundOutputs = new List<TxOut>();

        private ContractStateRepositoryRoot stateRoot;
        private ContractStateRepositoryRoot stateSnapshot;

        private SmartContractExecutorFactory executorFactory;

        private uint160 coinbaseAddress;
        private readonly CoinView coinView;

        private readonly ILogger<SmartContractBlockAssembler> logger;

        public SmartContractBlockAssembler(
            ChainedBlock chainTip,
            IConsensusLoop consensusLoop,
            IDateTimeProvider dateTimeProvider,
            ILoggerFactory loggerFactory,
            ITxMempool mempool,
            MempoolSchedulerLock mempoolLock,
            Network network,
            ContractStateRepositoryRoot stateRoot,
            SmartContractExecutorFactory executorFactory,
            CoinView coinView,
            AssemblerOptions options = null)
            : base(chainTip, consensusLoop, dateTimeProvider, loggerFactory, mempool, mempoolLock, network, options)
        {
            this.coinView = coinView;
            this.stateRoot = stateRoot;
            this.executorFactory = executorFactory;
            this.logger = loggerFactory.CreateLogger<SmartContractBlockAssembler>();
        }

        public override BlockTemplate CreateNewBlock(Script scriptPubKeyIn, bool mineWitnessTx = true)
        {
            GetSenderUtil.GetSenderResult getSenderResult = GetSenderUtil.GetAddressFromScript(scriptPubKeyIn);
            
            if (!getSenderResult.Success)
            {
                throw new ConsensusErrorException(new ConsensusError("sc-block-assembler-createnewblock", getSenderResult.Error));
            }

            this.coinbaseAddress = getSenderResult.Sender;

            this.stateSnapshot = this.stateRoot.GetSnapshotTo(this.consensusLoop.Tip.Header.HashStateRoot.ToBytes());

            base.CreateNewBlock(scriptPubKeyIn, mineWitnessTx);

            this.coinbase.Outputs.AddRange(this.refundOutputs);

            return this.pblocktemplate;
        }

        /// <summary>
        /// The block header for smart contract blocks is identical to the standard block,
        /// except it also has a second 32-byte root, the state root. This byte array
        /// represents the current state of contract code, storage and balances, and can
        /// be used in conjunction with getSnapshotTo at any time to recreate this state.
        /// </summary>
        protected override void UpdateHeaders()
        {
            base.UpdateHeaders();

            this.pblock.Header.HashStateRoot = new uint256(this.stateSnapshot.Root);
        }

        /// <summary>
        /// Overrides the <see cref="AddToBlock(TxMempoolEntry)"/> behaviour of <see cref="PowBlockAssembler"/>.
        /// <para>
        /// Determine whether or not the mempool entry contains smart contract execution 
        /// code. If not, then add to the block as per normal. Else extract and deserialize 
        /// the smart contract code from the TxOut's ScriptPubKey.
        /// </para>
        /// </summary>
        protected override void AddToBlock(TxMempoolEntry mempoolEntry)
        {
            TxOut smartContractTxOut = mempoolEntry.TryGetSmartContractTxOut();
            if (smartContractTxOut == null)
                base.AddToBlock(mempoolEntry);
            else
                this.AddContractToBlock(mempoolEntry, smartContractTxOut);
        }

        /// <summary>
        /// Execute the contract and add all relevant fees and refunds to the block.
        /// </summary>
        /// <remarks>TODO: At some point we need to change height to a ulong.</remarks> 
        private void AddContractToBlock(TxMempoolEntry mempoolEntry, TxOut smartContractTxOut)
        {
            var carrier = SmartContractCarrier.Deserialize(mempoolEntry.Transaction, smartContractTxOut);
            
            GetSenderUtil.GetSenderResult getSenderResult = GetSenderUtil.GetSender(mempoolEntry.Transaction, this.coinView, this.inBlock.Select(x => x.Transaction).ToList());

            if (!getSenderResult.Success)
            {
                throw new ConsensusErrorException(new ConsensusError("sc-block-assembler-addcontracttoblock", getSenderResult.Error));
            }

            carrier.Sender = getSenderResult.Sender;

            SmartContractExecutor executor = this.executorFactory.CreateExecutor(carrier, mempoolEntry.Fee, this.stateSnapshot);
            ISmartContractExecutionResult result = executor.Execute((ulong)this.height, this.coinbaseAddress);

            // Add fee from the execution result to the block.
            this.pblocktemplate.VTxFees.Add(result.Fee);
            this.fees += result.Fee;

            // If there are refunds, add them to the block
            if (result.Refunds.Any())
                this.refundOutputs.AddRange(result.Refunds);

            // Add the mempool entry transaction to the block 
            // and adjust BlockSize, BlockWeight and SigOpsCost.
            this.pblock.AddTransaction(mempoolEntry.Transaction);
            this.pblocktemplate.TxSigOpsCost.Add(mempoolEntry.SigOpCost);

            if (this.needSizeAccounting)
                this.blockSize += mempoolEntry.Transaction.GetSerializedSize();

            this.blockWeight += mempoolEntry.TxWeight;
            this.blockTx++;
            this.blockSigOpsCost += mempoolEntry.SigOpCost;
            this.inBlock.Add(mempoolEntry);

            // Add internal transactions made during execution
            if(result.InternalTransaction != null)
            {
                this.pblock.AddTransaction(result.InternalTransaction);
                if (this.needSizeAccounting)
                    this.blockSize += result.InternalTransaction.GetSerializedSize();
                this.blockTx++;
            }
        }
    }
}