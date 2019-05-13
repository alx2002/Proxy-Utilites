using System;
using System.Threading;
using Lib_K_Relay;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Client;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Utilities;
using static Lib_K_Relay.Utilities.PluginUtils;

namespace PacketExperiment
{
    public class SimpleHelperUtility : IPlugin
    {
        private bool _enabled = true;

        private UpdatePacket packet;

        public int playerHp;


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
            {"/givecompletion", "/selftp playerHP", "/spam msg", "/buy objectid amount", "/newguild name", "/relay ip", "/invisible guildmatename" };
        }

        public void Initialize(Proxy proxy)
        {
            proxy.HookCommand("givecompletion", Givecompletions);
            proxy.HookCommand("selftp", OnLowHp);
            proxy.HookCommand("condition", SetCondition); //Patched
            proxy.HookCommand("spam", SendMsg);
            proxy.HookCommand("buy", Attemptbuy);
            proxy.HookCommand("newguild", CreateG);
            proxy.HookCommand("relay", Connect);
            proxy.HookCommand("invisible", ChangeGRank);
        }

        #region Patched

        private void SetCondition(Client client, string command, string[] args)
        {
            try
            {
                var meme = byte.Parse(args[0]);
                //if (args[0] == "darkness")
                {
                    var set = (SetConditionPacket) Packet.Create(PacketType.SETCONDITION);
                    set.ConditionEffect = meme;
                    set.ConditionDuration = 3;
                    client.SendToServer(set);
                    CreateOryxNotification("Condition Monitor", "Set Condition for " + args[0]);
                }
            }
            catch (Exception q)
            {
                Console.WriteLine(q);
            }
        }

        #endregion

        /// <summary>
        ///     Attempts to Self TP at which percentage
        ///     Not Tested
        /// </summary>
        private void OnLowHp(Client Client, string command, string[] args)
        {
            var hptoselfat = int.Parse(args[0]);
            hptoselfat = playerHp;
            if (int.TryParse(args[0], out playerHp)) CreateOryxNotification("SelfTP", $"set to{playerHp}%");

            var update = packet;
            foreach (var obj in update.NewObjs)
            {
                var playerData = new PlayerData(obj.Status.ObjectId);

                playerData.Class = (Classes) obj.ObjectType;

                if (Client.PlayerData.Health != playerHp)

                {
                    var tp = (TeleportPacket) Packet.Create(PacketType.TELEPORT);
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
            try
            {
                var id = int.Parse(args[0]);
                var amount = int.Parse(args[1]);
                var buy = (BuyPacket) Packet.Create(PacketType.BUY);
                buy.ObjectId = id;
                buy.Quantity = amount;
                Client.SendToServer(buy);
            }
            catch (Exception q)
            {
            }
        }

        /// <summary>
        ///     Attempts to Purchase guild
        ///     Not Tested
        /// </summary>
        private void CreateG(Client Client, string command, string[] args)
        {
            var n = args[0];

            if (string.IsNullOrEmpty(n)) CreateOryxNotification("GuildCreator", "Enter a guildmate name!");

            var guild = (CreateGuildPacket) Packet.Create(PacketType.CREATEGUILD);
            guild.Name = n;
            Client.SendToServer(guild);
        }

        /// <summary>
        ///     Give *Invisible* Rank Exploit
        /// </summary>
        private void ChangeGRank(Client Client, string command, string[] args)
        {
            var guildmate = args[0];
            var guild = (ChangeGuildRankPacket) Packet.Create(PacketType.CHANGEGUILDRANK);
            guild.Name = guildmate;
            guild.GuildRank = 23;
        }


        /// <summary>
        ///     Attempts to connect to an IP.
        ///     Not Tested, may need more parameters
        /// </summary>
        private void Connect(Client Client, string command, string[] args)
        {
            var ip = args[0]; //0.0.0.0
            var recon = (ReconnectPacket) Packet.Create(PacketType.RECONNECT);
            recon.Host = ip;
            Client.SendToServer(recon); //try client
        }

        /// <summary>
        ///    Simple Text Sender
        ///    Add AFK trade bot functionality
        /// </summary>
        private void SendMsg(Client Client, string command, string[] args)
        {
            while (true)
            {
                var delay = 1200;
                var msg = args[0];
                var q = (TextPacket) Packet.Create(PacketType.TEXT);
                Thread.Sleep(delay);
                q.Name = Client.PlayerData.Name;
                q.Text = msg;
                q.NumStars = -1;
                q.Send = true;
                //Client.SendToClient(q);
                Client.SendToServer(q);
            }
        }

        /// <summary>
        ///     You must be on a portal position
        ///     Portal completion exploit
        ///     Not Tested
        /// </summary>
        protected void Givecompletions(Client client, string command, string[] args)
        {
            if (args.Length == 0) return;
            if (args[0] == "enable") _enabled = true;

            for (var index = 0; index > 10; index++)
                try
                {
                    var id = int.Parse(args[0]);
                    var portal = (UsePortalPacket) Packet.Create(PacketType.USEPORTAL);
/*Do we need this?*/
                    portal.ObjectId = id; //Enable Id in client to find this.
                    client.SendToServer(portal);//Try Client
                    client.SendToClient(
                        CreateOryxNotification("PortalCompletion Given!", index.ToString()));
                }
                catch (Exception q)
                {
                }
        }
    }
}