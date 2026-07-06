using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityAddon.Content.NPCs;
using CalamityAddon.Content.NPCs.WulfrumJumper;

namespace CalamityAddon.Content.Events
{
    internal class WulfrumRushGlobalNPC : GlobalNPC
    {
        // Список базовых мобов (из Calamity)
        public static int[] BaseInvasionMobs => new int[] {
            ModContent.NPCType<CalamityMod.NPCs.NormalNPCs.WulfrumDrone>(),
            ModContent.NPCType<CalamityMod.NPCs.NormalNPCs.WulfrumHovercraft>(),
            ModContent.NPCType<CalamityMod.NPCs.NormalNPCs.WulfrumRover>(),
            ModContent.NPCType<CalamityMod.NPCs.NormalNPCs.WulfrumGyrator>()
        };

        // Метод проверки: является ли моб частью нашего нашествия
        private bool IsInvasionMob(int type)
        {
            return BaseInvasionMobs.Contains(type) ||
                   type == ModContent.NPCType<WulfrumTank>() ||
                   type == ModContent.NPCType<WulfrumBomber>();
        }

        public override void OnKill(NPC npc)
        {
            // Считает только сервер или хост в одиночной игре
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            if (WulfrumRush.isInvasionActive)
            {
                if (IsInvasionMob(npc.type) && !WulfrumRush.Mob2Spawned)
                {
                    WulfrumRush.invasionKills++;

                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendData(MessageID.WorldData);
                }

                else if (WulfrumRush.Mob2Spawned && npc.type == ModContent.NPCType<WulfrumJumperP2>())
                {
                    WulfrumRush.invasionKills = WulfrumRush.invasionMaxProgress;

                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendData(MessageID.WorldData);
                }
            }
        }

        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            if (WulfrumRush.isInvasionActive)
            {
                if (WulfrumRush.Mob2Spawned)
                {
                    pool.Clear();
                    return;
                }

                pool.Clear();

                foreach (int mobID in BaseInvasionMobs)
                {
                    pool.Add(mobID, 1f);
                }

                if (WulfrumRush.invasionKills > 50)
                {
                    pool.Add(ModContent.NPCType<WulfrumBomber>(), 0.6f);
                }

                if (WulfrumRush.invasionKills > 100)
                {
                    pool.Add(ModContent.NPCType<WulfrumTank>(), 0.5f);
                }
            }
        }

        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (WulfrumRush.isInvasionActive)
            {
                if (WulfrumRush.Mob2Spawned)
                {
                    spawnRate = 0;
                    maxSpawns = 0;
                    return;
                }

                spawnRate = 45;
                maxSpawns = 30;
            }
        }
    }
}