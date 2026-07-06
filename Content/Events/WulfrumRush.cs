using CalamityAddon.Content.NPCs.WulfrumJumper;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityAddon.Content.Events
{
    public class WulfrumRush : ModSystem
    {
        public static bool isInvasionActive = false;
        public static int invasionKills = 0;
        public static int invasionMaxProgress = 150;
        public const int CustomInvasionType = -67;

        internal static bool Mob1Spawned = false;
        internal static bool Mob2Spawned = false;

        public static float InvasionCompletionRatio => MathHelper.Clamp((float)invasionKills / invasionMaxProgress, 0f, 1f);

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(isInvasionActive);
            writer.Write(invasionKills);
            writer.Write(Mob1Spawned);
            writer.Write(Mob2Spawned);
        }

        public override void NetReceive(BinaryReader reader)
        {
            isInvasionActive = reader.ReadBoolean();
            invasionKills = reader.ReadInt32();
            Mob1Spawned = reader.ReadBoolean();
            Mob2Spawned = reader.ReadBoolean();
        }

        public override void PostUpdateInvasions()
        {
            if (isInvasionActive) UpdateInvasion();
        }

        public static void StartInvasion()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            isInvasionActive = true;
            invasionKills = 0;
            Mob1Spawned = false;
            Mob2Spawned = false;

            Main.invasionType = CustomInvasionType;
            Main.invasionSize = invasionMaxProgress;
            Main.invasionWarn = 600;

            Broadcast("You feel the scrap movement around you...", new Color(175, 75, 255));

            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.WorldData);
        }

        private void UpdateInvasion()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                if (Main.invasionProgressAlpha < 1f)
                    Main.invasionProgressAlpha += 0.05f;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                bool boss2Active = NPC.AnyNPCs(ModContent.NPCType<WulfrumJumperP2>());

                if ((Mob2Spawned) && !boss2Active)
                {
                    if (invasionKills < invasionMaxProgress)
                    {
                        AbortInvasion();
                        return;
                    }
                }

                if (invasionKills >= 75 && !Mob1Spawned)
                {
                    SpawnBoss(ModContent.NPCType<WulfrumJumperP1>());
                    Mob1Spawned = true;
                }

                if (invasionKills >= 149 && !Mob2Spawned)
                {
                    SpawnBoss(ModContent.NPCType<WulfrumJumperP2>());
                    Mob2Spawned = true;
                }

                if (invasionKills >= invasionMaxProgress)
                    EndInvasion();
            }
        }

        private static void SpawnBoss(int type)
        {
            int target = Player.FindClosest(Vector2.Zero, 0, 0);
            if (target != -1)
                NPC.SpawnOnPlayer(target, type);

            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.WorldData);
        }

        private void AbortInvasion()
        {
            Broadcast("Wulfrum machines destroy you...", new Color(255, 80, 80));

            isInvasionActive = false;
            Main.invasionType = 0;
            Mob1Spawned = false;
            Mob2Spawned = false;

            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.WorldData);
        }

        private void EndInvasion()
        {
            Broadcast("Wulfrum invasion has been repelled!", new Color(175, 75, 255));

            DownedBossSystem.downedWulfrumRush = true;
            isInvasionActive = false;
            Main.invasionType = 0;

            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.WorldData);
        }

        private static void Broadcast(string message, Color color)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), color);
            }
            else
            {
                Main.NewText(message, color);
            }
        }
    }
}