using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using CalamityMod.CalPlayer;
using System;

namespace CalamityAddon.Content.Projectiles.Rogue
{
	public class WulfrumJavelinHeld : ModProjectile
	{
		// Текстура берется от самого предмета
		public override string Texture => "CalamityAddon/Content/Items/Weapons/Rogue/WulfrumJavelin";

		public float Charge
		{
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public const int MaxCharge = 90;

		public override void SetDefaults()
		{
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.DamageType = ModContent.GetInstance<RogueDamageClass>();
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			if (player.dead || !player.active) { Projectile.Kill(); return; }

			CalamityPlayer calPlayer = player.Calamity();

			// Если ЛКМ удерживается
			if (player.channel && !player.noItems && !player.CCed)
			{
				Projectile.timeLeft = 2;
				player.itemTime = 2;
				player.itemAnimation = 2;

				if (Charge < MaxCharge)
					Charge++;

				// ИСПРАВЛЕНИЕ: Используем rogueStealth, rogueStealthMax и rogueStealthGen (строка 119 в SupernovaHeld.cs)
				if (calPlayer.rogueStealth < calPlayer.rogueStealthMax)
				{
					calPlayer.rogueStealth += calPlayer.rogueStealthGen / 2f;
				}

				// Направление к курсору
				Projectile.velocity = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
				player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);

				// Анимация руки (SetCompositeArmFront)
				float armRotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
				float windUp = MathHelper.Lerp(0, -0.6f, Charge / MaxCharge);
				player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation + windUp * player.direction);

				Projectile.Center = player.MountedCenter;
				Projectile.rotation = armRotation + MathHelper.PiOver4 + windUp * player.direction;
			}
			else // Момент броска
			{
				if (Projectile.owner == Main.myPlayer)
				{
					float chargeMult = MathHelper.Lerp(1f, 2.5f, Charge / MaxCharge);
					Vector2 shootVel = Projectile.velocity * 12f * chargeMult;

					int p = Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.MountedCenter, shootVel,
						ModContent.ProjectileType<WulfrumJavelinProj>(), (int)(Projectile.damage * chargeMult), Projectile.knockBack, Projectile.owner, Charge);

					if (p != Main.maxProjectiles)
					{
						// ИСПРАВЛЕНИЕ: Скрытный удар считается как достижение максимума скрытности (строка 127 в SupernovaHeld.cs)
						Main.projectile[p].Calamity().stealthStrike = calPlayer.rogueStealth >= calPlayer.rogueStealthMax;
					}
				}
				Projectile.Kill();
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Player player = Main.player[Projectile.owner];
			Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			Vector2 drawPos = player.MountedCenter + Projectile.velocity * 10f - Main.screenPosition;
			SpriteEffects flip = player.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Main.EntitySpriteDraw(tex, drawPos, null, lightColor, Projectile.rotation, tex.Size() / 2, 1f, flip, 0);
			return false;
		}
	}
}