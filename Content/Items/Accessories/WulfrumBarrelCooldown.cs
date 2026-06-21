using System;
using CalamityMod.Cooldowns;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using static CalamityMod.CalamityUtils;
using static Terraria.ModLoader.ModContent;

namespace CalamityAddon.Content.Items.Accessories
{
    public class WulfrumBarrelCooldown : CooldownHandler
    {
        private static Color ringColorLerpStart = new Color(49, 220, 221);
        private static Color ringColorLerpEnd = new Color(99, 226, 142);
        public static new string ID => "WulfrumBarrelCooldown";
        public override bool CanTickDown => false;//!instance.player.Calamity().roverDrive || instance.timeLeft <= 0;
        public override bool ShouldDisplay => true;//instance.player.Calamity().roverDrive;
        public override LocalizedText DisplayName => Language.GetOrRegister("Mods.CalamityAddon.UI.WulfrumBarrel", () => "Мощность вульфрумовой энергии");
        public override string Texture => "CalamityAddon/Assets/Cooldowns/WulfrumBarrelActive";
        public override string OutlineTexture => "CalamityAddon/Assets/Cooldowns/WulfrumBarrelOutline";
        public override string OverlayTexture => "CalamityAddon/Assets/Cooldowns/WulfrumBarrelOverlay";
        public override Color OutlineColor => new Color(112, 244, 244);
        public override Color CooldownStartColor => Color.Lerp(ringColorLerpStart, ringColorLerpEnd, instance.Completion);
        public override Color CooldownEndColor => Color.Lerp(ringColorLerpStart, ringColorLerpEnd, instance.Completion);
        public override bool SavedWithPlayer => false;
        public override bool PersistsThroughDeath => false;

        public override void ApplyBarShaders(float opacity)
        {
            //Use the adjusted completion
            float progress = GetProgress();
            GameShaders.Misc["CalamityMod:CircularBarShader"].UseOpacity(opacity);
            GameShaders.Misc["CalamityMod:CircularBarShader"].UseSaturation(progress);
            GameShaders.Misc["CalamityMod:CircularBarShader"].UseColor(CooldownStartColor);
            GameShaders.Misc["CalamityMod:CircularBarShader"].UseSecondaryColor(CooldownEndColor);
            GameShaders.Misc["CalamityMod:CircularBarShader"].Apply();
        }

        public override void DrawExpanded(SpriteBatch spriteBatch, Vector2 position, float opacity, float scale)
        {
            base.DrawExpanded(spriteBatch, position, opacity, scale);
            var modPlayer = instance?.player?.GetModPlayer<WulfrumBarrelModPlayer>();
            if (modPlayer != null)
            {
                int damagePercent = (int)modPlayer.currentDamageBoost;
                string text = "+" + damagePercent + "%";
                Vector2 textSize = FontAssets.MouseText.Value.MeasureString(text);
                float Xoffset = -textSize.X * 0.5f;
                DrawBorderStringEightWay(spriteBatch, FontAssets.MouseText.Value, text, position + new Vector2(Xoffset, -textSize.Y * 0.5f) * scale, Color.White, Color.Black, scale);
            }
        }

        public override void DrawCompact(SpriteBatch spriteBatch, Vector2 position, float opacity, float scale)
        {
            Texture2D sprite = Request<Texture2D>(Texture).Value;
            Texture2D outline = Request<Texture2D>(OutlineTexture).Value;
            Texture2D overlay = Request<Texture2D>(OverlayTexture).Value;

            // Draw the outline
            spriteBatch.Draw(outline, position, null, OutlineColor * opacity, 0, outline.Size() * 0.5f, scale, SpriteEffects.None, 0f);

            // Draw the icon
            spriteBatch.Draw(sprite, position, null, Color.White * opacity, 0, sprite.Size() * 0.5f, scale, SpriteEffects.None, 0f);

            float progress = GetProgress();
            // Draw the small overlay
            int lostHeight = (int)Math.Ceiling(overlay.Height * (progress));
            Rectangle crop = new Rectangle(0, lostHeight, overlay.Width, overlay.Height - lostHeight);
            spriteBatch.Draw(overlay, position + Vector2.UnitY * lostHeight * scale, crop, OutlineColor * opacity * 0.9f, 0, sprite.Size() * 0.5f, scale, SpriteEffects.None, 0f);

            var modPlayer = instance?.player?.GetModPlayer<WulfrumBarrelModPlayer>();
            if (modPlayer != null)
            {
                int damagePercent = (int)modPlayer.currentDamageBoost;
                string text = "+" + damagePercent + "%";
                Vector2 textSize = FontAssets.MouseText.Value.MeasureString(text);
                float Xoffset = -textSize.X * 0.5f;
                DrawBorderStringEightWay(spriteBatch, FontAssets.MouseText.Value, text, position + new Vector2(Xoffset, -textSize.Y * 0.5f) * scale, Color.White, Color.Black, scale);
            }
        }
        private float GetProgress()
        {
            var modPlayer = instance?.player?.GetModPlayer<WulfrumBarrelModPlayer>();
            if (modPlayer == null || modPlayer.maxDamageBoost <= 0)
                return 0f;
            return MathHelper.Clamp(modPlayer.currentDamageBoost / modPlayer.maxDamageBoost, 0f, 1f);
        }
    }
}