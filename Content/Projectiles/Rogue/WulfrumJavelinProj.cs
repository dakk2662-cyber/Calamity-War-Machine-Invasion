using CalamityMod;
using CalamityMod.CalPlayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityAddon.Content.Projectiles.Rogue
{
    public class WulfrumJavelinProj : ModProjectile
    {
        // ai[0] == 1 означает, что это скрытный удар (Stealth Strike)
        public bool IsStealthStrike => Projectile.ai[0] == 1f;

        // ai[1] == 1 означает, что снаряд вошел в стадию взрыва
        public bool IsExploding => Projectile.ai[1] == 1f;
        public override string Texture => "CalamityAddon/Content/Items/Weapons/WulfrumJavelin";
        public override void SetStaticDefaults()
        {
            // Указываем, что снаряд взрывной
            ProjectileID.Sets.Explosive[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = ModContent.GetInstance<RogueDamageClass>();
            Projectile.penetrate = 1;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 300;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];
            CalamityPlayer calPlayer = player.Calamity();

            if (calPlayer.StealthStrikeAvailable())
            {
                Projectile.ai[0] = 1f; // Stealth Strike
                Projectile.penetrate = 3; // Пробивает 2-х врагов, взрывается на 3-м
                Projectile.velocity *= 1.5f;
                Projectile.timeLeft = 600;
            }
        }

        public override void AI()
        {
            if (Projectile.timeLeft <= 3)
            {
                PrepareBombToBlow();
            }

            if (!IsExploding)
            {
                // Логика полета
                Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
                Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi);
                Projectile.rotation += MathHelper.ToRadians(45f) * Projectile.spriteDirection;

                // Гравитация
                float gravity = IsStealthStrike ? 0.02f : 0.16f;
                Projectile.velocity.Y += gravity;
                if (Projectile.velocity.Y > 18f) Projectile.velocity.Y = 18f;

                // Эффект искр в полете
                if (Main.rand.NextBool(4))
                {
                    Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, 0, 0, 100, default, 0.7f);
                    d.noGravity = true;
                }
            }
            else
            {
                Projectile.velocity = Vector2.Zero;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, effects);
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (IsExploding) return;

            if (Projectile.penetrate <= 1)
            {
                Projectile.ai[1] = 1f;
                Projectile.timeLeft = 3;
                Projectile.velocity *= 0f;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!IsExploding)
            {
                Projectile.ai[1] = 1f;
                Projectile.timeLeft = 3;
                Projectile.velocity *= 0f;
            }
            return false;
        }

        public void PrepareBombToBlow()
        {
            Projectile.ai[1] = 1f;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;

            // Настройка радиуса взрыва
            int radius = IsStealthStrike ? 120 : 60;

            // Центрируем взрыв
            Vector2 oldCenter = Projectile.Center;
            Projectile.Resize(radius, radius);
            Projectile.Center = oldCenter;

            Projectile.penetrate = -1;
        }

        public override void OnKill(int timeLeft)
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            for (int i = 0; i < 25; i++)
            {
                int dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1.2f);
                Main.dust[dustIndex].velocity *= 1.4f;
            }

            for (int i = 0; i < 10; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, dustVel.X, dustVel.Y, 100, default, 1.5f);
            }

            for (int k = 0; k < 15; k++)
            {
                int dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GrassBlades, 0f, 0f, 100, default, 1f);
                Main.dust[dustIndex].noGravity = true;
                Main.dust[dustIndex].velocity *= 3f;
                Main.dust[dustIndex].scale = 1.2f;
            }
        }
        public override bool? CanDamage() => Projectile.timeLeft > 1;
    }
}