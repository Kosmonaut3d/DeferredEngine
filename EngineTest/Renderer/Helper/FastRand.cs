using System;
using Microsoft.Xna.Framework;

namespace DeferredEngine.Renderer.Helper
{
    internal static class FastRand
    {
        static int _state = 1;

        public static void Seed(int seed)
        {
            if (seed < 1)
                return;

            _state = seed;
        }

        /// <summary>
        /// Gets the next random integer value.
        /// </summary>
        /// <returns>A random positive integer.</returns>
        public static int NextInteger()
        {
            _state = 214013 * _state + 2531011;
            return (_state >> 16) & 0x7FFF;
        }

        /// <summary>
        /// Gets the next random integer value which is greater than zero and less than or equal to
        /// the specified maxmimum value.
        /// </summary>
        /// <param name="max">The maximum random integer value to return.</param>
        /// <returns>A random integer value between zero and the specified maximum value.</returns>
        public static int NextInteger(int max)
        {
            return (int)(max * NextSingle() + 0.5f);
        }

        /// <summary>
        /// Gets the next random integer between the specified minimum and maximum values.
        /// </summary>
        /// <param name="min">The inclusive minimum value.</param>
        /// <param name="max">The inclusive maximum value.</param>
        public static int NextInteger(int min, int max)
        {
            return (int)((max - min) * NextSingle() + 0.5f) + min;
        }

        /// <summary>
        /// Gets the next random single value.
        /// </summary>
        /// <returns>A random single value between 0 and 1.</returns>
        public static float NextSingle()
        {
            return NextInteger() / (float)short.MaxValue;
        }

        /// <summary>
        /// Gets the next random single value which is greater than zero and less than or equal to
        /// the specified maxmimum value.
        /// </summary>
        /// <param name="max">The maximum random single value to return.</param>
        /// <returns>A random single value between zero and the specified maximum value.</returns>
        public static float NextSingle(float max)
        {
            return max * NextSingle();
        }

        /// <summary>
        /// Gets the next random single value between the specified minimum and maximum values.
        /// </summary>
        /// <param name="min">The inclusive minimum value.</param>
        /// <param name="max">The inclusive maximum value.</param>
        /// <returns>A random single value between the specified minimum and maximum values.</returns>
        public static float NextSingle(float min, float max)
        {
            return (max - min) * NextSingle() + min;
        }

        /// <summary>
        /// Gets the next random angle value.
        /// </summary>
        /// <returns>A random angle value.</returns>
        public static float NextAngle()
        {
            return NextSingle((float)Math.PI * -1f, (float)Math.PI);
        }

        /// <summary>
        /// Gets the next random Color
        /// </summary>
        /// <returns></returns>
        public static Color NextColor()
        {
            return new Color(NextInteger(255), NextInteger(255), NextInteger(255));
        }

        public static float NextAngle(float maxAngle)
        {
            return NextSingle(-maxAngle, maxAngle);
        }

    }
}
