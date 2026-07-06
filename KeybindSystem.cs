using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace CalamityAddon.Content
{
    public class KeybindSystem : ModSystem
    {
        public static ModKeybind WDASKeybind { get; private set; }

        public override void Load()
        {
            WDASKeybind = KeybindLoader.RegisterKeybind(Mod, "WDASKeybind", "N");
        }

        public override void Unload()
        {
            WDASKeybind = null;
        }
    }
}
