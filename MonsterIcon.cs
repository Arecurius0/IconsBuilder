using System;
using System.Collections.Generic;
using System.Linq;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Abstract;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using JM.LinqFaster;
using SharpDX;

namespace IconsBuilder
{
    public class MonsterIcon : BaseIcon
    {
        public MonsterIcon(Entity entity, GameController gameController, IconsBuilderSettings settings, Dictionary<string, Size2> modIcons)
            : base(entity, settings)
        {
            Update(entity, settings, modIcons);
        }

        public long ID { get; set; }

        public void Update(Entity entity, IconsBuilderSettings settings, Dictionary<string, Size2> modIcons)
        {
            Show = () => entity.IsAlive;
            if(entity.IsHidden && settings.HideBurriedMonsters)
            {
                Show = () => !entity.IsHidden && entity.IsAlive;
            }
            ID = entity.Id;

            if (!_HasIngameIcon) MainTexture = new HudTexture("Icons.png");

            switch (Rarity)
            {
                case MonsterRarity.White:
                    MainTexture.Size = settings.SizeEntityWhiteIcon;
                    break;
                case MonsterRarity.Magic:
                    MainTexture.Size = settings.SizeEntityMagicIcon;
                    break;
                case MonsterRarity.Rare:
                    MainTexture.Size = settings.SizeEntityRareIcon;
                    break;
                case MonsterRarity.Unique:
                    MainTexture.Size = settings.SizeEntityUniqueIcon;
                    break;
                default:
                    throw new ArgumentException(
                        $"{nameof(MonsterIcon)} wrong rarity for {entity.Path}. Dump: {entity.GetComponent<ObjectMagicProperties>().DumpObject()}");

                    break;
            }

            if (_HasIngameIcon && entity.HasComponent<MinimapIcon>() && !entity.GetComponent<MinimapIcon>().Name.Equals("NPC") && entity.League != LeagueType.Heist)
                return;

            if (!entity.IsHostile)
            {
                if (!_HasIngameIcon)
                {
                    MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.LootFilterSmallGreenCircle);
                    Priority = IconPriority.Low;
                    Show = () => !settings.HideMinions && entity.IsAlive;
                }

                //Spirits icon
            }
            else if (Rarity == MonsterRarity.Unique && entity.Path.Contains("Metadata/Monsters/Spirit/"))
                MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.LootFilterLargeGreenHexagon);
            else
            {
                string modName = null;

                if (entity.HasComponent<ObjectMagicProperties>())
                {
                    var objectMagicProperties = entity.GetComponent<ObjectMagicProperties>();

                    var mods = objectMagicProperties?.Mods ?? null;

                    if (mods != null)
                    {
                        if (mods.Contains("MonsterConvertsOnDeath_")) Show = () => entity.IsAlive && entity.IsHostile;

                        modName = mods.FirstOrDefaultF(modIcons.ContainsKey);
                    }
                }

                if (modName != null)
                {
                    MainTexture = new HudTexture("sprites.png");
                    MainTexture.UV = SpriteHelper.GetUV(modIcons[modName], new Size2F(7, 8));
                    Priority = IconPriority.VeryHigh;
                }
                else
                {
                    switch (Rarity)
                    {
                        case MonsterRarity.White:
                            MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.LootFilterLargeRedCircle);
                            if (settings.ShowWhiteMonsterName)
                                Text = RenderName.Split(',').FirstOrDefault();
                            break;
                        case MonsterRarity.Magic:
                            MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.LootFilterLargeBlueCircle);
                            if (settings.ShowMagicMonsterName)
                                Text = RenderName.Split(',').FirstOrDefault();

                            break;
                        case MonsterRarity.Rare:
                            MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.LootFilterLargeYellowCircle);
                            if (settings.ShowRareMonsterName)
                                Text = RenderName.Split(',').FirstOrDefault();
                            break;
                        case MonsterRarity.Unique:
                            MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.LootFilterLargeCyanHexagon);
                            MainTexture.Color = Color.DarkOrange;
                            if (settings.ShowUniqueMonsterName)
                                Text = RenderName.Split(',').FirstOrDefault();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(
                                $"Rarity wrong was is {Rarity}. {entity.GetComponent<ObjectMagicProperties>().DumpObject()}");
                    }
                }
            }
        }
    }
}
