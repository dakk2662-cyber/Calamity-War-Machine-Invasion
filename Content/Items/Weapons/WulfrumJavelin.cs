using CalamityMod;
using CalamityMod.Items.Weapons.Rogue;
using CalamityAddon.Content.Projectiles.Rogue;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;

namespace CalamityAddon.Content.Items.Weapons
{
    public class WulfrumJavelin : RogueWeapon
    {
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.damage = 14;
            Item.DamageType = ModContent.GetInstance<RogueDamageClass>();
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useAnimation = 35;
            Item.useTime = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<WulfrumJavelinProj>();
            Item.shootSpeed = 11f;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(0, 0, 10, 0);
        }

        public override float StealthDamageMultiplier => 1.5f;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 shootFrom = player.MountedCenter;

            Projectile.NewProjectile(source, shootFrom, velocity, type, damage, knockback, player.whoAmI);

            return false;
        }
    }
}