using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Aspect
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

public class Manager_Aspect
{
    private static HashSet<ClassTitle> _usedTitles = new();
    private static Dictionary<ClassTitle, (Aspect, Aspect, Aspect)> _titleList = new(); public static Dictionary<ClassTitle, (Aspect, Aspect, Aspect)> TitleList { get { return _titleList; } }

    public static void InitialiseSpecialisations()
    {
        foreach (KeyValuePair<ClassTitle, (Aspect, Aspect, Aspect)> title in _titleList)
        {
            string aspects = string.Join(",", title.Value.Item1, title.Value.Item2, title.Value.Item3);
            if (_usedTitles.Contains(title.Key))
            {
                throw new ArgumentException("Item ID " + title.Key + " is already used");
            }
            _usedTitles.Add(title.Key);
        }

        _titleList.Add(ClassTitle.Aspectless, (Aspect.None, Aspect.None, Aspect.None));

        _titleList.Add(ClassTitle.Defender, (Aspect.Defiance, Aspect.None, Aspect.None));

        _titleList.Add(ClassTitle.Exorcist, (Aspect.Defiance, Aspect.Essence, Aspect.None));
        _titleList.Add(ClassTitle.Silentpsalm, (Aspect.Defiance, Aspect.Essence, Aspect.Esthesis));
        _titleList.Add(ClassTitle.Woundwarden, (Aspect.Defiance, Aspect.Essence, Aspect.Glory));
        _titleList.Add(ClassTitle.Hallowblaze, (Aspect.Defiance, Aspect.Essence, Aspect.Grace));
        _titleList.Add(ClassTitle.Deathwarden, (Aspect.Defiance, Aspect.Essence, Aspect.Hunt));
        _titleList.Add(ClassTitle.Hexwarden, (Aspect.Defiance, Aspect.Essence, Aspect.Involution));
        _titleList.Add(ClassTitle.Shadoward, (Aspect.Defiance, Aspect.Essence, Aspect.Shadow));
        _titleList.Add(ClassTitle.Tombwarden, (Aspect.Defiance, Aspect.Essence, Aspect.Soul));
        _titleList.Add(ClassTitle.Zenith, (Aspect.Defiance, Aspect.Essence, Aspect.Volition));

        _titleList.Add(ClassTitle.Ironbard, (Aspect.Defiance, Aspect.Esthesis, Aspect.None));
        _titleList.Add(ClassTitle.Justicar, (Aspect.Defiance, Aspect.Esthesis, Aspect.Glory));
        _titleList.Add(ClassTitle.Hymnguard, (Aspect.Defiance, Aspect.Esthesis, Aspect.Grace));
        _titleList.Add(ClassTitle.Argent, (Aspect.Defiance, Aspect.Esthesis, Aspect.Hunt));
        _titleList.Add(ClassTitle.Earthsinger, (Aspect.Defiance, Aspect.Esthesis, Aspect.Involution));
        _titleList.Add(ClassTitle.Nocturneanthem, (Aspect.Defiance, Aspect.Esthesis, Aspect.Shadow));
        _titleList.Add(ClassTitle.Tombcaller, (Aspect.Defiance, Aspect.Esthesis, Aspect.Soul));
        _titleList.Add(ClassTitle.Elegist, (Aspect.Defiance, Aspect.Esthesis, Aspect.Volition));

        _titleList.Add(ClassTitle.Bastion, (Aspect.Defiance, Aspect.Glory, Aspect.None));
        _titleList.Add(ClassTitle.Templar, (Aspect.Defiance, Aspect.Glory, Aspect.Grace));
        _titleList.Add(ClassTitle.Hordebreaker, (Aspect.Defiance, Aspect.Glory, Aspect.Hunt));
        _titleList.Add(ClassTitle.Crusader, (Aspect.Defiance, Aspect.Glory, Aspect.Involution));
        _titleList.Add(ClassTitle.Enforcer, (Aspect.Defiance, Aspect.Glory, Aspect.Shadow));
        _titleList.Add(ClassTitle.Skullknight, (Aspect.Defiance, Aspect.Glory, Aspect.Soul));
        _titleList.Add(ClassTitle.Honourguard, (Aspect.Defiance, Aspect.Glory, Aspect.Volition));

        _titleList.Add(ClassTitle.Guardian, (Aspect.Defiance, Aspect.Grace, Aspect.None));
        _titleList.Add(ClassTitle.Dawnsentinel, (Aspect.Defiance, Aspect.Grace, Aspect.Hunt));
        _titleList.Add(ClassTitle.Hierophant, (Aspect.Defiance, Aspect.Grace, Aspect.Involution));
        _titleList.Add(ClassTitle.Aethermend, (Aspect.Defiance, Aspect.Grace, Aspect.Shadow));
        _titleList.Add(ClassTitle.Eldritch_Shephard, (Aspect.Defiance, Aspect.Grace, Aspect.Soul));
        _titleList.Add(ClassTitle.Paladin, (Aspect.Defiance, Aspect.Grace, Aspect.Volition));

        _titleList.Add(ClassTitle.Dreadstone, (Aspect.Defiance, Aspect.Hunt, Aspect.None));
        _titleList.Add(ClassTitle.Battlebow, (Aspect.Defiance, Aspect.Hunt, Aspect.Involution));
        _titleList.Add(ClassTitle.Haunter, (Aspect.Defiance, Aspect.Hunt, Aspect.Shadow));
        _titleList.Add(ClassTitle.Immortus, (Aspect.Defiance, Aspect.Hunt, Aspect.Soul));
        _titleList.Add(ClassTitle.Dreadnaught, (Aspect.Defiance, Aspect.Hunt, Aspect.Volition));

        _titleList.Add(ClassTitle.Enthraller, (Aspect.Defiance, Aspect.Involution, Aspect.None));
        _titleList.Add(ClassTitle.Darkaegis, (Aspect.Defiance, Aspect.Involution, Aspect.Shadow));
        _titleList.Add(ClassTitle.Abyssalich, (Aspect.Defiance, Aspect.Involution, Aspect.Soul));
        _titleList.Add(ClassTitle.Archon, (Aspect.Defiance, Aspect.Involution, Aspect.Volition));

        _titleList.Add(ClassTitle.Blackguard, (Aspect.Defiance, Aspect.Shadow, Aspect.None));
        _titleList.Add(ClassTitle.Netherwarden, (Aspect.Defiance, Aspect.Shadow, Aspect.Soul));
        _titleList.Add(ClassTitle.Shadowknight, (Aspect.Defiance, Aspect.Shadow, Aspect.Volition));

        _titleList.Add(ClassTitle.Boneweaver, (Aspect.Defiance, Aspect.Soul, Aspect.None));
        _titleList.Add(ClassTitle.Soulwrought, (Aspect.Defiance, Aspect.Soul, Aspect.Volition));

        _titleList.Add(ClassTitle.Custodian, (Aspect.Defiance, Aspect.Volition, Aspect.None));

        _titleList.Add(ClassTitle.Thaumaturge, (Aspect.Essence, Aspect.None, Aspect.None));

        _titleList.Add(ClassTitle.Spellsinger, (Aspect.Essence, Aspect.Esthesis, Aspect.None));
        _titleList.Add(ClassTitle.Hexblade, (Aspect.Essence, Aspect.Esthesis, Aspect.Glory));
        _titleList.Add(ClassTitle.Mourningchorus, (Aspect.Essence, Aspect.Esthesis, Aspect.Grace));
        _titleList.Add(ClassTitle.Ebonsong, (Aspect.Essence, Aspect.Esthesis, Aspect.Hunt));
        _titleList.Add(ClassTitle.Stormcaller, (Aspect.Essence, Aspect.Esthesis, Aspect.Involution));
        _titleList.Add(ClassTitle.Blighter, (Aspect.Essence, Aspect.Esthesis, Aspect.Shadow));
        _titleList.Add(ClassTitle.Scion, (Aspect.Essence, Aspect.Esthesis, Aspect.Soul));
        _titleList.Add(ClassTitle.Dawncaller, (Aspect.Essence, Aspect.Esthesis, Aspect.Volition));

        _titleList.Add(ClassTitle.Spellsword, (Aspect.Essence, Aspect.Glory, Aspect.None));
        _titleList.Add(ClassTitle.Witcher, (Aspect.Essence, Aspect.Glory, Aspect.Grace));
        _titleList.Add(ClassTitle.Doombringer, (Aspect.Essence, Aspect.Glory, Aspect.Hunt));
        _titleList.Add(ClassTitle.Battlemage, (Aspect.Essence, Aspect.Glory, Aspect.Involution));
        _titleList.Add(ClassTitle.Shadowbane, (Aspect.Essence, Aspect.Glory, Aspect.Shadow));
        _titleList.Add(ClassTitle.Harbinger, (Aspect.Essence, Aspect.Glory, Aspect.Soul));
        _titleList.Add(ClassTitle.Inquisitor, (Aspect.Essence, Aspect.Glory, Aspect.Volition));

        _titleList.Add(ClassTitle.Traumapothicar, (Aspect.Essence, Aspect.Grace, Aspect.None));
        _titleList.Add(ClassTitle.Brightbow, (Aspect.Essence, Aspect.Grace, Aspect.Hunt));
        _titleList.Add(ClassTitle.Poxbane, (Aspect.Essence, Aspect.Grace, Aspect.Involution));
        _titleList.Add(ClassTitle.Nightcloak, (Aspect.Essence, Aspect.Grace, Aspect.Shadow));
        _titleList.Add(ClassTitle.Eidolon, (Aspect.Essence, Aspect.Grace, Aspect.Soul));
        _titleList.Add(ClassTitle.Purifier, (Aspect.Essence, Aspect.Grace, Aspect.Volition));

        _titleList.Add(ClassTitle.Arcanehunter, (Aspect.Essence, Aspect.Hunt, Aspect.None));
        _titleList.Add(ClassTitle.Hexranger, (Aspect.Essence, Aspect.Hunt, Aspect.Involution));
        _titleList.Add(ClassTitle.Nocturl, (Aspect.Essence, Aspect.Hunt, Aspect.Shadow));
        _titleList.Add(ClassTitle.Reaper, (Aspect.Essence, Aspect.Hunt, Aspect.Soul));
        _titleList.Add(ClassTitle.Fiendhunter, (Aspect.Essence, Aspect.Hunt, Aspect.Volition));

        _titleList.Add(ClassTitle.Arcanist, (Aspect.Essence, Aspect.Involution, Aspect.None));
        _titleList.Add(ClassTitle.Hellweaver, (Aspect.Essence, Aspect.Involution, Aspect.Shadow));
        _titleList.Add(ClassTitle.Athame, (Aspect.Essence, Aspect.Involution, Aspect.Soul));
        _titleList.Add(ClassTitle.Chaotician, (Aspect.Essence, Aspect.Involution, Aspect.Volition));

        _titleList.Add(ClassTitle.Daggerspell, (Aspect.Essence, Aspect.Shadow, Aspect.None));
        _titleList.Add(ClassTitle.Necroetherist, (Aspect.Essence, Aspect.Shadow, Aspect.Soul));
        _titleList.Add(ClassTitle.Planeshifter, (Aspect.Essence, Aspect.Shadow, Aspect.Volition));

        _titleList.Add(ClassTitle.Phantasm, (Aspect.Essence, Aspect.Soul, Aspect.None));
        _titleList.Add(ClassTitle.Demonologist, (Aspect.Essence, Aspect.Soul, Aspect.Volition));

        _titleList.Add(ClassTitle.Archivist, (Aspect.Essence, Aspect.Volition, Aspect.None));

        _titleList.Add(ClassTitle.Euphonic, (Aspect.Esthesis, Aspect.None, Aspect.None));

        _titleList.Add(ClassTitle.Duelist, (Aspect.Esthesis, Aspect.Glory, Aspect.None));
        _titleList.Add(ClassTitle.Sorrowsong, (Aspect.Esthesis, Aspect.Glory, Aspect.Grace));
        _titleList.Add(ClassTitle.Worldwalker, (Aspect.Esthesis, Aspect.Glory, Aspect.Hunt));
        _titleList.Add(ClassTitle.Vulgarist, (Aspect.Esthesis, Aspect.Glory, Aspect.Involution));
        _titleList.Add(ClassTitle.Edgewalker, (Aspect.Esthesis, Aspect.Glory, Aspect.Shadow));
        _titleList.Add(ClassTitle.Bloodskald, (Aspect.Esthesis, Aspect.Glory, Aspect.Soul));
        _titleList.Add(ClassTitle.Bladedancer, (Aspect.Esthesis, Aspect.Glory, Aspect.Volition));

        _titleList.Add(ClassTitle.Epiphanist, (Aspect.Esthesis, Aspect.Grace, Aspect.None));
        _titleList.Add(ClassTitle.Herald, (Aspect.Esthesis, Aspect.Grace, Aspect.Hunt));
        _titleList.Add(ClassTitle.Oracle, (Aspect.Esthesis, Aspect.Grace, Aspect.Involution));
        _titleList.Add(ClassTitle.Shadowcantor, (Aspect.Esthesis, Aspect.Grace, Aspect.Shadow));
        _titleList.Add(ClassTitle.Soulsong, (Aspect.Esthesis, Aspect.Grace, Aspect.Soul));
        _titleList.Add(ClassTitle.Seraphim, (Aspect.Esthesis, Aspect.Grace, Aspect.Volition));

        _titleList.Add(ClassTitle.Wanderer, (Aspect.Esthesis, Aspect.Hunt, Aspect.None));
        _titleList.Add(ClassTitle.Bowdancer, (Aspect.Esthesis, Aspect.Hunt, Aspect.Involution));
        _titleList.Add(ClassTitle.Darkstring, (Aspect.Esthesis, Aspect.Hunt, Aspect.Shadow));
        _titleList.Add(ClassTitle.Gravesinger, (Aspect.Esthesis, Aspect.Hunt, Aspect.Soul));
        _titleList.Add(ClassTitle.Gaian, (Aspect.Esthesis, Aspect.Hunt, Aspect.Volition));

        _titleList.Add(ClassTitle.Howler, (Aspect.Esthesis, Aspect.Involution, Aspect.None));
        _titleList.Add(ClassTitle.Siren, (Aspect.Esthesis, Aspect.Involution, Aspect.Shadow));
        _titleList.Add(ClassTitle.Necroharmonist, (Aspect.Esthesis, Aspect.Involution, Aspect.Soul));
        _titleList.Add(ClassTitle.Dreambreaker, (Aspect.Esthesis, Aspect.Involution, Aspect.Volition));

        _titleList.Add(ClassTitle.Gypsy, (Aspect.Esthesis, Aspect.Shadow, Aspect.None));
        _titleList.Add(ClassTitle.Requiem, (Aspect.Esthesis, Aspect.Shadow, Aspect.Soul));
        _titleList.Add(ClassTitle.Etherweaver, (Aspect.Esthesis, Aspect.Shadow, Aspect.Volition));

        _titleList.Add(ClassTitle.Soulsinger, (Aspect.Esthesis, Aspect.Soul, Aspect.None));
        _titleList.Add(ClassTitle.Lamentor, (Aspect.Esthesis, Aspect.Soul, Aspect.Volition));

        _titleList.Add(ClassTitle.Soothsayer, (Aspect.Esthesis, Aspect.Volition, Aspect.None));

        _titleList.Add(ClassTitle.Soldier, (Aspect.Glory, Aspect.None, Aspect.None));

        _titleList.Add(ClassTitle.Cleric, (Aspect.Glory, Aspect.Grace, Aspect.None));
        _titleList.Add(ClassTitle.Lightstrider, (Aspect.Glory, Aspect.Grace, Aspect.Hunt));
        _titleList.Add(ClassTitle.Confessor, (Aspect.Glory, Aspect.Grace, Aspect.Involution));
        _titleList.Add(ClassTitle.Twilightkeeper, (Aspect.Glory, Aspect.Grace, Aspect.Shadow));
        _titleList.Add(ClassTitle.Defiler, (Aspect.Glory, Aspect.Grace, Aspect.Soul));
        _titleList.Add(ClassTitle.Warpriest, (Aspect.Glory, Aspect.Grace, Aspect.Volition));

        _titleList.Add(ClassTitle.Ranger, (Aspect.Glory, Aspect.Hunt, Aspect.None));
        _titleList.Add(ClassTitle.Doomlord, (Aspect.Glory, Aspect.Hunt, Aspect.Involution));
        _titleList.Add(ClassTitle.Darkrunner, (Aspect.Glory, Aspect.Hunt, Aspect.Shadow));
        _titleList.Add(ClassTitle.Bloodarrow, (Aspect.Glory, Aspect.Hunt, Aspect.Soul));
        _titleList.Add(ClassTitle.Outrider, (Aspect.Glory, Aspect.Hunt, Aspect.Volition));

        _titleList.Add(ClassTitle.Dreadblade, (Aspect.Glory, Aspect.Involution, Aspect.None));
        _titleList.Add(ClassTitle.Executioner, (Aspect.Glory, Aspect.Involution, Aspect.Shadow));
        _titleList.Add(ClassTitle.Skulltaker, (Aspect.Glory, Aspect.Involution, Aspect.Soul));
        _titleList.Add(ClassTitle.Abolisher, (Aspect.Glory, Aspect.Involution, Aspect.Volition));

        _titleList.Add(ClassTitle.Jacknife, (Aspect.Glory, Aspect.Shadow, Aspect.None));
        _titleList.Add(ClassTitle.Nightbearer, (Aspect.Glory, Aspect.Shadow, Aspect.Soul));
        _titleList.Add(ClassTitle.Dervish, (Aspect.Glory, Aspect.Shadow, Aspect.Volition));

        _titleList.Add(ClassTitle.Bloodthrall, (Aspect.Glory, Aspect.Soul, Aspect.None));
        _titleList.Add(ClassTitle.Bloodreaver, (Aspect.Glory, Aspect.Soul, Aspect.Volition));

        _titleList.Add(ClassTitle.Feral, (Aspect.Glory, Aspect.Volition, Aspect.None));

        _titleList.Add(ClassTitle.Caretaker, (Aspect.Grace, Aspect.None, Aspect.None));

        _titleList.Add(ClassTitle.Naturalist, (Aspect.Grace, Aspect.Hunt, Aspect.None));
        _titleList.Add(ClassTitle.Primeval, (Aspect.Grace, Aspect.Hunt, Aspect.Involution));
        _titleList.Add(ClassTitle.Penance, (Aspect.Grace, Aspect.Hunt, Aspect.Shadow));
        _titleList.Add(ClassTitle.Wraithranger, (Aspect.Grace, Aspect.Hunt, Aspect.Soul));
        _titleList.Add(ClassTitle.Druid, (Aspect.Grace, Aspect.Hunt, Aspect.Volition));

        _titleList.Add(ClassTitle.Chaplain, (Aspect.Grace, Aspect.Involution, Aspect.None));
        _titleList.Add(ClassTitle.Mesmer, (Aspect.Grace, Aspect.Involution, Aspect.Shadow));
        _titleList.Add(ClassTitle.Cadaveric, (Aspect.Grace, Aspect.Involution, Aspect.Soul));
        _titleList.Add(ClassTitle.Auspician, (Aspect.Grace, Aspect.Involution, Aspect.Volition));

        _titleList.Add(ClassTitle.Shadowcure, (Aspect.Grace, Aspect.Shadow, Aspect.None));
        _titleList.Add(ClassTitle.Revenant, (Aspect.Grace, Aspect.Shadow, Aspect.Soul));
        _titleList.Add(ClassTitle.Nightingale, (Aspect.Grace, Aspect.Shadow, Aspect.Volition));

        _titleList.Add(ClassTitle.Netheraid, (Aspect.Grace, Aspect.Soul, Aspect.None));
        _titleList.Add(ClassTitle.Mortisculpt, (Aspect.Grace, Aspect.Soul, Aspect.Volition));

        _titleList.Add(ClassTitle.Alchemist, (Aspect.Grace, Aspect.Volition, Aspect.None));

        _titleList.Add(ClassTitle.Sharpshot, (Aspect.Hunt, Aspect.None, Aspect.None));

        _titleList.Add(ClassTitle.Farslayer, (Aspect.Hunt, Aspect.Involution, Aspect.None));
        _titleList.Add(ClassTitle.Trickster, (Aspect.Hunt, Aspect.Involution, Aspect.Shadow));
        _titleList.Add(ClassTitle.Deathgrim, (Aspect.Hunt, Aspect.Involution, Aspect.Soul));
        _titleList.Add(ClassTitle.Dreadhunter, (Aspect.Hunt, Aspect.Involution, Aspect.Volition));

        _titleList.Add(ClassTitle.Shadestrider, (Aspect.Hunt, Aspect.Shadow, Aspect.None));
        _titleList.Add(ClassTitle.Bonestalker, (Aspect.Hunt, Aspect.Shadow, Aspect.Soul));
        _titleList.Add(ClassTitle.Infiltrator, (Aspect.Hunt, Aspect.Shadow, Aspect.Volition));

        _titleList.Add(ClassTitle.Soulbow, (Aspect.Hunt, Aspect.Soul, Aspect.None));
        _titleList.Add(ClassTitle.Harvester, (Aspect.Hunt, Aspect.Soul, Aspect.Volition));

        _titleList.Add(ClassTitle.Astralranger, (Aspect.Hunt, Aspect.Volition, Aspect.None));

        _titleList.Add(ClassTitle.Enchantrix, (Aspect.Involution, Aspect.None, Aspect.None));

        _titleList.Add(ClassTitle.Nightwitch, (Aspect.Involution, Aspect.Shadow, Aspect.None));
        _titleList.Add(ClassTitle.Phantombinder, (Aspect.Involution, Aspect.Shadow, Aspect.Soul));
        _titleList.Add(ClassTitle.Enigmatist, (Aspect.Involution, Aspect.Shadow, Aspect.Volition));

        _titleList.Add(ClassTitle.Shaman, (Aspect.Involution, Aspect.Soul, Aspect.None));
        _titleList.Add(ClassTitle.Animist, (Aspect.Involution, Aspect.Soul, Aspect.Volition));

        _titleList.Add(ClassTitle.Ephemeralist, (Aspect.Involution, Aspect.Volition, Aspect.None));

        _titleList.Add(ClassTitle.Assassin, (Aspect.Shadow, Aspect.None, Aspect.None));

        _titleList.Add(ClassTitle.Cabalist, (Aspect.Shadow, Aspect.Soul, Aspect.None));
        _titleList.Add(ClassTitle.Fleshshaper, (Aspect.Shadow, Aspect.Soul, Aspect.Volition));

        _titleList.Add(ClassTitle.Cursedrinker, (Aspect.Shadow, Aspect.Volition, Aspect.None));

        _titleList.Add(ClassTitle.Necroscribe, (Aspect.Soul, Aspect.None, Aspect.None));

        _titleList.Add(ClassTitle.Evoker, (Aspect.Soul, Aspect.Volition, Aspect.None));

        _titleList.Add(ClassTitle.Philosopher, (Aspect.Volition, Aspect.None, Aspect.None));
    }

    public static void AddAspect(Actor_Base actor, Aspect aspectToAdd)
    {
        List<Aspect> actorAspectList = actor.ActorData.ActorAspects.ActorAspectList;

        for (int i = 0; i < actorAspectList.Count; i++)
        {
            if (actorAspectList[i] == Aspect.None)
            {
                actorAspectList[i] = aspectToAdd;
            }
        }

        actor.ActorData.ActorAspects.ActorTitle = GetCharacterTitle(actor);
    }

    public static ClassTitle GetCharacterTitle(Actor_Base actor = null, List<Aspect> aspectList = null)
    {
        List<Aspect> sortedSpecialisations = null;

        if (actor != null)
        {
            for (int i = 0; i < (3 - actor.ActorData.ActorAspects.ActorAspectList.Count); i++)
            {
                actor.ActorData.ActorAspects.ActorAspectList.Add(Aspect.None);
            }

            sortedSpecialisations = actor.ActorData.ActorAspects.ActorAspectList.OrderBy(s => s.ToString()).ToList();
        }
        else if (aspectList != null)
        {
            for (int i = 0; i < (3 - aspectList.Count); i++)
            {
                aspectList.Add(Aspect.None);
            }

            sortedSpecialisations = aspectList.OrderBy(s => s.ToString()).ToList();
        }

        if (sortedSpecialisations == null)
        {
            return ClassTitle.ErrUmmWellFck;
        }

        var aspectTuple = (sortedSpecialisations[0], sortedSpecialisations[1], sortedSpecialisations[2]);

        if (_titleList.Any(kvp => kvp.Value.Equals(aspectTuple)))
        {
            return actor.ActorData.ActorAspects.ActorTitle = _titleList.First(kvp => kvp.Value.Equals(aspectTuple)).Key;
        }
        else
        {
            return actor.ActorData.ActorAspects.ActorTitle = ClassTitle.ErrUmmWellFck;
        }
    }
}
