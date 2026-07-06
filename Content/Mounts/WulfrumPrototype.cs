using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace CalamityAddon.Content.Mounts
{
	public class WulfrumPrototype : ModMount
	{

		public override void SetStaticDefaults()
		{
			// Movement
			MountData.jumpHeight = 6;
			MountData.acceleration = 0.2f;
			MountData.jumpSpeed = 6f;
			MountData.blockExtraJumps = false;
			MountData.constantJump = true;
			MountData.heightBoost = 20;
			MountData.fallDamage = 0.5f;
			MountData.runSpeed = 4f;
			MountData.dashSpeed = 8f;
			MountData.flightTimeMax = 0;

			// Misc
			MountData.fatigueMax = 0;
			MountData.buff = ModContent.BuffType<WulfrumPrototypeBuff>();

			// Frame data and player offsets
			MountData.totalFrames = 4;
			MountData.playerYOffsets = Enumerable.Repeat(2, MountData.totalFrames).ToArray();
			MountData.xOffset = 6;
			MountData.yOffset = 16;
			MountData.playerHeadOffset = 16;
			MountData.bodyFrame = 3;
			// Standing
			MountData.standingFrameCount = 4;
			MountData.standingFrameDelay = 12;
			MountData.standingFrameStart = 0;
			// Running
			MountData.runningFrameCount = 4;
			MountData.runningFrameDelay = 12;
			MountData.runningFrameStart = 0;
			// Flying
			MountData.flyingFrameCount = 4;
			MountData.flyingFrameDelay = 12;
			MountData.flyingFrameStart = 0;
			// In-air
			MountData.inAirFrameCount = 4;
			MountData.inAirFrameDelay = 12;
			MountData.inAirFrameStart = 0;
			// Idle
			MountData.idleFrameCount = 4;
			MountData.idleFrameDelay = 12;
			MountData.idleFrameStart = 0;
			MountData.idleFrameLoop = true;
			// Swim
			MountData.swimFrameCount = MountData.inAirFrameCount;
			MountData.swimFrameDelay = MountData.inAirFrameDelay;
			MountData.swimFrameStart = MountData.inAirFrameStart;

			if (!Main.dedServ)
			{
				MountData.textureWidth = MountData.backTexture.Width();
				MountData.textureHeight = MountData.backTexture.Height();
			}

		}
	}

	public class WulfrumPrototypeDashPlayer : ModPlayer
	{

		public const int DashDown = 0;
		public const int DashUp = 1;
		public const int DashRight = 2;
		public const int DashLeft = 3;

		public const int DashCooldown = 60;
		public const int DashDuration = 20;
		public const int DashingSpeed = 14;

		public int DashDir = -1;
		public int DashDelay = 0;
		public int DashTimer = 0;

		private bool IsOnThisMount =>
			Player.mount.Active && Player.mount.Type == ModContent.MountType<WulfrumPrototype>();

		public override void ResetEffects()
		{
			DashDir = -1;

			if (!IsOnThisMount)
				return;
			if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[DashRight] < 15)
				DashDir = DashRight;
			else if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[DashLeft] < 15)
				DashDir = DashLeft;
		}

		public override void PreUpdateMovement()
		{
			if (!IsOnThisMount)
			{
				DashTimer = 0;
				DashDelay = 0;
				return;
			}

			if (DashDir != -1 && DashDelay == 0)
			{
				Vector2 newVelocity = Player.velocity;

				if (DashDir == DashRight && Player.velocity.X < DashingSpeed)
					newVelocity.X = DashingSpeed;
				else if (DashDir == DashLeft && Player.velocity.X > -DashingSpeed)
					newVelocity.X = -DashingSpeed;
				else
					goto skipStart;

				Player.velocity = newVelocity;
				DashDelay = DashCooldown;
				DashTimer = DashDuration;

				if (!Main.dedServ)
				{
					Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, Player.Center);

					for (int i = 0; i < 20; i++)
					{
						Dust d = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Electric, -Player.velocity.X * 0.5f, 0f, 100, Color.Lime, 1.5f);
						d.noGravity = true;
						d.velocity *= 1.5f;

						Dust s = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Smoke, -Player.velocity.X * 0.2f, 0f, 100, default, 1f);
						s.velocity *= 0.5f;
					}
				}
			}

		skipStart:
			if (DashTimer > 0)
			{
				if (!Main.dedServ)
				{
					for (int i = 0; i < 2; i++)
					{
						Dust d = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Electric, 0f, 0f, 100, Color.Lime, 1.2f);
						d.noGravity = true;
						d.velocity *= 0.2f;
					}
				}
				//Player.vortexStealthAlpha = 0.5f; // Делает игрока слегка прозрачным/светящимся
			}

			if (DashDelay > 0)
				DashDelay--;

			if (DashTimer > 0)
				DashTimer--;
		}
	}
}