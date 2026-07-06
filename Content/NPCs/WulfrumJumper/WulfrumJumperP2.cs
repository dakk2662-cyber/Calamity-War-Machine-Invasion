using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.Bestiary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.Localization;
using Terraria.GameContent.ItemDropRules;
using System;
using System.Collections.Generic;
using ReLogic.Content;
using CalamityAddon.Content.Gores.Wulfrum;
using CalamityAddon.Content.Projectiles;
using CalamityAddon.Content.Utilities;
using CalamityAddon.Content.Items.Accessories;
using CalamityAddon.Content.Items.Weapons;
using CalamityAddon.Content.Items.Ammo;
using CalamityAddon.Content.Items.Placeables.Furniture.BossRelics;
using CalamityAddon.Content.Items.Placeables.Furniture.Trophies;
using CalamityAddon.Content.Items.LoreItems;

namespace CalamityAddon.Content.NPCs.WulfrumJumper
{
    [AutoloadBossHead]
    public class WulfrumJumperP2 : ModNPC
    {
        private int springFrame = 0;
        private int animTimer = 0;
        private static Asset<Texture2D> springTex;

        private int ChargeRadius = 0;
        private const int ChargeRadiusMax = 400;
        private const int SuperchargeTime = 600;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 10;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Electrified] = false;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;

            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                CustomTexturePath = "CalamityAddon/Content/NPCs/WulfrumJumper/WulfrumJumper_Bestiary",
                Position = new Vector2(40f, 24f),
                PortraitPositionXOverride = 0f,
                PortraitPositionYOverride = 0f
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
        }

        public override void SetDefaults()
        {
            NPC.width = 76;
            NPC.height = 58;
            NPC.damage = 35;
            NPC.defense = 12;
            NPC.lifeMax = 1300;
            NPC.boss = true;
            NPC.aiStyle = -1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.HitSound = new SoundStyle("CalamityAddon/Content/Sounds/WulfrumHit", 3);
            NPC.DeathSound = new SoundStyle("CalamityAddon/Content/Sounds/WulfrumDeath");
            NPC.value = Item.buyPrice(0, 1, 5, 0);
            Music = 0;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,
                new FlavorTextBestiaryInfoElement(Language.GetTextValue("Mods.CalamityAddon.NPCs.WulfrumJumperP2.Bestiary"))
            });
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter >= 6)
            {
                NPC.frameCounter = 0;
                if (NPC.ai[0] != 2)
                {
                    NPC.frame.Y += frameHeight;
                    if (NPC.frame.Y >= frameHeight * 6) NPC.frame.Y = 0;
                }
                else
                {
                    if (NPC.frame.Y < frameHeight * 6) NPC.frame.Y = frameHeight * 6;
                    NPC.frame.Y += frameHeight;
                    if (NPC.frame.Y >= frameHeight * 10) NPC.frame.Y = frameHeight * 6;
                }
            }
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest();

            Player player = Main.player[NPC.target];

            if (player.dead || !player.active)
            {
                if (NPC.localAI[3] == 0f)
                {
                    NPC.velocity.Y = -24f;
                    NPC.velocity.X = (NPC.Center.X < player.Center.X ? -12f : 12f);
                    NPC.noTileCollide = true;
                    NPC.netUpdate = true;
                    NPC.localAI[3] = 1f;
                }

                NPC.velocity.Y -= 0.2f;

                NPC.EncourageDespawn(10);

                NPC.spriteDirection = (NPC.velocity.X > 0) ? -1 : 1;
                NPC.rotation = NPC.velocity.X * 0.05f;
                return;
            }
            else
            {
                NPC.localAI[3] = 0f;
            }

            bool onGround = NPC.velocity.Y == 0;

            UpdateSuperchargeField();

            if (NPC.ai[0] == 0) // ФАЗА ПРЫЖКОВ
            {
                if (onGround)
                {
                    if (NPC.localAI[0] == 1) // Приземление после высокого прыжка
                    {
                        NPC.TargetClosest(true);
                        NPC.spriteDirection = (player.Center.X < NPC.Center.X) ? 1 : -1;

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int damageWS = 15;
                            Vector2 wavePos = new Vector2(NPC.Center.X, NPC.position.Y + NPC.height - 10);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), wavePos, Vector2.Zero, ModContent.ProjectileType<WulfrumShockwave>(), damageWS, 5f, Main.myPlayer);
                        }
                        NPC.ai[0] = 1; // Переход в фазу ракет
                        NPC.ai[1] = 0; NPC.ai[3] = 0; NPC.localAI[0] = 0;
                        NPC.netUpdate = true;
                        return;
                    }

                    NPC.velocity.X *= 0.8f;
                    NPC.ai[1]++;
                    springFrame = 0;

                    if (NPC.ai[1] >= 45)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[2]++;
                        NPC.TargetClosest(true);

                        if (NPC.ai[2] >= 3)
                        {
                            NPC.velocity.Y = -14f;
                            NPC.velocity.X = MathHelper.Clamp((player.Center.X - NPC.Center.X) * 0.045f, -11f, 11f);
                            NPC.localAI[0] = 1;
                            NPC.ai[2] = 0;
                            SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                        }
                        else
                        {
                            NPC.velocity.Y = -10f;
                            NPC.velocity.X = 7.5f * NPC.direction;
                            NPC.localAI[0] = 0;
                            SoundEngine.PlaySound(SoundID.Item1, NPC.Center);
                        }
                        NPC.netUpdate = true;
                    }
                }
                else { UpdateSpringAnimation(); }
            }
            else if (NPC.ai[0] == 1) // РАКЕТЫ
            {
                NPC.velocity.X *= 0.7f;
                springFrame = 0;
                NPC.ai[1]++;

                if (NPC.ai[1] % 20 == 0 && NPC.ai[3] < 6)
                {
                    ShootHomingRocket();
                    NPC.ai[3]++;
                    NPC.netUpdate = true;
                }

                if (NPC.ai[3] >= 6 && NPC.ai[1] >= 180)
                {
                    NPC.ai[0] = 2;
                    NPC.ai[1] = 0;
                    NPC.ai[3] = 0;
                    NPC.netUpdate = true;
                }
            }
            else if (NPC.ai[0] == 2) // СПАВН МОБОВ
            {
                NPC.velocity.X *= 0.7f;
                springFrame = 0;
                NPC.ai[1]++;

                if (NPC.ai[1] == 40 || NPC.ai[1] == 80)
                {
                    SpawnInvasionMob();
                    NPC.ai[3]++;
                }

                if (NPC.ai[3] >= 2 && NPC.ai[1] >= 140)
                {
                    NPC.ai[0] = 0; NPC.ai[1] = 0; NPC.ai[3] = 0; NPC.frame.Y = 0;
                    NPC.netUpdate = true;
                }
            }
        }

        private void UpdateSuperchargeField()
        {
            // Плавно расширяем радиус до максимума
            ChargeRadius = (int)MathHelper.Lerp(ChargeRadius, ChargeRadiusMax, 0.05f);

            // Визуализация кольца (как у Mothership)
            if (!Main.dedServ && Main.rand.NextBool(4))
            {
                float dustCount = MathHelper.TwoPi * ChargeRadius / 10f;
                for (int i = 0; i < dustCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / dustCount;
                    Dust dust = Dust.NewDustPerfect(NPC.Center, DustID.Vortex);
                    dust.position = NPC.Center + angle.ToRotationVector2() * ChargeRadius;
                    dust.scale = 0.6f;
                    dust.noGravity = true;
                    dust.velocity = NPC.velocity;
                }
            }

            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            // Логика заряда окружающих мобов
            foreach (var npc in Main.ActiveNPCs)
            {
                if (npc.whoAmI == NPC.whoAmI) continue;
                if (NPC.Distance(npc.Center) > ChargeRadius) continue;
                List<int> wulfrumTypes = GetWulfrumNPCTypes();
                if (npc.ai[3] > 0f) continue; // Если уже заряжен

                npc.ai[3] = SuperchargeTime;
                npc.netUpdate = true;

                // Вспышка на заряженном мобе
                if (!Main.dedServ)
                {
                    for (int j = 0; j < 10; j++)
                        Dust.NewDust(npc.position, npc.width, npc.height, DustID.Electric);
                }
            }
        }

        private List<int> GetWulfrumNPCTypes()
        {
            List<int> types = new List<int>();

            types.Add(ModContent.NPCType<WulfrumTank>());
            types.Add(ModContent.NPCType<WulfrumBomber>());
            types.Add(ModContent.NPCType<WulfrumWormHead>());

            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                string[] names = { "WulfrumDrone", "WulfrumHovercraft", "WulfrumRover", "WulfrumGyrator" };
                foreach (string name in names)
                {
                    if (calamity.TryFind<ModNPC>(name, out ModNPC modNpc))
                        types.Add(modNpc.Type);
                }
            }
            return types;
        }

        private void UpdateSpringAnimation()
        {
            int maxFrame = (NPC.localAI[0] == 1) ? 4 : 2;
            if (springFrame < maxFrame)
            {
                animTimer++;
                if (animTimer >= 4) { animTimer = 0; springFrame++; }
            }
        }

        private void ShootHomingRocket()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            Vector2[] rocketOffsets = new Vector2[]
            {
            new Vector2(-10f, -30f),
            new Vector2(-18f, -30f),
            new Vector2(-24f, -30f),
            new Vector2(-6f, -22f),
            new Vector2(-14f, -22f),
            new Vector2(-20f, -22f)
            };

            int index = (int)NPC.ai[3] % 6;
            Vector2 currentOffset = rocketOffsets[index];

            Vector2 finalOffset = new Vector2(currentOffset.X * -NPC.spriteDirection, currentOffset.Y);
            Vector2 spawnPosition = NPC.Center + finalOffset;

            float launchSpeedX = 8f;
            float launchSpeedY = 9f;

            Vector2 velocity = new Vector2(NPC.direction * launchSpeedX, -launchSpeedY);
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(15f));
            velocity *= Main.rand.NextFloat(0.9f, 1.1f);

            Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPosition, velocity, ModContent.ProjectileType<WulfrumRocket>(), 20, 2f, Main.myPlayer);

            SoundEngine.PlaySound(SoundID.Item11, spawnPosition);
        }

        private void SpawnInvasionMob()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            List<int> possibleMobs = new List<int>();
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                if (calamity.TryFind<ModNPC>("WulfrumHovercraft", out ModNPC hovercraft)) possibleMobs.Add(hovercraft.Type);
                if (calamity.TryFind<ModNPC>("WulfrumDrone", out ModNPC drone)) possibleMobs.Add(drone.Type);
            }

            if (possibleMobs.Count == 0) return;
            int selectedType = possibleMobs[Main.rand.Next(possibleMobs.Count)];

            float localX = 40f;
            Vector2 spawnPos = NPC.Center + new Vector2(localX * NPC.spriteDirection, 10f);

            int index = NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPos.X, (int)spawnPos.Y, selectedType);

            if (index != Main.maxNPCs && Main.npc[index].active)
            {
                Main.npc[index].velocity = new Vector2(5f * -NPC.direction, -4f);

                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, index);

                if (!Main.dedServ)
                {
                    for (int i = 0; i < 15; i++)
                    {
                        Dust d = Dust.NewDustDirect(Main.npc[index].position, Main.npc[index].width, Main.npc[index].height, DustID.Smoke, 0f, 0f, 100, default, 1.3f);
                        d.velocity *= 0.5f;
                        d.noGravity = true;
                    }
                }
            }
            SoundEngine.PlaySound(SoundID.Item113, NPC.Center);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (springTex == null) springTex = ModContent.Request<Texture2D>(Texture + "Spring");
            Texture2D sTex = springTex.Value;
            Texture2D hTex = Terraria.GameContent.TextureAssets.Npc[NPC.type].Value;

            Vector2 drawPos = new Vector2((int)(NPC.Center.X - screenPos.X), (int)(NPC.position.Y + NPC.height - screenPos.Y));
            SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            // Пружина
            int sFrameHeight = sTex.Height / 5;
            Rectangle sRect = new Rectangle(0, sFrameHeight * springFrame, sTex.Width, sFrameHeight);
            spriteBatch.Draw(sTex, drawPos, sRect, drawColor, NPC.rotation, new Vector2(sTex.Width / 2f, sFrameHeight), NPC.scale, effects, 0f);

            // Корпус
            float hullVisualOffset = 20f + (springFrame * 8f);
            int hFrameHeight = hTex.Height / 10;
            spriteBatch.Draw(hTex, new Vector2(drawPos.X, drawPos.Y - (int)hullVisualOffset), NPC.frame, drawColor, NPC.rotation, new Vector2(hTex.Width / 2f, hFrameHeight), NPC.scale, effects, 0f);

            return false;
        }

        public override bool? CanFallThroughPlatforms()
        {
            Player player = Main.player[NPC.target];
            return player.active && !player.dead && player.Center.Y > NPC.position.Y + NPC.height;
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

                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.GoreType<JGore1>(), 1f);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.GoreType<JGore2>(), 1f);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.GoreType<JGore3>(), 1f);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.GoreType<JGore4>(), 1f);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.GoreType<JGore5>(), 1f);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.GoreType<JGore6>(), 1f);

                int randomGoreCount = Main.rand.Next(4, 8);
                for (int i = 0; i < randomGoreCount; i++)
                {
                    int index = Main.rand.Next(1, 11);

                    if (ModContent.TryFind<ModGore>("CalamityMod", "WulfrumEnemyGore" + index, out ModGore calGore))
                    {
                        Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity * 0.5f, calGore.Type, 1f);
                    }
                }
            }
        }

        public override void OnKill()
        {
            DownedBossSystem.downedWulfrumJumper = true;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());

            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<WulfrumJavelin>()));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<WDAS>()));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<WulfrumLRocket>(), 1, 25, 40));

            if (ModContent.TryFind("CalamityMod", "WulfrumMetalScrap", out ModItem wulfrumMetalScrap))
                npcLoot.Add(ItemDropRule.Common(wulfrumMetalScrap.Type, 1, 20, 30));

            if (ModContent.TryFind("CalamityMod", "EnergyCore", out ModItem energyCore))
                npcLoot.Add(ItemDropRule.Common(energyCore.Type, 1, 2, 4));

            npcLoot.Add(ItemDropRule.ByCondition(new MasterOrRevengeanceCondition(), ModContent.ItemType<WulfrumJumperRelic>()));

            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<WulfrumJumperTrophy>(), 10));

            npcLoot.AddConditionalPerPlayer(
                () => !DownedBossSystem.downedWulfrumJumper,
                ModContent.ItemType<LoreWulfrumInvasion>(),
                desc: DropHelper.FirstKillText.Value
            );
        }
    }

    public class WulfrumJumperP2SpawnControl : GlobalNPC
    {
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (NPC.AnyNPCs(ModContent.NPCType<WulfrumJumperP2>()))
            {
                maxSpawns = 0;
                spawnRate = int.MaxValue;
            }
        }

        public override bool PreKill(NPC npc)
        {
            if (NPC.AnyNPCs(ModContent.NPCType<WulfrumJumperP2>()) && IsWulfrumNPC(npc))
            {
                npc.value = 0;
                return false;
            }
            return true;
        }

        private bool IsWulfrumNPC(NPC npc)
        {
            if (npc.type == ModContent.NPCType<WulfrumTank>())
                return true;
            if (npc.type == ModContent.NPCType<WulfrumBomber>())
                return true;
            if (npc.type == ModContent.NPCType<WulfrumWormHead>())
                return true;

            if (!ModLoader.TryGetMod("CalamityMod", out Mod calamity))
                return false;

            string[] names = { "WulfrumDrone", "WulfrumHovercraft", "WulfrumRover", "WulfrumGyrator" };
            foreach (string name in names)
            {
                if (calamity.TryFind<ModNPC>(name, out ModNPC modNpc) && npc.type == modNpc.Type)
                    return true;
            }
            return false;
        }
    }
}
