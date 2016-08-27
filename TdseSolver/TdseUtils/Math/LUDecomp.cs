using System;



namespace TdseUtils
{
    /// <summary>
    /// This class performs LU decomposition.
    /// </summary>
    public class LUDecomp
    {
        // Class data
        private Matrix  m_luMatrix;
        private int     m_pivotSign;
        private int[]   m_pivotVector;
        

        /// <summary>
        /// Constructor.
        /// 
        /// Computes the LU decomposition of a square matrix: PA = LU, where
        /// P permutes the rows of A, L is lower triangular with ones on the diagonal, 
        /// and U is upper triangular.
        ///
        /// The LU decomposition always exists, even if the matrix is singular.
        /// </summary>
        public LUDecomp(Matrix A)
        {
            if (!A.IsSquare) 
            {
                throw new ArgumentException("LU decomposition requires a square matrix");
            }

            int N = A.NumRows;

            m_luMatrix = new Matrix(A);
            double[][] luData = m_luMatrix.Data;

            m_pivotSign = 1;
            m_pivotVector = new int[N];
            for (int i = 0; i < N; i++) { m_pivotVector[i] = i; }

            double[] tempRowi;
            double[] tempColj = new double[N];
        
            // Loop over columns
            for (int j = 0; j < N; j++)
            {
                for (int i = 0; i < N; i++) { tempColj[i] = luData[i][j]; }
        
                for (int i = 0; i < N; i++) 
                {
                    tempRowi = luData[i];
        
                    int kmax = Math.Min(i,j);
                    double s = 0.0;
                    for (int k = 0; k < kmax; k++)
                    {
                        s += tempRowi[k] * tempColj[k];
                    }
                    tempColj[i] -= s;
                    tempRowi[j] = tempColj[i];
                }
             
                // Find pivot and exchange if necessary
                int p = j;
                for (int i = j + 1; i < N; i++)
                {
                    if (Math.Abs(tempColj[i]) > Math.Abs(tempColj[p])) { p = i; }
                }

                if (p != j)
                {
                    for (int k = 0; k < N; k++) 
                    {
                        double t = luData[p][k]; 
                        luData[p][k] = luData[j][k]; 
                        luData[j][k] = t;
                    }
                            
                    int v = m_pivotVector[p]; 
                    m_pivotVector[p] = m_pivotVector[j]; 
                    m_pivotVector[j] = v;
                            
                    m_pivotSign = -m_pivotSign;
                }
        
                // Compute the multipliers.
                if ( (j < N) && (luData[j][j] != 0.0) ) 
                {
                    for (int i = j+1; i < N; i++) 
                    {
                        luData[i][j] /= luData[j][j];
                    }
                }
            }
        }
        

        /// <summary>
        /// Gets (a copy of) the lower triangular matrix.
        /// </summary>
        public Matrix LMatrix
        {
            get
            {
                int N = m_luMatrix.NumRows;
                Matrix LT = new Matrix(N, N);
                double[][] ltData = LT.Data;
                double[][] luData = m_luMatrix.Data;
                    
                for (int i = 0; i < N; i++) 
                {
                    for (int j = 0; j < N; j++)
                    {
                        ltData[i][j] = (i < j) ? 0.0 : (i == j) ? 1.0 : luData[i][j];
                    }
                }
                return LT;
            }
        }

        
        /// <summary>
        /// Gets (a copy of) the upper triangular matrix.
        /// </summary>
        public Matrix UMatrix
        {
            get
            {
                int N = m_luMatrix.NumRows;
                Matrix UT = new Matrix(N, N);
                double[][] utData = UT.Data;
                double[][] luData = m_luMatrix.Data;

                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < N; j++) 
                    {
                        utData[i][j] = (i > j) ? 0.0 : luData[i][j];
                    }
                }
                return UT;
            }
        }     	
            

        /// <summary>
        /// Computes the determinant of the input matrix.
        /// </summary>
        public double Determinant()
        {
            int N = m_luMatrix.NumRows;
            double det = (double) m_pivotSign;
            for (int j = 0; j < N; j++) 
            {
                det *= m_luMatrix[j, j];
            }
            return det;    
        }
        

        /// <summary>
        /// Computes the inverse of the input matrix.
        /// Returns null if the matrix is not invertible.
        /// </summary>
        public Matrix Inverse()
        {
            int N = m_luMatrix.NumRows;
            Matrix inv = new Matrix(N, N);
            double[][] invData = inv.Data;
            double[][] luData = m_luMatrix.Data;

            // Copy right hand side with pivoting
            for (int i = 0; i < N; i++)
            {
                int k = m_pivotVector[i];
                invData[i][k] = 1;
            }

            for (int k = 0; k < N; k++)
            {
                for (int i = k + 1; i < N; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        invData[i][j] -= invData[k][j] * luData[i][k];
                    }
                }
            }

            // Solve A*Ainv = I;
            for (int k = N - 1; k >= 0; k--)
            {
                for (int j = 0; j < N; j++)
                {
                    invData[k][j] /= luData[k][k];
                }
                for (int i = 0; i < k; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        invData[i][j] -= invData[k][j] * luData[i][k];
                    }
                }
            }

            // Check for NaNs and Infinities
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    double elem = invData[i][j];
                    if ( double.IsNaN(elem) || double.IsInfinity(elem) )
                    {
                        return null;
                    }
                }   
            }

            return inv;
        }


    }
}
