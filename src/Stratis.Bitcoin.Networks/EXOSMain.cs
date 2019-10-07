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
    public class EXOSMain : Network
    {
        /// <summary> Stratis maximal value for the calculated time offset. If the value is over this limit, the time syncing feature will be switched off. </summary>
        public const int StratisMaxTimeOffsetSeconds = 25 * 60;

        /// <summary> Fluid default value for the maximum tip age in seconds to consider the node in initial block download (2 hours). </summary>
        public const int StratisDefaultMaxTipAgeInSeconds = 2 * 60 * 60;

        /// <summary> The name of the root folder containing the different Fluid blockchains (FluidMain, FluidTest, FluidRegTest). </summary>
        public const string StratisRootFolderName = "exos";

        /// <summary> The default name used for the Fluid configuration file. </summary>
        public const string StratisDefaultConfigFilename = "exos.conf";

        public EXOSMain()
        {
            // The message start string is designed to be unlikely to occur in normal data.
            // The characters are rarely used upper ASCII, not valid as UTF-8, and produce
            // a large 4-byte int at any alignment.
            var messageStart = new byte[4];
            messageStart[0] = 0x28;
            messageStart[1] = 0x62;
            messageStart[2] = 0x48;
            messageStart[3] = 0x76;
            uint magic = BitConverter.ToUInt32(messageStart, 0);

            this.Name = "EXOSMain";
            this.NetworkType = NetworkType.Mainnet;
            this.Magic = magic;
            this.DefaultPort = 4562;
            this.DefaultMaxOutboundConnections = 16;
            this.DefaultMaxInboundConnections = 109;
            this.DefaultRPCPort = 4561;
            this.DefaultAPIPort = 39120; //EXOS
            this.DefaultSignalRPort = 39620;
            this.MaxTipAge = 2 * 60 * 60;
            this.MinTxFee = 10000;
            this.FallbackFee = 10000;
            this.MinRelayTxFee = 10000;
            this.RootFolderName = StratisRootFolderName;
            this.DefaultConfigFilename = StratisDefaultConfigFilename;
            this.MaxTimeOffsetSeconds = 25 * 60;
            this.CoinTicker = "EXOS";
            this.DefaultBanTimeSeconds = 16000; // 500 (MaxReorg) * 64 (TargetSpacing) / 2 = 4 hours, 26 minutes and 40 seconds

            var consensusFactory = new PosConsensusFactory();

            // Create the genesis block.
            this.GenesisTime = 1523205120;
            this.GenesisNonce = 842767;
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
                premineReward: Money.Coins(300000000),
                proofOfWorkReward: Money.Coins(12),
                powTargetTimespan: TimeSpan.FromSeconds(14 * 24 * 60 * 60), // two weeks
                powTargetSpacing: TimeSpan.FromSeconds(10 * 60),
                powAllowMinDifficultyBlocks: false,
                posNoRetargeting: false,
                powNoRetargeting: false,
                powLimit: new Target(new uint256("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")),
                minimumChainWork: null,
                isProofOfStake: true,
                lastPowBlock: 45000,
                proofOfStakeLimit: new BigInteger(uint256.Parse("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
                proofOfStakeLimitV2: new BigInteger(uint256.Parse("000000000000ffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
                proofOfStakeReward: Money.COIN
            );

            this.Base58Prefixes = new byte[12][];
            this.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { (28) };
            this.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { (87) };
            this.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (28 + 128) };
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
                { 0, new CheckpointInfo(new uint256("0x00000036090a68c523471da7a4f0f958c1b4403fef74a003be7f71877699cab7"), new uint256("0x0000000000000000000000000000000000000000000000000000000000000000")) },
                { 2, new CheckpointInfo(new uint256("0x892f74fe6b462ec612dc745f19b7f299ffb94aabb17a0826a03092c0eb6f83ec"), new uint256("0x8e61edcdfee2e948c7b6885b91a853cb82ba22ae5a5ef97776f2fb381f99cb09")) },
                { 10, new CheckpointInfo(new uint256("0xd7c9e4381c4b9331be82d13e0b40b932ac624f3fa8b4e80fb8e9f22769095c0a"), new uint256("0x221232ad2caa1635d2b1302b6eafc0254255a7bf5a2d3ea25ac6ac91f6e1c256")) },
                { 50, new CheckpointInfo(new uint256("0x71b54f5cb27d2b0b1dd34ba89c7ff4fa8e8b37da727373cf98134069fb84803e"), new uint256("0x8aaf92c1d948ba107711997b9d56c08ace5e6d3ab11dfbf2ffffc19798eff454")) },
                { 100, new CheckpointInfo(new uint256("0xf1fbdb26299ca5bac77963cc2ffd9e1830c3f106b87b235b5c731b4b3259d383"), new uint256("0xfeef70b0e16e80bb2f977ebc3f9aa72e4a5c052d2825e9fe6ee8a593555760ea")) },
                { 500, new CheckpointInfo(new uint256("0x72bc96ba513fb55431f335de897bb8ce80d898e17a3d4d5e05c48d1670d5f234"), new uint256("0x5e39f38d0a7f68b0aa59c0a5317d6b9ac1c208f01fe56df079f1d0d9d3da3b09")) },
                { 1000, new CheckpointInfo(new uint256("0xf8f562cd694b5ca517aca2e1cd5b953c22a185e715209bfc2a6ae8eb0a524289"), new uint256("0xa1f55377eef150d1c6863d77c618cf397080ddcdfaa0da2038f1682019c83631")) },
                { 5000, new CheckpointInfo(new uint256("0x9fbc9dc45a507a287d043b43a55e0623aca4368ca5c655bc1a7a97eef4951d1a"), new uint256("0x77412fe812a6f4d59b8cce8f4bd2ac7e8bfb5dc56b542348e66f9a4bc7c00441")) },
                { 10000, new CheckpointInfo(new uint256("0x45e01c13af7625b7289ed12f687864d485f603c2b5dcccfff26789bcbbc20439"), new uint256("0x2ccca90aa37406865a6feb5ec198f01d045a64a2aca5ff56ac96c88fe37d2514")) },
                { 14000, new CheckpointInfo(new uint256("0xecd5ae5e58ddde01087a4c7f2033252acc7237a7aa958f4cd0eb016b3c11cd0e"), new uint256("0xd608bfd6f8b4be11f1a0acd50f0cd269b35d40b9b298c564b0a79b9771025c19")) },
                { 18000, new CheckpointInfo(new uint256("0x42ee388a72f85e8a63ed81bdaa4d87040a009cff8471f15e5711ab824faedaa7"), new uint256("0x912d1fcd0f1aa5217893cdda2c536cdbed6039af4cb3dc10a9e9f9f8e7522e6e")) },
                { 20000, new CheckpointInfo(new uint256("0xead9d788c5e0a275d9a8434487248d5b5ed1db14de8ea1627dd70ad1e5fb5f5b"), new uint256("0xcaa8a9f4374e56ef86743040f089e022c50a9cb6cdeb432d66bc077aaf9e8078")) },
                { 25000, new CheckpointInfo(new uint256("0x9e889f90ee0a249a84c2c090117be70de8d53466a5f0bb312fde435ad50080f0"), new uint256("0xd49c521e4c46fb798dd366afbec6c90020d295228e32f0582f9fd5ce2674f3a0")) },
                { 30000, new CheckpointInfo(new uint256("0xcd878a41441aca4e2903941c374d0caf17d7080fd1c5e37aca9caab63e82f333"), new uint256("0xa58e327073c17ed360cfe6747ac2fe9ec6bff3f5be00415b3d2897ccf39d5189")) },
                { 35500, new CheckpointInfo(new uint256("0x3196c8456be83bc810ef0f6e2b34a0962435e89d0844b9db8070487d5ec91afa"), new uint256("0x309d454e9906e40c9472762633188af3957d586be33994a7586c23accb99bd40")) },
                { 40000, new CheckpointInfo(new uint256("0x4d7b1d7115714d16dce3266087309e898e02a7dc1eb8d3f450c8473836de5a19"), new uint256("0x14eb31bf1e59918b45f74afe0d179475a3a83dfc11e33c974520e5567c8ffb8e")) },
                { 45000, new CheckpointInfo(new uint256("0x5f110ad2e1fbbb98bcd4d85167c7904631304bbd311144118f631d59795c0f00"), new uint256("0xf4171d8e19d71d032915363acef2a52f29c0833fe6d685fd0cf7d98efa2942bb")) },
                { 47000, new CheckpointInfo(new uint256("0x0b834c3a8d77939f5fc2e372bc03925a1170e8d386671a8d0f21fa8e5a9d440e"), new uint256("0x5d96f845bf42cc466e5857952fd5e00e27a51c21cdb8b4399f71da1ae4bbba2b")) },
                { 60000, new CheckpointInfo(new uint256("0xe904061cd883995fa96fa162927f771a3e0a834866cf741da1bf6f64d9807aab"), new uint256("0xd6778815c7b9989ea2ddba8d69e7b7c94a7cd48320b1295e5c23784b31b184d9")) },
                { 80000, new CheckpointInfo(new uint256("0x4991919485bc0c4f1f3b89dde06f90cca64b6eee2887a8f2105ed1f7219b056e"), new uint256("0x5f667b4d09a5e0e6fb6f41a7f220f4b769bc225c3feb6f826f84c1a2aedc144a")) },
                { 100000, new CheckpointInfo(new uint256("0x5be1f76165c7133562cdcb3beeac6413aea18e538883e379d5929c6eb26999d1"), new uint256("0x2e0dc1a4fce268f5a85b41a4a9e37032a26b77a6a22d7eae2df5dd4b1e004353")) },
                { 125000, new CheckpointInfo(new uint256("0xd2f245d326e4e0f1e9e22f548496ca9e47025721d7fece27e071a9f63628f0b8"), new uint256("0x6884864f6017c768868774e600907c104efd3b1e2c529411f7d88a1c11a0478e")) },
                { 150000, new CheckpointInfo(new uint256("0x4b4325e2c02654284de2719033c0defba485bd08a6259ca67372600447bf084e"), new uint256("0x358255ae11f9e061268ee9a4947ab275b4c0c12cc4d38b1a581801e1e85da1cb")) },
                { 200000, new CheckpointInfo(new uint256("0x4e4e40dc5cc007135f5113e1ebb22b06c39cff15637b5f51d93340a9cad0dfdf"), new uint256("0xc12688b1c2f4f95762ddac23d078b5a3b4fa02b5c351ad1f544839ff4ef5c061")) },
                { 250000, new CheckpointInfo(new uint256("0x20c97546de02e60c2d53a9c95e65956a3d89e81eb5f7075882fac2d6cc24d316"), new uint256("0x4e2533fd3cf6c03c1eeeea174df123d9a60d50b15e39ee39ea9450514c473731")) },
                { 300000, new CheckpointInfo(new uint256("0x1dca0bf2f051429e911fa9b232fcdf69bbaed667fa55451a3ff4d6450ae5dc52"), new uint256("0xf424bdc3c5ce706a531986bf5ace04ad29d8f141396e36f71ff873a3ec26bb09")) }

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
                 new DNSSeedData("seednode1.oexo.cloud", "seednode1.oexo.cloud"),
                new DNSSeedData("seednode2.oexo.net", "seednode2.oexo.net"),
                new DNSSeedData("seednode3.oexo.cloud", "seednode3.oexo.cloud"),
                new DNSSeedData("seednode4.oexo.net", "seednode4.oexo.net")
            };

            this.SeedNodes = new List<NetworkAddress>
            {

            };

            var hostnames = new[] { "vps101.oexo.cloud", "vps102.oexo.net", "vps201.oexo.cloud", "vps202.oexo.net" };
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
            Assert(this.Consensus.HashGenesisBlock == uint256.Parse("0x00000036090a68c523471da7a4f0f958c1b4403fef74a003be7f71877699cab7"));
            Assert(this.Genesis.Header.HashMerkleRoot == uint256.Parse("0x85c4a8a116eb457ff74bb64908e71c6780bff7e69ad3dadc9df6cd753c21f937"));

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
            string pszTimestamp = "http://www.bbc.com/news/world-middle-east-43691291";

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
