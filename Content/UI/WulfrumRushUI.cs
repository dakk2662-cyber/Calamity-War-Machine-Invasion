using CalamityAddon.Content.Events;
using CalamityMod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityAddon.Content.UI
{
	public class WulfrumRushUI : InvasionProgressUI
	{
		public override float CompletionRatio => MathHelper.Clamp((float)WulfrumRush.invasionKills / WulfrumRush.invasionMaxProgress, 0f, 1f);
        public override string InvasionName => "Wulfrum Invasion";
        public override Color InvasionBarColor => new Color(131, 184, 126);
		public override Texture2D IconTexture => ModContent.Request<Texture2D>("CalamityAddon/Content/Events/WulfrumRush_Icon", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
		public override bool IsActive => WulfrumRush.isInvasionActive;
	}
}