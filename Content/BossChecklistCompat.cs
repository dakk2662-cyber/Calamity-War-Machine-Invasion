using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria.Localization;
using CalamityAddon.Content.NPCs.WulfrumMothership;
using CalamityAddon.Content.NPCs.WulfrumJumper;
using CalamityAddon.Content.Items.Summons;
using CalamityAddon.Content.Items.TreasureBags;
using CalamityAddon.Content.Items.Placeables.Furniture.BossRelics;
using CalamityAddon.Content.Items.Placeables.Furniture.Trophies;
using CalamityAddon.Content.Items.Weapons;
using CalamityAddon.Content.Items.Accessories;
using System.Collections.Generic;
using CalamityAddon.Content.Events;
using Terraria;
using System;

namespace CalamityAddon.Content
{
    public class BossChecklistCompat : ModSystem
    {
        public override void PostSetupContent()
        {
            if (!ModLoader.TryGetMod("BossChecklist", out Mod bossChecklistMod))
                return;

            // Wulfrum Mothership reg
            bossChecklistMod.Call(
                "LogBoss",
                Mod,
                nameof(WulfrumMothership),
                1.6f,
                () => DownedBossSystem.downedWulfrumMothership,
                ModContent.NPCType<WulfrumMothership>(),
                new Dictionary<string, object>()
                {
                    ["spawnInfo"] = Language.GetOrRegister("Mods.CalamityAddon.NPCs.WulfrumMothership.SpawnInfo"),
                    ["despawnMessage"] = Language.GetOrRegister("Mods.CalamityAddon.NPCs.WulfrumMothership.DespawnMessage"),
                    ["spawnItems"] = ModContent.ItemType<WulfrumHeart>(),
                    ["treasureBag"] = ModContent.ItemType<WulfrumMothershipBag>(),
                    ["availability"] = () => true,
                    ["collectibles"] = new List<int>()
                    {
                        ModContent.ItemType<WulfrumMothershipRelic>(),
                        ModContent.ItemType<Items.LoreItems.LoreWulfrumMothership>()
                    }
                }
            );

            // Wulfrum Invasion reg
            List<int> eventNPCs = new List<int> {
                ModContent.NPCType<NPCs.WulfrumTank>(),
                ModContent.NPCType<NPCs.WulfrumBomber>(),
                ModContent.NPCType<WulfrumJumperP2>()
            };

            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                string[] calamityWulfrumMobs = {
                    "WulfrumDrone",
                    "WulfrumHovercraft",
                    "WulfrumRover",
                    "WulfrumGyrator"
                };

                foreach (string name in calamityWulfrumMobs)
                {
                    if (calamity.TryFind<ModNPC>(name, out ModNPC mob))
                    {
                        eventNPCs.Add(mob.Type);
                    }
                }
            }

            bossChecklistMod.Call(
                "LogEvent",
                Mod,
                "WulfrumRush",
                1.6f,
                () => DownedBossSystem.downedWulfrumRush,
                eventNPCs,
                new Dictionary<string, object>()
                {
                    ["displayName"] = Language.GetOrRegister("Mods.CalamityAddon.Events.WulfrumRush.InvasionName"),
                    ["spawnInfo"] = Language.GetOrRegister("Mods.CalamityAddon.Events.WulfrumRush.SpawnInfo"),
                    ["spawnItems"] = ModContent.ItemType<UnstableBattery>(),
                    ["customPortrait"] = (Action<SpriteBatch, Rectangle, Color>)((spriteBatch, rect, color) => {
                        Texture2D texture = ModContent.Request<Texture2D>("CalamityAddon/Content/Events/WulfrumInvasion_Portrait").Value;
                        float scale = Math.Min((float)rect.Width / texture.Width, (float)rect.Height / texture.Height);
                        Vector2 position = new Vector2(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
                        Vector2 origin = texture.Size() / 2f;
                        spriteBatch.Draw(texture, position, null, color, 0f, origin, scale, SpriteEffects.None, 0f);
                    }),
                    ["overrideHeadTextures"] = "CalamityAddon/Content/Events/WulfrumRush_Icon",
                    ["availability"] = () => true,
                    ["collectibles"] = new List<int>()
                    {
                        ModContent.ItemType<WulfrumJumperTrophy>(),
                        ModContent.ItemType<WulfrumJavelin>(),
                        ModContent.ItemType<WDAS>(),
                        ModContent.ItemType<Items.LoreItems.LoreWulfrumInvasion>()
                    }
                }
            );
        }
    }
}