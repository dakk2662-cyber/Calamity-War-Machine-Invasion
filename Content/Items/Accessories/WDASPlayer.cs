using Terraria;
using Terraria.ModLoader;
using CalamityAddon.Content.Projectiles;
using Microsoft.Xna.Framework;

namespace CalamityAddon.Content.Items.Accessories
{
    public class WDASPlayer : ModPlayer
    {
        public bool hasWDAS = false;
        public int shardTimer = 0;

        public override void ResetEffects()
        {
            hasWDAS = false;
        }

        public override void PostUpdateEquips()
        {
            if (hasWDAS)
            {
                int maxShards = 4;
                int currentShards = Player.ownedProjectileCounts[ModContent.ProjectileType<WDASShard>()];

                if (currentShards < maxShards)
                {
                    shardTimer++;
                    if (shardTimer >= 90)
                    {
                        if (Player.whoAmI == Main.myPlayer)
                        {
                            // Спавним осколок. Начальный угол передаем через ai[0]
                            float angle = MathHelper.TwoPi / maxShards * currentShards;
                            Projectile.NewProjectile(Player.GetSource_Accessory(Player.HeldItem), Player.Center, Vector2.Zero,
                                ModContent.ProjectileType<WDASShard>(), 20, 2f, Player.whoAmI, angle);
                        }
                        shardTimer = 0;
                    }
                }
                else
                {
                    shardTimer = 0;
                }
            }
        }
    }
}