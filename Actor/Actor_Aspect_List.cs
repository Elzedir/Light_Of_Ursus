using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public enum AspectName
    {
        None,
        Defiance, // Defence, Fortification, Fortitude
        Essence,  // Sorcery, Magic
        Esthesis, // Songcraft, Artistry
        // Flow, // Swiftblade, Dual
        Glory,      // Battlerage, Warfare
        Grace,      // Vitalism, Devotion
        Hunt,       // Archery, Wild
        Involution, // Witchcraft, Conjury
        // Malice, // Malediction, Malice
        Shadow,   // Shadowplay, Finesse
        Soul,     // Occultism, Necromancy
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
        Arcanehunter, Arcanist,
        Archivist,
        Archon,
        Argent,
        Assassin,
        Astralranger,
        Athame,
        Auspician,
        Bastion,
        Battlemage, Battlebow,
        Blackguard,
        Bladedancer,
        Blighter,
        Bloodarrow, Bloodreaver, Bloodskald, Bloodthrall,
        Boneweaver, Bonestalker,
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
        Darkaegis, Darkrunner, Darksong, Darkstring,
        Dawncaller, Dawnsentinel,
        Deathgrim, Deathwarden,
        Defender,
        Defiler,
        Demonologist,
        Dervish,
        Doombringer, Doomlord,
        Dreadblade, Dreadhunter, Dreadnaught, Dreadstone,
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
        Hexblade, Hexranger, Hexwarden,
        Hierophant,
        Honorblade, Honourguard,
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
        Necroetherist, Necroharmonist, Necroscribe,
        Netherwarden, Netheraid,
        Nightingale, Nightbearer, Nightcloak, Nightwitch,
        Nocturneanthem, Nocturl,
        Oracle,
        Outrider,
        Paladin,
        Penance,
        Phantasm, Phantombinder,
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
        Shadoward, Shadowbane, Shadowcantor, Shadowcure, Shadowknight,
        Shadestrider,
        Shaman,
        Silentpsalm,
        Siren,
        Skullknight, Skulltaker,
        Soldier,
        Soulbow, Soulsinger, Soulsong, Soulwrought,
        Soothsayer,
        Sorrowsong,
        Spellsinger, Spellsword,
        Stormcaller,
        Templar,
        Thaumaturge,
        Tombcaller, Tombwarden,
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

    public class Actor_Aspect_List // Can turn this into an SO eventually since it will be static.
    {
        public static Dictionary<(AspectName, AspectName, AspectName), ClassTitle> AllClassList { get; private set; } = new();

        public static void InitialiseSpecialisations()
        {
            AllClassList.Add((AspectName.None, AspectName.None, AspectName.None), ClassTitle.Aspectless);

            AllClassList.Add((AspectName.Defiance, AspectName.None, AspectName.None), ClassTitle.Defender);
        
            AllClassList.Add((AspectName.Defiance, AspectName.Essence, AspectName.None),       ClassTitle.Exorcist);
            AllClassList.Add((AspectName.Defiance, AspectName.Essence, AspectName.Esthesis),   ClassTitle.Silentpsalm);
            AllClassList.Add((AspectName.Defiance, AspectName.Essence, AspectName.Glory),      ClassTitle.Woundwarden);
            AllClassList.Add((AspectName.Defiance, AspectName.Essence, AspectName.Grace),      ClassTitle.Hallowblaze);
            AllClassList.Add((AspectName.Defiance, AspectName.Essence, AspectName.Hunt),       ClassTitle.Deathwarden);
            AllClassList.Add((AspectName.Defiance, AspectName.Essence, AspectName.Involution), ClassTitle.Hexwarden);
            AllClassList.Add((AspectName.Defiance, AspectName.Essence, AspectName.Shadow),     ClassTitle.Shadoward);
            AllClassList.Add((AspectName.Defiance, AspectName.Essence, AspectName.Soul),       ClassTitle.Tombwarden);
            AllClassList.Add((AspectName.Defiance, AspectName.Essence, AspectName.Volition),   ClassTitle.Zenith);

            AllClassList.Add((AspectName.Defiance, AspectName.Esthesis, AspectName.None),       ClassTitle.Ironbard);
            AllClassList.Add((AspectName.Defiance, AspectName.Esthesis, AspectName.Glory),      ClassTitle.Justicar);
            AllClassList.Add((AspectName.Defiance, AspectName.Esthesis, AspectName.Grace),      ClassTitle.Hymnguard);
            AllClassList.Add((AspectName.Defiance, AspectName.Esthesis, AspectName.Hunt),       ClassTitle.Argent);
            AllClassList.Add((AspectName.Defiance, AspectName.Esthesis, AspectName.Involution), ClassTitle.Earthsinger);
            AllClassList.Add((AspectName.Defiance, AspectName.Esthesis, AspectName.Shadow),     ClassTitle.Nocturneanthem);
            AllClassList.Add((AspectName.Defiance, AspectName.Esthesis, AspectName.Soul),       ClassTitle.Tombcaller);
            AllClassList.Add((AspectName.Defiance, AspectName.Esthesis, AspectName.Volition),   ClassTitle.Elegist);

            AllClassList.Add((AspectName.Defiance, AspectName.Glory, AspectName.None),       ClassTitle.Bastion);
            AllClassList.Add((AspectName.Defiance, AspectName.Glory, AspectName.Grace),      ClassTitle.Templar);
            AllClassList.Add((AspectName.Defiance, AspectName.Glory, AspectName.Hunt),       ClassTitle.Hordebreaker);
            AllClassList.Add((AspectName.Defiance, AspectName.Glory, AspectName.Involution), ClassTitle.Crusader);
            AllClassList.Add((AspectName.Defiance, AspectName.Glory, AspectName.Shadow),     ClassTitle.Enforcer);
            AllClassList.Add((AspectName.Defiance, AspectName.Glory, AspectName.Soul),       ClassTitle.Skullknight);
            AllClassList.Add((AspectName.Defiance, AspectName.Glory, AspectName.Volition),   ClassTitle.Honourguard);

            AllClassList.Add((AspectName.Defiance, AspectName.Grace, AspectName.None),       ClassTitle.Guardian);
            AllClassList.Add((AspectName.Defiance, AspectName.Grace, AspectName.Hunt),       ClassTitle.Dawnsentinel);
            AllClassList.Add((AspectName.Defiance, AspectName.Grace, AspectName.Involution), ClassTitle.Hierophant);
            AllClassList.Add((AspectName.Defiance, AspectName.Grace, AspectName.Shadow),     ClassTitle.Aethermend);
            AllClassList.Add((AspectName.Defiance, AspectName.Grace, AspectName.Soul),       ClassTitle.Eldritch_Shephard);
            AllClassList.Add((AspectName.Defiance, AspectName.Grace, AspectName.Volition),   ClassTitle.Paladin);

            AllClassList.Add((AspectName.Defiance, AspectName.Hunt, AspectName.None),       ClassTitle.Dreadstone);
            AllClassList.Add((AspectName.Defiance, AspectName.Hunt, AspectName.Involution), ClassTitle.Battlebow);
            AllClassList.Add((AspectName.Defiance, AspectName.Hunt, AspectName.Shadow),     ClassTitle.Haunter);
            AllClassList.Add((AspectName.Defiance, AspectName.Hunt, AspectName.Soul),       ClassTitle.Immortus);
            AllClassList.Add((AspectName.Defiance, AspectName.Hunt, AspectName.Volition),   ClassTitle.Dreadnaught);
        
            AllClassList.Add((AspectName.Defiance, AspectName.Involution, AspectName.None),     ClassTitle.Enthraller);
            AllClassList.Add((AspectName.Defiance, AspectName.Involution, AspectName.Shadow),   ClassTitle.Darkaegis);
            AllClassList.Add((AspectName.Defiance, AspectName.Involution, AspectName.Soul),     ClassTitle.Abyssalich);
            AllClassList.Add((AspectName.Defiance, AspectName.Involution, AspectName.Volition), ClassTitle.Archon);
        
            AllClassList.Add((AspectName.Defiance, AspectName.Shadow, AspectName.None),     ClassTitle.Blackguard);
            AllClassList.Add((AspectName.Defiance, AspectName.Shadow, AspectName.Soul),     ClassTitle.Netherwarden);
            AllClassList.Add((AspectName.Defiance, AspectName.Shadow, AspectName.Volition), ClassTitle.Shadowknight);
        
            AllClassList.Add((AspectName.Defiance, AspectName.Soul, AspectName.None),     ClassTitle.Boneweaver);
            AllClassList.Add((AspectName.Defiance, AspectName.Soul, AspectName.Volition), ClassTitle.Soulwrought);
        
            AllClassList.Add((AspectName.Defiance, AspectName.Volition, AspectName.None), ClassTitle.Custodian);
        
            AllClassList.Add((AspectName.Essence, AspectName.None, AspectName.None), ClassTitle.Thaumaturge);
        
            AllClassList.Add((AspectName.Essence, AspectName.Esthesis, AspectName.None),       ClassTitle.Spellsinger);
            AllClassList.Add((AspectName.Essence, AspectName.Esthesis, AspectName.Glory),      ClassTitle.Hexblade);
            AllClassList.Add((AspectName.Essence, AspectName.Esthesis, AspectName.Grace),      ClassTitle.Mourningchorus);
            AllClassList.Add((AspectName.Essence, AspectName.Esthesis, AspectName.Hunt),       ClassTitle.Ebonsong);
            AllClassList.Add((AspectName.Essence, AspectName.Esthesis, AspectName.Involution), ClassTitle.Stormcaller);
            AllClassList.Add((AspectName.Essence, AspectName.Esthesis, AspectName.Shadow),     ClassTitle.Blighter);
            AllClassList.Add((AspectName.Essence, AspectName.Esthesis, AspectName.Soul),       ClassTitle.Scion);
            AllClassList.Add((AspectName.Essence, AspectName.Esthesis, AspectName.Volition),   ClassTitle.Dawncaller);

            AllClassList.Add((AspectName.Essence, AspectName.Glory, AspectName.None),       ClassTitle.Spellsword);
            AllClassList.Add((AspectName.Essence, AspectName.Glory, AspectName.Grace),      ClassTitle.Witcher);
            AllClassList.Add((AspectName.Essence, AspectName.Glory, AspectName.Hunt),       ClassTitle.Doombringer);
            AllClassList.Add((AspectName.Essence, AspectName.Glory, AspectName.Involution), ClassTitle.Battlemage);
            AllClassList.Add((AspectName.Essence, AspectName.Glory, AspectName.Shadow),     ClassTitle.Shadowbane);
            AllClassList.Add((AspectName.Essence, AspectName.Glory, AspectName.Soul),       ClassTitle.Harbinger);
            AllClassList.Add((AspectName.Essence, AspectName.Glory, AspectName.Volition),   ClassTitle.Inquisitor);
        
            AllClassList.Add((AspectName.Essence, AspectName.Grace, AspectName.None),       ClassTitle.Traumapothicar);
            AllClassList.Add((AspectName.Essence, AspectName.Grace, AspectName.Hunt),       ClassTitle.Brightbow);
            AllClassList.Add((AspectName.Essence, AspectName.Grace, AspectName.Involution), ClassTitle.Poxbane);
            AllClassList.Add((AspectName.Essence, AspectName.Grace, AspectName.Shadow),     ClassTitle.Nightcloak);
            AllClassList.Add((AspectName.Essence, AspectName.Grace, AspectName.Soul),       ClassTitle.Eidolon);
            AllClassList.Add((AspectName.Essence, AspectName.Grace, AspectName.Volition),   ClassTitle.Purifier);
        
            AllClassList.Add((AspectName.Essence, AspectName.Hunt, AspectName.None),       ClassTitle.Arcanehunter);
            AllClassList.Add((AspectName.Essence, AspectName.Hunt, AspectName.Involution), ClassTitle.Hexranger);
            AllClassList.Add((AspectName.Essence, AspectName.Hunt, AspectName.Shadow),     ClassTitle.Nocturl);
            AllClassList.Add((AspectName.Essence, AspectName.Hunt, AspectName.Soul),       ClassTitle.Reaper);
            AllClassList.Add((AspectName.Essence, AspectName.Hunt, AspectName.Volition),   ClassTitle.Fiendhunter);
        
            AllClassList.Add((AspectName.Essence, AspectName.Involution, AspectName.None),     ClassTitle.Arcanist);
            AllClassList.Add((AspectName.Essence, AspectName.Involution, AspectName.Shadow),   ClassTitle.Hellweaver);
            AllClassList.Add((AspectName.Essence, AspectName.Involution, AspectName.Soul),     ClassTitle.Athame);
            AllClassList.Add((AspectName.Essence, AspectName.Involution, AspectName.Volition), ClassTitle.Chaotician);

            AllClassList.Add((AspectName.Essence, AspectName.Shadow, AspectName.None),     ClassTitle.Daggerspell);
            AllClassList.Add((AspectName.Essence, AspectName.Shadow, AspectName.Soul),     ClassTitle.Necroetherist);
            AllClassList.Add((AspectName.Essence, AspectName.Shadow, AspectName.Volition), ClassTitle.Planeshifter);
        
            AllClassList.Add((AspectName.Essence, AspectName.Soul, AspectName.None),     ClassTitle.Phantasm);
            AllClassList.Add((AspectName.Essence, AspectName.Soul, AspectName.Volition), ClassTitle.Demonologist);
        
            AllClassList.Add((AspectName.Essence, AspectName.Volition, AspectName.None), ClassTitle.Archivist);
        
            AllClassList.Add((AspectName.Esthesis, AspectName.None, AspectName.None), ClassTitle.Euphonic);
        
            AllClassList.Add((AspectName.Esthesis, AspectName.Glory, AspectName.None),       ClassTitle.Duelist);
            AllClassList.Add((AspectName.Esthesis, AspectName.Glory, AspectName.Grace),      ClassTitle.Sorrowsong);
            AllClassList.Add((AspectName.Esthesis, AspectName.Glory, AspectName.Hunt),       ClassTitle.Worldwalker);
            AllClassList.Add((AspectName.Esthesis, AspectName.Glory, AspectName.Involution), ClassTitle.Vulgarist);
            AllClassList.Add((AspectName.Esthesis, AspectName.Glory, AspectName.Shadow),     ClassTitle.Edgewalker);
            AllClassList.Add((AspectName.Esthesis, AspectName.Glory, AspectName.Soul),       ClassTitle.Bloodskald);
            AllClassList.Add((AspectName.Esthesis, AspectName.Glory, AspectName.Volition),   ClassTitle.Bladedancer);
        
            AllClassList.Add((AspectName.Esthesis, AspectName.Grace, AspectName.None),       ClassTitle.Epiphanist);
            AllClassList.Add((AspectName.Esthesis, AspectName.Grace, AspectName.Hunt),       ClassTitle.Herald);
            AllClassList.Add((AspectName.Esthesis, AspectName.Grace, AspectName.Involution), ClassTitle.Oracle);
            AllClassList.Add((AspectName.Esthesis, AspectName.Grace, AspectName.Shadow),     ClassTitle.Shadowcantor);
            AllClassList.Add((AspectName.Esthesis, AspectName.Grace, AspectName.Soul),       ClassTitle.Soulsong);
            AllClassList.Add((AspectName.Esthesis, AspectName.Grace, AspectName.Volition),   ClassTitle.Seraphim);

            AllClassList.Add((AspectName.Esthesis, AspectName.Hunt, AspectName.None),       ClassTitle.Wanderer);
            AllClassList.Add((AspectName.Esthesis, AspectName.Hunt, AspectName.Involution), ClassTitle.Bowdancer);
            AllClassList.Add((AspectName.Esthesis, AspectName.Hunt, AspectName.Shadow),     ClassTitle.Darkstring);
            AllClassList.Add((AspectName.Esthesis, AspectName.Hunt, AspectName.Soul),       ClassTitle.Gravesinger);
            AllClassList.Add((AspectName.Esthesis, AspectName.Hunt, AspectName.Volition),   ClassTitle.Gaian);

            AllClassList.Add((AspectName.Esthesis, AspectName.Involution, AspectName.None),     ClassTitle.Howler);
            AllClassList.Add((AspectName.Esthesis, AspectName.Involution, AspectName.Shadow),   ClassTitle.Siren);
            AllClassList.Add((AspectName.Esthesis, AspectName.Involution, AspectName.Soul),     ClassTitle.Necroharmonist);
            AllClassList.Add((AspectName.Esthesis, AspectName.Involution, AspectName.Volition), ClassTitle.Dreambreaker);

            AllClassList.Add((AspectName.Esthesis, AspectName.Shadow, AspectName.None),     ClassTitle.Gypsy);
            AllClassList.Add((AspectName.Esthesis, AspectName.Shadow, AspectName.Soul),     ClassTitle.Requiem);
            AllClassList.Add((AspectName.Esthesis, AspectName.Shadow, AspectName.Volition), ClassTitle.Etherweaver);

            AllClassList.Add((AspectName.Esthesis, AspectName.Soul, AspectName.None),     ClassTitle.Soulsinger);
            AllClassList.Add((AspectName.Esthesis, AspectName.Soul, AspectName.Volition), ClassTitle.Lamentor);

            AllClassList.Add((AspectName.Esthesis, AspectName.Volition, AspectName.None), ClassTitle.Soothsayer);

            AllClassList.Add((AspectName.Glory, AspectName.None, AspectName.None), ClassTitle.Soldier);

            AllClassList.Add((AspectName.Glory, AspectName.Grace, AspectName.None),       ClassTitle.Cleric);
            AllClassList.Add((AspectName.Glory, AspectName.Grace, AspectName.Hunt),       ClassTitle.Lightstrider);
            AllClassList.Add((AspectName.Glory, AspectName.Grace, AspectName.Involution), ClassTitle.Confessor);
            AllClassList.Add((AspectName.Glory, AspectName.Grace, AspectName.Shadow),     ClassTitle.Twilightkeeper);
            AllClassList.Add((AspectName.Glory, AspectName.Grace, AspectName.Soul),       ClassTitle.Defiler);
            AllClassList.Add((AspectName.Glory, AspectName.Grace, AspectName.Volition),   ClassTitle.Warpriest);

            AllClassList.Add((AspectName.Glory, AspectName.Hunt, AspectName.None),       ClassTitle.Ranger);
            AllClassList.Add((AspectName.Glory, AspectName.Hunt, AspectName.Involution), ClassTitle.Doomlord);
            AllClassList.Add((AspectName.Glory, AspectName.Hunt, AspectName.Shadow),     ClassTitle.Darkrunner);
            AllClassList.Add((AspectName.Glory, AspectName.Hunt, AspectName.Soul),       ClassTitle.Bloodarrow);
            AllClassList.Add((AspectName.Glory, AspectName.Hunt, AspectName.Volition),   ClassTitle.Outrider);

            AllClassList.Add((AspectName.Glory, AspectName.Involution, AspectName.None),     ClassTitle.Dreadblade);
            AllClassList.Add((AspectName.Glory, AspectName.Involution, AspectName.Shadow),   ClassTitle.Executioner);
            AllClassList.Add((AspectName.Glory, AspectName.Involution, AspectName.Soul),     ClassTitle.Skulltaker);
            AllClassList.Add((AspectName.Glory, AspectName.Involution, AspectName.Volition), ClassTitle.Abolisher);

            AllClassList.Add((AspectName.Glory, AspectName.Shadow, AspectName.None),     ClassTitle.Jacknife);
            AllClassList.Add((AspectName.Glory, AspectName.Shadow, AspectName.Soul),     ClassTitle.Nightbearer);
            AllClassList.Add((AspectName.Glory, AspectName.Shadow, AspectName.Volition), ClassTitle.Dervish);

            AllClassList.Add((AspectName.Glory, AspectName.Soul, AspectName.None),     ClassTitle.Bloodthrall);
            AllClassList.Add((AspectName.Glory, AspectName.Soul, AspectName.Volition), ClassTitle.Bloodreaver);

            AllClassList.Add((AspectName.Glory, AspectName.Volition, AspectName.None), ClassTitle.Feral);

            AllClassList.Add((AspectName.Grace, AspectName.None, AspectName.None), ClassTitle.Caretaker);

            AllClassList.Add((AspectName.Grace, AspectName.Hunt, AspectName.None),       ClassTitle.Naturalist);
            AllClassList.Add((AspectName.Grace, AspectName.Hunt, AspectName.Involution), ClassTitle.Primeval);
            AllClassList.Add((AspectName.Grace, AspectName.Hunt, AspectName.Shadow),     ClassTitle.Penance);
            AllClassList.Add((AspectName.Grace, AspectName.Hunt, AspectName.Soul),       ClassTitle.Wraithranger);
            AllClassList.Add((AspectName.Grace, AspectName.Hunt, AspectName.Volition),   ClassTitle.Druid);

            AllClassList.Add((AspectName.Grace, AspectName.Involution, AspectName.None),     ClassTitle.Chaplain);
            AllClassList.Add((AspectName.Grace, AspectName.Involution, AspectName.Shadow),   ClassTitle.Mesmer);
            AllClassList.Add((AspectName.Grace, AspectName.Involution, AspectName.Soul),     ClassTitle.Cadaveric);
            AllClassList.Add((AspectName.Grace, AspectName.Involution, AspectName.Volition), ClassTitle.Auspician);

            AllClassList.Add((AspectName.Grace, AspectName.Shadow, AspectName.None),     ClassTitle.Shadowcure);
            AllClassList.Add((AspectName.Grace, AspectName.Shadow, AspectName.Soul),     ClassTitle.Revenant);
            AllClassList.Add((AspectName.Grace, AspectName.Shadow, AspectName.Volition), ClassTitle.Nightingale);

            AllClassList.Add((AspectName.Grace, AspectName.Soul, AspectName.None),     ClassTitle.Netheraid);
            AllClassList.Add((AspectName.Grace, AspectName.Soul, AspectName.Volition), ClassTitle.Mortisculpt);

            AllClassList.Add((AspectName.Grace, AspectName.Volition, AspectName.None), ClassTitle.Alchemist);

            AllClassList.Add((AspectName.Hunt, AspectName.None, AspectName.None), ClassTitle.Sharpshot);

            AllClassList.Add((AspectName.Hunt, AspectName.Involution, AspectName.None),     ClassTitle.Farslayer);
            AllClassList.Add((AspectName.Hunt, AspectName.Involution, AspectName.Shadow),   ClassTitle.Trickster);
            AllClassList.Add((AspectName.Hunt, AspectName.Involution, AspectName.Soul),     ClassTitle.Deathgrim);
            AllClassList.Add((AspectName.Hunt, AspectName.Involution, AspectName.Volition), ClassTitle.Dreadhunter);
        
            AllClassList.Add((AspectName.Hunt, AspectName.Shadow, AspectName.None),     ClassTitle.Shadestrider);
            AllClassList.Add((AspectName.Hunt, AspectName.Shadow, AspectName.Soul),     ClassTitle.Bonestalker);
            AllClassList.Add((AspectName.Hunt, AspectName.Shadow, AspectName.Volition), ClassTitle.Infiltrator);
        
            AllClassList.Add((AspectName.Hunt, AspectName.Soul, AspectName.None),     ClassTitle.Soulbow);
            AllClassList.Add((AspectName.Hunt, AspectName.Soul, AspectName.Volition), ClassTitle.Harvester);
        
            AllClassList.Add((AspectName.Hunt, AspectName.Volition, AspectName.None), ClassTitle.Astralranger);
        
            AllClassList.Add((AspectName.Involution, AspectName.None, AspectName.None), ClassTitle.Enchantrix);
        
            AllClassList.Add((AspectName.Involution, AspectName.Shadow, AspectName.None),     ClassTitle.Nightwitch);
            AllClassList.Add((AspectName.Involution, AspectName.Shadow, AspectName.Soul),     ClassTitle.Phantombinder);
            AllClassList.Add((AspectName.Involution, AspectName.Shadow, AspectName.Volition), ClassTitle.Enigmatist);
        
            AllClassList.Add((AspectName.Involution, AspectName.Soul, AspectName.None),     ClassTitle.Shaman);
            AllClassList.Add((AspectName.Involution, AspectName.Soul, AspectName.Volition), ClassTitle.Animist);
        
            AllClassList.Add((AspectName.Involution, AspectName.Volition, AspectName.None), ClassTitle.Ephemeralist);
        
            AllClassList.Add((AspectName.Shadow, AspectName.None, AspectName.None), ClassTitle.Assassin);
        
            AllClassList.Add((AspectName.Shadow, AspectName.Soul, AspectName.None),     ClassTitle.Cabalist);
            AllClassList.Add((AspectName.Shadow, AspectName.Soul, AspectName.Volition), ClassTitle.Fleshshaper);
        
            AllClassList.Add((AspectName.Shadow, AspectName.Volition, AspectName.None), ClassTitle.Cursedrinker);
        
            AllClassList.Add((AspectName.Soul, AspectName.None, AspectName.None), ClassTitle.Necroscribe);
        
            AllClassList.Add((AspectName.Soul, AspectName.Volition, AspectName.None), ClassTitle.Evoker);
        
            AllClassList.Add((AspectName.Volition, AspectName.None, AspectName.None), ClassTitle.Philosopher);
        }

        public static ClassTitle GetCharacterTitle(List<AspectName> aspectList)
        {
            if (AllClassList.TryGetValue((aspectList[0], aspectList[1], aspectList[2]), out var title))
            {
                return title;
            }

            Debug.LogWarning("No class found for aspects: " + aspectList[0] + ", " + aspectList[1] + ", " + aspectList[2]);
            return ClassTitle.None;
        }
    }
}