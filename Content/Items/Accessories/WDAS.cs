using CalamityAddon.Content.Projectiles;
using CalamityAddon.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using System.Collections.Generic;

namespace CalamityAddon.Content.Items.Accessories
{
    public class WDAS : ModItem
    {
        private Texture2D texture2;
        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 36;
            Item.accessory = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.buyPrice(0, 1, 50, 0);
            texture2 = ModContent.Request<Texture2D>("CalamityAddon/Content/Items/Accessories/WDAS2").Value; //Название текстуры сюда
        }
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (Main.LocalPlayer.GetModPlayer<WDASPlayer>().defensiveMode)
            {
                spriteBatch.Draw(texture2, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
                return false;
            }
            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var keys = KeybindSystem.WDASKeybind.GetAssignedKeys();
            string keyName = keys.Count > 0 ? keys[0] : "???";

            // Получаем текст из локализации и вставляем туда название кнопки через string.Format
            string description = Language.GetTextValue("Mods.CalamityAddon.Items.WDAS.KeybindInfo", keyName);
            TooltipLine keyLine = new TooltipLine(Mod, "WDASKey", description)
            {
                OverrideColor = Color.Gray
            };
            tooltips.Add(keyLine);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<WDASPlayer>().hasWDAS = true;
        }
    }
    public class WDASPlayer : ModPlayer
    {
        public bool hasWDAS = false;
        public bool defensiveMode = false;
        public int shardTimer = 0;

        public override void ResetEffects()
        {
            hasWDAS = false;
        }

        public override void PostUpdateEquips()
        {
            if (hasWDAS && !defensiveMode) //Если есть аккс и установлен атакующий режим
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
            else if (hasWDAS)
            { //Если есть аккс и установлен защитный режим (нет смысла его в проверку добавлять, разве что для удобства чтения)
                Player.statDefense += 5;
            }
        }
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (KeybindSystem.WDASKeybind.JustPressed)
            {
                defensiveMode = !defensiveMode;
                if (defensiveMode)
                {
                    for (int i = 0; i < Main.projectile.Length; i++)
                    {
                        Projectile proj = Main.projectile[i];
                        if (proj.active && proj.owner == Player.whoAmI && proj.type == ModContent.ProjectileType<WDASShard>())
                        {
                            proj.Kill();
                        }
                    }
                }
            }
        }
    }
}