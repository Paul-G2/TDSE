using System;
using System.IO;


namespace TdseUtils
{
    /// <summary>
    /// This class represents an N-dimensional double-precision vector.
    /// </summary>
    public class Vector
    {
        #region Class data
        protected double[] m_data;
        #endregion Class data


        /// <summary>
        /// Constructor. 
        /// Creates a vector of a given length, with all elements initialized to zero. 
        /// </summary>
        public Vector(int size)
        {
            m_data = new double[size];
        }


        /// <summary>
        /// Copy constructor.
        /// </summary>
        public Vector(Vector src)
        {
            int size = src.Length;
            m_data = new double[size];
            Array.Copy(src.m_data, m_data, size);
        }


        /// <summary>
        /// Constructor.
        /// Creates a vector of a given length, with all elements initialized to a given value. 
        /// </summary>
        public Vector(int size, double value)
        {
            m_data = new double[size];
            for (int i=0; i<size; i++) 
            { 
                m_data[i] = value; 
            }
        }


        /// <summary>
        /// Constructor.
        /// Creates a vector of length 1, initialized to a given value. 
        /// </summary>
        public Vector(double x)
        {
            m_data = new double[]{x};
        }


        /// <summary>
        /// Constructor.
        /// Creates a vector of length 2, initialized to given values. 
        /// </summary>
        public Vector(double x, double y)
        {
            m_data = new double[]{x,y};
        }


        /// <summary>
        /// Constructor.
        /// Creates a vector of length 3, initialized to given values. 
        /// </summary>
        public Vector(double x, double y, double z)
        {
            m_data = new double[]{x,y,z};
        }
        
        
        /// <summary>
        /// Constructor.
        /// Creates a vector of a given length, with its elements initialized by a given function. 
        /// </summary>
        public Vector(int size,  Func<int, double> initFunc)
        {
            m_data = new double[size];
            for (int i=0; i<size; i++) 
            { 
                m_data[i] = initFunc(i); 
            }
        }


        /// <summary>
        /// Constructor.
        /// The passed-in array is used by reference, without copying.
        /// </summary>
        public Vector(double[] array)
        {
            m_data = (array == null) ? new double[0] : array;
        }


        /// <summary>
        /// Gets the number of elements in the Vector.
        /// </summary>
        public int Length
        {
            get
            {
                return m_data.Length;
            }
        }


        /// <summary>
        /// Gets the underlying array.
        /// </summary>
        public double[] Data
        {
            get
            {
                return m_data;
            }
        }


        /// <summary>
        /// Gets or sets a component of the vector.
        /// </summary>
        public double this[int indx]
        {
            get
            {
                return m_data[indx];
            }

            set
            {
                m_data[indx] = value;
            }
        }
        
        
        /// <summary>
        /// Indicates whether this Vector is equal to another one, within
        /// a given tolerance.
        /// </summary>
        public bool ValueEquals(Vector that, double tolerance = 0.0)
        {
            if ( ReferenceEquals(this, that) ) {  return true; }

            if (that == null) {  return false; }

            int thisLength = m_data.Length;
            if (that.Length != thisLength) { return false; }

            unsafe
            {
                fixed (double *pThis = m_data, pThat = that.m_data)
                {
                    for (int i=0; i<thisLength; i++)
                    {
                        if (Math.Abs(pThis[i] - pThat[i]) > tolerance) { return false; }
                    }
                }
            }
            return true;
        }


        /// <summary>
        /// Copies the data from another vector into this one.
        /// </summary>
        public void CopyDataFrom(Vector other)
        {
            if (other.Length != this.Length)
            {
                throw new ArgumentException("Vector size mismatch.");
            }

            Array.Copy(other.m_data, m_data, Length);
        }


        /// <summary>
        /// Negation operator. Returns a vector that is the negative of the given one.
        /// </summary>
        public static Vector operator-(Vector value)
        {
            Vector result = new Vector(value);
            result.MultiplyBy(-1.0);

            return result;
        }
        
        
        /// <summary>
        /// Addition operator
        /// </summary>
        public static Vector operator +(Vector A, Vector B)
        {
            // Check the input
            int ALength = A.Length;
            if (B.Length != ALength)
            {
                throw new ArgumentException("Vector size mismatch.");
            }

            Vector result = new Vector(A);
            unsafe
            {
                fixed (double *pResult = result.m_data, pB = B.m_data)
                {
                    for (int i=0; i<ALength; i++)
                    {
                        pResult[i] += pB[i];
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// In-place addition.
        /// </summary>
        public void Add(Vector that)
        {
            // Check the input
            int thisLength = m_data.Length;
            if (that.Length != thisLength)
            {
                throw new ArgumentException("Vector size mismatch.");
            }

            unsafe
            {
                fixed (double *pThis = m_data, pThat = that.m_data)
                {
                    for (int i=0; i<thisLength; i++)
                    {
                        pThis[i] += pThat[i];
                    }
                }
            }
        }


        /// <summary>
        /// Subtraction operator
        /// </summary>
        public static Vector operator -(Vector A, Vector B)
        {
            // Check the input
            int ALength = A.Length;
            if (B.Length != ALength)
            {
                throw new ArgumentException("Vector size mismatch.");
            }

            Vector result = new Vector(A);
            unsafe
            {
                fixed (double *pResult = result.m_data, pB = B.m_data)
                {
                    for (int i=0; i<ALength; i++)
                    {
                        pResult[i] -= pB[i];
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// In-place subtraction.
        /// </summary>
        public void Subtract(Vector that)
        {
            // Check the input
            int thisLength = m_data.Length;
            if (that.Length != thisLength)
            {
                throw new ArgumentException("Vector size mismatch.");
            }

            unsafe
            {
                fixed (double *pThis = m_data, pThat = that.m_data)
                {
                    for (int i=0; i<thisLength; i++)
                    {
                        pThis[i] -= pThat[i];
                    }
                }
            }
        }
        
        
        /// <summary>
        /// Multiplication by a scalar.
        /// </summary>
        public static Vector operator *(Vector A, double scale)
        {
            Vector result = new Vector(A);
            int length = A.Length;
            unsafe
            {
                fixed (double *pResult = result.m_data)
                {
                    for (int i=0; i<length; i++)
                    {
                        pResult[i] *= scale;
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// Multiplication by a scalar.
        /// </summary>
        public static Vector operator *(double scale, Vector A)
        {
            return A * scale;
        }


        /// <summary>
        /// In-place multiplication by a scalar.
        /// </summary>
        public void MultiplyBy(double scale)
        {
            int thisLength = m_data.Length;
            unsafe
            {
                fixed (double *pThis = m_data)
                {
                    for (int i=0; i<thisLength; i++)
                    {
                        pThis[i] *= scale;
                    }
                }
            }
        }
        
        
        /// <summary>
        /// Division by a scalar.
        /// </summary>
        public static Vector operator /(Vector A, double scale)
        {
            Vector result = new Vector(A);
            int length = A.Length;
            unsafe
            {
                fixed (double *pResult = result.m_data)
                {
                    for (int i=0; i<length; i++)
                    {
                        pResult[i] /= scale;
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// In-place division by a scalar.
        /// </summary>
        public void DivideBy(double scale)
        {
            int thisLength = m_data.Length;
            unsafe
            {
                fixed (double *pThis = m_data)
                {
                    for (int i=0; i<thisLength; i++)
                    {
                        pThis[i] /= scale;
                    }
                }
            }
        }
        
        
        /// <summary>
        /// Dot product.
        /// </summary>
        public double Dot(Vector that)
        {
            // Check the input
            int thisLength = Length;
            if (that.Length != thisLength)
            {
                throw new ArgumentException("Vector size mismatch.");
            }

            double result = 0.0;
            unsafe
            {
                fixed (double *pThisData = m_data, pThatData = that.m_data)
                {
                    for (int i=0; i<thisLength; i++)
                    {
                        result += pThisData[i] * pThatData[i];
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// Gets the squared norm of the Vector.
        /// </summary>
        public double NormSq()
        {
            double result = 0.0;
            unsafe
            {
                int length = Length;
                fixed (double *pData = m_data)
                {
                    for (int i=0; i<length; i++)
                    {
                        double comp = pData[i];
                        result += comp*comp;
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// Gets the norm of the Vector.
        /// </summary>
        public double Norm()
        {
            return Math.Sqrt( NormSq() );
        }


        /// <summary>
        /// Gets a normalized version of the Vector.
        /// </summary>
        public Vector Normalized()
        {
            return this * ( 1.0/Norm() );
        }


        /// <summary>
        /// Normalizes the Vector.
        /// </summary>
        public void Normalize()
        {
            this.MultiplyBy( 1.0/Norm() );
        }

         
        /// <summary>
        /// Returns a string representation of the vector.
        /// </summary>
        public override string ToString()
        {
            return ToString(null);
        }


        /// <summary>
        /// Returns a string representation of the vector.
        /// </summary>
        public string ToString(string format)
        {
            using (StringWriter sw = new StringWriter())
            {
                sw.Write("( ");
                for (int i = 0; i < Length; i++)
                {
                    if ( string.IsNullOrEmpty(format) )
                    {
                        sw.Write( (this.m_data[i]).ToString() + " ");
                    }
                    else
                    { 
                        sw.Write( (this.m_data[i]).ToString(format) + " ");
                    }
                }
                sw.Write(")");

                return sw.ToString();
            }
        }

    }
}
