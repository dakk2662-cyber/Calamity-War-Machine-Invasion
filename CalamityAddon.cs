using CalamityAddon.Content.Events;
using CalamityAddon.Content.Items.Summons;
using MonoMod.RuntimeDetour;
using System;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityAddon
{
    public class CalamityAddon : Mod
    {
        private static Hook rightClickHook;
        private static Func<Func<object, int, int, bool>, object, int, int, bool> ourDetour;

        // Идентификаторы сетевых сообщений
        internal enum MessageType : byte
        {
            RequestStartInvasion
        }

        public override void Load()
        {
            // Пытаемся найти тип тайла приманки из Calamity
            Type targetTileType = Type.GetType("CalamityMod.Tiles.Furniture.WulfrumLure, CalamityMod");
            if (targetTileType != null)
            {
                MethodInfo targetMethod = targetTileType.GetMethod("RightClick", new Type[] { typeof(int), typeof(int) });
                if (targetMethod != null)
                {
                    ourDetour = RightClick_Detour;
                    rightClickHook = new Hook(targetMethod, ourDetour);
                }
            }
        }

        public override void Unload()
        {
            rightClickHook?.Dispose();
            rightClickHook = null;
            ourDetour = null;
        }

        private static bool RightClick_Detour(Func<object, int, int, bool> orig, object self, int i, int j)
        {
            Player player = Main.LocalPlayer;
            int batteryType = ModContent.ItemType<UnstableBattery>();

            // Если у игрока есть батарейка и нашествие не активно
            if (player.HasItem(batteryType) && !WulfrumRush.isInvasionActive)
            {
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    // В мультиплеере отправляем пакет-запрос на сервер
                    var packet = ModContent.GetInstance<CalamityAddon>().GetPacket();
                    packet.Write((byte)MessageType.RequestStartInvasion);
                    packet.Send();
                }
                else
                {
                    // В одиночной игре запускаем нашествие напрямую
                    WulfrumRush.StartInvasion();
                }
                return true; // Возвращаем true, чтобы не срабатывала стандартная логика приманки
            }

            return orig(self, i, j);
        }

        // Обработка сетевых пакетов
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            MessageType msgType = (MessageType)reader.ReadByte();

            if (msgType == MessageType.RequestStartInvasion)
            {
                // whoAmI в данном методе — это индекс игрока, который отправил пакет
                Player player = Main.player[whoAmI];
                int batteryType = ModContent.ItemType<UnstableBattery>();

                // Сервер проверяет: жив ли игрок, есть ли у него батарейка и не идет ли уже нашествие
                if (player.active && !player.dead && player.HasItem(batteryType) && !WulfrumRush.isInvasionActive)
                {
                    WulfrumRush.StartInvasion();
                }
            }
        }
    }
}