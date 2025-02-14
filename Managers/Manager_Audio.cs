using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Generic;
using FMOD;
using UnityEngine;
using Debug = UnityEngine.Debug;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class Manager_Audio : MonoBehaviour
{
    [SerializeField] EventReference _currentSongReference;
    EventInstance _currentSongInstance;

    [SerializeField] public List<LocalParameter> LocalParameters;
    [SerializeField] public List<GlobalParameter> GlobalParameters;

    public void Play(AudioSource audioSource)
    {
        audioSource.Play();
    }
    
    public void PlaySong(EventReference audio)
    {
        _currentSongReference = audio;
        _currentSongInstance = RuntimeManager.CreateInstance(audio);

        _currentSongInstance.getDescription(out EventDescription eventDescription);
        eventDescription.getParameterDescriptionCount(out int parameterCount);

        for (int i = 0; i < parameterCount; i++)
        {
            RESULT result = eventDescription.getParameterDescriptionByIndex(i, out PARAMETER_DESCRIPTION parameterDescription);
            if (result != RESULT.OK) Debug.LogError($"Failed to get parameter description for index {i}: {result}");

            // Find a way to split local and global parameters

            LocalParameters.Add(new LocalParameter().SetParameterID(parameterDescription.name, parameterDescription.id, this));
        }

        _currentSongInstance.set3DAttributes(gameObject.To3DAttributes());
        _currentSongInstance.start();
    }

    void Update()
    {
        // Change this to me only updated according to code rather than this which is for editor.
        foreach (LocalParameter parameter in LocalParameters) UpdateLocalParameter(parameter);
        foreach (GlobalParameter parameter in GlobalParameters) UpdateGlobalParameter(parameter);

        _currentSongInstance.set3DAttributes(gameObject.To3DAttributes());
    }

    public void UpdateLocalParameter(LocalParameter parameter)
    {
        _currentSongInstance.setParameterByID(parameter.ParameterID, parameter.Value);
    }

    public void UpdateGlobalParameter(GlobalParameter parameter)
    {
        RuntimeManager.StudioSystem.setParameterByID(parameter.ParameterID, parameter.Value);
    }

    void OnDestroy()
    {
        _currentSongInstance.stop(STOP_MODE.IMMEDIATE);
    }
}

[Serializable]
public class LocalParameter
{
    [SerializeField] string _name;
    public FMOD.Studio.PARAMETER_ID ParameterID { get; private set; }
    [field: SerializeField] [field: Range(0, 1)] public float Value { get; private set; }
    Manager_Audio _manager;

    public void SetValue(float value)
    {
        Value = value;
        _manager.UpdateLocalParameter(this);
    }

    public LocalParameter SetParameterID(string name, FMOD.Studio.PARAMETER_ID parameterID, Manager_Audio manager)
    {
        _name = name;
        ParameterID = parameterID;
        _manager = manager;

        return this;
    }
}

[Serializable]
public class GlobalParameter
{
    [SerializeField] string _name;
    public FMOD.Studio.PARAMETER_ID ParameterID { get; private set; }
    [Range(0, 1)] public float Value;

    public GlobalParameter SetParameterID(string name, FMOD.Studio.PARAMETER_ID parameterID)
    {
        _name = name;
        ParameterID = parameterID;

        return this;
    }
}
