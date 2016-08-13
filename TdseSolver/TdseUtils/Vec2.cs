using System;
using System.Drawing;


namespace TdseUtils
{
    /// <summary>
    /// This class represents a 2-component, single-precision vector.
    /// </summary>
    public class Vec2 
    {
        #region Class data
        public float X;
        public float Y;
        #endregion Class data


        /// <summary>
        /// Constructor. 
        /// Creates a vector with all elements initialized to zero. 
        /// </summary>
        public Vec2()
        {
            X = 0.0f;
            Y = 0.0f;
        }


        /// <summary>
        /// Constructor. 
        /// Creates a vector with specified component values. 
        /// </summary>
        public Vec2(float x, float y)
        {
            X = x;
            Y = y;
        }


        /// <summary>
        /// Constructor. 
        /// Creates a vector with specified component values. 
        /// </summary>
        public Vec2(double x, double y)
        {
            X = (float)x;
            Y = (float)y;
        }


        /// <summary>
        /// Constructor.
        /// Creates a Vec2 from a PointF.
        /// </summary>
        public Vec2(PointF src)
        {
            X = src.X;
            Y = src.Y;
        }


        /// <summary>
        /// Constructor.
        /// Creates a Vec2 from a Point.
        /// </summary>
        public Vec2(Point src)
        {
            X = src.X;
            Y = src.Y;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public Vec2(float[] array)
        {
            if ( (array == null) || (array.Length != 2) )
            {
                throw new ArgumentException("Invalid array passed to Vec2 constructor.");
            }

            X = array[0];
            Y = array[1];
        }


        /// <summary>
        /// Copy constructor.
        /// </summary>
        public Vec2(Vec2 src)
        {
            X = src.X;
            Y = src.Y;
        }


        /// <summary>
        /// Converts the Vec2 to a PointF.
        /// </summary>
        public PointF ToPointF()
        {
            return new PointF( X, Y );
        }


        /// <summary>
        /// Converts the Vec2 to a Point.
        /// </summary>
        public Point ToPoint()
        {
            return new Point( (int)Math.Round(X), (int)Math.Round(Y) );
        }


        /// <summary>
        /// Converts an array of Vec2's to an array of PointF's.
        /// </summary>
        public static PointF[] ToPointFArray( Vec2[] inArray )
        {
            if (inArray == null) { return null; }

            int N = inArray.Length;
            PointF[] outArray = new PointF[N];
            for (int i=0; i<N; i++)
            {
                outArray[i] = inArray[i].ToPointF();
            }
            return outArray;
        }


        /// <summary>
        /// Converts an array of Vec2's to an array of Points.
        /// </summary>
        public static Point[] ToPointArray( Vec2[] inArray )
        {
            if (inArray == null) { return null; }

            int N = inArray.Length;
            Point[] outArray = new Point[N];
            for (int i=0; i<N; i++)
            {
                outArray[i] = inArray[i].ToPoint();
            }
            return outArray;
        }


        /// <summary>
        /// Converts an array of PointF's to an array of Vec2's.
        /// </summary>
        public static Vec2[] ToVec2Array( PointF[] inArray )
        {
            if (inArray == null) { return null; }

            int N = inArray.Length;
            Vec2[] outArray = new Vec2[N];
            for (int i=0; i<N; i++)
            {
                outArray[i] = new Vec2( inArray[i] );
            }
            return outArray;
        }


        /// <summary>
        /// Converts an array of Point's to an array of Vec2's.
        /// </summary>
        public static Vec2[] ToVec2Array( Point[] inArray )
        {
            if (inArray == null) { return null; }

            int N = inArray.Length;
            Vec2[] outArray = new Vec2[N];
            for (int i=0; i<N; i++)
            {
                outArray[i] = new Vec2( inArray[i] );
            }
            return outArray;
        }


        /// <summary>
        /// Gets or sets a component of the vector.
        /// </summary>
        public float this[int indx]
        {
            get
            {
                if (indx == 0)
                {
                    return X;
                }
                else if (indx == 1)
                {
                    return Y;
                }
                else
                {
                    throw new ArgumentException("Invalid argument passed to Vec2 index operator.");
                }
            }

            set
            {
                if (indx == 0)
                {
                    X = value;
                }
                else if (indx == 1)
                {
                    Y = value;
                }
                else
                {
                    throw new ArgumentException("Invalid argument passed to Vec2 index operator.");
                }
            }
        }
        

        /// <summary>
        /// The zero vector.
        /// </summary>
        public static Vec2 Zero
        {
            get { return new Vec2(0.0f, 0.0f); }
        }


        /// <summary>
        /// The unit vector in the x-direction.
        /// </summary>
        public static Vec2 UnitX
        {
            get { return new Vec2( 1.0f, 0.0f ); }
        }


        /// <summary>
        /// The unit vector in the y-direction.
        /// </summary>
        public static Vec2 UnitY
        {
            get { return new Vec2( 0.0f, 1.0f ); }
        }


        /// <summary>
        /// Indicates whether this Vector is equal to another one, within
        /// a given tolerance.
        /// </summary>
        public bool ValueEquals(Vec2 that, float tolerance = 0.0f)
        {
            if ( ReferenceEquals(this, that) ) {  return true; }

            if (that == null) {  return false; }

            if (Math.Abs(this.X - that.X) > tolerance) { return false; }
            if (Math.Abs(this.Y - that.Y) > tolerance) { return false; }

            return true;
        }


        /// <summary>
        /// Copies the data from another vector into this one.
        /// </summary>
        public void CopyDataFrom(Vec2 other)
        {
            X = other.X;
            Y = other.Y;
        }


        /// <summary>
        /// Negation operator. Returns a vector that is the negative of the given one.
        /// </summary>
        public static Vec2 operator-(Vec2 value)
        {
            return new Vec2(-value.X, -value.Y);
        }
        
        
        /// <summary>
        /// Addition operator
        /// </summary>
        public static Vec2 operator +(Vec2 A, Vec2 B)
        {
            return  new Vec2(A.X + B.X,  A.Y + B.Y);
        }

        
        /// <summary>
        /// In-place addition.
        /// </summary>
        public void Add(Vec2 that)
        {
            X += that.X;
            Y += that.Y;
        }
        
        
        /// <summary>
        /// Subtraction operator
        /// </summary>
        public static Vec2 operator -(Vec2 A, Vec2 B)
        {
            return  new Vec2(A.X - B.X,  A.Y - B.Y);
        }


        /// <summary>
        /// In-place subtraction.
        /// </summary>
        public void Subtract(Vec2 that)
        {
            X -= that.X;
            Y -= that.Y;
        }
        
        
        /// <summary>
        /// Multiplication by a scalar.
        /// </summary>
        public static Vec2 operator *(Vec2 A, float scale)
        {
            return  new Vec2(scale*A.X,  scale*A.Y);
        }


        /// <summary>
        /// Multiplication by a scalar.
        /// </summary>
        public static Vec2 operator *(Vec2 A, double scale)
        {
            return  new Vec2(scale*A.X,  scale*A.Y);
        }


        /// <summary>
        /// Multiplication by a scalar.
        /// </summary>
        public static Vec2 operator *(float scale, Vec2 A)
        {
            return  new Vec2(scale*A.X,  scale*A.Y);
        }


        /// <summary>
        /// Multiplication by a scalar.
        /// </summary>
        public static Vec2 operator *(double scale, Vec2 A)
        {
            return  new Vec2(scale*A.X,  scale*A.Y);
        }


        /// <summary>
        /// In-place multiplication.
        /// </summary>
        public void MultiplyBy(float scale)
        {
            X *= scale;
            Y *= scale;
        }
        
        
        /// <summary>
        /// In-place multiplication.
        /// </summary>
        public void MultiplyBy(double scale)
        {
            X = (float) (X*scale);
            Y = (float) (Y*scale);
        }
        
        
        /// <summary>
        /// Division by a scalar.
        /// </summary>
        public static Vec2 operator /(Vec2 A, float scale)
        {
            return  new Vec2(A.X/scale,  A.Y/scale);
        }


        /// <summary>
        /// Division by a scalar.
        /// </summary>
        public static Vec2 operator /(Vec2 A, double scale)
        {
            return  new Vec2(A.X/scale,  A.Y/scale);
        }


        /// <summary>
        /// In-place division.
        /// </summary>
        public void DivideBy(double scale)
        {
            X = (float) (X/scale);
            Y = (float) (Y/scale);
        }
        
        
        /// <summary>
        /// Dot product.
        /// </summary>
        public float Dot(Vec2 that)
        {
            return (X*that.X + Y*that.Y);
        }


        /// <summary>
        /// Cross product.
        /// </summary>
        public float Cross(Vec2 that)
        {
             return (X*that.Y - Y*that.X);
        }


        /// <summary>
        /// Creates a vector that is perpendicular to (and has the same magnitude as) this one.
        /// </summary>
        public Vec2 Perp()
        {
            return new Vec2(Y, -X);
        }


        /// <summary>
        /// Gets the squared norm of the Vector.
        /// </summary>
        public float NormSq()
        {
            return X*X + Y*Y;
        }


        /// <summary>
        /// Gets the norm of the Vector.
        /// </summary>
        public float Norm()
        {
            return (float) Math.Sqrt( X*X + Y*Y );
        }


        /// <summary>
        /// Gets a normalized version of the Vector.
        /// </summary>
        public Vec2 Normalized()
        {
            return this * ( 1.0f/Norm() );
        }


        /// <summary>
        /// Normalizes the Vector.
        /// </summary>
        public void Normalize()
        {
            this.MultiplyBy( 1.0f/Norm() );
        }


        /// <summary>
        /// Gets a rotated version of the vector.
        /// </summary>
        public Vec2 Rotate(double angleRads)
        {
            double cos = Math.Cos(angleRads);
            double sin = Math.Sin(angleRads);

            return new Vec2(cos*X - sin*Y, sin*X + cos*Y);
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
            if ( string.IsNullOrEmpty(format) )
            {
                return "(" + X.ToString() + ", " + Y.ToString() + ")";
            }
            else
            {
                return "(" + X.ToString(format) + ", " + Y.ToString(format) + ")";
            }
        }

    }
}