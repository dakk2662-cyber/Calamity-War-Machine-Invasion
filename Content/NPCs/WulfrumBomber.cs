//WulfrumBomber

using CalamityAddon.Content.Items.Placeables.Banners;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using CalamityAddon.Content.Projectiles;
using CalamityAddon.Content.Gores.Wulfrum;
using Terraria.GameContent.Bestiary;
using Terraria.Localization;
using Terraria.ModLoader.Utilities;
using Terraria.GameContent.ItemDropRules;

namespace CalamityAddon.Content.NPCs
{
    public class WulfrumBomber : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 16;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 38;
            NPC.height = 50;
            NPC.damage = 5;
            NPC.defense = 6;
            NPC.lifeMax = 42;
            NPC.HitSound = new SoundStyle("CalamityAddon/Content/Sounds/WulfrumHit", 3);
            NPC.DeathSound = new SoundStyle("CalamityAddon/Content/Sounds/WulfrumDeath");
            NPC.value = Item.buyPrice(0, 0, 1, 20);
            NPC.knockBackResist = 0.3f;
            NPC.aiStyle = -1;
            NPC.noGravity = true;
            NPC.noTileCollide = true;

            Banner = Type;
            BannerItem = ModContent.ItemType<WulfrumBomberBanner>();
            ItemID.Sets.KillsToBanner[BannerItem] = 50;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,
                new FlavorTextBestiaryInfoElement(Language.GetTextValue("Mods.CalamityAddon.NPCs.WulfrumBomber.Bestiary"))
            });
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest();

            Player player = Main.player[NPC.target];
            bool supercharged = NPC.ai[3] > 0;

            if (supercharged)
            {
                NPC.ai[3]--;
                Lighting.AddLight(NPC.Center, 0.3f, 0.8f, 0.2f);
            }

            // ========== Movement ==========
            NPC.ai[0]++;
            float attackDelay = supercharged ? 150f : 210f;
            float hoverHeight = 170f;
            Vector2 targetPos;

            if (NPC.ai[0] < attackDelay)
            {
                float retreatSide = (NPC.Center.X < player.Center.X) ? -1f : 1f;
                targetPos = player.Center + new Vector2(retreatSide * 500f, -hoverHeight - 100f);
            }
            else
            {
                float horizontalOffset = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 1.2f) * 190f;
                targetPos = player.Center + new Vector2(horizontalOffset, -hoverHeight);
            }

            float speed = supercharged ? 7f : 5f;
            float inertia = supercharged ? 20f : 35f;
            int bombDamage = supercharged ? 16 : 10;

            Vector2 desiredVelocity = (targetPos - NPC.Center).SafeNormalize(Vector2.Zero) * speed;
            NPC.velocity = (NPC.velocity * (inertia - 1) + desiredVelocity) / inertia;

            NPC.spriteDirection = (NPC.velocity.X > 0) ? 1 : -1;
            NPC.rotation = NPC.velocity.X * 0.05f;

            // ========== Attack ==========
            if (NPC.ai[0] >= attackDelay && Math.Abs(NPC.Center.X - player.Center.X) < 30f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    DropBomb(bombDamage);

                    if (supercharged)
                    {
                        NPC.ai[1] = 25f;
                    }
                }
                NPC.ai[0] = 0;
                NPC.netUpdate = true;
            }

            if (NPC.ai[1] > 0)
            {
                NPC.ai[1]--;
                if (NPC.ai[1] == 1)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        DropBomb(bombDamage);
                }
            }
        }

        private void DropBomb(int damage)
        {
            int projectileType = ModContent.ProjectileType<WulfrumBigis>();

            Vector2 bombVel = new Vector2(NPC.velocity.X * 0.2f, 3f);
            Vector2 spawnPos = new Vector2(NPC.Center.X, NPC.position.Y + NPC.height);
            Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, bombVel, projectileType, damage, 0f, Main.myPlayer);

            SoundEngine.PlaySound(SoundID.Item10, NPC.Center);
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            bool supercharged = NPC.ai[3] > 0;

            int minFrame = supercharged ? 8 : 0;
            int maxFrame = supercharged ? 16 : 8;

            if (NPC.frameCounter >= 6)
            {
                NPC.frameCounter = 0;
                int currentFrame = NPC.frame.Y / frameHeight;

                if (currentFrame < minFrame || currentFrame >= maxFrame)
                {
                    currentFrame = minFrame;
                }
                else
                {
                    currentFrame++;
                    if (currentFrame >= maxFrame)
                        currentFrame = minFrame;
                }

                NPC.frame.Y = currentFrame * frameHeight;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (Main.dedServ) return;

            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GrassBlades, hit.HitDirection, -1f, 0, default, 1f);
            }

            if (NPC.life <= 0)
            {
                for (int k = 0; k < 20; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GrassBlades, hit.HitDirection, -1f, 0, default, 1.5f);
                }

                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.GoreType<BomberGore1>(), 1f);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.GoreType<BomberGore2>(), 1f);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.GoreType<BomberGore3>(), 1f);

                int randomGoreCount = Main.rand.Next(2, 4);
                for (int i = 0; i < randomGoreCount; i++)
                {
                    int index = Main.rand.Next(1, 9);

                    if (ModContent.TryFind<ModGore>("CalamityMod", "WulfrumEnemyGore" + index, out ModGore calGore))
                    {
                        Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity * 0.5f, calGore.Type, 1f);
                    }
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            if (ModContent.TryFind("CalamityMod", "WulfrumMetalScrap", out ModItem wulfrumMetalScrap)) {
                npcLoot.Add(ItemDropRule.Common(wulfrumMetalScrap.Type, 1, 2, 3));
            if (ModContent.TryFind("CalamityMod", "EnergyCore", out ModItem energyCore)) {
                npcLoot.Add(ItemDropRule.ByCondition(new SuperchargedCondition(), energyCore.Type, 1));
                }
            }
        }
    }
}