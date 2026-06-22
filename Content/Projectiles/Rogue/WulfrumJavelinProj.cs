using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using CalamityMod.CalPlayer;

namespace CalamityAddon.Content.Projectiles.Rogue
{
	public class WulfrumJavelinProj : ModProjectile
	{
		public override string Texture => "CalamityAddon/Content/Items/Weapons/Rogue/WulfrumJavelin";

		public override void SetDefaults()
		{
			Projectile.width = 44;
			Projectile.height = 44;
			Projectile.friendly = true;
			Projectile.DamageType = ModContent.GetInstance<RogueDamageClass>();
			Projectile.penetrate = -1;
			Projectile.aiStyle = -1;
		}

		public override void AI()
		{
			// Если прилипло (ai[1] хранит индекс NPC)
			if (Projectile.ai[1] > 0)
			{
				NPC target = Main.npc[(int)Projectile.ai[1] - 1];
				if (target.active && !target.dontTakeDamage)
				{
					Projectile.Center = target.Center + Projectile.velocity; // velocity тут как смещение
					Projectile.gfxOffY = target.gfxOffY;

					// Нанесение постепенного урона
					if (Projectile.timeLeft % 30 == 0)
					{
						var info = target.CalculateHitInfo(Projectile.damage / 3, 1);
						target.StrikeNPC(info);
					}
				}
				else Projectile.Kill();
			}
			else
			{
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
				Projectile.velocity.Y += 0.2f; // Гравитация
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Projectile.ai[1] == 0)
			{
				Projectile.ai[1] = target.whoAmI + 1;
				Projectile.velocity = Projectile.Center - target.Center; // Сохраняем смещение
				Projectile.netUpdate = true;
				Projectile.tileCollide = false;

				// Время "прилипания" зависит от силы заряда (ai[0] передана из Held снаряда)
				Projectile.timeLeft = (int)MathHelper.Lerp(120, 480, Projectile.ai[0] / 90f);
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.Kill();
			return false;
		}
	}
}