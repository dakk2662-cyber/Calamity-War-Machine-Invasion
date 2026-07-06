using Terraria;
using Terraria.ModLoader;

namespace CalamityAddon.Content.Mounts
{
    public class WulfrumPrototypeBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;        
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.mount.SetMount(ModContent.MountType<WulfrumPrototype>(), player);
            player.buffTime[buffIndex] = 10;
        }
    }
}