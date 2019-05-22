using System;
using Lib_K_Relay;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Client;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Utilities;
using static System.Threading.Thread;
using static Lib_K_Relay.Utilities.PluginUtils;

namespace ProxyUtilites
{
    public class SimpleHelperUtility : IPlugin
    {
        #region Bin
        Random num = new Random();

        private readonly string[] ignorelist = { "net", ".com", "org", "me", "gg", "maxing", "delivery", "maxing", "RPG", "io", "auto", "^^", "cheap", "deliver", "STS", "com", "hacked", "client" };

        private readonly UpdatePacket updatePacket;

        private readonly TeleportPacket tp= Packet.Create(PacketType.TELEPORT) as TeleportPacket;

        private BuyPacket Buy = Packet.Create(PacketType.BUY) as BuyPacket;

        private BuyResultPacket buyResult = Packet.Create(PacketType.BUYRESULT) as BuyResultPacket;

        private CreateGuildPacket createGuild = Packet.Create(PacketType.CREATEGUILD) as CreateGuildPacket;

        private CreateGuildResultPacket createGuildResult = Packet.Create(PacketType.CREATEGUILDRESULT) as CreateGuildResultPacket;

        private ChangeGuildRankPacket changeRank = Packet.Create(PacketType.CHANGEGUILDRANK) as ChangeGuildRankPacket;

        private TextPacket textPacket = Packet.Create(PacketType.TEXT) as TextPacket;

        private EscapePacket Nexus = Packet.Create(PacketType.ESCAPE) as EscapePacket;

        private HelloPacket helloPacket = Packet.Create(PacketType.HELLO) as HelloPacket;

        private UsePortalPacket usePortalPacket = (UsePortalPacket)Packet.Create(PacketType.USEPORTAL);

        private int playerHp;
        #endregion


        public string GetAuthor()
        {
            return "CD";
        }

        public string GetName()
        {
            return "SimpleHelperUtility";
        }

        public string GetDescription()
        {
            return "A utility beyond the generic stuff out there. ";
        }


        public string[] GetCommands()
        {
            return new[]
            {
                "/givecompletion", "/selftp playerHP", "/spam msg", "/buy objectid amount", "/newguild name",
                "/relay ip", "/invisible guildmatename"
            };
        }

        public void Initialize(Proxy proxy)
        {
            proxy.HookCommand("givecompletion", Givecompletions);
            proxy.HookCommand("selftp", OnLowHp);
            proxy.HookCommand("spam", SendMsg);
            proxy.HookCommand("buy", Attemptbuy);
            proxy.HookCommand("newguild", CreateG);
            proxy.HookCommand("invisible", ChangeGRank);
            proxy.HookCommand("login", MuleLogin);

            proxy.HookPacket(PacketType.UPDATE, Update);
            proxy.HookPacket(PacketType.TEXT, OnText);
        }

        private void Update(Client client, Packet packet)
        {
            //
        }

        /// <summary>
        ///     Attempts to Self TP at which percentage
        ///     Not Tested
        /// </summary>
        private void OnLowHp(Client Client, string command, string[] args)
        {
            var hptoselfat = int.Parse(args[0]);
            hptoselfat = playerHp;
            if (int.TryParse(args[0], out playerHp)) CreateOryxNotification("SelfTP", $"set to tp at {playerHp} hp");

            var update = updatePacket;
            foreach (var obj in update.NewObjs)
            {
                var playerData = new PlayerData(obj.Status.ObjectId);

                playerData.Class = (Classes) obj.ObjectType;

                if (Client.PlayerData.Health != playerHp)

                {
                    tp.ObjectId = Convert.ToInt32(Client.PlayerData.Name);
                    Client.SendToClient(tp);
                }
            }
        }

        /// <summary>
        ///     Attempts to Buy
        ///     Not Tested
        /// </summary>
        private void Attemptbuy(Client Client, string command, string[] args)
        {
                var id = int.Parse(args[0]);
                var amount = int.Parse(args[1]);
                Buy.ObjectId = id;
                Buy.Quantity = amount;
                Client.SendToServer(Buy);
                Client.SendToServer(buyResult);
                switch (buyResult.Result)
                {
                    case 0:
                        CreateOryxNotification("AUTOBUY", "Success!");
                        break;
                    case 2:
                        CreateOryxNotification("AUTOBUY", "Item not found.");
                        break;
                    case 3:
                        CreateOryxNotification("AUTOBUY", "Not enough gold");
                        break;
                    case 4:
                        CreateOryxNotification("AUTOBUY", "Your inventory is full!");
                        break;
                    case 5:
                        CreateOryxNotification("AUTOBUY", "Rank up!");
                        break;
                    case 6:
                        CreateOryxNotification("AUTOBUY", "Not enough fame!");
                        break;
                    }
        }

        /// <summary>
        ///     Attempts to Purchase guild
        ///     Not Tested
        /// </summary>
        private void CreateG(Client Client, string command, string[] args)
        {
            var n = args[0];

            if (!string.IsNullOrEmpty(n))
            {
                CreateOryxNotification("GuildCreator", "Enter a guild name!");
            }
            else
            {
                CreateOryxNotification("GuildCreator", "You must ever a guild name.");
            }
            createGuild.Name = n;
            Client.SendToServer(createGuild);
            CreateOryxNotification("GuildCreator", createGuildResult.Success ? "GZ on the new guild!" : createGuildResult.ErrorText);
        }


        /// <summary>
        ///     *Invisible* Rank Exploit
        /// </summary>
        private void ChangeGRank(Client Client, string command, string[] args)

        {
            if (args.Length != 0)
            {
                int pick = num.Next(20, 30);
                var guildmate = args[0];
                changeRank.Name = guildmate;
                changeRank.GuildRank = pick;
            }
            else
            { CreateOryxNotification(" ", "Enter a player name!");}}

        /// <summary>
        ///    Spam detection
        ///    Add Regex
        /// </summary>
        private void OnText(Client client, Packet p)
        {
            TextPacket text = p as TextPacket;
            foreach (string name in ignorelist)
            {
                if (text.Name.Contains(name)||text.NumStars<12)
                {
                    text.Send = false;
                    return;
                }
            }
        }

        /// <summary>
        ///    Simple Text Sender
        ///    Add AFK trade bot functionality
        /// </summary>
        private void SendMsg(Client Client, string command, string[] args)
        {
            const int delay = 1200;
            var msg = args[0];
            while (true)
            {
                Sleep(delay);
                textPacket.Name = Client.PlayerData.Name;
                textPacket.Text = msg;
                textPacket.NumStars = -1;
                textPacket.Send = true;
                Client.SendToServer(textPacket);
            }
        }
        /// <summary>
        ///     Swaps Items from backpack
        ///     Incomplete! 
        /// </summary>
        private void Swap(Client client, string command, string[] args)
        {
            if (args.Length == 0)
            {
                var swapPacket = (InvSwapPacket) Packet.Create(PacketType.INVSWAP);
                swapPacket.Position = new LocationRecord();
                swapPacket.Time = -1;
                swapPacket.SlotObject1 = new SlotObject();
                client.SendToClient(swapPacket);
            }
        }

        /// <summary>
        ///     Attempts to connect on account in the background
        ///     Not Tested, needs wk
        /// </summary>
        private void MuleLogin(Client client, string command, string[] args)
        {
            helloPacket.GUID = "name@example.com";//
            helloPacket.Password = "password123";//
            helloPacket.BuildVersion = "X31.7.0";
            // calculate randoms
            helloPacket.GameNet = "rotmg";
            helloPacket.GameId = -5;
            helloPacket.Secret = "";
            helloPacket.Key=new byte[0];
            helloPacket.KeyTime = -1;
            helloPacket.MapJSON = "";
            helloPacket.EntryTag = "";
            helloPacket.PlayPlatform = "rotmg";
            helloPacket.PlatformToken = "";
            helloPacket.UserToken = "";
            client.SendToServer(helloPacket);
            client.SendToServer(Nexus);
        }
        /// <summary>
        ///     You must be on a portal position
        ///     Portal completion exploit
        ///     Not Tested
        /// </summary>
        protected void Givecompletions(Client client, string command, string[] args)
        {
            if (args.Length != 0)
                CreateOryxNotification("Portal Doer", "Stand on the portal and enter ID");
            else
            {
                for (var index = 10 + 1; index <= 0; index--)
                    try
                    {
                        var id = int.Parse(args[0]);
                        usePortalPacket.ObjectId = id;
                        client.SendToServer(usePortalPacket);
                        client.SendToClient(
                            CreateOryxNotification("Portal Doer", index.ToString()));
                    }
                    catch (Exception)
                    {
                    }
            }
        }
    }
}