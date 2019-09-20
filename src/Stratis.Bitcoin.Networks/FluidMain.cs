using System;
using System.Collections.Generic;
using System.Net;
using NBitcoin;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.DataEncoders;
using NBitcoin.Protocol;
using NBitcoin.Rules;
using Stratis.Bitcoin.Features.Consensus.Rules.CommonRules;
using Stratis.Bitcoin.Features.Consensus.Rules.ProvenHeaderRules;
using Stratis.Bitcoin.Networks.Deployments;
using Stratis.Bitcoin.Networks.Policies;

namespace Stratis.Bitcoin.Networks
{
    public class FluidMain : Network
    {
        /// <summary> Stratis maximal value for the calculated time offset. If the value is over this limit, the time syncing feature will be switched off. </summary>
        public const int StratisMaxTimeOffsetSeconds = 25 * 60;

        /// <summary> Fluid default value for the maximum tip age in seconds to consider the node in initial block download (2 hours). </summary>
        public const int StratisDefaultMaxTipAgeInSeconds = 2 * 60 * 60;

        /// <summary> The name of the root folder containing the different Fluid blockchains (FluidMain, FluidTest, FluidRegTest). </summary>
        public const string StratisRootFolderName = "fluid";

        /// <summary> The default name used for the Fluid configuration file. </summary>
        public const string StratisDefaultConfigFilename = "fluid.conf";

        public FluidMain()
        {
            // The message start string is designed to be unlikely to occur in normal data.
            // The characters are rarely used upper ASCII, not valid as UTF-8, and produce
            // a large 4-byte int at any alignment.
            var messageStart = new byte[4];
            messageStart[0] = 0x69;
            messageStart[1] = 0x75;
            messageStart[2] = 0x38;
            messageStart[3] = 0x06;
            uint magic = BitConverter.ToUInt32(messageStart, 0); 

            this.Name = "FluidMain";
            this.NetworkType = NetworkType.Mainnet;
            this.Magic = magic;
            this.DefaultPort = 39391;
            this.DefaultMaxOutboundConnections = 16;
            this.DefaultMaxInboundConnections = 109;
            this.DefaultRPCPort = 39390;
            this.DefaultAPIPort = 37222; //Fluid
            this.DefaultSignalRPort = 38825;
            this.MaxTipAge = 2 * 60 * 60;
            this.MinTxFee = 10000;
            this.FallbackFee = 10000;
            this.MinRelayTxFee = 10000;
            this.RootFolderName = StratisRootFolderName;
            this.DefaultConfigFilename = StratisDefaultConfigFilename;
            this.MaxTimeOffsetSeconds = 25 * 60;
            this.CoinTicker = "FLUID";
            this.DefaultBanTimeSeconds = 16000; // 500 (MaxReorg) * 64 (TargetSpacing) / 2 = 4 hours, 26 minutes and 40 seconds

            var consensusFactory = new PosConsensusFactory();

            // Create the genesis block.
            this.GenesisTime = 1519534128;
            this.GenesisNonce = 55270;
            this.GenesisBits = 0x1e0fffff;
            this.GenesisVersion = 1;
            this.GenesisReward = Money.Zero;

            Block genesisBlock = CreateStratisGenesisBlock(consensusFactory, this.GenesisTime, this.GenesisNonce, this.GenesisBits, this.GenesisVersion, this.GenesisReward);

            this.Genesis = genesisBlock;

            // Taken from FluidQt.
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
                    new DateTime(2018, 12, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2019, 12, 1, 0, 0, 0, DateTimeKind.Utc))
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
                bip34Hash: new uint256("0x000000000000024b89b42a942fe0d9fea3bb44ab7bd1b19115dd6a759c0808b8"),
                ruleChangeActivationThreshold: 1916, // 95% of 2016
                minerConfirmationWindow: 2016, // nPowTargetTimespan / nPowTargetSpacing
                maxReorgLength: 500,
                defaultAssumeValid: new uint256("0xc31bf78b55f17c1755cb474bd820eb1db8aa564a0d794ac47b415a8f25237a26"), // 
                maxMoney: long.MaxValue,
                coinbaseMaturity: 50,
                premineHeight: 2,
                premineReward: Money.Coins(500000000),
                proofOfWorkReward: Money.Coins(12),
                powTargetTimespan: TimeSpan.FromSeconds(14 * 24 * 60 * 60), // two weeks
                powTargetSpacing: TimeSpan.FromSeconds(10 * 60),
                powAllowMinDifficultyBlocks: false,
                posNoRetargeting: false,
                powNoRetargeting: false,
                powLimit: new Target(new uint256("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")),
                minimumChainWork: null,
                isProofOfStake: true,
                lastPowBlock: 10000,
                proofOfStakeLimit: new BigInteger(uint256.Parse("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
                proofOfStakeLimitV2: new BigInteger(uint256.Parse("000000000000ffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
                proofOfStakeReward: Money.COIN
            );

            this.Base58Prefixes = new byte[12][];
            this.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { (35) };
            this.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { (95) };
            this.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (35 + 128) };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_NO_EC] = new byte[] { 0x01, 0x42 };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_EC] = new byte[] { 0x01, 0x43 };
            this.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x88), (0xB2), (0x1E) };
            this.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x88), (0xAD), (0xE4) };
            this.Base58Prefixes[(int)Base58Type.PASSPHRASE_CODE] = new byte[] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2 };
            this.Base58Prefixes[(int)Base58Type.CONFIRMATION_CODE] = new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9A };
            this.Base58Prefixes[(int)Base58Type.STEALTH_ADDRESS] = new byte[] { 0x2a };
            this.Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 23 };
            this.Base58Prefixes[(int)Base58Type.COLORED_ADDRESS] = new byte[] { 0x13 };

            this.Checkpoints = new Dictionary<int, CheckpointInfo>
            {
                { 0, new CheckpointInfo(new uint256("0x000007c70a931f5cafa42d48eb0f7a627007cada84d84442e7369cc962a0d38f"), new uint256("0x0000000000000000000000000000000000000000000000000000000000000000")) },
                { 2, new CheckpointInfo(new uint256("0x3fa697d0ba0e948bcd268f2fc316d2c7b41904eeb1bced590053ea0f406cf014"), new uint256("0x0b44e08d070104e6c7a527772aa63acf2b814d61f2db91e184a7d7ec97329388")) },
                { 50, new CheckpointInfo(new uint256("0x5ba5192b53f4c23238e18835195abddbd1e1b6c7ecb320371a83ee57ba43d8cc"), new uint256("0x54b87a44bebb9925fdc52f0df049279b170c41692713473a6592f1efc89515fb")) },
                { 100, new CheckpointInfo(new uint256("0x4de52b8d1803c0672cf012e9087e967001b0d1912299abed8dc5c90ba2ff5dba"), new uint256("0x52298a813f241c15bacdbd131f9c18b032775dfa3a8fc1d82f4cc4c894bc7bb8")) },
                { 500, new CheckpointInfo(new uint256("0xc8f2c0bfcea07a71a9d3b5b968995b03bd507f21f2a1697790f0ab421387a500"), new uint256("0x0c1d113888b4d93d76510e911d0db70cfe9a4bea8c4a76e8124c90392688d2db")) },
                { 1000, new CheckpointInfo(new uint256("0x93a5c8c3f0f1cfb75122cf02b4413f642b5870ede34689c9e3adf2aa8cb05918"), new uint256("0x5e255d7a0946f20c3a15e59ab1bbeee6b06e50f495723adf9afb191512aa6ad9")) },
                { 4000, new CheckpointInfo(new uint256("0x476fac3dc504ad4c154adce532bdfce9b136dc37c29f04141cc16d3290579223"), new uint256("0x23d8c533391908f0bc129576e45dad69d4567f881b7c16e10dc930fd2e7bd73e")) },
                { 6000, new CheckpointInfo(new uint256("0x3750a9ad41a606254923b2f7ad0f22e82864623e65160a7160f270a4c991201c"), new uint256("0x922dad07d28798fd0c3f7af3a1d82aaf4b5fb55a8061bb3e78ded99bfd27fe6b")) },
                { 6002, new CheckpointInfo(new uint256("0x9f4aae7b2c472741dd72a964c6ff680aaafcf2198e651eae665a756f280532b7"), new uint256("0x1af1a0edcf90f8c60d558ebe7735d3fdc1f2641d00052bf5cc60971c265e239e")) },
                { 10000, new CheckpointInfo(new uint256("0x7bf91167a8fbcbd25f28025769f106ad6bc49d0407c971d0841e58528b65aa92"), new uint256("0x358b62222fb9c847f1c98db73c7bed2e5dc948249e29ca8e5af9b9c07fbd30bc")) },
                { 15000, new CheckpointInfo(new uint256("0x43288ba814b30026d2acdc67064c4ae5f0e54a3463961aae84dd25025bbbb780"), new uint256("0x6f0b7dd91ff9cad3cafbcfb6544e7ab6614816c1dad34063fa00abc849767f7d")) },
                { 25000, new CheckpointInfo(new uint256("0xb1834c79a933c62c4e0720bc8bdbbac69b7e87abc66c886a6ef99761e1223fd0"), new uint256("0xb29f03876532db48c54671314a4b8fcdd61601bad954f7881e478f6afe78789f")) },
                { 50000, new CheckpointInfo(new uint256("0x6984a19d0439eca15727e9af64389d99ea51275123c5653563d7247fa8ad48db"), new uint256("0xc11209e4839dea73ba741d138ed41f10dc24aaf4444a5564c24d6c7603809478")) },
                { 75000, new CheckpointInfo(new uint256("0xc4ca3757d914c4a321c80b098bc59d64204872f90f78c78fa44adc72ee38f837"), new uint256("0x7972152c084c941e3ebe9bd8e14cd16cf990031b567140a20d04901e42524c55")) },
                { 100000, new CheckpointInfo(new uint256("0x237a4e2c62efa76be9a4983a90841b1b474e003e0911fbdd563e228510f02149"), new uint256("0x0f5e0de1003137b44865d438539787131a9c7df22776508fcea71d1ec1cb7626")) },
                { 125000, new CheckpointInfo(new uint256("0x0669972d495336c85f7bf0ba74f329c8b7c13bad315245b4d9a1549391039628"), new uint256("0x066d6660360941c5b91e0284bee96a43b655095edd52e228974bb39d254d20e2")) },
                { 150000, new CheckpointInfo(new uint256("0xc1408a25304f05a038a361ae345640cca0c581bebb422941dfb3f94c47442d28"), new uint256("0x3be68cfba7dc70676774050b9314bb4ec8f609b430fb5bce3dae3c929b5f17ea")) },
                { 200000, new CheckpointInfo(new uint256("0x99884e0373156f847f089b5b23ff8d0ff912a6a9fe65357db7a14de57113d663"), new uint256("0x9abb8a94ad5f8fe2464309d4a384bc4750506fc746b264efacdd5a1a4b40ba4a")) },
                { 250000, new CheckpointInfo(new uint256("0x1827656f4838dedc29d0e5c95334e881981d9155770f2a52c3913ee71d7c410b"), new uint256("0xae45101b915ae34db06dd6aa28e931682a0cc1f4a971b1766719c7890c6f8ab0")) },
                { 300000, new CheckpointInfo(new uint256("0xc31bf78b55f17c1755cb474bd820eb1db8aa564a0d794ac47b415a8f25237a26"), new uint256("0x18033e85441654c1f00729c0986bcd647e28c4728b560c357d06578d219e2e83")) }

            };

            this.Bech32Encoders = new Bech32Encoder[2];
            // Bech32 is currently unsupported on Stratis - once supported uncomment lines below
            //var encoder = new Bech32Encoder("bc");
            //this.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            //this.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;
            this.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = null;
            this.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = null;

            this.DNSSeeds = new List<DNSSeedData>
            {
                new DNSSeedData("seed1.fluidchains.net", "seed1.fluidchains.net"),
                new DNSSeedData("seed2.fluidchains.cloud", "seed2.fluidchains.cloud"),
                new DNSSeedData("seed3.fluidchains.net", "seed3.fluidchains.net"),
                new DNSSeedData("seed4.fluidchains.cloud", "seed4.fluidchains.cloud")
            };

            this.SeedNodes = new List<NetworkAddress>
            {
                
            };

            var hostnames = new[] { "vps201.rutan.cloud", "vps202.rutan.network", "vps101.rutan.cloud", "vps102.rutan.network" };
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
            Assert(this.Consensus.HashGenesisBlock == uint256.Parse("0x000007c70a931f5cafa42d48eb0f7a627007cada84d84442e7369cc962a0d38f"));
            Assert(this.Genesis.Header.HashMerkleRoot == uint256.Parse("0x50b30f93db7ee932ee4a01a9673cc0cb114cd8239c5806e8ea47b3c497057c9f"));

            this.RegisterRules(this.Consensus);
        }

        protected void RegisterRules(IConsensus consensus)
        {
            consensus.ConsensusRules
                .Register<HeaderTimeChecksRule>()
                .Register<HeaderTimeChecksPosRule>()
                .Register<StratisBugFixPosFutureDriftRule>()
                .Register<CheckDifficultyPosRule>()
                .Register<StratisHeaderVersionRule>()
                .Register<ProvenHeaderSizeRule>()
                .Register<ProvenHeaderCoinstakeRule>();

            consensus.ConsensusRules
                .Register<BlockMerkleRootRule>()
                .Register<PosBlockSignatureRepresentationRule>()
                .Register<PosBlockSignatureRule>();

            consensus.ConsensusRules
                .Register<SetActivationDeploymentsPartialValidationRule>()
                .Register<PosTimeMaskRule>()

                // rules that are inside the method ContextualCheckBlock
                .Register<TransactionLocktimeActivationRule>()
                .Register<CoinbaseHeightActivationRule>()
                .Register<WitnessCommitmentsRule>()
                .Register<BlockSizeRule>()

                // rules that are inside the method CheckBlock
                .Register<EnsureCoinbaseRule>()
                .Register<CheckPowTransactionRule>()
                .Register<CheckPosTransactionRule>()
                .Register<CheckSigOpsRule>()
                .Register<PosCoinstakeRule>();

            consensus.ConsensusRules
                .Register<SetActivationDeploymentsFullValidationRule>()

                .Register<CheckDifficultyHybridRule>()

                // rules that require the store to be loaded (coinview)
                .Register<LoadCoinviewRule>()
                .Register<TransactionDuplicationActivationRule>()
                .Register<PosCoinviewRule>() // implements BIP68, MaxSigOps and BlockReward calculation
                                             // Place the PosColdStakingRule after the PosCoinviewRule to ensure that all input scripts have been evaluated
                                             // and that the "IsColdCoinStake" flag would have been set by the OP_CHECKCOLDSTAKEVERIFY opcode if applicable.
                .Register<PosColdStakingRule>()
                .Register<SaveCoinviewRule>();
        }

        protected static Block CreateStratisGenesisBlock(ConsensusFactory consensusFactory, uint nTime, uint nNonce, uint nBits, int nVersion, Money genesisReward)
        {
            string pszTimestamp = "https://www.olympic.org/news/today-at-pyeongchang-2018-sunday-25-february";

            Transaction txNew = consensusFactory.CreateTransaction();
            txNew.Version = 1;
            txNew.Time = nTime;
            txNew.AddInput(new TxIn()
            {
                ScriptSig = new Script(Op.GetPushOp(0), new Op()
                {
                    Code = (OpcodeType)0x1,
                    PushData = new[] { (byte)42 }
                }, Op.GetPushOp(Encoders.ASCII.DecodeData(pszTimestamp)))
            });
            txNew.AddOutput(new TxOut()
            {
                Value = genesisReward,
            });

            Block genesis = consensusFactory.CreateBlock();
            genesis.Header.BlockTime = Utils.UnixTimeToDateTime(nTime);
            genesis.Header.Bits = nBits;
            genesis.Header.Nonce = nNonce;
            genesis.Header.Version = nVersion;
            genesis.Transactions.Add(txNew);
            genesis.Header.HashPrevBlock = uint256.Zero;
            genesis.UpdateMerkleRoot();
            return genesis;
        }
    }
}
