using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityAddon.Content.Items.Placeables.Furniture.BossRelics
{
    public class WulfrumJumperRelic : ModItem
    {
        //public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {            
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.BossRelics.WulfrumJumperRelic>(), 0);

            Item.width = 30;
            Item.height = 40;
            Item.rare = ItemRarityID.Master;
            Item.master = true;
            Item.value = Item.sellPrice(gold: 1);
        }
    }
}