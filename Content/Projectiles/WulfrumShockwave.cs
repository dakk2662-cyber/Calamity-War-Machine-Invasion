using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace CalamityAddon.Content.Projectiles
{
	public class WulfrumShockwave : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_0";

		public override void SetDefaults()
		{
			Projectile.width = 400; // Ширина ударной волны
			Projectile.height = 40;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 2;
			Projectile.hide = true;
		}

		public override void AI()
		{
			if (Projectile.localAI[0] == 0f)
			{
				// Звук удара
				SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.2f, Volume = 1f }, Projectile.Center);

				// Создаем частицы по всей ширине
				for (int i = 0; i < 50; i++)
				{
					Vector2 dustPos = Projectile.position + new Vector2(Main.rand.NextFloat(Projectile.width), Projectile.height);
					int tileX = (int)(dustPos.X / 16);
					int tileY = (int)(dustPos.Y / 16);

					// Безопасно получаем тип пыли
					int dustType = DustID.Dirt;
					if (WorldGen.InWorld(tileX, tileY) && Main.tile[tileX, tileY].HasTile)
					{
						// Простая логика: если камень - серая пыль, иначе - коричневая
						if (Main.tileLighted[Main.tile[tileX, tileY].TileType]) // Если это что-то светящееся/особое
							dustType = DustID.Electric;
						else if (Main.tile[tileX, tileY].TileType == TileID.Stone)
							dustType = DustID.Stone;
					}

					// Частицы земли/камня вверх
					Dust d = Dust.NewDustDirect(dustPos, 0, 0, dustType);
					d.velocity.Y = Main.rand.NextFloat(-6f, -3f);
					d.velocity.X = Main.rand.NextFloat(-2f, 2f);
					d.scale = Main.rand.NextFloat(1.2f, 1.8f);
					d.noGravity = false;

					// Добавим немного дыма для эффекта взрыва
					if (i % 2 == 0)
					{
						Dust smoke = Dust.NewDustDirect(dustPos, 0, 0, DustID.Smoke);
						smoke.velocity.Y = Main.rand.NextFloat(-4f, -2f);
						smoke.noGravity = true;
					}
				}
				Projectile.localAI[0] = 1f;
			}
		}
	}
}