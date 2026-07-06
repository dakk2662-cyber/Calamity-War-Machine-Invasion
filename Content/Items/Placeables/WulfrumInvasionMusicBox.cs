using CalamityAddon.Content.Tiles;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityAddon.Content.Items.Placeables
{
    public class WulfrumInvasionMusicBox : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.CanGetPrefixes[Type] = false;
            ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.MusicBox;
            MusicLoader.AddMusicBox(Mod, MusicLoader.GetMusicSlot(Mod, "Content/Sounds/Music/WulfrumRushTheme"), ModContent.ItemType<WulfrumInvasionMusicBox>(), ModContent.TileType<WulfrumInvasionMusicBoxTile>());
        }

        public override void SetDefaults()
        {
            Item.DefaultToMusicBox(ModContent.TileType<WulfrumInvasionMusicBoxTile>(), 0);
        }
    }
}