using System;
using System.Runtime.Serialization;


namespace TdseSolver
{
    /// <summary>
    /// This structure represents a single-precision complex number.
    /// </summary>
    public struct Complex
    {
        #region Class data
        public float Re;
        public float Im;
        #endregion Class data


        /// <summary>
        /// Constructor. 
        /// </summary>
        public Complex(float re, float im)
        {
            Re = re;
            Im = im;
        }


        /// <summary>
        /// Constructor. 
        /// </summary>
        public Complex(double re, double im)
        {
            Re = (float)re;
            Im = (float)im;
        }


        /// <summary>
        /// Returns the square root of -1.
        /// </summary>
        public static readonly Complex I = new Complex(0.0f, 1.0f);


        /// <summary>
        /// Returns the complex number zero.
        /// </summary>
        public static readonly Complex Zero = new Complex(0.0f, 0.0f);


        /// <summary>
        /// Indicates whether this ComplexNumber is equal to another one, within
        /// a given tolerance.
        /// </summary>
        public bool ValueEquals(Complex that, float tolerance = 0.0f)
        {
            if ( ReferenceEquals(this, that) ) {  return true; }

            if (Math.Abs(this.Re - that.Re) > tolerance) { return false; }
            if (Math.Abs(this.Im - that.Im) > tolerance) { return false; }

            return true;
        }

        
        /// <summary>
        /// Gets the phase of the complex number, in radians. The result will be in the range [0, 2PI).
        /// </summary>
        public float Phase
        {
            get
            {
                double angle = Math.Atan2(Im, Re);
                if (angle < 0.0) { angle = 2*Math.PI + angle; }
                return (float) angle;
            }
        }


        /// <summary>
        /// Gets the complex conjugate of the number.
        /// </summary>
        public Complex Conj
        {
            get
            {
                return new Complex(Re, -Im);
            }
        }


        /// <summary>
        /// Negation operator.
        /// </summary>
        public static Complex operator -(Complex A)
        {
            return new Complex(-A.Re, -A.Im);
        }
        
        
        /// <summary>
        /// Addition operator.
        /// </summary>
        public static Complex operator+(Complex A, Complex B)
        {
            return new Complex(A.Re + B.Re, A.Im + B.Im);
        }


        /// <summary>
        /// Addition operator.
        /// </summary>
        public static Complex operator+(Complex A, float B)
        {
            return new Complex(A.Re + B, A.Im);
        }


        /// <summary>
        /// Addition operator.
        /// </summary>
        public static Complex operator+(float A, Complex B)
        {
            return new Complex(A + B.Re, B.Im);
        }


        /// <summary>
        /// Subtraction operator.
        /// </summary>
        public static Complex operator-(Complex A, Complex B)
        {
            return new Complex(A.Re - B.Re, A.Im - B.Im);
        }


        /// <summary>
        /// Subtraction operator.
        /// </summary>
        public static Complex operator-(Complex A, float B)
        {
            return new Complex(A.Re - B, A.Im);
        }


        /// <summary>
        /// Subtraction operator.
        /// </summary>
        public static Complex operator-(float A, Complex B)
        {
            return new Complex(A - B.Re, -B.Im);
        }
        
        
        /// <summary>
        /// Multiplication operator.
        /// </summary>
        public static Complex operator*(Complex A, Complex B)
        {
            return new Complex(A.Re*B.Re - A.Im*B.Im, A.Re*B.Im + A.Im*B.Re);
        }


        /// <summary>
        /// Multiplication-by-scalar operator.
        /// </summary>
        public static Complex operator*(Complex A, float F)
        {
            return new Complex(F*A.Re, F*A.Im);
        }


        /// <summary>
        /// Multiplication-by-scalar operator.
        /// </summary>
        public static Complex operator*(Complex A, double F)
        {
            return new Complex(F*A.Re, F*A.Im);
        }


        /// <summary>
        /// Multiplication-by-scalar operator.
        /// </summary>
        public static Complex operator*(float F, Complex A)
        {
            return new Complex(F*A.Re, F*A.Im);
        }


        /// <summary>
        /// Multiplication-by-scalar operator.
        /// </summary>
        public static Complex operator*(double F, Complex A)
        {
            return new Complex(F*A.Re, F*A.Im);
        }


        /// <summary>
        /// Division operator.
        /// </summary>
        public static Complex operator/(Complex A, Complex B)
        {
            double den = B.Re*B.Re + B.Im*B.Im;
            return new Complex( (A.Re*B.Re + A.Im*B.Im)/den,  (A.Im*B.Re - A.Re*B.Im)/den );
        }


        /// <summary>
        /// Division operator.
        /// </summary>
        public static Complex operator/(float F, Complex B)
        {
            double den = B.Re*B.Re + B.Im*B.Im;
            return new Complex( (F*B.Re)/den,  (-F*B.Im)/den );
        }


        /// <summary>
        /// Division operator.
        /// </summary>
        public static Complex operator/(double F, Complex B)
        {
            double den = B.Re*B.Re + B.Im*B.Im;
            return new Complex( (F*B.Re)/den,  (-F*B.Im)/den );
        }


        /// <summary>
        /// Division-by-scalar operator.
        /// </summary>
        public static Complex operator/(Complex A, float F)
        {
            return new Complex(A.Re/F, A.Im/F);
        }


        /// <summary>
        /// Division-by-scalar operator.
        /// </summary>
        public static Complex operator/(Complex A, double F)
        {
            return new Complex(A.Re/F, A.Im/F);
        }


        /// <summary>
        /// Returns the magnitude of the complex number.
        /// </summary>
        public float Magnitude
        {
            get { return (float)Math.Sqrt(Re*Re + Im*Im); }
        }

        
        /// <summary>
        /// Returns the squared magnitude of the complex number.
        /// </summary>
        public float MagnitudeSq
        {
            get { return Re*Re + Im*Im; }
        }


        /// <summary>
        /// Returns the polar representation of a complex number.
        /// </summary>
        public void ToPolar(out float magnitude, out float phase)
        {
            magnitude = Magnitude;
            phase = Phase;
        }


        /// <summary>
        /// Returns the square root of a complex number.
        /// </summary>
        public static Complex Sqrt(Complex A)
        {
            float mag = A.Magnitude;
            double re = Math.Sqrt( 0.5*(mag + A.Re) );
            double im = Math.Sqrt( 0.5*(mag - A.Re) ) * Math.Sign(A.Im);

            return new Complex(re, im);
        }


        /// <summary>
        /// Returns the exponential of a complex number.
        /// </summary>
        public static Complex Exp(Complex A)
        {
            double expR = Math.Exp(A.Re);
            return new Complex( expR*Math.Cos(A.Im), expR*Math.Sin(A.Im) );
        }
        

        /// <summary>
        /// Returns the natural logarithm of a complex number.
        /// </summary>
        public static Complex Log(Complex A)
        {
            double expR = Math.Exp(A.Re);
            return new Complex( Math.Log(A.Magnitude), A.Phase );
        }


        /// <summary>
        /// Returns the sine of a complex number.
        /// </summary>
        public static Complex Sin(Complex A)
        {
            double re = Math.Sin(A.Re) * Math.Cosh(A.Im);
            double im = Math.Cos(A.Re) * Math.Sinh(A.Im);
            return new Complex(re, im);
        }


        /// <summary>
        /// Returns the cosine of a complex number.
        /// </summary>
        public static Complex Cos(Complex A)
        {
            double re =  Math.Cos(A.Re) * Math.Cosh(A.Im);
            double im = -Math.Sin(A.Re) * Math.Sinh(A.Im);
            return new Complex(re, im);
        }


        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        public override string ToString()
        {
            return "(" + Re.ToString() + ", " + Im.ToString() + ")";
        }
 
    }
}
