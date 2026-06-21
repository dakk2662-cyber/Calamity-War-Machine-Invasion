using CalamityMod;
using CalamityMod.CalPlayer;
using log4net.Core;
using Terraria;
using Terraria.Map;
using Terraria.ModLoader;

namespace CalamityAddon.Content.Items.Accessories
{
    [AutoloadEquip(EquipType.Back)]
    internal class WulfrumBarrel : ModItem
    {
        public override void SetDefaults()
        {
            Item.accessory = true;
            Item.width = 40;
            Item.height = 46;
            Item.rare = 4;
            Item.value = 50000;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<WulfrumBarrelModPlayer>();
            modPlayer.wulfrumbarrel = true;
        }
    }
    public class WulfrumBarrelModPlayer : ModPlayer
    {
        public int timer = 0;
        public bool wulfrumbarrel = false;
        public float currentDamageBoost = 0f;
        public float maxDamageBoost = 30f;
        public int maxTime = 60;

        // Переменная для отслеживания появления босса
        public bool bossActiveLastFrame = false;

        public override void ResetEffects()
        {
            wulfrumbarrel = false;
        }

        public override void UpdateEquips()
        {
            CalamityPlayer calPlayer = Player.Calamity();

            bool anyBossActive = false;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].boss)
                {
                    anyBossActive = true;
                    break;
                }
            }

            if (anyBossActive && !bossActiveLastFrame)
            {
                timer = 0;
                currentDamageBoost = 0f;
            }
            bossActiveLastFrame = anyBossActive;

            if (wulfrumbarrel)
            {
                if (currentDamageBoost < maxDamageBoost)
                {
                    timer++;
                    int stepTime = maxTime * 60 / 3;
                    if (timer >= stepTime)
                    {
                        currentDamageBoost += maxDamageBoost / 3f;
                        timer = 0;
                    }
                }

                Player.GetDamage(DamageClass.Generic) += currentDamageBoost / 100f;

                if (!calPlayer.cooldowns.ContainsKey(WulfrumBarrelCooldown.ID))
                {
                    Player.AddCooldown(WulfrumBarrelCooldown.ID, 1);
                }
            }
            else
            {
                timer = 0;
                currentDamageBoost = 0f;
                if (calPlayer.cooldowns.ContainsKey(WulfrumBarrelCooldown.ID))
                    calPlayer.cooldowns.Remove(WulfrumBarrelCooldown.ID);
            }
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            timer = 0;
            currentDamageBoost = 0f;
        }

        public override void OnEnterWorld()
        {
            bossActiveLastFrame = false;
        }
    }
}