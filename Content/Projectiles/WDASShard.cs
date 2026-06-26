using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityAddon.Content.Items.Accessories;
using System;

namespace CalamityAddon.Content.Projectiles
{
    public class WDASShard : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
            ProjectileID.Sets.MinionShot[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 1200;
            Projectile.DamageType = DamageClass.Generic;
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            Projectile.frame = Main.rand.Next(3);
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead || !player.GetModPlayer<WDASPlayer>().hasWDAS)
            {
                Projectile.Kill();
                return;
            }

            if (Projectile.ai[1] == 0f)
            {
                // --- РЕЖИМ ОРБИТЫ ---
                double deg = Projectile.ai[0];
                double rad = deg * (Math.PI / 180);
                int dist = 45;

                Projectile.position.X = player.Center.X - (int)(Math.Cos(rad) * dist) - Projectile.width / 2;
                Projectile.position.Y = player.Center.Y - (int)(Math.Sin(rad) * dist) - Projectile.height / 2;

                Projectile.ai[0] += 3f;
                Projectile.rotation = (float)rad + MathHelper.PiOver2;

                NPC target = FindClosestNPC(600f);
                if (target != null)
                {
                    Projectile.ai[1] = 1f;
                    Projectile.velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 12f;
                    Projectile.netUpdate = true;
                }
            }
            else
            {
                // --- РЕЖИМ АТАКИ ---
                Projectile.rotation += 0.4f * (float)Projectile.direction;

                NPC target = FindClosestNPC(800f);
                if (target != null)
                {
                    Vector2 desiredVelocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 12f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 0.05f);
                }
            }
        }

        private NPC FindClosestNPC(float range)
        {
            NPC closest = null;
            float closestDist = range;
            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, 0, 0, 100, default, 0.8f);
            }
        }
    }
}