using System;
using UnityEngine;

// Erilaiset resurssityypit
public enum ResourceType
{
    Water,
    Ore,
    Power,
    Metal
}
/// <summary>
/// Maastossa oleva resurssi josta Extractor voi kerätä sitä
/// </summary>
public class Resource : MonoBehaviour
{
    [SerializeField] public ResourceType resourceType { get; private set; }
    [SerializeField] private ResourceType product;
    [SerializeField] private int amount;

    private string name;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        amount = 100;
        name = gameObject.name;
        resourceType = product;
    }

    private void OnGUI()
    {
        Vector3 textpos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1);
        string text = name;
        text += "\n" + $"{resourceType.ToString()}: {amount}/100";
        GUI.Label(new Rect(textpos.x, Screen.height - textpos.y, 1000, 3000), text);
    }
}
