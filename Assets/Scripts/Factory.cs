using System;
using UnityEngine;

/// <summary>
/// Tämä rakennus tuottaa asioita saamistaan resursseista
/// </summary>
public class Factory : Building
{
    void Start()
    {
        OnStart();
        name = "Factory";
        debugInfo = "Not producing";
    }

    // Update is called once per frame
    void Update()
    {
        // TODO 
        // tuota output listan resursseja jos kaikkia resursseja
        // on saatavilla
    }
}
