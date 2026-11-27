using System;
using UnityEngine;

namespace FootballCareer.UnityClient.Match
{
    public enum MatchMomentType
    {
        Goal,
        MissedChance,
        MatchEnded
    }

    /// <summary>One event emitted by gameplay; presentation reacts without changing the score.</summary>
    [Serializable]
    public struct MatchMoment
    {
        public MatchMomentType type;
        public string playerId;
        public string playerName;
        public string celebrationId;
        public int minute;
        public int homeScore;
        public int awayScore;
    }

    [Serializable]
    public sealed class MatchResultData
    {
        public string fixtureId;
        public string homeClubId;
        public string awayClubId;
        public int homeGoals;
        public int awayGoals;
        public int playerGoals;
        public int playerAssists;
        public float playerRating;
        public bool wasPlayed;
    }
}
