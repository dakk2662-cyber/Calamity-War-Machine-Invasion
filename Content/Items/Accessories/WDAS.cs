using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityAddon.Content.Projectiles; // Путь к снаряду

namespace CalamityAddon.Content.Items.Accessories
{
    public class WDAS : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 36;
            Item.accessory = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.buyPrice(0, 1, 50, 0);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // Включаем систему WDAS для игрока
            player.GetModPlayer<WDASPlayer>().hasWDAS = true;
        }
    }
}