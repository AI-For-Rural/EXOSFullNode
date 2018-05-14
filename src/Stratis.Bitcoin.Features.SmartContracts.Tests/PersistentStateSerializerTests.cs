﻿using NBitcoin;
using Stratis.SmartContracts;
using Stratis.SmartContracts.Core;
using Stratis.SmartContracts.Core.Serialization;
using Xunit;

namespace Stratis.Bitcoin.Features.SmartContracts.Tests
{
    public class PersistentStateSerializerTests
    {
        private PersistentStateSerializer serializer;

        public PersistentStateSerializerTests()
        {
            this.serializer = new PersistentStateSerializer();
        }

        [Fact]
        public void PersistentState_CanSerializeAllTypes()
        {
            // Checking that these all work for now. 
            // TODO: Check that these actually are serialized in a performant way
            TestType<Address>(new uint160(123456).ToAddress(Network.SmartContractsRegTest));
            TestType<bool>(true);
            TestType<int>((int)32);
            TestType<long>((long)6775492);
            TestType<uint>((uint)101);
            TestType<ulong>((ulong)1245);
            TestType<byte>(new byte());
            TestType<sbyte>(new sbyte());
            TestType<byte[]>(new byte[] { 127, 123 });
            TestType<char>('c');
            TestType<string>("Test String");
        }

        [Fact]
        public void PersistentState_CanSerializeValueType()
        {
        }

        private void TestType<T>(T input)
        {
            byte[] testBytes = this.serializer.Serialize(input, Network.SmartContractsRegTest);
            T output = this.serializer.Deserialize<T>(testBytes, Network.SmartContractsRegTest);
            Assert.Equal(input, output);
        }
    }

    public struct TestValueType
    {
        public Address AddressField;
        public Address AddressProp { get; set; }

        public bool BoolField;
        public bool BoolProp { get; set; }

        public int IntField;
        public int IntProp { get; set; }

        public long LongField;
        public long LongProp { get; set; }

        public uint UintField;
        public uint UintProp { get; set; }
    }
}