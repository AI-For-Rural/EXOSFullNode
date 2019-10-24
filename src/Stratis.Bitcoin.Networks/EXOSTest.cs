using System;
using System.Collections.Generic;
using System.Net;
using NBitcoin;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.Protocol;
using Stratis.Bitcoin.Networks.Deployments;
using Stratis.Bitcoin.Networks.Policies;

namespace Stratis.Bitcoin.Networks
{
    public class EXOSTest : EXOSMain
    {
        public EXOSTest()
        {
            // The message start string is designed to be unlikely to occur in normal data.
            // The characters are rarely used upper ASCII, not valid as UTF-8, and produce
            // a large 4-byte int at any alignment.
            var messageStart = new byte[4];
            messageStart[0] = 0x25;
            messageStart[1] = 0x16;
            messageStart[2] = 0x74;
            messageStart[3] = 0x62;
            uint magic = BitConverter.ToUInt32(messageStart, 0);

            this.Name = "EXOSTest";
            this.NetworkType = NetworkType.Testnet;
            this.Magic = magic;
            this.DefaultPort = 24562;
            this.DefaultMaxOutboundConnections = 16;
            this.DefaultMaxInboundConnections = 109;
            this.DefaultRPCPort = 24561;
            this.DefaultAPIPort = 39121;
            this.DefaultSignalRPort = 38621;
            this.CoinTicker = "TEXOS";
            this.DefaultBanTimeSeconds = 16000; // 500 (MaxReorg) * 64 (TargetSpacing) / 2 = 4 hours, 26 minutes and 40 seconds

            var powLimit = new Target(new uint256("0000ffff00000000000000000000000000000000000000000000000000000000"));

            var consensusFactory = new PosConsensusFactory();

            // Create the genesis block.
            this.GenesisTime = 1571889600;
            this.GenesisNonce = 94433;
            this.GenesisBits = powLimit;
            this.GenesisVersion = 1;
            this.GenesisReward = Money.Zero;

            Block genesisBlock = CreateStratisGenesisBlock(consensusFactory, this.GenesisTime, this.GenesisNonce, this.GenesisBits, this.GenesisVersion, this.GenesisReward);

         //  genesisBlock.Header.Time = 1571889600;
         //  genesisBlock.Header.Nonce = 0;
         //  genesisBlock.Header.Bits = powLimit;

            this.Genesis = genesisBlock;

            // Taken from StratisX.
            var consensusOptions = new PosConsensusOptions(
                maxBlockBaseSize: 1_000_000,
                maxStandardVersion: 2,
                maxStandardTxWeight: 100_000,
                maxBlockSigopsCost: 20_000,
                maxStandardTxSigopsCost: 20_000 / 5
            );

            var buriedDeployments = new BuriedDeploymentsArray
            {
                [BuriedDeployments.BIP34] = 0,
                [BuriedDeployments.BIP65] = 0,
                [BuriedDeployments.BIP66] = 0
            };

            var bip9Deployments = new StratisBIP9Deployments()
            {
                [StratisBIP9Deployments.ColdStaking] = new BIP9DeploymentsParameters(2,
                    new DateTime(2018, 11, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2019, 6, 1, 0, 0, 0, DateTimeKind.Utc))
            };

            this.Consensus = new NBitcoin.Consensus(
                consensusFactory: consensusFactory,
                consensusOptions: consensusOptions,
                coinType: 105,
                hashGenesisBlock: genesisBlock.GetHash(),
                subsidyHalvingInterval: 210000,
                majorityEnforceBlockUpgrade: 750,
                majorityRejectBlockOutdated: 950,
                majorityWindow: 1000,
                buriedDeployments: buriedDeployments,
                bip9Deployments: bip9Deployments,
                bip34Hash: new uint256("0x0000a24a922acc0b13a208297a18b3fcdab6492d1da54317e201270d250b2bf1"),
                ruleChangeActivationThreshold: 1916, // 95% of 2016
                minerConfirmationWindow: 2016, // nPowTargetTimespan / nPowTargetSpacing
                maxReorgLength: 500,
                defaultAssumeValid: new uint256("0x92c528a1e3cadf32f5e3e00e351f49ad70368deb92825b96619438c4aead7b5a"), // 800
                maxMoney: long.MaxValue,
                coinbaseMaturity: 10,
                premineHeight: 2,
                premineReward: Money.Coins(300000000),
                proofOfWorkReward: Money.Coins(12),
                powTargetTimespan: TimeSpan.FromSeconds(14 * 24 * 60 * 60), // two weeks
                powTargetSpacing: TimeSpan.FromSeconds(10 * 60),
                powAllowMinDifficultyBlocks: false,
                posNoRetargeting: false,
                powNoRetargeting: false,
                powLimit: powLimit,
                minimumChainWork: null,
                isProofOfStake: true,
                lastPowBlock: 45000,
                proofOfStakeLimit: new BigInteger(uint256.Parse("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
                proofOfStakeLimitV2: new BigInteger(uint256.Parse("000000000000ffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
                proofOfStakeReward: Money.COIN
            );

            this.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { (75) };
            this.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { (206) };
            this.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (75 + 128) };
            this.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x35), (0x87), (0xCF) };
            this.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x35), (0x83), (0x94) };

            this.Checkpoints = new Dictionary<int, CheckpointInfo>
            {
                { 0, new CheckpointInfo(new uint256("0x0000a24a922acc0b13a208297a18b3fcdab6492d1da54317e201270d250b2bf1"), new uint256("0x0000000000000000000000000000000000000000000000000000000000000000")) },
                { 2, new CheckpointInfo(new uint256("0xa3c8ffdd06ea336e89462b40a480fbb415dbaf1b8a30dce27c701f98eb6e9fb8")) },
                { 10, new CheckpointInfo(new uint256("0x00581645bd761f7ead5fd6bed0c88ac61be8a1af55a124155594792c48254dc9")) },
                { 50, new CheckpointInfo(new uint256("0x505c959472bd21398c62369a2fe4ba2eeaa6cde53a0918acfe1f9b6877c4524e")) },
                { 500, new CheckpointInfo(new uint256("0x65808fad20db4a463637da3e735ce447159453aafb07c36716f8ee9e1a39ba9c")) }

            };

            this.DNSSeeds = new List<DNSSeedData>
            {
               
            };

            this.SeedNodes = new List<NetworkAddress>
            {

            };
            var hostnames = new[] {""};
            foreach (string host in hostnames)
            {
                var addressList = Dns.GetHostAddresses(host).GetValue(0).ToString();
                NetworkAddress addr = new NetworkAddress
                {
                    Endpoint = Utils.ParseIpEndpoint(addressList, this.DefaultPort)
                };
                this.SeedNodes.Add(addr);
            }

            this.StandardScriptsRegistry = new StratisStandardScriptsRegistry();

            // 64 below should be changed to TargetSpacingSeconds when we move that field.
            Assert(this.DefaultBanTimeSeconds <= this.Consensus.MaxReorgLength * 64 / 2);
            Assert(this.Consensus.HashGenesisBlock == uint256.Parse("0x0000a24a922acc0b13a208297a18b3fcdab6492d1da54317e201270d250b2bf1"));

            this.RegisterRules(this.Consensus);
        }
    }
}
