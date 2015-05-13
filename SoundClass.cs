/*
 * Projekt: Schiffeversenken Pirat Edition
 * Klasse: SoundClass
 * Beschreibung: Die Klasse stellt Methoden zur verfügung mit deren Hilfe Wav-Dateien asynchron wiedergegeben werden können
 * Autor: Markus Bohnert
 * Team: Simon Hodler, Markus Bohnert
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Media;
using System.ComponentModel;

namespace Battleships
{
    public class SoundClass
    {
        public void playSoundAsync(String resource)
        {
            SoundPlayer sp = new SoundPlayer();
            // load sound from compiled resource
            sp.Stream = this.GetType().Assembly.GetManifestResourceStream("Battleships.Sounds." + resource);
            // play sound
            sp.Play();
        }
    }
}
