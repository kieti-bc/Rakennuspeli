using System;
using UnityEngine;
/// <summary>
/// Tämä rakennus tuottaa sähköä
/// </summary>
public class SolarPanel : Building
{
    [SerializeField] private float generationSpeed;

    private void Start()
    {
        OnStart();
        name = gameObject.name;
    }

    void Update()
    {
        AddResource(ResourceType.Power, generationSpeed * Time.deltaTime);
    }
}
