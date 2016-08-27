using System;
using System.IO;


namespace TdseUtils
{
    /// <summary>
    /// This class represents a double-precision matrix.
    /// </summary>
    public class Matrix
    {
        #region Class data
        protected double[][] m_data;
        protected int        m_numRows;
        protected int        m_numCols;
        #endregion Class data

    
        /// <summary>
        /// Constructor. 
        /// Creates a matrix of a given size, with all elements initialized to zero. 
        /// </summary>
        public Matrix(int numRows, int numColumns)
        {
            m_numRows = numRows;
            m_numCols = numColumns;
            m_data = new double[numRows][];

            for (int i = 0; i < numRows; i++)
            {
                m_data[i] = new double[numColumns];
            }
        }
    

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public Matrix(Matrix src)
        {
            m_numRows = src.m_numRows;
            m_numCols = src.m_numCols;
            m_data = new double[m_numRows][];

            for (int i = 0; i < m_numRows; i++)
            {
                m_data[i] = new double[m_numCols];
                Array.Copy(src.m_data[i], m_data[i], m_numCols);
            }
        }



    
        /// <summary>
        /// Constructor.
        /// The passed-in array is used by reference, without deep-copying.
        /// </summary>
        public Matrix( double[][] array )
        {
            m_numRows = array.Length;
            m_numCols = (m_numRows == 0) ? 0 : array[0].Length;
    
            for (int i = 0; i < m_numRows; i++)
            {
                if (array[i].Length != m_numCols)
                {
                    throw new ArgumentException("Invalid array passed to Matrix constructor."); 
                }
            }
            this.m_data = array;
        }			
    

        /// <summary>
        /// Creates an identity matrix of a given size.
        /// </summary>
        public static Matrix Identity(int size)
        {
            return Matrix.Diagonal(size, 1.0);
        }


        /// <summary>
        /// Creates a diagonal matrix of a given size.
        /// </summary>
        public static Matrix Diagonal(int size, double value)
        {
            Matrix result = new Matrix(size, size);

            double[][] rData = result.m_data;
            for (int i=0; i<size; i++)
            {
                rData[i][i] = value;
            }
            return result;
        }


        /// <summary>
        /// Creates a diagonal matrix from a given list of values.
        /// </summary>
        public static Matrix Diagonal(double[] values)
        {
            int size = values.Length;
            Matrix result = new Matrix(size, size);

            double[][] rData = result.m_data;
            for (int i=0; i<size; i++)
            {
                rData[i][i] = values[i];
            }
            return result;
        }
        

        /// <summary>
        /// Gets the underlying data array.
        /// </summary>
        public double[][] Data
        {
            get 
            { 
                return this.m_data; 
            }
        }

    
        /// <summary>
        /// Gets the number of rows in the matrix.
        /// </summary>
        public int NumRows
        {
            get 
            { 
                return this.m_numRows; 
            }
        }

        
        /// <summary>
        /// Gets the number of columns in the matrix.
        /// </summary>
        public int NumCols
        {
            get 
            { 
                return this.m_numCols; 
            }
        }


        /// <summary>
        /// Gets or sets an element of the matrix.
        /// </summary>
        public double this[int row, int column]
        {
            set 
            { 
                this.m_data[row][column] = value; 
            }
            
            get 
            { 
                return this.m_data[row][column]; 
            }
        }
        

        /// <summary>
        /// Gets or sets a row of the matrix.
        /// </summary>
        public double[] this[int row]
        {
            set 
            { 
                if (value.Length != m_numCols)
                {
                    throw new ArgumentException("Row size mismatch.");
                }
                this.m_data[row] = value; 
            }
            
            get 
            { 
                return this.m_data[row]; 
            }
        }
        

        /// <summary>
        /// Indicates whether this Matrix is equal to another one, within
        /// a given tolerance.
        /// </summary>
        public bool ValueEquals(Matrix that, double tolerance = 0.0)
        {
            if ( ReferenceEquals(this, that) ) {  return true; }

            if (that == null) {  return false; }
            if (m_numRows != that.m_numRows) { return false; }
            if (m_numCols != that.m_numCols) { return false; }

            for (int i=0; i<m_numRows; i++)
            {
                double[] thisRowData = m_data[i];
                double[] thatRowData = that.m_data[i];
                for (int j=0; j<m_numCols; j++)
                {
                    if (Math.Abs(thisRowData[j] - thatRowData[j]) > tolerance) { return false; }
                }
            }

            return true;
        }


        /// <summary>
        /// Indicates whether the matrix is square.
        /// </summary>
        public bool IsSquare
        {
            get 
            { 
                return (m_numRows == m_numCols); 
            }
        }


        /// <summary>
        /// Indicates whether the matrix is symmetric.
        /// </summary>
        public bool IsSymmetric(double tol = 0.0)
        {
            if (this.IsSquare)
            {
                for (int i = 0; i < m_numRows; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if ( Math.Abs(m_data[i][j] - m_data[j][i]) > tol )
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            return false;
        }


        /// <summary>
        /// Indicates whether the matrix is a zero matrix.
        /// </summary>
        public bool IsZero(double tol = 0.0)
        {
            for (int i = 0; i < m_numRows; i++)
            {
                for (int j = 0; j < m_numCols; j++)
                {
                    if ( Math.Abs(m_data[i][j]) > tol )
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        /// <summary>
        /// Returns the trace of the matrix (i.e. the sum of the diagonal elements.)
        /// </summary>
        public double Trace()
        {
            if ( !IsSquare )
            {
                throw new ArgumentException("Cannot take the trace of a non-square matrix." );
            }

            double trace = 0.0;
            for (int i=0; i<m_numRows; i++)
            {
                trace += m_data[i][i];
            }
            return trace;
        }
        

        /// <summary>
        /// Returns the transpose of this matrix.
        /// </summary>
        public Matrix Transpose()
        {
            Matrix T = new Matrix(m_numCols, m_numRows);
            double[][] tData = T.m_data;
            for (int i = 0; i < m_numRows; i++)
            {
                for (int j = 0; j < m_numCols; j++)
                {
                    tData[j][i] = m_data[i][j];
                }
            }
            return T;
        }


        /// <summary>
        /// Computes the determinant of the matrix.
        /// </summary>
        public double Determinant()
        {
            if (!IsSquare)
            {
                throw new ArgumentException("Non-square matrix passed to Matrix.Determinnant.");
            }
            return new LUDecomp(this).Determinant();
        }


        /// <summary>
        /// Computes the inverse of the matrix.
        /// </summary>
        public Matrix Inverse()
        {
            if (!IsSquare)
            {
                throw new ArgumentException("Non-square matrix passed to Matrix.Inverse.");
            }
            return new LUDecomp(this).Inverse();
        }


        /// <summary>
        /// Returns a string representation of the matrix.
        /// </summary>
        public override string ToString()
        {
            return ToString(null);
        }


        /// <summary>
        /// Returns a string representation of the matrix.
        /// </summary>
        public string ToString(string format)
        {
            using (StringWriter sw = new StringWriter())
            {
                for (int i = 0; i < m_numRows; i++)
                {
                    for (int j = 0; j < m_numCols; j++)
                    {
                        if ( string.IsNullOrEmpty(format) )
                        {
                            sw.Write( (this.m_data[i][j]).ToString() + " ");
                        }
                        else
                        { 
                            sw.Write( (this.m_data[i][j]).ToString(format) + " ");
                        }
                    }
                    sw.WriteLine();
                }

                return sw.ToString();
            }
        }

    }

}
