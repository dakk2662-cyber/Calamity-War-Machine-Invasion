using Terraria;
using Terraria.ModLoader;

namespace CalamityAddon.Content.Events
{
    public class WulfrumRushSceneEffect : ModSceneEffect
    {
        public override bool IsSceneEffectActive(Player player)
        {
            return WulfrumRush.isInvasionActive;
        }

        public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Content/Sounds/Music/WulfrumRushTheme");
    }
}