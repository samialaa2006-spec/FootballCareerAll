using System;
using UnityEngine;

namespace FootballCareer.UnityClient.Career
{
    /// <summary>Serializable Unity-side profile. Keep fields aligned with the API DTO through explicit adapters.</summary>
    [Serializable]
    public sealed class CareerPlayerProfile
    {
        public string playerName = "Alex Novak";
        public string nationality = "England";
        public string clubId = "ars";
        public string leagueId = "epl";
        public string position = "ST";
        [Range(17, 38)] public int age = 17;
        [Range(1, 99)] public int overall = 60;
        [Range(1, 99)] public int potential = 80;
        public int appearances;
        public int goals;
        public int assists;
        public PlayerCustomizationProfile customization = new PlayerCustomizationProfile();
    }

    [Serializable]
    public sealed class SeasonProgress
    {
        public int currentMatchday = 1;
        public int totalMatchdays = 22;
        public int seasonNumber = 1;
        public bool isRetired;
    }

    [Serializable]
    public sealed class CareerSnapshot
    {
        public int schemaVersion = 1;
        public CareerPlayerProfile player = new CareerPlayerProfile();
        public SeasonProgress season = new SeasonProgress();
        public string savedAtUtc;
    }
}
