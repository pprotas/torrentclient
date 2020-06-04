using System;
using System.IO;
using TorrentClient.Client;

namespace TorrentClient.TestConsole
{
    class Program
    {
        public static BitTorrentClient Client;
        public static void Main(string[] args)
        {
            if (args.Length != 3 || !int.TryParse(args[0], out int port) || !File.Exists(args[1]))
            {
                Console.WriteLine("Error: requires port, torrent file and download directory as first, second and third arguments");
                return;
            }

            Client = new BitTorrentClient(port, args[1], args[2]);
            Client.Start();
        }
    }
}
