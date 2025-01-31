using System.Collections.Generic;
using UnityEngine;

namespace Actors
{
    public abstract class Actor_Aspect_List
    {
        static Dictionary<(AspectName, AspectName, AspectName), ClassTitle> s_defaultAspects;

        public static Dictionary<(AspectName, AspectName, AspectName), ClassTitle> DefaultAspects =>
            s_defaultAspects ??= _initialiseDefaultAspects();

        public static ClassTitle GetCharacterTitle(List<AspectName> aspectList)
        {
            if (aspectList.Count != 3)
            {
                var iterations = 3 - aspectList.Count;
                
                for (var i = 0; i < iterations; i++)
                {
                    aspectList.Add(AspectName.None);
                }
            }
            
            if (DefaultAspects.TryGetValue((aspectList[0], aspectList[1], aspectList[2]), out var title))
            {
                return title;
            }

            Debug.LogWarning($"No class found for aspects: {aspectList[0]}, {aspectList[1]}, {aspectList[2]}");
            return ClassTitle.None;
        }

        static Dictionary<(AspectName, AspectName, AspectName), ClassTitle> _initialiseDefaultAspects()
        {
            return new Dictionary<(AspectName, AspectName, AspectName), ClassTitle>
            {
                { (AspectName.None, AspectName.None, AspectName.None), ClassTitle.Aspectless },

                { (AspectName.Defiance, AspectName.None, AspectName.None), ClassTitle.Defender },
                
                { (AspectName.Defiance, AspectName.Essence, AspectName.None), ClassTitle.Exorcist },
                { (AspectName.Defiance, AspectName.Essence, AspectName.Esthesis), ClassTitle.Silentpsalm },
                { (AspectName.Defiance, AspectName.Essence, AspectName.Glory), ClassTitle.Woundwarden },
                { (AspectName.Defiance, AspectName.Essence, AspectName.Grace), ClassTitle.Hallowblaze },
                { (AspectName.Defiance, AspectName.Essence, AspectName.Hunt), ClassTitle.Deathwarden },
                { (AspectName.Defiance, AspectName.Essence, AspectName.Involution), ClassTitle.Hexwarden },
                { (AspectName.Defiance, AspectName.Essence, AspectName.Shadow), ClassTitle.Shadoward },
                { (AspectName.Defiance, AspectName.Essence, AspectName.Soul), ClassTitle.Tombwarden },
                { (AspectName.Defiance, AspectName.Essence, AspectName.Volition), ClassTitle.Zenith },

                { (AspectName.Defiance, AspectName.Esthesis, AspectName.None), ClassTitle.Ironbard },
                { (AspectName.Defiance, AspectName.Esthesis, AspectName.Glory), ClassTitle.Justicar },
                { (AspectName.Defiance, AspectName.Esthesis, AspectName.Grace), ClassTitle.Hymnguard },
                { (AspectName.Defiance, AspectName.Esthesis, AspectName.Hunt), ClassTitle.Argent },
                { (AspectName.Defiance, AspectName.Esthesis, AspectName.Involution), ClassTitle.Earthsinger },
                { (AspectName.Defiance, AspectName.Esthesis, AspectName.Shadow), ClassTitle.Nocturneanthem },
                { (AspectName.Defiance, AspectName.Esthesis, AspectName.Soul), ClassTitle.Tombcaller },
                { (AspectName.Defiance, AspectName.Esthesis, AspectName.Volition), ClassTitle.Elegist },

                { (AspectName.Defiance, AspectName.Glory, AspectName.None), ClassTitle.Bastion },
                { (AspectName.Defiance, AspectName.Glory, AspectName.Grace), ClassTitle.Templar },
                { (AspectName.Defiance, AspectName.Glory, AspectName.Hunt), ClassTitle.Hordebreaker },
                { (AspectName.Defiance, AspectName.Glory, AspectName.Involution), ClassTitle.Crusader },
                { (AspectName.Defiance, AspectName.Glory, AspectName.Shadow), ClassTitle.Enforcer },
                { (AspectName.Defiance, AspectName.Glory, AspectName.Soul), ClassTitle.Skullknight },
                { (AspectName.Defiance, AspectName.Glory, AspectName.Volition), ClassTitle.Honourguard },
                
                { (AspectName.Defiance, AspectName.Grace, AspectName.None), ClassTitle.Guardian },
                { (AspectName.Defiance, AspectName.Grace, AspectName.Hunt), ClassTitle.Dawnsentinel },
                { (AspectName.Defiance, AspectName.Grace, AspectName.Involution), ClassTitle.Hierophant },
                { (AspectName.Defiance, AspectName.Grace, AspectName.Shadow), ClassTitle.Aethermend },
                { (AspectName.Defiance, AspectName.Grace, AspectName.Soul), ClassTitle.Eldritch_Shephard },
                { (AspectName.Defiance, AspectName.Grace, AspectName.Volition), ClassTitle.Paladin },
                
                { (AspectName.Defiance, AspectName.Hunt, AspectName.None), ClassTitle.Dreadstone },
                { (AspectName.Defiance, AspectName.Hunt, AspectName.Involution), ClassTitle.Battlebow },
                { (AspectName.Defiance, AspectName.Hunt, AspectName.Shadow), ClassTitle.Haunter },
                { (AspectName.Defiance, AspectName.Hunt, AspectName.Soul), ClassTitle.Immortus },
                { (AspectName.Defiance, AspectName.Hunt, AspectName.Volition), ClassTitle.Dreadnaught },
                
                { (AspectName.Defiance, AspectName.Involution, AspectName.None), ClassTitle.Enthraller },
                { (AspectName.Defiance, AspectName.Involution, AspectName.Shadow), ClassTitle.Darkaegis },
                { (AspectName.Defiance, AspectName.Involution, AspectName.Soul), ClassTitle.Abyssalich },
                { (AspectName.Defiance, AspectName.Involution, AspectName.Volition), ClassTitle.Archon },
                
                { (AspectName.Defiance, AspectName.Shadow, AspectName.None), ClassTitle.Blackguard },
                { (AspectName.Defiance, AspectName.Shadow, AspectName.Soul), ClassTitle.Netherwarden },
                { (AspectName.Defiance, AspectName.Shadow, AspectName.Volition), ClassTitle.Shadowknight },
                
                { (AspectName.Defiance, AspectName.Soul, AspectName.None), ClassTitle.Boneweaver },
                { (AspectName.Defiance, AspectName.Soul, AspectName.Volition), ClassTitle.Soulwrought },
                
                { (AspectName.Defiance, AspectName.Volition, AspectName.None), ClassTitle.Custodian },
                
                { (AspectName.Essence, AspectName.None, AspectName.None), ClassTitle.Thaumaturge },

                { (AspectName.Essence, AspectName.Esthesis, AspectName.None), ClassTitle.Spellsinger },
                { (AspectName.Essence, AspectName.Esthesis, AspectName.Glory), ClassTitle.Hexblade },
                { (AspectName.Essence, AspectName.Esthesis, AspectName.Grace), ClassTitle.Mourningchorus },
                { (AspectName.Essence, AspectName.Esthesis, AspectName.Hunt), ClassTitle.Ebonsong },
                { (AspectName.Essence, AspectName.Esthesis, AspectName.Involution), ClassTitle.Stormcaller },
                { (AspectName.Essence, AspectName.Esthesis, AspectName.Shadow), ClassTitle.Blighter },
                { (AspectName.Essence, AspectName.Esthesis, AspectName.Soul), ClassTitle.Scion },
                { (AspectName.Essence, AspectName.Esthesis, AspectName.Volition), ClassTitle.Dawncaller },
                
                { (AspectName.Essence, AspectName.Glory, AspectName.None), ClassTitle.Spellsword },
                { (AspectName.Essence, AspectName.Glory, AspectName.Grace), ClassTitle.Witcher },
                { (AspectName.Essence, AspectName.Glory, AspectName.Hunt), ClassTitle.Doombringer },
                { (AspectName.Essence, AspectName.Glory, AspectName.Involution), ClassTitle.Battlemage },
                { (AspectName.Essence, AspectName.Glory, AspectName.Shadow), ClassTitle.Shadowbane },
                { (AspectName.Essence, AspectName.Glory, AspectName.Soul), ClassTitle.Harbinger },
                { (AspectName.Essence, AspectName.Glory, AspectName.Volition), ClassTitle.Inquisitor },
                
                { (AspectName.Essence, AspectName.Grace, AspectName.None), ClassTitle.Traumapothicar },
                { (AspectName.Essence, AspectName.Grace, AspectName.Hunt), ClassTitle.Brightbow },
                { (AspectName.Essence, AspectName.Grace, AspectName.Involution), ClassTitle.Poxbane },
                { (AspectName.Essence, AspectName.Grace, AspectName.Shadow), ClassTitle.Nightcloak },
                { (AspectName.Essence, AspectName.Grace, AspectName.Soul), ClassTitle.Eidolon },
                { (AspectName.Essence, AspectName.Grace, AspectName.Volition), ClassTitle.Purifier },
                
                { (AspectName.Essence, AspectName.Hunt, AspectName.None), ClassTitle.Arcanehunter },
                { (AspectName.Essence, AspectName.Hunt, AspectName.Involution), ClassTitle.Hexranger },
                { (AspectName.Essence, AspectName.Hunt, AspectName.Shadow), ClassTitle.Nocturl },
                { (AspectName.Essence, AspectName.Hunt, AspectName.Soul), ClassTitle.Reaper },
                { (AspectName.Essence, AspectName.Hunt, AspectName.Volition), ClassTitle.Fiendhunter },
                
                { (AspectName.Essence, AspectName.Involution, AspectName.None), ClassTitle.Arcanist },
                { (AspectName.Essence, AspectName.Involution, AspectName.Shadow), ClassTitle.Hellweaver },
                { (AspectName.Essence, AspectName.Involution, AspectName.Soul), ClassTitle.Athame },
                { (AspectName.Essence, AspectName.Involution, AspectName.Volition), ClassTitle.Chaotician },
                
                { (AspectName.Essence, AspectName.Shadow, AspectName.None), ClassTitle.Daggerspell },
                { (AspectName.Essence, AspectName.Shadow, AspectName.Soul), ClassTitle.Necroetherist },
                { (AspectName.Essence, AspectName.Shadow, AspectName.Volition), ClassTitle.Planeshifter },
                
                { (AspectName.Essence, AspectName.Soul, AspectName.None), ClassTitle.Phantasm },
                { (AspectName.Essence, AspectName.Soul, AspectName.Volition), ClassTitle.Demonologist },
                
                { (AspectName.Essence, AspectName.Volition, AspectName.None), ClassTitle.Archivist },
                
                { (AspectName.Esthesis, AspectName.None, AspectName.None), ClassTitle.Euphonic },

                { (AspectName.Esthesis, AspectName.Glory, AspectName.None), ClassTitle.Duelist },
                { (AspectName.Esthesis, AspectName.Glory, AspectName.Grace), ClassTitle.Sorrowsong },
                { (AspectName.Esthesis, AspectName.Glory, AspectName.Hunt), ClassTitle.Worldwalker },
                { (AspectName.Esthesis, AspectName.Glory, AspectName.Involution), ClassTitle.Vulgarist },
                { (AspectName.Esthesis, AspectName.Glory, AspectName.Shadow), ClassTitle.Edgewalker },
                { (AspectName.Esthesis, AspectName.Glory, AspectName.Soul), ClassTitle.Bloodskald },
                { (AspectName.Esthesis, AspectName.Glory, AspectName.Volition), ClassTitle.Bladedancer },
                
                { (AspectName.Esthesis, AspectName.Grace, AspectName.None), ClassTitle.Epiphanist },
                { (AspectName.Esthesis, AspectName.Grace, AspectName.Hunt), ClassTitle.Herald },
                { (AspectName.Esthesis, AspectName.Grace, AspectName.Involution), ClassTitle.Oracle },
                { (AspectName.Esthesis, AspectName.Grace, AspectName.Shadow), ClassTitle.Shadowcantor },
                { (AspectName.Esthesis, AspectName.Grace, AspectName.Soul), ClassTitle.Soulsong },
                { (AspectName.Esthesis, AspectName.Grace, AspectName.Volition), ClassTitle.Seraphim },
                
                { (AspectName.Esthesis, AspectName.Hunt, AspectName.None), ClassTitle.Wanderer },
                { (AspectName.Esthesis, AspectName.Hunt, AspectName.Involution), ClassTitle.Bowdancer },
                { (AspectName.Esthesis, AspectName.Hunt, AspectName.Shadow), ClassTitle.Darkstring },
                { (AspectName.Esthesis, AspectName.Hunt, AspectName.Soul), ClassTitle.Gravesinger },
                { (AspectName.Esthesis, AspectName.Hunt, AspectName.Volition), ClassTitle.Gaian },
                
                { (AspectName.Esthesis, AspectName.Involution, AspectName.None), ClassTitle.Howler },
                { (AspectName.Esthesis, AspectName.Involution, AspectName.Shadow), ClassTitle.Siren },
                { (AspectName.Esthesis, AspectName.Involution, AspectName.Soul), ClassTitle.Necroharmonist },
                { (AspectName.Esthesis, AspectName.Involution, AspectName.Volition), ClassTitle.Dreambreaker },
                
                { (AspectName.Esthesis, AspectName.Shadow, AspectName.None), ClassTitle.Gypsy },
                { (AspectName.Esthesis, AspectName.Shadow, AspectName.Soul), ClassTitle.Requiem },
                { (AspectName.Esthesis, AspectName.Shadow, AspectName.Volition), ClassTitle.Etherweaver },
                
                { (AspectName.Esthesis, AspectName.Soul, AspectName.None), ClassTitle.Soulsinger },
                { (AspectName.Esthesis, AspectName.Soul, AspectName.Volition), ClassTitle.Lamentor },
                
                { (AspectName.Esthesis, AspectName.Volition, AspectName.None), ClassTitle.Soothsayer },
                
                { (AspectName.Glory, AspectName.None, AspectName.None), ClassTitle.Soldier },
                
                { (AspectName.Glory, AspectName.Grace, AspectName.None), ClassTitle.Cleric },
                { (AspectName.Glory, AspectName.Grace, AspectName.Hunt), ClassTitle.Lightstrider },
                { (AspectName.Glory, AspectName.Grace, AspectName.Involution), ClassTitle.Confessor },
                { (AspectName.Glory, AspectName.Grace, AspectName.Shadow), ClassTitle.Twilightkeeper },
                { (AspectName.Glory, AspectName.Grace, AspectName.Soul), ClassTitle.Defiler },
                { (AspectName.Glory, AspectName.Grace, AspectName.Volition), ClassTitle.Warpriest },
                
                { (AspectName.Glory, AspectName.Hunt, AspectName.None), ClassTitle.Ranger },
                { (AspectName.Glory, AspectName.Hunt, AspectName.Involution), ClassTitle.Doomlord },
                { (AspectName.Glory, AspectName.Hunt, AspectName.Shadow), ClassTitle.Darkrunner },
                { (AspectName.Glory, AspectName.Hunt, AspectName.Soul), ClassTitle.Bloodarrow },
                { (AspectName.Glory, AspectName.Hunt, AspectName.Volition), ClassTitle.Outrider },
                
                { (AspectName.Glory, AspectName.Involution, AspectName.None), ClassTitle.Dreadblade },
                { (AspectName.Glory, AspectName.Involution, AspectName.Shadow), ClassTitle.Executioner },
                { (AspectName.Glory, AspectName.Involution, AspectName.Soul), ClassTitle.Skulltaker },
                { (AspectName.Glory, AspectName.Involution, AspectName.Volition), ClassTitle.Abolisher },
                
                { (AspectName.Glory, AspectName.Shadow, AspectName.None), ClassTitle.Jacknife },
                { (AspectName.Glory, AspectName.Shadow, AspectName.Soul), ClassTitle.Nightbearer },
                { (AspectName.Glory, AspectName.Shadow, AspectName.Volition), ClassTitle.Dervish },
                
                { (AspectName.Glory, AspectName.Soul, AspectName.None), ClassTitle.Bloodthrall },
                { (AspectName.Glory, AspectName.Soul, AspectName.Volition), ClassTitle.Bloodreaver },
                
                { (AspectName.Glory, AspectName.Volition, AspectName.None), ClassTitle.Feral },
                
                { (AspectName.Grace, AspectName.None, AspectName.None), ClassTitle.Caretaker },

                { (AspectName.Grace, AspectName.Hunt, AspectName.None), ClassTitle.Naturalist },
                { (AspectName.Grace, AspectName.Hunt, AspectName.Involution), ClassTitle.Primeval },
                { (AspectName.Grace, AspectName.Hunt, AspectName.Shadow), ClassTitle.Penance },
                { (AspectName.Grace, AspectName.Hunt, AspectName.Soul), ClassTitle.Wraithranger },
                { (AspectName.Grace, AspectName.Hunt, AspectName.Volition), ClassTitle.Druid },
                
                { (AspectName.Grace, AspectName.Involution, AspectName.None), ClassTitle.Chaplain },
                { (AspectName.Grace, AspectName.Involution, AspectName.Shadow), ClassTitle.Mesmer },
                { (AspectName.Grace, AspectName.Involution, AspectName.Soul), ClassTitle.Cadaveric },
                { (AspectName.Grace, AspectName.Involution, AspectName.Volition), ClassTitle.Auspician },
                
                { (AspectName.Grace, AspectName.Shadow, AspectName.None), ClassTitle.Shadowcure },
                { (AspectName.Grace, AspectName.Shadow, AspectName.Soul), ClassTitle.Revenant },
                { (AspectName.Grace, AspectName.Shadow, AspectName.Volition), ClassTitle.Nightingale },
                
                { (AspectName.Grace, AspectName.Soul, AspectName.None), ClassTitle.Netheraid },
                { (AspectName.Grace, AspectName.Soul, AspectName.Volition), ClassTitle.Mortisculpt },
                
                { (AspectName.Grace, AspectName.Volition, AspectName.None), ClassTitle.Alchemist },
                
                { (AspectName.Hunt, AspectName.None, AspectName.None), ClassTitle.Sharpshot },
                
                { (AspectName.Hunt, AspectName.Involution, AspectName.None), ClassTitle.Farslayer },
                { (AspectName.Hunt, AspectName.Involution, AspectName.Shadow), ClassTitle.Trickster },
                { (AspectName.Hunt, AspectName.Involution, AspectName.Soul), ClassTitle.Deathgrim },
                { (AspectName.Hunt, AspectName.Involution, AspectName.Volition), ClassTitle.Dreadhunter },
                
                { (AspectName.Hunt, AspectName.Shadow, AspectName.None), ClassTitle.Shadestrider },
                { (AspectName.Hunt, AspectName.Shadow, AspectName.Soul), ClassTitle.Bonestalker },
                { (AspectName.Hunt, AspectName.Shadow, AspectName.Volition), ClassTitle.Infiltrator },
                
                { (AspectName.Hunt, AspectName.Soul, AspectName.None), ClassTitle.Soulbow },
                { (AspectName.Hunt, AspectName.Soul, AspectName.Volition), ClassTitle.Harvester },
                
                { (AspectName.Hunt, AspectName.Volition, AspectName.None), ClassTitle.Astralranger },
                
                { (AspectName.Involution, AspectName.None, AspectName.None), ClassTitle.Enchantrix },
                
                { (AspectName.Involution, AspectName.Shadow, AspectName.None), ClassTitle.Nightwitch },
                { (AspectName.Involution, AspectName.Shadow, AspectName.Soul), ClassTitle.Phantombinder },
                { (AspectName.Involution, AspectName.Shadow, AspectName.Volition), ClassTitle.Enigmatist },
                
                { (AspectName.Involution, AspectName.Soul, AspectName.None), ClassTitle.Shaman },
                { (AspectName.Involution, AspectName.Soul, AspectName.Volition), ClassTitle.Animist },
                
                { (AspectName.Involution, AspectName.Volition, AspectName.None), ClassTitle.Ephemeralist },
                
                { (AspectName.Shadow, AspectName.None, AspectName.None), ClassTitle.Assassin },
                
                { (AspectName.Shadow, AspectName.Soul, AspectName.None), ClassTitle.Cabalist },
                { (AspectName.Shadow, AspectName.Soul, AspectName.Volition), ClassTitle.Fleshshaper },
                
                { (AspectName.Shadow, AspectName.Volition, AspectName.None), ClassTitle.Cursedrinker },
                
                { (AspectName.Soul, AspectName.None, AspectName.None), ClassTitle.Necroscribe },
                
                { (AspectName.Soul, AspectName.Volition, AspectName.None), ClassTitle.Evoker },
                
                { (AspectName.Volition, AspectName.None, AspectName.None), ClassTitle.Philosopher }
            };
        }
    }

    public enum AspectName
    {
        None,
        Defiance, // Defence, Fortification, Fortitude
        Essence, // Sorcery, Magic
        Esthesis, // Songcraft, Artistry

        // Flow, // Swiftblade, Dual
        Glory, // Battlerage, Warfare
        Grace, // Vitalism, Devotion
        Hunt, // Archery, Wild
        Involution, // Witchcraft, Conjury

        // Malice, // Malediction, Malice
        Shadow, // Shadowplay, Finesse
        Soul, // Occultism, Necromancy
        Volition, // Auramancy, Will
    }

    public enum ClassTitle
    {
        None,
        ErrUmmWellFck,
        Aspectless,
        Abolisher,
        Abyssalich,
        Aethermend,
        Alchemist,
        Animist,
        Arcanehunter,
        Arcanist,
        Archivist,
        Archon,
        Argent,
        Assassin,
        Astralranger,
        Athame,
        Auspician,
        Bastion,
        Battlemage,
        Battlebow,
        Blackguard,
        Bladedancer,
        Blighter,
        Bloodarrow,
        Bloodreaver,
        Bloodskald,
        Bloodthrall,
        Boneweaver,
        Bonestalker,
        Bowdancer,
        Brightbow,
        Cabalist,
        Cadaveric,
        Caretaker,
        Catharsis,
        Cavestalker,
        Chaplain,
        Chaotician,
        Cleric,
        Confessor,
        Crusader,
        Cursedrinker,
        Custodian,
        Daggerspell,
        Darkaegis,
        Darkrunner,
        Darksong,
        Darkstring,
        Dawncaller,
        Dawnsentinel,
        Deathgrim,
        Deathwarden,
        Defender,
        Defiler,
        Demonologist,
        Dervish,
        Doombringer,
        Doomlord,
        Dreadblade,
        Dreadhunter,
        Dreadnaught,
        Dreadstone,
        Dreambreaker,
        Druid,
        Duelist,
        Earthsinger,
        Ebonsong,
        Ectomancer,
        Edgewalker,
        Eidolon,
        Elegist,
        Eldritch_Shephard,
        Enchantrix,
        Enforcer,
        Enigmatist,
        Enthraller,
        Ephemeralist,
        Epiphanist,
        Etherweaver,
        Euphonic,
        Euphoric,
        Evoker,
        Executioner,
        Exorcist,
        Farslayer,
        Feral,
        Fiendhunter,
        Fleshshaper,
        Gaian,
        Gravesinger,
        Guardian,
        Gypsy,
        Hallowblaze,
        Harbinger,
        Harvester,
        Haunter,
        Hellweaver,
        Herald,
        Hexblade,
        Hexranger,
        Hexwarden,
        Hierophant,
        Honorblade,
        Honourguard,
        Hordebreaker,
        Howler,
        Hymnguard,
        Immortus,
        Infiltrator,
        Inquisitor,
        Invocator,
        Ironbard,
        Jacknife,
        Justicar,
        Lamentor,
        Lightstrider,
        Mesmer,
        Mortisculpt,
        Mourningchorus,
        Naturalist,
        Necroetherist,
        Necroharmonist,
        Necroscribe,
        Netherwarden,
        Netheraid,
        Nightingale,
        Nightbearer,
        Nightcloak,
        Nightwitch,
        Nocturneanthem,
        Nocturl,
        Oracle,
        Outrider,
        Paladin,
        Penance,
        Phantasm,
        Phantombinder,
        Philosopher,
        Planeshifter,
        Poxbane,
        Poxranger,
        Primeval,
        Purifier,
        Ranger,
        Reaper,
        Requiem,
        Revenant,
        Scion,
        Seraphim,
        Sharpshot,
        Shadoward,
        Shadowbane,
        Shadowcantor,
        Shadowcure,
        Shadowknight,
        Shadestrider,
        Shaman,
        Silentpsalm,
        Siren,
        Skullknight,
        Skulltaker,
        Soldier,
        Soulbow,
        Soulsinger,
        Soulsong,
        Soulwrought,
        Soothsayer,
        Sorrowsong,
        Spellsinger,
        Spellsword,
        Stormcaller,
        Templar,
        Thaumaturge,
        Tombcaller,
        Tombwarden,
        Traumapothicar,
        Trickster,
        Twilightkeeper,
        Vulgarist,
        Wanderer,
        Warpriest,
        Witcher,
        Worldwalker,
        Woundwarden,
        Wraithranger,
        Zenith
    }
}