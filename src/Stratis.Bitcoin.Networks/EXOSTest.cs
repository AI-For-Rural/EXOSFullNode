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
            this.DefaultPort = 14562;
            this.DefaultMaxOutboundConnections = 16;
            this.DefaultMaxInboundConnections = 109;
            this.DefaultRPCPort = 14561;
            this.DefaultAPIPort = 39121;
            this.DefaultSignalRPort = 38621;
            this.CoinTicker = "TEXOS";
            this.DefaultBanTimeSeconds = 16000; // 500 (MaxReorg) * 64 (TargetSpacing) / 2 = 4 hours, 26 minutes and 40 seconds

            var powLimit = new Target(new uint256("0000ffff00000000000000000000000000000000000000000000000000000000"));

            var consensusFactory = new PosConsensusFactory();

            // Create the genesis block.
            this.GenesisTime = 1523205120;
            this.GenesisNonce = 842767;
            this.GenesisBits = 0x1e0fffff;
            this.GenesisVersion = 1;
            this.GenesisReward = Money.Zero;

            Block genesisBlock = CreateStratisGenesisBlock(consensusFactory, this.GenesisTime, this.GenesisNonce, this.GenesisBits, this.GenesisVersion, this.GenesisReward);

            genesisBlock.Header.Time = 1528761600;
            genesisBlock.Header.Nonce = 440504;
            genesisBlock.Header.Bits = powLimit;

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
                bip34Hash: new uint256("0x0000059bb2c2048493efcb0f1a034972b3ce4089d54c93b69aaab212fb369887"),
                ruleChangeActivationThreshold: 1916, // 95% of 2016
                minerConfirmationWindow: 2016, // nPowTargetTimespan / nPowTargetSpacing
                maxReorgLength: 500,
                defaultAssumeValid: new uint256("0xffed8f737ce62b33e5bdd675d0243686bc67f8d7041dd3d7e49ddc3062dec785"), // 78000
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
                lastPowBlock: 45005,
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
                { 0, new CheckpointInfo(new uint256("0x0000059bb2c2048493efcb0f1a034972b3ce4089d54c93b69aaab212fb369887"), new uint256("0x0000000000000000000000000000000000000000000000000000000000000000")) },
                { 2, new CheckpointInfo(new uint256("0xf1317999b79c983da36aeca960d4bda3957558db3e99c2474965049ef8ce2050"), new uint256("0x0e7256c115bbee91374f77b8033caff2df57736321df3ee28f24e7d8ddd69590")) },
                { 10, new CheckpointInfo(new uint256("0xa97253fddf3aee8cb585b7599638e1a45f86bfcf3244528ad84a6144d12be1bc"), new uint256("0x4f5bbbcbf2da4abf247bb6d807fab8a0e47d9461450da6f2d1395c9a2e3841d1")) },
                { 50, new CheckpointInfo(new uint256("0x31f10f8829be7c6ba6935c4ec78409462788822677bd2a9f1e5dcaf0a8dc53e4"), new uint256("0x05220e0ae37c38911790316d2b71593019fe95502b0d03636db77ccb219026b5")) },
                { 500, new CheckpointInfo(new uint256("0xd732052f1a209cfe5c28897f4e3e07585292baf7accfab03734eb9c969a741fa"), new uint256("0x762358a331eb58e23930c409a7c15122504912d9de46c1a818918b7b117ec98b")) },
                { 1000, new CheckpointInfo(new uint256("0x64fe6c261f1387587710486203480be57cd342bd5966a46d8ee4ace52158323f"), new uint256("0xea320c1fe57db0ebd7cc3ec3aba0b00931473765c4507b41e03cd39e6715446e")) },
                { 5000, new CheckpointInfo(new uint256("0xa1de4325a7d8cad4d2e7f61fb8fde91807f483d25a421a074e666c1af24cd370"), new uint256("0xbeab25cf4802f67f4777f23f269b93c5e99ecc754ca48f38b2c4c59b2185e141")) },
                { 10000, new CheckpointInfo(new uint256("0xbf2588c4eaec9fd2b27ba577ff8004892570914e3dc7121a08936cb68aad196a"), new uint256("0x9660bb96adc9c81e0db373965677661c2a6d38a9d6b925be7eea69c947712e33")) },
                { 25000, new CheckpointInfo(new uint256("0xffee3f64cb9adb1bb719a3d202945ba218be7548793ff4346b2c8e1a7bc989b3"), new uint256("0xc82bf5587395301fdef2cbcf9c03f4a634006aac07b0d55d394590ef0fbb0cdb")) },
                { 45000, new CheckpointInfo(new uint256("0x0b230068a9e83f9e405d31d6681190c0466c390f56536bb61772682ef851aade"), new uint256("0x5104ba9e879e71c5e643c9e5098ead862a303aa4ab9ee1bd25ed0e067b543d31")) },
                { 99000, new CheckpointInfo(new uint256("0x1608eb6833d886b462ccd2cc2971c0f76bb86739d038c92d075f73ee4bd8fae0"), new uint256("0xa22b1fe5f66716fd122254a2546a97f8a0588a7160b68cae1c1bf5f0d5a48632")) }

            };

            this.DNSSeeds = new List<DNSSeedData>
            {
                new DNSSeedData("testseednode1.oexo.cloud", "testseednode1.oexo.cloud"),
                new DNSSeedData("testseednode2.oexo.net", "testseednode2.oexo.net"),
                new DNSSeedData("testseednode3.oexo.cloud", "testseednode3.oexo.cloud"),
                new DNSSeedData("testseednode4.oexo.net", "testseednode4.oexo.net")
            };

            this.SeedNodes = new List<NetworkAddress>
            {

            };
            var hostnames = new[] { "vps151.oexo.cloud", "vps152.oexo.net", "vps251.oexo.cloud", "vps252.oexo.net" };
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
            Assert(this.Consensus.HashGenesisBlock == uint256.Parse("0x0000059bb2c2048493efcb0f1a034972b3ce4089d54c93b69aaab212fb369887"));

            this.RegisterRules(this.Consensus);
        }
    }
}