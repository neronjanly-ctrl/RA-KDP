using Windows.Foundation;

namespace NCDK
{
    public static class VectorExtensions
    {
        /// <summary>
        /// Convert a NCDK <see cref="NCDK.Numerics.Vector2"/> vector to a system point.
        /// </summary>
        /// <param name="vector"><see cref="NCDK.Numerics.Vector2"/> vector</param>
        /// <returns>System point</returns>
        public static Point ToPoint(this NCDK.Numerics.Vector2 vector) => new Point(vector.X, vector.Y);

        /// <summary>
        /// Convert a system <see cref="Windows.Foundation.Point"/> point to a NCDK vector.
        /// </summary>
        /// <param name="point"><see cref="Windows.Foundation.Point"/> point</param>
        /// <returns>NCDK vector</returns>
        public static NCDK.Numerics.Vector2 ToNCDK(this Point point) => new Numerics.Vector2(point.X, point.Y);

        /// <summary>
        /// Convert a system vector to a <see cref="NCDK.Numerics.Vector2"/>.
        /// </summary>
        /// <param name="vector">System vector</param>
        /// <returns><see cref="NCDK.Numerics.Vector2"/> vector</returns>
        public static NCDK.Numerics.Vector2 ToNCDK(this System.Numerics.Vector2 vector) => new NCDK.Numerics.Vector2(vector.X, vector.Y);

        /// <summary>
        /// Convert a NCDK <see cref="NCDK.Numerics.Vector2"/> to a system vector.
        /// </summary>
        /// <param name="vector">NCDK vector</param>
        /// <returns><see cref="System.Numerics.Vector2"/> vector</returns>
        public static System.Numerics.Vector2 ToSystem(this NCDK.Numerics.Vector2 vector) => new System.Numerics.Vector2((float)vector.X, (float)vector.Y);
    }
}
