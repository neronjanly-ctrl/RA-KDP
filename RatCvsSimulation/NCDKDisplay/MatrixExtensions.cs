using Windows.Foundation;

namespace NCDK
{
    public static class MatrixExtensions
    {
        /// <summary>
        /// Convert a system matrix to a NCDK <see cref="NCDK.Numerics.Matrix3x2"/>.
        /// </summary>
        /// <param name="matrix">System matrix</param>
        /// <returns><see cref="NCDK.Numerics.Matrix3x2"/> matrix</returns>
        public static Numerics.Matrix3x2 ToNCDK(this System.Numerics.Matrix3x2 matrix) => new NCDK.Numerics.Matrix3x2(matrix.M11, matrix.M12, matrix.M21, matrix.M22, matrix.M31, matrix.M32);

        /// <summary>
        /// Convert a NCDK <see cref="NCDK.Numerics.Matrix3x2"/> to a system matrix.
        /// </summary>
        /// <param name="matrix">NCDK matrix</param>
        /// <returns><see cref="System.Numerics.Matrix3x2"/> matrix</returns>
        public static System.Numerics.Matrix3x2 ToSystem(this NCDK.Numerics.Matrix3x2 matrix) => new System.Numerics.Matrix3x2((float)matrix.M11, (float)matrix.M12, (float)matrix.M21, (float)matrix.M22, (float)matrix.M31, (float)matrix.M32);
    }
}
