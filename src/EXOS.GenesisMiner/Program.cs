using System;
using System.IO;
using NBitcoin;
using Stratis.Bitcoin.Networks;

namespace GenesisMiner
{
    /// <summary>
    /// This tool generate all networks genesis. 
    /// </summary>
    class Program
    {
        private const string SwitchAllGenesis = "a";
        private const string SwitchMinerOnDemand = "m";
        private const string SwitchMenu = "menu";
        private const string SwitchExit = "exit";
        

        static void Main(string[] args)
        {

             if (args.Length > 0)
            {
                SwitchCommand(args, args[0], string.Join(" ", args));
                return;
            }

            Console.SetIn(new StreamReader(Console.OpenStandardInput(), Console.InputEncoding, false, bufferSize: 1024));

            // Start with the banner and the help message.
            Setup.OutputHeader();
            Setup.OutputMenu();

            while (true)
            {
                try
                {
                    Console.Write("Enter your option: ");
                    string userInput = Console.ReadLine().Trim();

                    string command = null;
                    if (!string.IsNullOrEmpty(userInput))
                    {
                        args = userInput.Split(" ");
                        command = args[0];
                    }
                    else
                    {
                        args = null;
                    }

                    Console.WriteLine();

                    SwitchCommand(args, command, userInput);
                }
                catch (Exception ex)
                {
                    Setup.OutputErrorLine($"An error occurred: {ex.Message}");
                    Console.WriteLine();
                    Setup.OutputMenu();
                }
            }
        }

        private static void SwitchCommand(string[] args, string command, string userInput)
        {
            switch (command)
            {
                case SwitchExit:
                    {
                        Environment.Exit(0);
                        break;
                    }
                case SwitchMenu:
                    {
                        HandleSwitchMenuCommand(args);
                        break;
                    }
                case SwitchAllGenesis:
                    {
                        HandleSwitchAllGenesisCommand(userInput);
                        break;
                    }
                case SwitchMinerOnDemand:
                    {
                        HandleSwitchMinerOnDemandCommand(args);
                        break;
                    }
            }
        }

        private static void HandleSwitchMenuCommand(string[] args)
        {
            if (args.Length != 1)
                throw new ArgumentException("Invalid option");

            Setup.OutputMenu();
        }

        
        private static void HandleSwitchAllGenesisCommand(string userInput)
        {
            Miner miner = new Miner();

            if (string.IsNullOrEmpty(userInput))
                throw new ArgumentException("The -chain=\"<chain>\" argument is missing.");

            string text = userInput.Substring(userInput.IndexOf("chain=") + 6);
            
            if (string.Equals(text, "BTC", StringComparison.CurrentCultureIgnoreCase))
            {
                string coinbaseText = "The Times 03/Jan/2009 Chancellor on brink of second bailout for banks";
                miner.MineAllNetworks(new PosConsensusFactory(), Networks.Bitcoin, coinbaseText);
            }
            else 
            if (string.Equals(text, "EXOS", StringComparison.CurrentCultureIgnoreCase))
            {
                string coinbaseText = "http://www.bbc.com/news/world-middle-east-43691291";
                miner.MineAllNetworks(new PosConsensusFactory(), Networks.EXOS, coinbaseText);
            }
        }

        private static void HandleSwitchMinerOnDemandCommand(string[] args)
        {
            DateTimeOffset dateNow = DateTimeOffset.Now;

            string coinbaseText = null;
            string target = null;
            string time = null;
            string money = null;
                      
            coinbaseText = Array.Find(args, element =>
                element.StartsWith("-text=", StringComparison.Ordinal));

            target = Array.Find(args, element =>
                element.StartsWith("-target=", StringComparison.Ordinal));

            time = Array.Find(args, element =>
                element.StartsWith("-time=", StringComparison.Ordinal));

            money = Array.Find(args, element =>
                element.StartsWith("-money=", StringComparison.Ordinal));

            if (string.IsNullOrEmpty(coinbaseText))
                throw new ArgumentException("The -text=<text> argument is missing.");

            if (string.IsNullOrEmpty(target))
                throw new ArgumentException("The -target=<target> argument is missing.");

            var newTarget = new uint256(target);

            var newTime = Utils.DateTimeToUnixTime(DateTimeOffset.Now);
            if (!string.IsNullOrEmpty(time))
            {
                newTime = Convert.ToUInt32(time);
            }

            var moneyDecimal = decimal.Parse(money);
            var reward = Money.Coins(moneyDecimal);

            Miner.MineGenesisBlock(new PosConsensusFactory(), coinbaseText, new Target(newTarget), newTime, reward);

        }
            
    }
}
