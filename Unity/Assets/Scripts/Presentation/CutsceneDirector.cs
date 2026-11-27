using UnityEngine;
using FootballCareer.UnityClient.Match;

namespace FootballCareer.UnityClient.Presentation
{
    /// <summary>
    /// Thin bridge from gameplay moments to Animator parameters. Configure matching triggers and
    /// integer parameters in the imported character's Animator Controller. Timeline/Cinemachine
    /// cameras can be added later without making career logic depend on presentation assets.
    /// </summary>
    public sealed class CutsceneDirector : MonoBehaviour
    {
        [SerializeField] private Animator characterAnimator;
        [SerializeField] private string goalTrigger = "GoalCelebration";
        [SerializeField] private string missTrigger = "MissReaction";
        [SerializeField] private string signingTrigger = "ClubSigning";
        [SerializeField] private string celebrationStyleParameter = "CelebrationStyle";
        [SerializeField, Min(0f)] private float minimumReplayInterval = 1.5f;

        private int celebrationStyleHash;
        private float lastPlayedAt = float.NegativeInfinity;

        private void Awake()
        {
            celebrationStyleHash = Animator.StringToHash(celebrationStyleParameter);
        }

        public void PlayMatchMoment(MatchMoment moment, int selectedCelebrationStyle)
        {
            if (!CanPlay() || characterAnimator == null)
                return;

            switch (moment.type)
            {
                case MatchMomentType.Goal:
                    characterAnimator.SetInteger(celebrationStyleHash, Mathf.Max(0, selectedCelebrationStyle));
                    Trigger(goalTrigger);
                    break;
                case MatchMomentType.MissedChance:
                    Trigger(missTrigger);
                    break;
                case MatchMomentType.MatchEnded:
                    return;
            }
        }

        public void PlayClubSigning()
        {
            if (!CanPlay() || characterAnimator == null)
                return;

            Trigger(signingTrigger);
        }

        private bool CanPlay() => Time.unscaledTime - lastPlayedAt >= minimumReplayInterval;

        private void Trigger(string triggerName)
        {
            if (string.IsNullOrWhiteSpace(triggerName))
                return;

            characterAnimator.ResetTrigger(triggerName);
            characterAnimator.SetTrigger(triggerName);
            lastPlayedAt = Time.unscaledTime;
        }
    }
}
