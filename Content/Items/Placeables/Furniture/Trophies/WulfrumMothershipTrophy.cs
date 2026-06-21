//using CalamityAddon.Content.Tiles.Furniture.BossTrophies;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityAddon.Content.Items.Placeables.Furniture.Trophies
{
	public class WulfrumMothershipTrophy : ModItem
	{
		//public new string LocalizationCategory => "Items.Placeables";
		public override void SetDefaults()
		{
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.BossTrophies.WulfrumMothershipTrophyTile>(), 0);

            Item.width = 32;
            Item.height = 32;            
			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(gold: 1);
        }
	}
}