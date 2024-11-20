using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace IpCalculator
{
    internal class Program
    {
        static string invalid = "Invalid option...";

        static readonly string[] Masks = {"0.0.0.0", "128.0.0.0", "192.0.0.0", "224.0.0.0", "240.0.0.0", "248.0.0.0", "252.0.0.0", "254.0.0.0",
            "255.0.0.0", "255.128.0.0", "255.192.0.0", "255.224.0.0", "255.240.0.0", "255.248.0.0", "255.252.0.0", "255.254.0.0",
            "255.255.0.0","255.255.128.0", "255.255.192.0", "255.255.224.0", "255.255.240.0", "255.255.248.0", "255.255.252.0",
            "255.255.254.0", "255.255.255.0", "255.255.255.128", "255.255.255.192", "255.255.255.224", "255.255.255.240",
            "255.255.255.248", "255.255.255.252", "255.255.255.254", "255.255.255.255" };
        public class NetworkInfo
        {
            public string NetworkAddress { get; set; }
            public string BroadcastAddress { get; set; }
            public string FirstUsableAddress { get; set; }
            public string LastUsableAddress { get; set; }
            public int TotalHosts { get; set; }
        }

        public class VlsmObject
        {
            public string Network { get; set; }
            public int Mask { get; set; }
            public string FirstHost { get; set; }
            public string LastHost { get; set; }
            public string Broadcast { get; set; }
            public int WantedHost { get; set; }

            public VlsmObject(string network, int mask, string firstHost, string lastHost, string broadcast, int wantedHost)
            {
                Network = network;
                Mask = mask;
                FirstHost = firstHost;
                LastHost = lastHost;
                Broadcast = broadcast;
                WantedHost = wantedHost;
            }
        }
        static bool FirstVLSM;

        static List<VlsmObject> ListOfNetworks = new List<VlsmObject>();
        public class CalculateIP
        {
            public static NetworkInfo CalculateNetworkInfo(string ipAddress, string subnetMask)
            {
                if (!IsValidIPAddress(ipAddress) || !IsValidSubnetMask(subnetMask))
                    throw new ArgumentException("Invalid IP address or subnet mask format.");

                uint ip = ConvertToUint(ipAddress);
                uint mask = ConvertToUint(subnetMask);

                uint networkAddress = ip & mask;
                uint broadcastAddress = networkAddress | ~mask;
                uint firstUsableAddress = networkAddress + 1;
                uint lastUsableAddress = broadcastAddress - 1;

                int hostBits = 32 - CountBits(mask);
                int totalHosts = (int)Math.Pow(2, hostBits) - 2;

                return new NetworkInfo
                {
                    NetworkAddress = ConvertToIpString(networkAddress),
                    BroadcastAddress = ConvertToIpString(broadcastAddress),
                    FirstUsableAddress = ConvertToIpString(firstUsableAddress),
                    LastUsableAddress = ConvertToIpString(lastUsableAddress),
                    TotalHosts = totalHosts
                };
            }

            public static bool IsValidIPAddress(string ipAddress)
            {
                string[] octets = ipAddress.Split('.');
                if (octets.Length != 4) return false;
                foreach (string octet in octets)
                {
                    if (!int.TryParse(octet, out int num) || num < 0 || num > 255)
                        return false;
                }
                return true;
            }

            public static bool IsValidSubnetMask(string subnetMask)
            {
                uint mask = ConvertToUint(subnetMask);
                if (mask == 0xFFFFFFFF || mask == 0xFFFFFFFE)
                    return false;

                bool seenZero = false;
                for (int i = 0; i < 32; i++)
                {
                    if ((mask & (1u << (31 - i))) == 0)
                        seenZero = true;
                    else if (seenZero)
                        return false;
                }
                return true;
            }

            public static uint ConvertToUint(string ipAddress)
            {
                string[] octets = ipAddress.Split('.');
                uint ip = 0;
                for (int i = 0; i < 4; i++)
                    ip |= (uint)(byte.Parse(octets[i]) << ((3 - i) * 8));
                return ip;
            }

            public static string ConvertToIpString(uint ip)
            {
                return string.Join(".", new string[]
                {
                    ((ip >> 24) & 0xFF).ToString(),
                    ((ip >> 16) & 0xFF).ToString(),
                    ((ip >> 8) & 0xFF).ToString(),
                    (ip & 0xFF).ToString()
                });
            }

            public static int CountBits(uint mask)
            {
                int count = 0;
                while (mask > 0)
                {
                    count += (int)(mask & 1);
                    mask >>= 1;
                }
                return count;
            }
        }

        static void Main()
        {
            while (true)
            {
                ShowMenu();
                int choice;
                if (!int.TryParse(Console.ReadLine(), out choice))
                {
                    Console.Clear();
                    Console.WriteLine(invalid);
                    continue;
                }
                switch (choice)
                {
                    case 1:
                        CalculateIPv4Address();
                        break;
                    case 2:
                        CalculateVLSM();
                        break;
                    case 3:
                        ShowHelp();
                        break;
                    case 4:
                        Console.WriteLine("Ending program...");
                        return;
                    default:
                        Console.WriteLine(invalid);
                        break;
                }
            }
        }

        static void CalculateIPv4Address()
        {
            Console.Clear();
            Console.Write("Enter IP Address (e.g., 192.168.0.1): ");
            string ipAddress = Console.ReadLine();
            Console.Write("Enter Subnet Mask (e.g.,255.255.255.0): ");
            string subnetMask = Console.ReadLine();

            try
            {
                NetworkInfo networkInfo = CalculateIP.CalculateNetworkInfo(ipAddress, subnetMask);
                Console.WriteLine("\nCalculation Results:");
                Console.WriteLine("----------------------");
                Console.WriteLine("Network Address: " + networkInfo.NetworkAddress);
                Console.WriteLine("Broadcast Address: " + networkInfo.BroadcastAddress);
                Console.WriteLine("First Usable Address: " + networkInfo.FirstUsableAddress);
                Console.WriteLine("Last Usable Address: " + networkInfo.LastUsableAddress);
                Console.WriteLine("Total Hosts: " + networkInfo.TotalHosts);
                Console.WriteLine("----------------------\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        static void CalculateVLSM()
        {
            Console.Clear();
            Console.Write("Enter IP Address: ");
            string ipAddress = Console.ReadLine();
            string IP = IpCheck(ipAddress);
            Console.Write("Enter Subnet Mask (CIDR or dotted-decimal): ");
            string subnetMask = Console.ReadLine();
            int Mask = MaskCheck(subnetMask);
            Console.Write("Enter host requirements separated by commas: ");
            int[] hostSorted = HostsSorting();
            int Overflow = OverflowCheck(hostSorted, Mask);
            if (hostSorted == null)
            {
                return;
            }

            bool FirstVLSM = true;
            for (int i = 0; i < Overflow; i++)
            {
                if (i == 0)
                {
                    ListOfNetworks.Add(CalculateVLSM(IP, Mask, hostSorted[i]));
                }
                else
                {
                    ListOfNetworks.Add(CalculateVLSM(ListOfNetworks[i - 1].Broadcast, Mask, hostSorted[i]));
                }
            }
            foreach (VlsmObject item in ListOfNetworks)
            {
                Console.WriteLine("-------------------");
                Console.WriteLine("Network: " + item.Network);
                Console.WriteLine("Mask: " + item.Mask);
                Console.WriteLine("First Host: " + item.FirstHost);
                Console.WriteLine("Last Host: " + item.LastHost);
                Console.WriteLine("Broadcast: " + item.Broadcast);
                Console.WriteLine("Wanted host: " + item.WantedHost);
            }

        }

        static void ShowMenu()
        {
            Console.WriteLine(MaskCheck("255.255.255.0"));
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("IPv4 Calculator v1.0");
            Console.WriteLine("1. Calculate IPv4 Address");
            Console.WriteLine("2. VLSM Calculation");
            Console.WriteLine("3. Help");
            Console.WriteLine("4. Exit");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Choose an option: ");
        }

        static void ShowHelp()
        {
            Console.Clear();
            Console.WriteLine("Help - IPv4 Calculator v1.0");
            Console.WriteLine("----------------------------");
            Console.WriteLine("1. Calculate IPv4 Address: Enter an IP address and subnet mask to get the network address, broadcast address, usable IP range, and total hosts.");
            Console.WriteLine("2. VLSM Calculation: This feature allows you to perform Variable Length Subnet Mask calculations based on your host requirements.");
            Console.WriteLine("3. Help: Displays this help information.");
            Console.WriteLine("4. Exit: Ends the program.");
            Console.WriteLine("----------------------------\n");
        }
        static int[] HostsSorting()
        {
            string hostRequirementsInput = Console.ReadLine();
            string[] hostRequirementsStrings = hostRequirementsInput.Split(',');
            List<int> hostRequirements = new List<int>();
            bool validInput = true;
            foreach (var requirement in hostRequirementsStrings)
            {
                if (int.TryParse(requirement.Trim(), out int result))
                {
                    hostRequirements.Add(result);
                }
                else
                {
                    Console.WriteLine("Invalid input for host requirements.");
                    validInput = false;
                }
            }
            if (validInput)
            {
                hostRequirements.Sort((x, y) => y.CompareTo(x));
                return hostRequirements.ToArray();
            }
            else
            {
                return null;
            }
        }

        static int MaskCheck(string MaskLine)
        {
            int IpMask;
            if (MaskLine.Contains("."))
            {
                IpMask = Array.IndexOf(Masks, MaskLine);
            }
            else if (MaskLine.Contains("/"))
            {
                IpMask = int.Parse(MaskLine.Replace("/", string.Empty));
            }
            else
            {
                IpMask = int.Parse(MaskLine);
            }

            return IpMask;
        }
        static string IpCheck(string IpAddress)
        {
            string[] Testing = IpAddress.Split('.');
            string IpToReturn = IpAddress;

            bool UserError = false;

            for (int i = 0; i < Testing.Length; i++)
            {
                try
                {
                    if (!string.IsNullOrEmpty(Testing[i]))
                    {
                        int x = int.Parse(Testing[i]);
                        if (x < 0 || x > 255)
                        {
                            UserError = true;
                            break;
                        }
                    }
                    else
                    {
                        UserError = true;
                        break;
                    }
                }
                catch
                {
                    UserError = true;
                    break;
                }
            }

            if (UserError)
            {
                return "invalid ip";
            }

            if (Testing[0].Length == 8)
            {
                try
                {
                    int Foct = Convert.ToInt32(Testing[0], 2);
                    int Soct = Convert.ToInt32(Testing[1], 2);
                    int Toct = Convert.ToInt32(Testing[2], 2);
                    int Fouroct = Convert.ToInt32(Testing[3], 2);
                    IpToReturn = Foct + "." + Soct + "." + Toct + "." + Fouroct;
                }
                catch
                {
                    return "invalid ip";
                }
            }
            else if (IpAddress.Contains("."))
            {
                if (Testing.Length == 2)
                {
                    IpToReturn += ".0.0";
                }
                else if (Testing.Length == 3)
                {
                    IpToReturn += ".0";
                }
            }
            else
            {
                IpToReturn += ".0.0.0";
            }

            return IpToReturn;
        }
        static VlsmObject CalculateVLSM(string ipAddress, int mask, int requiredHosts)
        {

            string network, firstHost, lastHost, broadcast;
            int vlsmMask, overflowHosts;

            int firstOctet, secondOctet, thirdOctet, fourthOctet;
            int totalHostsRequired = 2;
            int adjustedMask = 0;

            string[] octets = ipAddress.Split('.');

            int tempMask = 0;
            int hostsInCurrentMask = 2;

            while (tempMask < 31 - mask)
            {
                hostsInCurrentMask *= 2;
                tempMask++;
            }

            if (mask >= 8 && mask <= 30)
            {

                while (totalHostsRequired - 2 < requiredHosts)
                {
                    totalHostsRequired *= 2;
                    adjustedMask++;
                }

                overflowHosts = totalHostsRequired;

                if (31 - adjustedMask >= mask && hostsInCurrentMask >= overflowHosts && requiredHosts > 0)
                {
                    vlsmMask = 31 - adjustedMask;
                    totalHostsRequired -= 3;

                    firstOctet = int.Parse(octets[0]);
                    secondOctet = int.Parse(octets[1]);
                    thirdOctet = int.Parse(octets[2]);
                    fourthOctet = int.Parse(octets[3]);

                    AdjustOctets(ref thirdOctet, ref fourthOctet, ref secondOctet);
                    network = FormatIP(firstOctet, secondOctet, thirdOctet, fourthOctet);

                    IncrementOctet(ref fourthOctet, ref thirdOctet, ref secondOctet);
                    firstHost = FormatIP(firstOctet, secondOctet, thirdOctet, fourthOctet);

                    fourthOctet += totalHostsRequired;
                    AdjustLastHostOctets(ref thirdOctet, ref secondOctet, ref fourthOctet);
                    lastHost = FormatIP(firstOctet, secondOctet, thirdOctet, fourthOctet);

                    IncrementOctet(ref fourthOctet, ref thirdOctet, ref secondOctet);
                    broadcast = FormatIP(firstOctet, secondOctet, thirdOctet, fourthOctet);
                }
                else
                {
                    return CreateOverflowVLSMObject();
                }
            }
            else
            {
                return CreateErrorVLSMObject();
            }

            return new VlsmObject(network, vlsmMask, firstHost, lastHost, broadcast, requiredHosts);
        }

        private static void AdjustOctets(ref int thirdOctet, ref int fourthOctet, ref int secondOctet)
        {
            if (fourthOctet == 255)
            {
                thirdOctet++;
                fourthOctet = 0;
                if (thirdOctet == 256)
                {
                    thirdOctet = 0;
                    secondOctet++;
                }
            }
        }

        private static void IncrementOctet(ref int fourthOctet, ref int thirdOctet, ref int secondOctet)
        {
            if (fourthOctet == 255)
            {
                fourthOctet = 0;
                thirdOctet++;
                if (thirdOctet == 256)
                {
                    thirdOctet = 0;
                    secondOctet++;
                }
            }
            else
            {
                fourthOctet++;
            }
        }

        private static void AdjustLastHostOctets(ref int thirdOctet, ref int secondOctet, ref int fourthOctet)
        {
            if (fourthOctet >= 256)
            {
                int overflow = fourthOctet / 256;
                thirdOctet += overflow;
                fourthOctet %= 256;

                if (thirdOctet >= 256)
                {
                    secondOctet += thirdOctet / 256;
                    thirdOctet %= 256;
                }
            }
        }

        private static string FormatIP(int firstOctet, int secondOctet, int thirdOctet, int fourthOctet)
        {
            return $"{firstOctet}.{secondOctet}.{thirdOctet}.{fourthOctet}";
        }

        private static VlsmObject CreateOverflowVLSMObject()
        {
            return new VlsmObject("Overflow", 0, "Overflow", "Overflow", "Overflow", 0);
        }

        private static VlsmObject CreateErrorVLSMObject()
        {
            return new VlsmObject("Error", 0, "Error", "Error", "Error", 0);
        }

        static int OverflowCheck(int[] ArrayHost, int Mask)
        {
            int HostsWithoutOverFlow = 0;
            int MainMask = 2;
            int HostOver = 0;

            for (int i = 0; i <= 30 - Mask; i++)
            {
                MainMask *= 2;
            }

            foreach (int item in ArrayHost)
            {
                if (MainMask <= HostOver)
                {
                    break;
                }

                int ManyHosts = 2;
                int MaskAddFromHosts = 0;

                while (ManyHosts - 2 < item)
                {
                    ManyHosts *= 2;
                    MaskAddFromHosts++;
                }

                HostOver += ManyHosts;
                HostsWithoutOverFlow++;
            }
            return HostsWithoutOverFlow;
        }
    }
}