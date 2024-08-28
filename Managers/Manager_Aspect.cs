using System.Collections.Generic;

public enum AspectName
{
    None,
    Defiance, // Defence, Fortification
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

public enum ClassName
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

public class Manager_Aspect // Can turn this into an SO eventually since it will be static.
{
    public static Dictionary<(AspectName, AspectName, AspectName), ClassName> AllClassList { get; private set; } = new();

    public static void InitialiseSpecialisations()
    {
        AllClassList.Add((AspectName.None, AspectName.None, AspectName.None), ClassName.Aspectless);

        AllClassList.Add((AspectName.Defiance, AspectName.None, AspectName.None), ClassName.Defender);
        
        AllClassList.Add((AspectName.Defiance, AspectName.Essence, AspectName.None), ClassName.Exorcist);
        AllClassList.Add((AspectName.Defiance, AspectName.Essence, AspectName.Esthesis), ClassName.Silentpsalm);
        AllClassList.Add((AspectName.Defiance, AspectName.Essence, AspectName.Glory), ClassName.Woundwarden);
        AllClassList.Add((AspectName.Defiance, AspectName.Essence, AspectName.Grace), ClassName.Hallowblaze);
        AllClassList.Add((AspectName.Defiance, AspectName.Essence, AspectName.Hunt), ClassName.Deathwarden);
        AllClassList.Add((AspectName.Defiance, AspectName.Essence, AspectName.Involution), ClassName.Hexwarden);
        AllClassList.Add((AspectName.Defiance, AspectName.Essence, AspectName.Shadow), ClassName.Shadoward);
        AllClassList.Add((AspectName.Defiance, AspectName.Essence, AspectName.Soul), ClassName.Tombwarden);
        AllClassList.Add((AspectName.Defiance, AspectName.Essence, AspectName.Volition), ClassName.Zenith);

        AllClassList.Add((AspectName.Defiance, AspectName.Esthesis, AspectName.None), ClassName.Ironbard);
        AllClassList.Add((AspectName.Defiance, AspectName.Esthesis, AspectName.Glory), ClassName.Justicar);
        AllClassList.Add((AspectName.Defiance, AspectName.Esthesis, AspectName.Grace), ClassName.Hymnguard);
        AllClassList.Add((AspectName.Defiance, AspectName.Esthesis, AspectName.Hunt), ClassName.Argent);
        AllClassList.Add((AspectName.Defiance, AspectName.Esthesis, AspectName.Involution), ClassName.Earthsinger);
        AllClassList.Add((AspectName.Defiance, AspectName.Esthesis, AspectName.Shadow), ClassName.Nocturneanthem);
        AllClassList.Add((AspectName.Defiance, AspectName.Esthesis, AspectName.Soul), ClassName.Tombcaller);
        AllClassList.Add((AspectName.Defiance, AspectName.Esthesis, AspectName.Volition), ClassName.Elegist);

        AllClassList.Add((AspectName.Defiance, AspectName.Glory, AspectName.None), ClassName.Bastion);
        AllClassList.Add((AspectName.Defiance, AspectName.Glory, AspectName.Grace), ClassName.Templar);
        AllClassList.Add((AspectName.Defiance, AspectName.Glory, AspectName.Hunt), ClassName.Hordebreaker);
        AllClassList.Add((AspectName.Defiance, AspectName.Glory, AspectName.Involution), ClassName.Crusader);
        AllClassList.Add((AspectName.Defiance, AspectName.Glory, AspectName.Shadow), ClassName.Enforcer);
        AllClassList.Add((AspectName.Defiance, AspectName.Glory, AspectName.Soul), ClassName.Skullknight);
        AllClassList.Add((AspectName.Defiance, AspectName.Glory, AspectName.Volition), ClassName.Honourguard);

        AllClassList.Add((AspectName.Defiance, AspectName.Grace, AspectName.None), ClassName.Guardian);
        AllClassList.Add((AspectName.Defiance, AspectName.Grace, AspectName.Hunt), ClassName.Dawnsentinel);
        AllClassList.Add((AspectName.Defiance, AspectName.Grace, AspectName.Involution), ClassName.Hierophant);
        AllClassList.Add((AspectName.Defiance, AspectName.Grace, AspectName.Shadow), ClassName.Aethermend);
        AllClassList.Add((AspectName.Defiance, AspectName.Grace, AspectName.Soul), ClassName.Eldritch_Shephard);
        AllClassList.Add((AspectName.Defiance, AspectName.Grace, AspectName.Volition), ClassName.Paladin);

        AllClassList.Add((AspectName.Defiance, AspectName.Hunt, AspectName.None), ClassName.Dreadstone);
        AllClassList.Add((AspectName.Defiance, AspectName.Hunt, AspectName.Involution), ClassName.Battlebow);
        AllClassList.Add((AspectName.Defiance, AspectName.Hunt, AspectName.Shadow), ClassName.Haunter);
        AllClassList.Add((AspectName.Defiance, AspectName.Hunt, AspectName.Soul), ClassName.Immortus);
        AllClassList.Add((AspectName.Defiance, AspectName.Hunt, AspectName.Volition), ClassName.Dreadnaught);
        
        AllClassList.Add((AspectName.Defiance, AspectName.Involution, AspectName.None), ClassName.Enthraller);
        AllClassList.Add((AspectName.Defiance, AspectName.Involution, AspectName.Shadow), ClassName.Darkaegis);
        AllClassList.Add((AspectName.Defiance, AspectName.Involution, AspectName.Soul), ClassName.Abyssalich);
        AllClassList.Add((AspectName.Defiance, AspectName.Involution, AspectName.Volition), ClassName.Archon);
        
        AllClassList.Add((AspectName.Defiance, AspectName.Shadow, AspectName.None), ClassName.Blackguard);
        AllClassList.Add((AspectName.Defiance, AspectName.Shadow, AspectName.Soul), ClassName.Netherwarden);
        AllClassList.Add((AspectName.Defiance, AspectName.Shadow, AspectName.Volition), ClassName.Shadowknight);
        
        AllClassList.Add((AspectName.Defiance, AspectName.Soul, AspectName.None), ClassName.Boneweaver);
        AllClassList.Add((AspectName.Defiance, AspectName.Soul, AspectName.Volition), ClassName.Soulwrought);
        
        AllClassList.Add((AspectName.Defiance, AspectName.Volition, AspectName.None), ClassName.Custodian);
        
        AllClassList.Add((AspectName.Essence, AspectName.None, AspectName.None), ClassName.Thaumaturge);
        
        AllClassList.Add((AspectName.Essence, AspectName.Esthesis, AspectName.None), ClassName.Spellsinger);
        AllClassList.Add((AspectName.Essence, AspectName.Esthesis, AspectName.Glory), ClassName.Hexblade);
        AllClassList.Add((AspectName.Essence, AspectName.Esthesis, AspectName.Grace), ClassName.Mourningchorus);
        AllClassList.Add((AspectName.Essence, AspectName.Esthesis, AspectName.Hunt), ClassName.Ebonsong);
        AllClassList.Add((AspectName.Essence, AspectName.Esthesis, AspectName.Involution), ClassName.Stormcaller);
        AllClassList.Add((AspectName.Essence, AspectName.Esthesis, AspectName.Shadow), ClassName.Blighter);
        AllClassList.Add((AspectName.Essence, AspectName.Esthesis, AspectName.Soul), ClassName.Scion);
        AllClassList.Add((AspectName.Essence, AspectName.Esthesis, AspectName.Volition), ClassName.Dawncaller);

        AllClassList.Add((AspectName.Essence, AspectName.Glory, AspectName.None), ClassName.Spellsword);
        AllClassList.Add((AspectName.Essence, AspectName.Glory, AspectName.Grace), ClassName.Witcher);
        AllClassList.Add((AspectName.Essence, AspectName.Glory, AspectName.Hunt), ClassName.Doombringer);
        AllClassList.Add((AspectName.Essence, AspectName.Glory, AspectName.Involution), ClassName.Battlemage);
        AllClassList.Add((AspectName.Essence, AspectName.Glory, AspectName.Shadow), ClassName.Shadowbane);
        AllClassList.Add((AspectName.Essence, AspectName.Glory, AspectName.Soul), ClassName.Harbinger);
        AllClassList.Add((AspectName.Essence, AspectName.Glory, AspectName.Volition), ClassName.Inquisitor);
        
        AllClassList.Add((AspectName.Essence, AspectName.Grace, AspectName.None), ClassName.Traumapothicar);
        AllClassList.Add((AspectName.Essence, AspectName.Grace, AspectName.Hunt), ClassName.Brightbow);
        AllClassList.Add((AspectName.Essence, AspectName.Grace, AspectName.Involution), ClassName.Poxbane);
        AllClassList.Add((AspectName.Essence, AspectName.Grace, AspectName.Shadow), ClassName.Nightcloak);
        AllClassList.Add((AspectName.Essence, AspectName.Grace, AspectName.Soul), ClassName.Eidolon);
        AllClassList.Add((AspectName.Essence, AspectName.Grace, AspectName.Volition), ClassName.Purifier);
        
        AllClassList.Add((AspectName.Essence, AspectName.Hunt, AspectName.None), ClassName.Arcanehunter);
        AllClassList.Add((AspectName.Essence, AspectName.Hunt, AspectName.Involution), ClassName.Hexranger);
        AllClassList.Add((AspectName.Essence, AspectName.Hunt, AspectName.Shadow), ClassName.Nocturl);
        AllClassList.Add((AspectName.Essence, AspectName.Hunt, AspectName.Soul), ClassName.Reaper);
        AllClassList.Add((AspectName.Essence, AspectName.Hunt, AspectName.Volition), ClassName.Fiendhunter);
        
        AllClassList.Add((AspectName.Essence, AspectName.Involution, AspectName.None), ClassName.Arcanist);
        AllClassList.Add((AspectName.Essence, AspectName.Involution, AspectName.Shadow), ClassName.Hellweaver);
        AllClassList.Add((AspectName.Essence, AspectName.Involution, AspectName.Soul), ClassName.Athame);
        AllClassList.Add((AspectName.Essence, AspectName.Involution, AspectName.Volition), ClassName.Chaotician);

        AllClassList.Add((AspectName.Essence, AspectName.Shadow, AspectName.None), ClassName.Daggerspell);
        AllClassList.Add((AspectName.Essence, AspectName.Shadow, AspectName.Soul), ClassName.Necroetherist);
        AllClassList.Add((AspectName.Essence, AspectName.Shadow, AspectName.Volition), ClassName.Planeshifter);
        
        AllClassList.Add((AspectName.Essence, AspectName.Soul, AspectName.None), ClassName.Phantasm);
        AllClassList.Add((AspectName.Essence, AspectName.Soul, AspectName.Volition), ClassName.Demonologist);
        
        AllClassList.Add((AspectName.Essence, AspectName.Volition, AspectName.None), ClassName.Archivist);
        
        AllClassList.Add((AspectName.Esthesis, AspectName.None, AspectName.None), ClassName.Euphonic);
        
        AllClassList.Add((AspectName.Esthesis, AspectName.Glory, AspectName.None), ClassName.Duelist);
        AllClassList.Add((AspectName.Esthesis, AspectName.Glory, AspectName.Grace), ClassName.Sorrowsong);
        AllClassList.Add((AspectName.Esthesis, AspectName.Glory, AspectName.Hunt), ClassName.Worldwalker);
        AllClassList.Add((AspectName.Esthesis, AspectName.Glory, AspectName.Involution), ClassName.Vulgarist);
        AllClassList.Add((AspectName.Esthesis, AspectName.Glory, AspectName.Shadow), ClassName.Edgewalker);
        AllClassList.Add((AspectName.Esthesis, AspectName.Glory, AspectName.Soul), ClassName.Bloodskald);
        AllClassList.Add((AspectName.Esthesis, AspectName.Glory, AspectName.Volition), ClassName.Bladedancer);
        
        AllClassList.Add((AspectName.Esthesis, AspectName.Grace, AspectName.None), ClassName.Epiphanist);
        AllClassList.Add((AspectName.Esthesis, AspectName.Grace, AspectName.Hunt), ClassName.Herald);
        AllClassList.Add((AspectName.Esthesis, AspectName.Grace, AspectName.Involution), ClassName.Oracle);
        AllClassList.Add((AspectName.Esthesis, AspectName.Grace, AspectName.Shadow), ClassName.Shadowcantor);
        AllClassList.Add((AspectName.Esthesis, AspectName.Grace, AspectName.Soul), ClassName.Soulsong);
        AllClassList.Add((AspectName.Esthesis, AspectName.Grace, AspectName.Volition), ClassName.Seraphim);

        AllClassList.Add((AspectName.Esthesis, AspectName.Hunt, AspectName.None), ClassName.Wanderer);
        AllClassList.Add((AspectName.Esthesis, AspectName.Hunt, AspectName.Involution), ClassName.Bowdancer);
        AllClassList.Add((AspectName.Esthesis, AspectName.Hunt, AspectName.Shadow), ClassName.Darkstring);
        AllClassList.Add((AspectName.Esthesis, AspectName.Hunt, AspectName.Soul), ClassName.Gravesinger);
        AllClassList.Add((AspectName.Esthesis, AspectName.Hunt, AspectName.Volition), ClassName.Gaian);

        AllClassList.Add((AspectName.Esthesis, AspectName.Involution, AspectName.None), ClassName.Howler);
        AllClassList.Add((AspectName.Esthesis, AspectName.Involution, AspectName.Shadow), ClassName.Siren);
        AllClassList.Add((AspectName.Esthesis, AspectName.Involution, AspectName.Soul), ClassName.Necroharmonist);
        AllClassList.Add((AspectName.Esthesis, AspectName.Involution, AspectName.Volition), ClassName.Dreambreaker);

        AllClassList.Add((AspectName.Esthesis, AspectName.Shadow, AspectName.None), ClassName.Gypsy);
        AllClassList.Add((AspectName.Esthesis, AspectName.Shadow, AspectName.Soul), ClassName.Requiem);
        AllClassList.Add((AspectName.Esthesis, AspectName.Shadow, AspectName.Volition), ClassName.Etherweaver);

        AllClassList.Add((AspectName.Esthesis, AspectName.Soul, AspectName.None), ClassName.Soulsinger);
        AllClassList.Add((AspectName.Esthesis, AspectName.Soul, AspectName.Volition), ClassName.Lamentor);

        AllClassList.Add((AspectName.Esthesis, AspectName.Volition, AspectName.None), ClassName.Soothsayer);

        AllClassList.Add((AspectName.Glory, AspectName.None, AspectName.None), ClassName.Soldier);

        AllClassList.Add((AspectName.Glory, AspectName.Grace, AspectName.None), ClassName.Cleric);
        AllClassList.Add((AspectName.Glory, AspectName.Grace, AspectName.Hunt), ClassName.Lightstrider);
        AllClassList.Add((AspectName.Glory, AspectName.Grace, AspectName.Involution), ClassName.Confessor);
        AllClassList.Add((AspectName.Glory, AspectName.Grace, AspectName.Shadow), ClassName.Twilightkeeper);
        AllClassList.Add((AspectName.Glory, AspectName.Grace, AspectName.Soul), ClassName.Defiler);
        AllClassList.Add((AspectName.Glory, AspectName.Grace, AspectName.Volition), ClassName.Warpriest);

        AllClassList.Add((AspectName.Glory, AspectName.Hunt, AspectName.None), ClassName.Ranger);
        AllClassList.Add((AspectName.Glory, AspectName.Hunt, AspectName.Involution), ClassName.Doomlord);
        AllClassList.Add((AspectName.Glory, AspectName.Hunt, AspectName.Shadow), ClassName.Darkrunner);
        AllClassList.Add((AspectName.Glory, AspectName.Hunt, AspectName.Soul), ClassName.Bloodarrow);
        AllClassList.Add((AspectName.Glory, AspectName.Hunt, AspectName.Volition), ClassName.Outrider);

        AllClassList.Add((AspectName.Glory, AspectName.Involution, AspectName.None), ClassName.Dreadblade);
        AllClassList.Add((AspectName.Glory, AspectName.Involution, AspectName.Shadow), ClassName.Executioner);
        AllClassList.Add((AspectName.Glory, AspectName.Involution, AspectName.Soul), ClassName.Skulltaker);
        AllClassList.Add((AspectName.Glory, AspectName.Involution, AspectName.Volition), ClassName.Abolisher);

        AllClassList.Add((AspectName.Glory, AspectName.Shadow, AspectName.None), ClassName.Jacknife);
        AllClassList.Add((AspectName.Glory, AspectName.Shadow, AspectName.Soul), ClassName.Nightbearer);
        AllClassList.Add((AspectName.Glory, AspectName.Shadow, AspectName.Volition), ClassName.Dervish);

        AllClassList.Add((AspectName.Glory, AspectName.Soul, AspectName.None), ClassName.Bloodthrall);
        AllClassList.Add((AspectName.Glory, AspectName.Soul, AspectName.Volition), ClassName.Bloodreaver);

        AllClassList.Add((AspectName.Glory, AspectName.Volition, AspectName.None), ClassName.Feral);

        AllClassList.Add((AspectName.Grace, AspectName.None, AspectName.None), ClassName.Caretaker);

        AllClassList.Add((AspectName.Grace, AspectName.Hunt, AspectName.None), ClassName.Naturalist);
        AllClassList.Add((AspectName.Grace, AspectName.Hunt, AspectName.Involution), ClassName.Primeval);
        AllClassList.Add((AspectName.Grace, AspectName.Hunt, AspectName.Shadow), ClassName.Penance);
        AllClassList.Add((AspectName.Grace, AspectName.Hunt, AspectName.Soul), ClassName.Wraithranger);
        AllClassList.Add((AspectName.Grace, AspectName.Hunt, AspectName.Volition), ClassName.Druid);

        AllClassList.Add((AspectName.Grace, AspectName.Involution, AspectName.None), ClassName.Chaplain);
        AllClassList.Add((AspectName.Grace, AspectName.Involution, AspectName.Shadow), ClassName.Mesmer);
        AllClassList.Add((AspectName.Grace, AspectName.Involution, AspectName.Soul), ClassName.Cadaveric);
        AllClassList.Add((AspectName.Grace, AspectName.Involution, AspectName.Volition), ClassName.Auspician);

        AllClassList.Add((AspectName.Grace, AspectName.Shadow, AspectName.None), ClassName.Shadowcure);
        AllClassList.Add((AspectName.Grace, AspectName.Shadow, AspectName.Soul), ClassName.Revenant);
        AllClassList.Add((AspectName.Grace, AspectName.Shadow, AspectName.Volition), ClassName.Nightingale);

        AllClassList.Add((AspectName.Grace, AspectName.Soul, AspectName.None), ClassName.Netheraid);
        AllClassList.Add((AspectName.Grace, AspectName.Soul, AspectName.Volition), ClassName.Mortisculpt);

        AllClassList.Add((AspectName.Grace, AspectName.Volition, AspectName.None), ClassName.Alchemist);

        AllClassList.Add((AspectName.Hunt, AspectName.None, AspectName.None), ClassName.Sharpshot);

        AllClassList.Add((AspectName.Hunt, AspectName.Involution, AspectName.None), ClassName.Farslayer);
        AllClassList.Add((AspectName.Hunt, AspectName.Involution, AspectName.Shadow), ClassName.Trickster);
        AllClassList.Add((AspectName.Hunt, AspectName.Involution, AspectName.Soul), ClassName.Deathgrim);
        AllClassList.Add((AspectName.Hunt, AspectName.Involution, AspectName.Volition), ClassName.Dreadhunter);
        
        AllClassList.Add((AspectName.Hunt, AspectName.Shadow, AspectName.None), ClassName.Shadestrider);
        AllClassList.Add((AspectName.Hunt, AspectName.Shadow, AspectName.Soul), ClassName.Bonestalker);
        AllClassList.Add((AspectName.Hunt, AspectName.Shadow, AspectName.Volition), ClassName.Infiltrator);
        
        AllClassList.Add((AspectName.Hunt, AspectName.Soul, AspectName.None), ClassName.Soulbow);
        AllClassList.Add((AspectName.Hunt, AspectName.Soul, AspectName.Volition), ClassName.Harvester);
        
        AllClassList.Add((AspectName.Hunt, AspectName.Volition, AspectName.None), ClassName.Astralranger);
        
        AllClassList.Add((AspectName.Involution, AspectName.None, AspectName.None), ClassName.Enchantrix);
        
        AllClassList.Add((AspectName.Involution, AspectName.Shadow, AspectName.None), ClassName.Nightwitch);
        AllClassList.Add((AspectName.Involution, AspectName.Shadow, AspectName.Soul), ClassName.Phantombinder);
        AllClassList.Add((AspectName.Involution, AspectName.Shadow, AspectName.Volition), ClassName.Enigmatist);
        
        AllClassList.Add((AspectName.Involution, AspectName.Soul, AspectName.None), ClassName.Shaman);
        AllClassList.Add((AspectName.Involution, AspectName.Soul, AspectName.Volition), ClassName.Animist);
        
        AllClassList.Add((AspectName.Involution, AspectName.Volition, AspectName.None), ClassName.Ephemeralist);
        
        AllClassList.Add((AspectName.Shadow, AspectName.None, AspectName.None), ClassName.Assassin);
        
        AllClassList.Add((AspectName.Shadow, AspectName.Soul, AspectName.None), ClassName.Cabalist);
        AllClassList.Add((AspectName.Shadow, AspectName.Soul, AspectName.Volition), ClassName.Fleshshaper);
        
        AllClassList.Add((AspectName.Shadow, AspectName.Volition, AspectName.None), ClassName.Cursedrinker);
        
        AllClassList.Add((AspectName.Soul, AspectName.None, AspectName.None), ClassName.Necroscribe);
        
        AllClassList.Add((AspectName.Soul, AspectName.Volition, AspectName.None), ClassName.Evoker);
        
        AllClassList.Add((AspectName.Volition, AspectName.None, AspectName.None), ClassName.Philosopher);
    }

    public static ClassName GetCharacterTitle(List<AspectName> aspectList)
    {
        if (AllClassList.ContainsKey((aspectList[0], aspectList[1], aspectList[2])))
        {
            return AllClassList[(aspectList[0], aspectList[1], aspectList[2])];
        }

        return ClassName.None;
    }
}
