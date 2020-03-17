using System;
using System.Collections.Generic;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Abstract;
using ExileCore.Shared.Cache;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using JM.LinqFaster;
using SharpDX;

namespace IconsBuilder
{
    internal class DeliriumIcon : BaseIcon
    {
        public DeliriumIcon(Entity entity, GameController gameController, IconsBuilderSettings settings, Dictionary<string, Size2> modIcons): base(entity, settings)
        {
            Update(entity, settings, modIcons);
        }
        public override string ToString()
        {
            return $"{Entity.Metadata} : {Entity.Type} ({Entity.Address}) T: {Text}";
        }
        public void Update(Entity entity, IconsBuilderSettings settings, Dictionary<string, Size2> modIcons)
        {
            Show = () => Entity.IsAlive;

            MainTexture = new HudTexture("Icons.png");
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
                    Text = entity.RenderName;
                    break;
                default:
                    throw new ArgumentException("Delirium icon rarity corrupted.");
                    break;
            }

            if (_HasIngameIcon && entity.HasComponent<MinimapIcon>() && !entity.GetComponent<MinimapIcon>().Name.Equals("NPC"))
                return;
            
            if (entity.Path.StartsWith("Metadata/Monsters/LeagueAffliction/DoodadDaemons", StringComparison.Ordinal))
            {
                var pathstring = "Metadata/Monsters/LeagueAffliction/DoodadDaemons/DoodadDaemon";
                //proximity spawning volatile ->bad
                if (entity.Path.StartsWith(pathstring + "BloodBag", StringComparison.Ordinal))
                {
                    MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.RedFlag);
                    Text = settings.DeliriumText.Value? "Avoid" : ""; 
                }
                else if(entity.Path.StartsWith(pathstring + "EggFodder", StringComparison.Ordinal))
                {
                    MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.NPC);
                }
                else if (entity.Path.StartsWith(pathstring + "GlobSpawn", StringComparison.Ordinal))
                {
                    MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.MyPlayer);
                }
                else
                {
                    Show = () => false;
                    MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.QuestObject);
                    return;
                }
                MainTexture.Size = settings.SizeEntityProximityMonsterIcon;
                Hidden = () => false;
                
                Priority = IconPriority.Medium;
                return;
            }
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

                    var mods = objectMagicProperties.Mods;

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
                            break;
                        case MonsterRarity.Magic:
                            MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.LootFilterLargeBlueCircle);

                            break;
                        case MonsterRarity.Rare:
                            MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.LootFilterLargeYellowCircle);
                            break;
                        case MonsterRarity.Unique:
                            MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.LootFilterLargeCyanHexagon);
                            MainTexture.Color = Color.DarkOrange;
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