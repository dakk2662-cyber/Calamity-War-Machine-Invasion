using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityAddon.Content
{
    public class DownedBossSystem : ModSystem
    {
        public static bool downedWulfrumMothership = false;
        public static bool downedWulfrumJumper = false;
        public static bool downedWulfrumRush = false;

        public override void OnWorldUnload()
        {
            downedWulfrumMothership = false;
            downedWulfrumJumper = false;
            downedWulfrumRush = false;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["downedWulfrumMothership"] = downedWulfrumMothership;
            tag["downedWulfrumJumper"] = downedWulfrumJumper;
            tag["downedWulfrumRush"] = downedWulfrumRush;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            downedWulfrumMothership = tag.GetBool("downedWulfrumMothership");
            downedWulfrumJumper = tag.GetBool("downedWulfrumJumper");
            downedWulfrumRush = tag.GetBool("downedWulfrumRush");
        }
    }
}