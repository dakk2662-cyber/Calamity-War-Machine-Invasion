using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityAddon.Content.Mounts
{
    public class WulfrumPrototypeItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 30;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item79; 
            Item.noMelee = true;
            Item.mountType = ModContent.MountType<WulfrumPrototype>();
        }
    }
}