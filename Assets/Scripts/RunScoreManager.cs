using UnityEngine;

namespace Project.Score
{
    public class RunScoreManager : MonoBehaviour
    {
        public static RunScoreManager I { get; private set; }

        const string BestKey = "BEST_SCORE";

        public int RunScore { get; private set; }
        public int BestScore { get; private set; }

        void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);

            BestScore = PlayerPrefs.GetInt(BestKey, 0);
            RunScore = 0;
        }

        public void ResetRun()
        {
            RunScore = 0;
        }

        public void Add(int amount = 1)
        {
            RunScore += amount;
        }

        /// Call once on game over
        public void CommitBestIfNeeded()
        {
            if (RunScore > BestScore)
            {
                BestScore = RunScore;
                PlayerPrefs.SetInt(BestKey, BestScore);
                PlayerPrefs.Save();
            }
        }
    }
}
