//-----------------------------------------------------------------------
// <copyright file="SoundClass.cs" company="Ozzymud">
// Copyright 2005 Ozzymud
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// <author>Ozzymud</author>
//-----------------------------------------------------------------------

namespace Battleships
{
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Text;

public class SoundClass
    {
        public void PlaySoundAsync(string resource)
        {
            SoundPlayer sp = new SoundPlayer();
            
            // load sound from compiled resource
            sp.Stream = this.GetType().Assembly.GetManifestResourceStream("Battleships.Sounds." + resource);
            sp.Play(); // play sound
        }
    }
}
