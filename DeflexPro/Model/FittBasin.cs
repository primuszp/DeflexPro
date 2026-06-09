using System;
using System.Collections.Generic;

namespace DeflexPro.Model
{
    static class FittBasin
    {
        public static double Deflection(double x, double def, double radius, double alpha, double beta)
        {
            return ((def * 4.0 * radius * radius) / (alpha * Math.Pow(x, beta) + (4.0 * radius * radius)));
        }

        public static double Deflection(double x, double def, double radius, double c)
        {
            return Deflection(x, def, radius, c, 2);
        }

        public static double Deflection2(double x, double def, double n, double l, double c)
        {
            return def / Math.Pow(1.0 + (2.0 / (2.0 * n + 1.0) * Math.Pow(x / l, c)), n);
        }

        public static double RegParameters(List<Deflection> deflections, double plateRadius, int count)
        {
            double sum = 0.0d;

            if (count >= deflections.Count)
                count = deflections.Count - 1;

            for (int i = 1; i <= count; i++)
            {
                if (deflections[i].Measure != 0.0d && deflections[i].Measure < deflections[0].Measure)
                {
                    sum += deflections[0].Measure / (deflections[i].Measure * (Math.Pow(deflections[i].Sensor.X / (2 * plateRadius), 2) + 1));
                }
            }
            return sum /= count;
        }

        public static double[] RegParameters(List<Deflection> deflections, double plateRadius)
        {
            int sensorCount = deflections.Count;

            double[] logX = new double[sensorCount - 1];
            double[] logY = new double[sensorCount - 1];

            for (int i = 1; i < sensorCount; i++)
            {
                if (deflections[i].Sensor.LocationType == Sensor.Location.Normal &&
                   (deflections[i].Measure != 0.0d && deflections[i].Measure < deflections[0].Measure))
                {
                    logX[i - 1] = Math.Log10(deflections[i].Sensor.X);
                    logY[i - 1] = Math.Log10(((deflections[0].Measure * 4.0 * plateRadius * plateRadius) / deflections[i].Measure) - 4.0 * plateRadius * plateRadius);
                }
            }

            double a = Math.Pow(10, TengelyMetszet(logX, logY));
            double b = Mslope(logX, logY);

            return new double[] { a, b };
        }

        #region Statics


        public static double Slope(double[] vector_A, double[] vector_B)
        {
            double AB = 0;
            double AA = 0;
            double M = 0;

            int PNumber = vector_A.Length;

            if (vector_A.Length == vector_B.Length)
            {
                for (int i = 0; i < PNumber; i++)
                {
                    AB = AB + vector_A[i] * vector_B[i];     // Az xy szorzat szummája
                    AA = AA + vector_A[i] * vector_A[i];     // Az xx szorzat szummája
                }
                M = (AB / AA);
            }
            return M;
        }

        public static double Mslope(double[] vector_A, double[] vector_B)
        {
            double AB = 0;
            double AA = 0;
            double A = 0;
            double B = 0;
            double M = 0;

            int PNumber = vector_A.Length;

            if (vector_A.Length == vector_B.Length)
            {
                for (int i = 0; i < PNumber; i++)
                {
                    AB = AB + vector_A[i] * vector_B[i];     // Az xy szorzat szummája
                    AA = AA + vector_A[i] * vector_A[i];     // Az xx szorzat szummája
                    A = A + vector_A[i];                     // Az x szummája
                    B = B + vector_B[i];                     // Az y szummája
                }
                M = (PNumber * AB - A * B) / (PNumber * AA - (A * A));
            }
            return M;
        }

        public static double TengelyMetszet(double[] vector_A, double[] vector_B)
        {
            double AB = 0;
            double AA = 0;
            double A = 0;
            double B = 0;
            double TM = 0;

            int PNumber = vector_A.Length;

            if (vector_A.Length == vector_B.Length)
            {
                for (int i = 0; i < PNumber; i++)
                {
                    AB = AB + vector_A[i] * vector_B[i];     // Az xy szorzat szummája
                    AA = AA + vector_A[i] * vector_A[i];     // Az xx szorzat szummája
                    A = A + vector_A[i];                     // Az x szummája
                    B = B + vector_B[i];                     // Az y szummája
                }
                TM = ((A * AB) - (AA * B)) / ((A * A) - (AA * (PNumber)));
            }
            return TM;
        }

        private static double Mean(double[] vector)
        {
            double sum = 0;

            for (int i = 0; i < vector.Length; i++)
            {
                sum = sum + vector[i];
            }

            return (sum / vector.Length);
        }

        private static double Correlation(double[] vector_A, double[] vector_B)
        {
            double ab = 0.0;
            double aa = 0.0;
            double bb = 0.0;
            double cc = 0.0;

            int PNumber = vector_A.Length;

            if (vector_A.Length == vector_B.Length)
            {
                for (int i = 0; i < PNumber; i++)
                {
                    ab = ab + ((vector_A[i] - Mean(vector_A)) * (vector_B[i] - Mean(vector_B)));
                    aa = aa + Math.Pow(vector_A[i] - Mean(vector_A), 2);
                    bb = bb + Math.Pow(vector_B[i] - Mean(vector_B), 2);
                }
                cc = ab / Math.Sqrt(aa * bb);
            }
            return cc;
        }

        #endregion
    }
}
