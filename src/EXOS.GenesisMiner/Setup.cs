using System;
using System.Reflection;
using System.Text;

namespace GenesisMiner
{
    internal static class Setup
        {
            /// <summary>
            /// Print the utility's header and menu.
            /// </summary>
            public static void OutputHeader()
            {
                var builder = new StringBuilder();

                builder.AppendLine($"Genesis Miner Tool v{Assembly.GetEntryAssembly().GetName().Version}");
                builder.AppendLine("Copyright (c) 2019 Fluidchains Inc");

                Console.WriteLine(builder);
            }

            /// <summary>
            /// Shows the help message woth examples.
            /// This is output on -h command and also in some cases if validation fails.
            /// </summary>
            public static void OutputMenu()
            {
                var builder = new StringBuilder();

                builder.AppendLine("Menu:");
                builder.AppendLine("a   Create genesis blocks for Mainnet, Testnet and Regtest.");
                builder.AppendLine("      args: [-chain=<chain>] ");
                builder.AppendLine("      chain: [BTC][EXOS] Preloaded chains (See Stratis.Bitcoin.Networks)");
                builder.AppendLine("      Example:  a -chain=exos");
                builder.AppendLine("m   Mine genesis with external parameters ");
                builder.AppendLine("      args: [-text=<text>][-target=<target>][-time=<time>][-money=<money>] ");
                builder.AppendLine("      Example:  ");
                builder.AppendLine("      args: [-text=<text>]  A bit of text or a url to be included in the genesis block");
                builder.AppendLine("      -target=<target> Use a uint representation: 0000ffff00000000000000000000000000000000000000000000000000000000");
                builder.AppendLine("      -time=<time> Use a UnixTime format. Example: 1523200000");
                builder.AppendLine("      -money=<money> Unespendable rewards. Use a int value");
                builder.AppendLine("menu  Show this menu.");
                builder.AppendLine("exit  Close the utility.");

                Console.WriteLine(builder);
            }

            /// <summary>
            ///  Output completion message and secret warning.
            /// </summary>
            public static void OutputSuccess()
            {
                Console.WriteLine("Done!");
                Console.WriteLine();
            }

            /// <summary>
            /// Shows an error message, in red.
            /// </summary>
            /// <param name="message">The message to show.</param>
            public static void OutputErrorLine(string message)
            {
                ConsoleColor colorSaved = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                Console.ForegroundColor = colorSaved;
            }
        }
}


