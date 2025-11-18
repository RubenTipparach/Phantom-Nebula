using System.Numerics;

namespace PhantomNebula.Game
{
    /// <summary>
    /// Represents a billboard explosion effect that scales up and fizzles out with a dither pattern.
    /// </summary>
    public class Explosion
    {
        /// <summary>
        /// World position of the explosion center.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Current lifetime in seconds.
        /// </summary>
        public float Lifetime { get; private set; }

        /// <summary>
        /// Maximum lifetime before the explosion is considered dead.
        /// </summary>
        public float MaxLifetime { get; private set; }

        /// <summary>
        /// Starting scale of the explosion (1.0 = normal size).
        /// </summary>
        public float StartScale { get; private set; }

        /// <summary>
        /// Peak scale that the explosion reaches.
        /// </summary>
        public float PeakScale { get; private set; }

        /// <summary>
        /// Current scale of the explosion (grows then fades out).
        /// </summary>
        public float CurrentScale
        {
            get
            {
                float normalizedLifetime = Lifetime / MaxLifetime;

                // Scale up in first 30% of lifetime, then stay at peak for a bit, then shrink
                if (normalizedLifetime < 0.3f)
                {
                    // Grow phase (0-30%)
                    float growProgress = normalizedLifetime / 0.3f;
                    return StartScale + (PeakScale - StartScale) * growProgress;
                }
                else if (normalizedLifetime < 0.7f)
                {
                    // Hold phase (30-70%)
                    return PeakScale;
                }
                else
                {
                    // Shrink phase (70-100%)
                    float shrinkProgress = (normalizedLifetime - 0.7f) / 0.3f;
                    return PeakScale * (1.0f - shrinkProgress);
                }
            }
        }

        /// <summary>
        /// Current alpha value from 1.0 (fully opaque) to 0.0 (fully transparent).
        /// Fades out in the final 30% of lifetime.
        /// </summary>
        public float CurrentAlpha
        {
            get
            {
                float normalizedLifetime = Lifetime / MaxLifetime;

                // Full opacity until 70% through lifetime, then fade out
                if (normalizedLifetime < 0.7f)
                {
                    return 1.0f;
                }
                else
                {
                    float fadeProgress = (normalizedLifetime - 0.7f) / 0.3f;
                    return 1.0f - fadeProgress;
                }
            }
        }

        /// <summary>
        /// Dither pattern animation value (0-1, repeats).
        /// Used for the dither dissolve effect.
        /// </summary>
        public float DitherPhase
        {
            get
            {
                // Cycles through 0-1 multiple times during lifetime
                // Faster phase cycling = more fizzle effect
                return (Lifetime * 4.0f) % 1.0f;
            }
        }

        /// <summary>
        /// Whether this explosion has finished and should be removed.
        /// </summary>
        public bool IsAlive => Lifetime < MaxLifetime;

        /// <summary>
        /// Creates a new explosion effect.
        /// </summary>
        /// <param name="position">World position to spawn at</param>
        /// <param name="maxLifetime">How long the explosion lasts (default 1.5 seconds)</param>
        /// <param name="startScale">Initial scale (default 0.5)</param>
        /// <param name="peakScale">Maximum scale reached (default 3.0)</param>
        public Explosion(
            Vector3 position,
            float maxLifetime = 1.5f,
            float startScale = 0.5f,
            float peakScale = 3.0f)
        {
            Position = position;
            MaxLifetime = maxLifetime;
            StartScale = startScale;
            PeakScale = peakScale;
            Lifetime = 0f;
        }

        /// <summary>
        /// Updates the explosion lifetime.
        /// </summary>
        public void Update(float deltaTime)
        {
            Lifetime += deltaTime;
        }
    }
}
