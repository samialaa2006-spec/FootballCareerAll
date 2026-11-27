using System;
using UnityEngine;

namespace FootballCareer.UnityClient.Career
{
    /// <summary>
    /// Asset-independent customization choices. Resolve these IDs to the licensed starter asset's
    /// meshes/materials in a separate character customizer adapter.
    /// </summary>
    [Serializable]
    public sealed class PlayerCustomizationProfile
    {
        public string facePresetId = "face_default";
        public string skinToneId = "skin_medium";
        public string hairStyleId = "hair_short";
        public string hairColorId = "hair_dark";
        public string facialHairId = "facial_hair_none";
        public string kitId = "kit_home_generic";
        public string bootColorId = "boot_blue";
        [Range(1, 99)] public int shirtNumber = 9;
        public string celebrationId = "celebration_default";
    }
}
