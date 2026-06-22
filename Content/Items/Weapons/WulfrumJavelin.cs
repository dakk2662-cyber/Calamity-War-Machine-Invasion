using CalamityMod;
using CalamityMod.CalPlayer;
using CalamityMod.Items;
using CalamityMod.Items.Weapons.Rogue;
using CalamityAddon.Content.Projectiles.Rogue;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityAddon.Content.Items.Weapons
{
    public class WulfrumJavelin : RogueWeapon
    {
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.damage = 15;
            Item.DamageType = ModContent.GetInstance<RogueDamageClass>();
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.UseSound = null; // Звук будет при броске
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<WulfrumJavelinHeld>();
            Item.shootSpeed = 1f;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(0, 0, 10, 0);
        }
    }
}