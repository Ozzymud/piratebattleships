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
        public string currentSoundDir;

        public SoundClass()
        {
            currentSoundDir = System.IO.Directory.GetCurrentDirectory() + "\\Sounds";
        }

        public void playSoundAsync(String soundlocation)
        {
            SoundPlayer sound = new SoundPlayer();
            // Speicherort des Sounds
            sound.SoundLocation = soundlocation;
            // LoadCompleted Event hinzufügen
            sound.LoadCompleted += new AsyncCompletedEventHandler(sound_LoadCompleted);
            // Sounddatei asynchron laden
            sound.LoadAsync();
        }

        private void sound_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            // Sound wiedergeben
            ((SoundPlayer)sender).Play();
        }
    }
}
