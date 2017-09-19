﻿using AuroraEmu.Game.Players;
using AuroraEmu.Network.Game.Packets;
using DotNetty.Transport.Channels;
using System.Collections.Generic;
using AuroraEmu.Game.Rooms;
using AuroraEmu.Game.Items;
using AuroraEmu.Game.Rooms.User;
using AuroraEmu.Game.Players.Components;
using DotNetty.Buffers;
using AuroraEmu.Game.Subscription;
using System;
using AuroraEmu.Network.Game.Packets.Composers.Users;
using AuroraEmu.Network.Game.Packets.Composers.Misc;

namespace AuroraEmu.Game.Clients
{
    public class Client : IDisposable
    {
        private readonly IChannel _channel;

        public Player Player { get; private set; }

        public int? RoomCount { get; set; }
        public int LoadingRoomId { get; set; }
        public int CurrentRoomId { get; set; }
        public UserActor UserActor { get; set; }
        public Room CurrentRoom { get; set; }

        public Dictionary<int, Item> Items { get; set; }
        public Dictionary<string, SubscriptionData> SubscriptionData { get; set; }

        public Client(IChannel channel)
        {
            _channel = channel;
            Items = new Dictionary<int, Item>();
            SubscriptionData = new Dictionary<string, SubscriptionData>();
        }

        public void Disconnect()
        {
            _channel.DisconnectAsync();
        }

        public void SendComposer(MessageComposer composer)
        {
            Send(composer, true);
        }

        public void QueueComposer(MessageComposer composer)
        {
            Send(composer, false);
        }

        public void Send(MessageComposer composer, bool flush)
        {
            Engine.Logger.Info(System.Text.Encoding.GetEncoding(0).GetString(composer.GetBytes()));
            if (flush)
            {
                _channel.WriteAndFlushAsync(Unpooled.CopiedBuffer(composer.GetBytes()));
            } else
            {
                _channel.WriteAsync(Unpooled.CopiedBuffer(composer.GetBytes()));
            }
        }

        public IChannel Flush()
        {
            return _channel.Flush();
        }

        public void Login(string sso)
        {
            Player = Engine.MainDI.PlayerController.GetPlayerBySSO(sso);

            if (Player != null)
            {
                SendComposer(new UserRightsMessageComposer());
                SendComposer(new MessageComposer(3));
                SendComposer(new HabboBroadcastMessageComposer($"Welcome {Player.Username} to Aurora BETA, enjoy your stay!"));

                Player.BadgesComponent = new BadgesComponent(Player.Id);
                Player.MessengerComponent = new MessengerComponent(Player);
                Engine.MainDI.SubscriptionController.GetSubscriptionData(SubscriptionData, Player.Id);
            }
            else
            {
                Disconnect();
            }
        }


        public void IncreaseCredits(int amount)
        {
            Player.Coins += amount;
            SendComposer(new CreditBalanceMessageComposer(Player.Coins));
        }

        public void DecreaseCredits(int amount)
        {
            Player.Coins -= amount;
            SendComposer(new CreditBalanceMessageComposer(Player.Coins));
        }

        public void IncreasePixels(int amount)
        {
            Player.Pixels += amount;
            SendComposer(new HabboActivityPointNotificationMessageComposer(Player.Pixels, 0));
        }


        public void DecreasePixels(int amount)
        {
            Player.Pixels -= amount;
            SendComposer(new HabboActivityPointNotificationMessageComposer(Player.Pixels, 0));
        }

        public void Dispose()
        {
            if (Player != null)
            {
                Player.BadgesComponent.Badges.Clear();
                Player.MessengerComponent.Friends.Clear();
                Player.MessengerComponent.Requests.Clear();
                SubscriptionData.Clear();
                Items.Clear();
                using (var dbClient = Engine.MainDI.ConnectionPool.PopConnection())
                {
                    dbClient.SetQuery("UPDATE players SET coins = @coins, pixels = @pixels WHERE id = @id");
                    dbClient.AddParameter("@coins", Player.Coins);
                    dbClient.AddParameter("@pixels", Player.Pixels);
                    dbClient.AddParameter("@id", Player.Id);
                    dbClient.Execute();
                }
            }
            GC.SuppressFinalize(this);
        }
    }
}
