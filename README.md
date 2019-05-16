EXOSFullNode
===============

https://civxeconomy.com/

Bitcoin Implementation in C#
----------------------------

EXOS is an implementation of the Bitcoin protocol in C# on the [.NET Core](https://dotnet.github.io/) platform.  
The node can run on the Bitcoin and EXOS networks.  
EXOS token is based on the [NBitcoin](https://github.com/MetacoSA/NBitcoin) project and [Stratis](https://github.com/stratisproject/StratisBitcoinFullNode) project.

[.NET Core](https://dotnet.github.io/) is an open source cross platform framework and enables the development of applications and services on Windows, macOS and Linux.  
Join our community on [Discord](https://discord.gg/eCNMCMt).  

The design
----------

**A Modular Approach**

A Blockchain is made of many components, from a FullNode that validates blocks to a Simple Wallet that track addresses.
The end goal is to develop a set of [Nuget](https://en.wikipedia.org/wiki/NuGet) packages from which an implementer can cherry pick what he needs.

* **NBitcoin**
* **Stratis.Bitcoin.Core**  - The bare minimum to run a pruned node.
* **Stratis.Bitcoin.Store** - Store and relay blocks to peers.
* **Stratis.Bitcoin.MemoryPool** - Track pending transaction.
* **Stratis.Bitcoin.Wallet** - Send and Receive coins
* **Stratis.Bitcoin.Miner** - POS or POW
* **Stratis.Bitcoin.Explorer**


Create a Blockchain in a .NET Core style programming
```
  var node = new FullNodeBuilder()
   .UseNodeSettings(nodeSettings)
   .UseConsensus()
   .UseBlockStore()
   .UseMempool()
   .AddMining()
   .AddRPC()
   .Build();

  node.Run();
```

What's Next
----------

We plan to add many more features on top of the Stratis Bitcoin blockchain:
Sidechains, Private/Permissioned blockchain, Compiled Smart Contracts, NTumbleBit/Breeze wallet and more...

Running a FullNode
------------------

Our full node is currently in alpha.  

```
git clone https://github.com/exofoundation/CivXFullNode.git
cd StratisBitcoinFullNode\src

dotnet build

```

To run on the Bitcoin network: ``` Stratis.BitcoinD\dotnet run ```  
To run on the CivX network: ``` Stratis.StratisD\dotnet run ```  

Getting Started Guide
-----------
More details on getting started are available [here](https://github.com/exofoundation/CivXFullNode/blob/master/Documentation/getting-started.md)

Development
-----------
Up for some blockchain development?

Check this guides for more info:
* [Contributing Guide](Documentation/contributing.md)
* [Coding Style](Documentation/coding-style.md)

There is a lot to do and we welcome contributers developers and testers who want to get some Blockchain experience.

Testing
-------
* [Testing Guidelines](Documentation/testing-guidelines.md)
