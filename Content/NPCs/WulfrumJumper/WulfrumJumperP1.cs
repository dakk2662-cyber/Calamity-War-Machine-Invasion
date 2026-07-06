using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.Bestiary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.Localization;
using System;
using ReLogic.Content;
using CalamityAddon.Content.Projectiles;

namespace CalamityAddon.Content.NPCs.WulfrumJumper
{
    [AutoloadBossHead]
    public class WulfrumJumperP1 : ModNPC
    {
        private int springFrame = 0;
        private int animTimer = 0;
        private static Asset<Texture2D> springTex;

        private bool isDespawnJumping = false;
        private const float DespawnJumpStrength = -24f; // Очень высокий прыжок
        private const float DespawnHorizontalSpeed = 12f;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Electrified] = false;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Hide = true
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
        }

        public override void SetDefaults()
        {
            NPC.width = 76;
            NPC.height = 58;
            NPC.damage = 35;
            NPC.defense = 12;
            NPC.lifeMax = 700;
            NPC.boss = true;
            NPC.aiStyle = -1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.HitSound = new SoundStyle("CalamityAddon/Content/Sounds/WulfrumHit", 3);
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.value = Item.buyPrice(0, 1, 5, 0);
            Music = 0;
        }

        // Метод перехвата смерти
        public override bool CheckDead()
        {
            if (!isDespawnJumping)
            {
                NPC.life = 1;
                NPC.active = true;
                isDespawnJumping = true;

                // Эффект при начале побега
                SoundEngine.PlaySound(new SoundStyle("CalamityAddon/Content/Sounds/WulfrumHurry2"), NPC.Center);
                BeginDespawnJump();
                return false; // Отменяем смерть
            }
            return true;
        }

        private void BeginDespawnJump()
        {
            NPC.dontTakeDamage = true;
            NPC.chaseable = false;
            Player player = Main.player[NPC.target];
            int jumpDir = (NPC.Center.X < player.Center.X) ? -1 : 1;
            NPC.velocity = new Vector2(jumpDir * DespawnHorizontalSpeed, DespawnJumpStrength);
            NPC.netUpdate = true;
        }

        public override void AI()
        {
            if (isDespawnJumping)
            {
                NPC.dontTakeDamage = true;
                NPC.rotation = NPC.velocity.X * 0.05f;

                float distY = Math.Abs(NPC.Center.Y - Main.player[NPC.target].Center.Y);
                float distX = Math.Abs(NPC.Center.X - Main.player[NPC.target].Center.X);

                if (distY > (Main.screenHeight / 2) + 100 || distX > (Main.screenWidth / 2) + 100)
                {
                    NPC.active = false;
                    NPC.netUpdate = true;
                }
                return;
            }

            if (NPC.target < 0 || NPC.target == 255 || !Main.player[NPC.target].active || Main.player[NPC.target].dead)
            {
                NPC.TargetClosest(true);
            }

            Player player = Main.player[NPC.target];

            // Обычная логика...
            int facingDirection = (player.Center.X < NPC.Center.X) ? -1 : 1;
            NPC.direction = facingDirection;
            NPC.spriteDirection = -facingDirection;
            bool onGround = NPC.velocity.Y == 0;

            if (NPC.ai[0] == 0) // ФАЗА ПРЫЖКОВ
            {
                if (onGround)
                {
                    if (NPC.localAI[0] == 1) // Приземление после высокого прыжка
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int damageWS = 15;
                            Vector2 wavePos = new Vector2(NPC.Center.X, NPC.position.Y + NPC.height - 10);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), wavePos, Vector2.Zero, ModContent.ProjectileType<WulfrumShockwave>(), damageWS, 5f, Main.myPlayer);
                        }

                        NPC.ai[0] = 1;
                        NPC.ai[1] = 0;
                        NPC.localAI[0] = 0;
                        NPC.netUpdate = true;
                        return;
                    }

                    NPC.velocity.X *= 0.8f;
                    NPC.ai[1]++;
                    springFrame = 0;
                    animTimer = 0;

                    if (NPC.ai[1] >= 60)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[2]++;
                        if (NPC.ai[2] >= 4) // ВЫСОКИЙ ПРЫЖОК
                        {
                            NPC.velocity.Y = -14f;
                            NPC.velocity.X = 6f * NPC.direction;
                            NPC.localAI[0] = 1;
                            NPC.ai[2] = 0;
                            SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                        }
                        else // МАЛЕНЬКИЙ ПРЫЖОК
                        {
                            NPC.velocity.Y = -10f;
                            NPC.velocity.X = 4f * NPC.direction;
                            NPC.localAI[0] = 0;
                            SoundEngine.PlaySound(SoundID.Item1, NPC.Center);
                        }
                        NPC.netUpdate = true;
                    }
                }
                else
                {
                    int maxFrame = (NPC.localAI[0] == 1) ? 4 : 2;
                    if (springFrame < maxFrame)
                    {
                        animTimer++;
                        if (animTimer >= 4) { animTimer = 0; springFrame++; }
                    }
                }
            }
            else if (NPC.ai[0] == 1) // ФАЗА РАКЕТ
            {
                NPC.velocity.X *= 0.7f;
                NPC.ai[1]++;
                springFrame = 0;

                if (NPC.ai[1] % 25 == 0 && NPC.ai[3] < 3)
                {
                    ShootHomingRocket();
                    NPC.ai[3]++;
                    NPC.netUpdate = true;
                }

                if (NPC.ai[3] >= 3 && NPC.ai[1] >= 120)
                {
                    NPC.ai[0] = 0;
                    NPC.ai[1] = 0;
                    NPC.ai[3] = 0;
                    NPC.netUpdate = true;
                }
            }
        }

        private void ShootHomingRocket()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            Vector2[] rocketOffsets = new Vector2[]
            {
                new Vector2(-10f, -30f),
                new Vector2(-18f, -30f),
                new Vector2(-24f, -30f)
            };

            int index = (int)MathHelper.Clamp(NPC.ai[3], 0, 2);
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

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter >= 6)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= frameHeight * Main.npcFrameCount[NPC.type]) NPC.frame.Y = 0;
            }
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
            int hFrameHeight = hTex.Height / Main.npcFrameCount[NPC.type];
            spriteBatch.Draw(hTex, new Vector2(drawPos.X, drawPos.Y - (int)hullVisualOffset), NPC.frame, drawColor, NPC.rotation, new Vector2(hTex.Width / 2f, hFrameHeight), NPC.scale, effects, 0f);

            return false;
        }

        public override bool? CanFallThroughPlatforms()
        {
            if (NPC.target < 0 || NPC.target == 255) return false;
            Player player = Main.player[NPC.target];
            return player.active && !player.dead && player.Center.Y > NPC.position.Y + NPC.height;
        }
    }

    public class WulfrumJumperSpawnControl : GlobalNPC
    {
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (NPC.AnyNPCs(ModContent.NPCType<WulfrumJumperP1>()))
            {
                maxSpawns = 0;
                spawnRate = int.MaxValue;
            }
        }
    }
}