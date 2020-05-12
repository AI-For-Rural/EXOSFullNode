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
    class EXOSTest : EXOSMain
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
            this.DefaultAPIPort = 38221;
            this.DefaultSignalRPort = 39824;
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

            genesisBlock.Header.Time = 1572376229;
            genesisBlock.Header.Nonce = 40540;
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
                [StratisBIP9Deployments.ColdStaking] = new BIP9DeploymentsParameters("ColdStaking", 2,
                    new DateTime(2018, 11, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2019, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                    BIP9DeploymentsParameters.DefaultTestnetThreshold)
            };

            this.Consensus = new NBitcoin.Consensus(
                consensusFactory: consensusFactory,
                consensusOptions: consensusOptions,
                coinType: 248,
                hashGenesisBlock: genesisBlock.GetHash(),
                subsidyHalvingInterval: 210000,
                majorityEnforceBlockUpgrade: 750,
                majorityRejectBlockOutdated: 950,
                majorityWindow: 1000,
                buriedDeployments: buriedDeployments,
                bip9Deployments: bip9Deployments,
                bip34Hash: new uint256("0x000000000000024b89b42a942fe0d9fea3bb44ab7bd1b19115dd6a759c0808b8"),
                minerConfirmationWindow: 2016, // nPowTargetTimespan / nPowTargetSpacing
                maxReorgLength: 500,
                defaultAssumeValid: new uint256("0x690e7e30ae3fa6c10855db0f8bc10110a54f5c73019f5581ee038186154397d0"), // 1100000
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
                { 0, new CheckpointInfo(new uint256("0x00000bf810e65773b5a0e5a43ea656080e10108424dcf475abc4228bfc52148f"), new uint256("0000000000000000000000000000000000000000000000000000000000000000")) },
                { 2, new CheckpointInfo(new uint256("0xc523b576d127b614a25ff9f15542f9d6dbcc45ac143ea1add589a42912a645ae"), new uint256("0x271e210bf4e0ae765dbb6803fb968326de4d616bff19301ba872347d22cbf240")) },
                { 10, new CheckpointInfo(new uint256("0x12a9b2dcdcf1714313f96661f4b7ce577fb5d7fcdf1bc734c38a67c4098c044a"), new uint256("0x32d31ea17bc2140df3b57d3530f45c6424c5b0916da20ebb546c8b0555d0ac47")) },
                { 50, new CheckpointInfo(new uint256("0x0071b0576f1ce5d58330df7c1c36687979168f2abce842c002180592a1a163aa"), new uint256("0x454a45229abced2334ac9f3647082e3586bb58d3d464043c5000bc4fc7f31b87")) },
                { 100, new CheckpointInfo(new uint256("0x68212577007f1038109ea256fcf3537bc86d489ff644aff63c383374333917b7"), new uint256("0xd7c7fd41315ff4f60286da63cec92ca48013fcfdd3cf0985f444e3c809e20337")) },
                { 500, new CheckpointInfo(new uint256("0x7be041a5823d33f0d8554156f3bc0b306f8ec174d89b9f581084b4ea9bd84b73"), new uint256("0xa13acf2577637fa6e9f4d84adf54077ba36f88ad7a07007e57deb6d10265b0a7")) },
                { 1000, new CheckpointInfo(new uint256("0xa1a254d3c4e4977ab3e0fd8e408c12b7bede46f61e5f0eb4edc50338d139369a"), new uint256("0x5623a38e093499ccb0785204a9e6f8369bd504f91c4ee089623d88161c788b8f")) },
                { 5000, new CheckpointInfo(new uint256("0xb5b24c870687522318003fe3a0654e067040587166e29b7dcdb9718443ea4bbf"), new uint256("0x7ef275a27cf82312a1de8dcb7e4c64f3a371fa8c4f41d9f1505b1ff0322e451d")) },
                { 10000, new CheckpointInfo(new uint256("0x5793cf5687e09615058a7054f5c0e146248db8917e601c0b72b245773539f795"), new uint256("0x43ff4cdacba13b2ded8f0844f1cbb2762bcf98fe38f2764e19f50f0ebca331da")) },
                { 15000, new CheckpointInfo(new uint256("0x7754d2d2533994f9a551bad2d784b0cc47eceb652c51bdcda1dc4c360455d35b"), new uint256("0x726afc5c97690157afd022ad39ff080dad4ebbafa2a625f6d13cf90662b46267")) },
                { 30000, new CheckpointInfo(new uint256("0x7836bc776fcd5eae89e53519f05ddf43e3a73c57d9ff13092b063cd11334a1e9"), new uint256("0xa6d60033722f26566107739101bd29d7770df01ce68088989fffe370d16048ab")) },
                { 45000, new CheckpointInfo(new uint256("0x6a0df2a0a66aef10b99e294e0db496e6faeec07a59e434f93be50de1eb327f8d"), new uint256("0xae0fc9185d908f431c78e7035f2d27d35cd0c1f6f523ce46e50fc08784607b60")) },
                { 47000, new CheckpointInfo(new uint256("0xa9bfd5f2206e2579a1f948aefa9f1425824cb7182cd6d7ef3408776a47d24f75"), new uint256("0x48db745eebcb19aa2f9d07404b90f624c6a1657180620fb5701165770f4296a6")) }

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
            Assert(this.Consensus.HashGenesisBlock == uint256.Parse("0x00000e246d7b73b88c9ab55f2e5e94d9e22d471def3df5ea448f5576b1d156b9"));

            this.RegisterRules(this.Consensus);
            this.RegisterMempoolRules(this.Consensus);
        }

    }
}
