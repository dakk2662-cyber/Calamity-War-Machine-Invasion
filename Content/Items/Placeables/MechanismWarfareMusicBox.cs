using CalamityAddon.Content.Tiles;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityAddon.Content.Items.Placeables
{
	public class MechanismWarfareMusicBox : ModItem
	{
		public override void SetStaticDefaults()
		{
			ItemID.Sets.CanGetPrefixes[Type] = false;
			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.MusicBox;
			MusicLoader.AddMusicBox(Mod, MusicLoader.GetMusicSlot(Mod, "Content/Sounds/Music/MechanismWarfare"), ModContent.ItemType<MechanismWarfareMusicBox>(), ModContent.TileType<MechanismWarfareMusicBoxTile>());
		}

		public override void SetDefaults()
		{
			Item.DefaultToMusicBox(ModContent.TileType<MechanismWarfareMusicBoxTile>(), 0);
		}
	}
}