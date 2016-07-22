using System;
using System.Runtime.Serialization;

namespace TdseUtils
{
    /// <summary>
    /// This class represents a 3-component, single-precision vector.
    /// </summary>
    public class Vec3 
    {
        #region Class data
        public float X;
        public float Y;
        public float Z;
        #endregion Class data


        /// <summary>
        /// Constructor. 
        /// Creates a vector with all elements initialized to zero. 
        /// </summary>
        public Vec3()
        {
            X = 0.0f;
            Y = 0.0f;
            Z = 0.0f;
        }


        /// <summary>
        /// Constructor. 
        /// Creates a vector with specified component values. 
        /// </summary>
        public Vec3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }


        /// <summary>
        /// Constructor. 
        /// Creates a vector with specified component values. 
        /// </summary>
        public Vec3(double x, double y, double z)
        {
            X = (float) x;
            Y = (float) y;
            Z = (float) z;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public Vec3(float[] array)
        {
            if ( (array == null) || (array.Length != 3) )
            {
                throw new ArgumentException("Invalid array passed to Vec3 constructor.");
            }

            X = array[0];
            Y = array[1];
            Z = array[2];
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public Vec3(double[] array)
        {
            if ( (array == null) || (array.Length != 3) )
            {
                throw new ArgumentException("Invalid array passed to Vec3 constructor.");
            }

            X = (float) array[0];
            Y = (float) array[1];
            Z = (float) array[2];
        }


        /// <summary>
        /// Copy constructor.
        /// </summary>
        public Vec3(Vec3 src)
        {
            X = src.X;
            Y = src.Y;
            Z = src.Z;
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
                else if (indx == 2)
                {
                    return Z;
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
                else if (indx == 2)
                {
                    Z = value;
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
        public static Vec3 Zero
        {
            get { return new Vec3(0.0f, 0.0f, 0.0f); }
        }


        /// <summary>
        /// The unit vector in the x-direction.
        /// </summary>
        public static Vec3 UnitX
        {
            get { return new Vec3( 1.0f, 0.0f, 0.0f ); }
        }


        /// <summary>
        /// The unit vector in the y-direction.
        /// </summary>
        public static Vec3 UnitY
        {
            get { return new Vec3( 0.0f, 1.0f, 0.0f ); }
        }


        /// <summary>
        /// The unit vector in the z-direction.
        /// </summary>
        public static Vec3 UnitZ
        {
            get { return new Vec3( 0.0f, 0.0f, 1.0f ); }
        }


        /// <summary>
        /// Indicates whether this Vector is equal to another one, within
        /// a given tolerance.
        /// </summary>
        public bool ValueEquals(Vec3 that, float tolerance = 0.0f)
        {
            if ( ReferenceEquals(this, that) ) {  return true; }

            if (that == null) {  return false; }

            if (Math.Abs(this.X - that.X) > tolerance) { return false; }
            if (Math.Abs(this.Y - that.Y) > tolerance) { return false; }
            if (Math.Abs(this.Z - that.Z) > tolerance) { return false; }

            return true;
        }


        /// <summary>
        /// Copies the data from another vector into this one.
        /// </summary>
        public void CopyDataFrom(Vec3 other)
        {
            X = other.X;
            Y = other.Y;
            Z = other.Z;
        }


        /// <summary>
        /// Negation operator. Returns a vector that is the negative of the given one.
        /// </summary>
        public static Vec3 operator-(Vec3 value)
        {
            return new Vec3(-value.X, -value.Y, -value.Z);
        }
        
        
        /// <summary>
        /// Addition operator.
        /// </summary>
        public static Vec3 operator +(Vec3 A, Vec3 B)
        {
            return  new Vec3(A.X + B.X,  A.Y + B.Y,  A.Z + B.Z);
        }

        
        /// <summary>
        /// In-place addition.
        /// </summary>
        public void Add(Vec3 that)
        {
            X += that.X;
            Y += that.Y;
            Z += that.Z;
        }
        
        
        /// <summary>
        /// Subtraction operator.
        /// </summary>
        public static Vec3 operator -(Vec3 A, Vec3 B)
        {
            return  new Vec3(A.X - B.X,  A.Y - B.Y,  A.Z - B.Z);
        }


        /// <summary>
        /// In-place subtraction.
        /// </summary>
        public void Subtract(Vec3 that)
        {
            X -= that.X;
            Y -= that.Y;
            Z -= that.Z;
        }


        /// <summary>
        /// Multiplication by a scalar.
        /// </summary>
        public static Vec3 operator *(Vec3 A, float scale)
        {
            return  new Vec3(scale*A.X,  scale*A.Y,  scale*A.Z);
        }


        /// <summary>
        /// Multiplication by a scalar.
        /// </summary>
        public static Vec3 operator *(Vec3 A, double scale)
        {
            return  new Vec3(scale*A.X,  scale*A.Y,  scale*A.Z);
        }


        /// <summary>
        /// Multiplication by a scalar.
        /// </summary>
        public static Vec3 operator *(float scale, Vec3 A)
        {
            return  new Vec3(scale*A.X,  scale*A.Y,  scale*A.Z);
        }


        /// <summary>
        /// Multiplication by a scalar.
        /// </summary>
        public static Vec3 operator *(double scale, Vec3 A)
        {
            return  new Vec3(scale*A.X,  scale*A.Y,  scale*A.Z);
        }


        /// <summary>
        /// In-place multiplication.
        /// </summary>
        public void MultiplyBy(float scale)
        {
            X *= scale;
            Y *= scale;
            Z *= scale;
        }


        /// <summary>
        /// In-place multiplication.
        /// </summary>
        public void MultiplyBy(double scale)
        {
            X = (float) (scale * X);
            Y = (float) (scale * Y);
            Z = (float) (scale * Z);
        }


        /// <summary>
        /// Division by a scalar.
        /// </summary>
        public static Vec3 operator /(Vec3 A, float scale)
        {
            return  new Vec3(A.X/scale,  A.Y/scale,  A.Z/scale);
        }


        /// <summary>
        /// Division by a scalar.
        /// </summary>
        public static Vec3 operator /(Vec3 A, double scale)
        {
            return  new Vec3(A.X/scale,  A.Y/scale,  A.Z/scale);
        }


        /// <summary>
        /// In-place division.
        /// </summary>
        public void DivideBy(double scale)
        {
            X = (float) (X/scale);
            Y = (float) (Y/scale);
            Z = (float) (Z/scale);
        }
        
        
        /// <summary>
        /// Dot product.
        /// </summary>
        public float Dot(Vec3 that)
        {
            return (X*that.X + Y*that.Y + Z*that.Z);
        }


        /// <summary>
        /// Cross product.
        /// </summary>
        public Vec3 Cross(Vec3 that)
        {
            return new Vec3(Y*that.Z - Z*that.Y,  Z*that.X - X*that.Z,  X*that.Y - Y*that.X);
        }


        /// <summary>
        /// Gets the squared norm of the Vector.
        /// </summary>
        public float NormSq()
        {
            return X*X + Y*Y + Z*Z;
        }


        /// <summary>
        /// Gets the norm of the Vector.
        /// </summary>
        public float Norm()
        {
            return  (float) Math.Sqrt( X*X + Y*Y + Z*Z );
        }


        /// <summary>
        /// Gets a normalized version of the Vector.
        /// </summary>
        public Vec3 Normalized()
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
                return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
            }
            else
            {
                return "(" + X.ToString(format) + ", " + Y.ToString(format) + ", " + Z.ToString(format) + ")";
            }
        }

    }
}

