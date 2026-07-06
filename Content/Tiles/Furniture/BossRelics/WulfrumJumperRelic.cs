using CalamityAddon.Content.Tiles.BaseTiles;
using Terraria.ModLoader;

namespace CalamityAddon.Content.Tiles.Furniture.BossRelics
{
    public class WulfrumJumperRelic : BaseBossRelic
    {
        public override string RelicTextureName => "CalamityAddon/Content/Tiles/Furniture/BossRelics/WulfrumJumperRelic";

        public override int AssociatedItem => ModContent.ItemType<Items.Placeables.Furniture.BossRelics.WulfrumJumperRelic>();
    }
}