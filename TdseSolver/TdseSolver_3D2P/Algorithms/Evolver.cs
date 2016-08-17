using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Vec3 = TdseUtils.Vec3;


namespace TdseSolver_3D2P
{
    /// <summary>
    /// This class computes the time evolution of a wavefunction.
    /// </summary>
    class Evolver : TdseUtils.Proc
    {
        // Declare a delegate for computing the potential energy
        public delegate float VDelegate(float rx, float ry, float rz, float mass, float sx, float sy, float sz);


        // Class data
        private int            m_gridSizeX             = 0;
        private int            m_gridSizeY             = 0;
        private int            m_gridSizeZ             = 0;
        private float          m_latticeSpacing        = 0.0f; 
        private float          m_totalTime             = float.NaN;
        private float          m_deltaT                = float.NaN;
        private int            m_totalNumTimeSteps     = 0;
        private int            m_currentTimeStepIndex  = 0;

        private float          m_mass1                 = 0.0f;
        private float          m_mass2                 = 0.0f;
        private float          m_totalMass             = 0.0f;
        private float          m_reducedMass           = 0.0f;
        private VDelegate      m_potential             = null;

        private Vec3           m_initialMomentum1      = Vec3.Zero;
        private Vec3           m_initialMomentum2      = Vec3.Zero;
        private Vec3           m_initialPosition1      = Vec3.Zero;
        private Vec3           m_initialPosition2      = Vec3.Zero;
        private Vec3           m_sigmaRel              = Vec3.Zero;
        private Vec3           m_sigmaCm               = Vec3.Zero;

        private int            m_dampingBorderWidth    = 0;
        private float          m_dampingFactor         = 0.0f;

        private int            m_numFramesToSave       = 0;
        private int            m_lastSavedFrame        = -1;
        private string         m_outputDir             = "";
        private bool           m_multiThread           = true;

        private VisscherWf     m_visscherWf            = null;



        /// <summary>
        /// Constructor.
        /// </summary>
        public Evolver(RunParams parms, VDelegate V, string outputDir)
        {
            m_gridSizeX            = parms.GridSpec.SizeX;
            m_gridSizeY            = parms.GridSpec.SizeY;
            m_gridSizeZ            = parms.GridSpec.SizeZ;
            m_latticeSpacing       = parms.LatticeSpacing;
            m_totalTime            = parms.TotalTime;
            m_deltaT               = parms.TimeStep;
            m_totalNumTimeSteps    = (int) Math.Round(parms.TotalTime/parms.TimeStep) + 1;
            m_currentTimeStepIndex = 0;

            m_mass1                = parms.Mass1;
            m_mass2                = parms.Mass2;
            m_totalMass            = (parms.Mass1 + parms.Mass2);
            m_reducedMass          = (parms.Mass1 * parms.Mass2)/m_totalMass;
            m_potential            = V;
            
            m_initialMomentum1     = new Vec3(parms.InitialWavePacketMomentum1);
            m_initialMomentum2     = new Vec3(parms.InitialWavePacketMomentum2);
            m_initialPosition1     = new Vec3(parms.InitialWavePacketCenter1);
            m_initialPosition2     = new Vec3(parms.InitialWavePacketCenter2);
            m_sigmaRel             = parms.InitialWavePacketSize * Math.Sqrt(m_totalMass/m_mass2);
            m_sigmaCm              = parms.InitialWavePacketSize * Math.Sqrt(m_mass1/m_totalMass);
            
            m_dampingBorderWidth   = parms.DampingBorderWidth;
            m_dampingFactor        = parms.DampingFactor;

            m_numFramesToSave      = parms.NumFramesToSave;
            m_lastSavedFrame       = -1;
            m_outputDir            = outputDir;
            m_multiThread          = parms.MultiThread;

            m_visscherWf           = null;
        }


        /// <summary>
        /// Gets the total number of timesteps that the Evolver will step through.
        /// </summary>
        public int TotalNumTimeSteps
        {
            get { return m_totalNumTimeSteps; }
        }


        /// <summary>
        /// Gets the current time step index.
        /// </summary>
        public int CurrentTimeStepIndex
        {
            get { return m_currentTimeStepIndex; }
        }


        /// <summary>
        /// Gets the wavefunction that's being evolved.
        /// </summary>
        public WaveFunction Wf
        {
            get
            {
                return (m_visscherWf == null) ? null : m_visscherWf.ToRegularWavefunction();
            }
        }


        /// <summary>
        /// Worker method.
        /// </summary>
        protected override void WorkerMethod()
        {
            // Reset counters
            m_currentTimeStepIndex = 0;
            m_lastSavedFrame = -1;

            // Precompute the potential everywhere on the grid
            float[][][] V = PrecomputeV();

            // Create the initial relative wavefunction
            Vec3 r0 = m_initialPosition1 - m_initialPosition2;
            Vec3 p0 = (m_mass2/m_totalMass)*m_initialMomentum1 - (m_mass1/m_totalMass)*m_initialMomentum2;
            int sx = 2*m_gridSizeX - 1;  // The range of r = r1-r2 is twice the range of r1, r2
            int sy = 2*m_gridSizeY - 1;
            int sz = 2*m_gridSizeZ - 1;

            WaveFunction initialWf = WaveFunctionUtils.CreateGaussianWavePacket(
                new GridSpec(sx,sy,sz), m_latticeSpacing, true, m_reducedMass, r0, m_sigmaRel, p0, m_multiThread
            );

            m_visscherWf = new VisscherWf(initialWf, V, m_reducedMass, m_deltaT, m_multiThread);
            initialWf = null; // Allow initialWf to be garbage collected
            TimeStepCompleted();
            if (IsCancelled) { return; }


            // Main loop: Evolve the relative wavefunction
            while (m_currentTimeStepIndex < m_totalNumTimeSteps)
            {
                // Evolve the wavefunction by one timestep
                EvolveByOneTimeStep(m_visscherWf, V);
                m_currentTimeStepIndex++;

                // Report progress to the client
                TimeStepCompleted();
                if (IsCancelled) { return; }
            }
        }


        /// <summary>
        /// This method is invoked after each time step.
        /// </summary>
        private void TimeStepCompleted()
        {
            // If the current frame is a keyframe, then save it.
            double frameInterval = (m_numFramesToSave <= 1) ? 1 : m_totalTime/(m_numFramesToSave-1);
            if ( m_currentTimeStepIndex == (int)Math.Round( (m_lastSavedFrame+1)*frameInterval/m_deltaT ) )
            {
                if (m_lastSavedFrame + 1 < m_numFramesToSave)
                {
                    string outFile = Path.Combine(m_outputDir, "Frame_" + (m_lastSavedFrame + 1).ToString("D4") + ".vtk");
                    SaveOutputFile(outFile);
                }
                m_lastSavedFrame++;
            }

            ReportProgress();
        }


        /// <summary>
        /// Computes the potential at all grid locations. 
        /// </summary>
        private float[][][] PrecomputeV()
        {
            int sx  = 2*m_gridSizeX - 1; // The range of r = r1-r2 is twice the range of r1, r2
            int sy  = 2*m_gridSizeY - 1;
            int sz  = 2*m_gridSizeZ - 1;
            int hsx = m_gridSizeX - 1;
            int hsy = m_gridSizeY - 1;
            int hsz = m_gridSizeZ - 1;

            float a = m_latticeSpacing;
            float domainSizeX = sx * a;
            float domainSizeY = sy * a;
            float domainSizeZ = sz * a;

            float[][][] V = TdseUtils.Misc.Allocate3DArray(sz, sy, sx);

            TdseUtils.Misc.ForLoop(0, sz, z =>
            {
                for (int y=0; y<sy; y++)
                {
                    float[] Vzy = V[z][y];
                    for (int x=0; x<sx; x++)
                    {
                        Vzy[x] = m_potential((x-hsx)*a, (y-hsy)*a, (z-hsz)*a, m_reducedMass, domainSizeX, domainSizeY, domainSizeZ);
                    }
                }
            }, m_multiThread);

            return V;
        }



        /// <summary>
        /// Evolves the wavefunction by a single time step
        /// </summary>
        private void EvolveByOneTimeStep(VisscherWf wf, float[][][] V)
        {
            GridSpec wfGrid = wf.GridSpec;
            int sx = wfGrid.SizeX;
            int sy = wfGrid.SizeY;
            int sz = wfGrid.SizeZ;
            int sxm1 = sx - 1;
            int sym1 = sy - 1;
            int szm1 = sz - 1;

            float keFactor = (1.0f/12.0f) / (2 * m_reducedMass * wf.LatticeSpacing * wf.LatticeSpacing);


            // Compute the next real part in terms of the current imaginary part
            TdseUtils.Misc.ForLoop(0, sz, z =>
            {
                int zp  = (z  < szm1) ?  z + 1 : 0;
                int zpp = (zp < szm1) ? zp + 1 : 0;
                int zm  = (z  > 0) ?  z - 1 : szm1;
                int zmm = (zm > 0) ? zm - 1 : szm1;

                for (int y = 0; y < sy; y++)
                {
                    int yp  = (y  < sym1) ?  y + 1 : 0;
                    int ypp = (yp < sym1) ? yp + 1 : 0;
                    int ym  = (y  > 0) ?  y - 1 : sym1;
                    int ymm = (ym > 0) ? ym - 1 : sym1;

                    float[] V_z_y      = V[z][y];
                    float[] wfR_z_y    =  wf.RealPart[z][y];
                    float[] wfI_z_y    =  wf.ImagPartP[z][y];
                    float[] wfI_z_ym   =  wf.ImagPartP[z][ym];
                    float[] wfI_z_yp   =  wf.ImagPartP[z][yp];
                    float[] wfI_z_ymm  =  wf.ImagPartP[z][ymm];
                    float[] wfI_z_ypp  =  wf.ImagPartP[z][ypp];
                    float[] wfI_zm_y   =  wf.ImagPartP[zm][y];
                    float[] wfI_zp_y   =  wf.ImagPartP[zp][y];
                    float[] wfI_zmm_y  =  wf.ImagPartP[zmm][y];
                    float[] wfI_zpp_y  =  wf.ImagPartP[zpp][y];

                    for (int x = 0; x < sx; x++)
                    {
                        int xp  = (x  < sxm1) ?  x + 1 : 0;
                        int xpp = (xp < sxm1) ? xp + 1 : 0;
                        int xm  = (x  > 0) ?  x - 1 : sxm1;
                        int xmm = (xm > 0) ? xm - 1 : sxm1;

                        // Discretization of the 2nd derivative
                        float ke = keFactor * (
                            90.0f * wfI_z_y[x] -
                            16.0f * (wfI_zm_y[x] + wfI_zp_y[x] + wfI_z_yp[x] + wfI_z_ym[x] + wfI_z_y[xm] + wfI_z_y[xp]) +
                            (wfI_zmm_y[x] + wfI_zpp_y[x] + wfI_z_ypp[x] + wfI_z_ymm[x] + wfI_z_y[xmm] + wfI_z_y[xpp]) 
                        );

                        float pe = V_z_y[x] * wfI_z_y[x];

                        wfR_z_y[x] += m_deltaT * (ke + pe);
                    }
                }
            }, m_multiThread);


            // Swap prev and post imaginary parts
            float[][][] temp = wf.ImagPartM;
            wf.ImagPartM = wf.ImagPartP;
            wf.ImagPartP = temp;


            // Compute the next imaginary part in terms of the current real part
            TdseUtils.Misc.ForLoop(0, sz, z =>
            {
                int zp  = (z  < szm1) ?  z + 1 : 0;
                int zpp = (zp < szm1) ? zp + 1 : 0;
                int zm  = (z  > 0) ?  z - 1 : szm1;
                int zmm = (zm > 0) ? zm - 1 : szm1;

                for (int y = 0; y < sy; y++)
                {
                    int yp  = (y  < sym1) ?  y + 1 : 0;
                    int ypp = (yp < sym1) ? yp + 1 : 0;
                    int ym  = (y  > 0) ?  y - 1 : sym1;
                    int ymm = (ym > 0) ? ym - 1 : sym1;

                    float[] V_z_y      =  V[z][y];
                    float[] wfIM_z_y   =  wf.ImagPartM[z][y];
                    float[] wfIP_z_y   =  wf.ImagPartP[z][y];
                    float[] wfR_z_y    =  wf.RealPart[z][y];
                    float[] wfR_z_ym   =  wf.RealPart[z][ym];
                    float[] wfR_z_yp   =  wf.RealPart[z][yp];
                    float[] wfR_z_ymm  =  wf.RealPart[z][ymm];
                    float[] wfR_z_ypp  =  wf.RealPart[z][ypp];
                    float[] wfR_zm_y   =  wf.RealPart[zm][y];
                    float[] wfR_zp_y   =  wf.RealPart[zp][y];
                    float[] wfR_zmm_y  =  wf.RealPart[zmm][y];
                    float[] wfR_zpp_y  =  wf.RealPart[zpp][y];

                    for (int x = 0; x < sx; x++)
                    {
                        int xp  = (x  < sxm1) ?  x + 1 : 0;
                        int xpp = (xp < sxm1) ? xp + 1 : 0;
                        int xm  = (x  > 0) ?  x - 1 : sxm1;
                        int xmm = (xm > 0) ? xm - 1 : sxm1;

                        // Discretization of the 2nd derivative
                        float ke = keFactor * (
                            90.0f * wfR_z_y[x] -
                            16.0f * (wfR_zm_y[x] + wfR_zp_y[x] + wfR_z_yp[x] + wfR_z_ym[x] + wfR_z_y[xm] + wfR_z_y[xp]) +
                            (wfR_zmm_y[x] + wfR_zpp_y[x] + wfR_z_ypp[x] + wfR_z_ymm[x] + wfR_z_y[xmm] + wfR_z_y[xpp]) 
                        );

                        float pe = V_z_y[x] * wfR_z_y[x];

                        wfIP_z_y[x] = wfIM_z_y[x] - m_deltaT * (ke + pe);
                    }
                }
            }, m_multiThread);


            // Optionally perform damping to suppress reflection and transmission at the borders. 
            if ( (m_dampingBorderWidth > 0) && (m_dampingFactor > 0.0f) )
            {
                ApplyDamping(wf);
            }

        }

        /// <summary>
        /// Damps the wavefunction apmlitude near the region boundary.
        /// </summary>
        private void ApplyDamping(VisscherWf wf)
        {
            int sx = wf.GridSpec.SizeX;
            int sy = wf.GridSpec.SizeY;
            int sz = wf.GridSpec.SizeZ;

            int d = m_dampingBorderWidth;
            float[] factors = new float[d];
            for (int i=0; i<d; i++)
            {
                factors[i] = (float) ( 1.0 - m_dampingFactor*m_deltaT*(1.0 - Math.Sin( ((Math.PI/2)*i)/d )) ); 
            }

            // Front Z border
            for (int z=0; z<d; z++)
            {
                float f = factors[z];

                for (int y = 0; y < sy; y++)
                {
                    float[] wfDataIPzy = wf.ImagPartP[z][y];
                    float[] wfDataIMzy = wf.ImagPartM[z][y];
                    float[] wfDataRzy  = wf.RealPart[z][y];

                    for (int x = 0; x < sx; x++)
                    {
                        wfDataRzy[x]  *= f;
                        wfDataIPzy[x] *= f;
                        wfDataIMzy[x] *= f;
                    }
                }
            }

            // Back Z border
            for (int z=sz-d; z<sz; z++)
            {
                float f = factors[sz-1-z];

                for (int y = 0; y < sy; y++)
                {
                    float[] wfDataIPzy = wf.ImagPartP[z][y];
                    float[] wfDataIMzy = wf.ImagPartM[z][y];
                    float[] wfDataRzy  = wf.RealPart[z][y];

                    for (int x = 0; x < sx; x++)
                    {
                        wfDataRzy[x]  *= f;
                        wfDataIPzy[x] *= f;
                        wfDataIMzy[x] *= f;
                    }
                }
            }

            // Y borders
            for (int z=0; z<sz; z++)
            {
                for (int y=0; y<d; y++)
                {
                    float f = factors[y];
                    float[] wfDataIPzy = wf.ImagPartP[z][y];
                    float[] wfDataIMzy = wf.ImagPartM[z][y];
                    float[] wfDataRzy  = wf.RealPart[z][y];

                    for (int x = 0; x < sx; x++)
                    {
                        wfDataRzy[x]  *= f;
                        wfDataIPzy[x] *= f;
                        wfDataIMzy[x] *= f;
                    }
                }
                for (int y=sy-d; y<sy; y++)
                {
                    float f = factors[sy-1-y];
                    float[] wfDataIPzy = wf.ImagPartP[z][y];
                    float[] wfDataIMzy = wf.ImagPartM[z][y];
                    float[] wfDataRzy  = wf.RealPart[z][y];

                    for (int x = 0; x < sx; x++)
                    {
                        wfDataRzy[x]  *= f;
                        wfDataIPzy[x] *= f;
                        wfDataIMzy[x] *= f;
                    }
                }
            }


            // X borders
            for (int z=0; z<sz; z++)
            {
                for (int y=0; y<sy; y++)
                {
                    float[] wfDataIPzy = wf.ImagPartP[z][y];
                    float[] wfDataIMzy = wf.ImagPartM[z][y];
                    float[] wfDataRzy  = wf.RealPart[z][y];

                    for (int x = 0; x < d; x++)
                    {
                        float f = factors[x];
                        wfDataRzy[x]  *= f;
                        wfDataIPzy[x] *= f;
                        wfDataIMzy[x] *= f;
                    }

                    for (int x = sx-d; x < sx; x++)
                    {
                        float f = factors[sx-1-x];
                        wfDataRzy[x]  *= f;
                        wfDataIPzy[x] *= f;
                        wfDataIMzy[x] *= f;
                    }
                }
            }
        }
        

        /// <summary>
        /// Saves the current probability densities to a vtk file.
        /// </summary>
        private void SaveOutputFile(string fileSpec)
        {            
            float time = m_currentTimeStepIndex * m_deltaT;

            ProbabilityDensity prob1 = GetSingleParticleProbability(1, time);  
            if (prob1 == null) { return; }
            System.Diagnostics.Trace.WriteLine(prob1.Norm().ToString());

            ProbabilityDensity prob2 = GetSingleParticleProbability(2, time);  
            if (prob2 == null) { return; }
            System.Diagnostics.Trace.WriteLine("          " + prob2.Norm().ToString());

            ProbabilityDensity.SaveToVtkFile(new ProbabilityDensity[]{prob1,prob2}, fileSpec);
        }


        /// <summary>
        /// Computes a single particle probability density by integrating over one of the particle coordinates.
        /// </summary>
        private unsafe ProbabilityDensity GetSingleParticleProbability(int particleIndex, double time)
        {
            int psx = m_gridSizeX;
            int psy = m_gridSizeY;
            int psz = m_gridSizeZ;

            // Precompute some constants we'll need
            double fm1             = m_mass1/m_totalMass;
            double fm2             = m_mass2/m_totalMass;
            double sigmaCmFactorX  = Math.Pow(m_sigmaCm.X, 4) + (time*time)/(m_totalMass*m_totalMass);
            double sigmaCmFactorY  = Math.Pow(m_sigmaCm.Y, 4) + (time*time)/(m_totalMass*m_totalMass);
            double sigmaCmFactorZ  = Math.Pow(m_sigmaCm.Z, 4) + (time*time)/(m_totalMass*m_totalMass);
            double RnormX          = m_sigmaCm.X / Math.Sqrt( Math.PI * sigmaCmFactorX );
            double RnormY          = m_sigmaCm.Y / Math.Sqrt( Math.PI * sigmaCmFactorY );
            double RnormZ          = m_sigmaCm.Z / Math.Sqrt( Math.PI * sigmaCmFactorZ );
            Vec3   R0              = fm1*m_initialPosition1 + fm2*m_initialPosition2;
            Vec3   P0              = (m_initialMomentum1 + m_initialMomentum2);
            double RxOffset        = R0.X + time*(P0.X/m_totalMass);
            double RyOffset        = R0.Y + time*(P0.Y/m_totalMass);
            double RzOffset        = R0.Z + time*(P0.Z/m_totalMass);
            double RxScale         = -(m_sigmaCm.X*m_sigmaCm.X)/sigmaCmFactorX;
            double RyScale         = -(m_sigmaCm.Y*m_sigmaCm.Y)/sigmaCmFactorY;
            double RzScale         = -(m_sigmaCm.Z*m_sigmaCm.Z)/sigmaCmFactorZ;


            // Precompute the relative wavefunction probabilities
            ProbabilityDensity relDensity = m_visscherWf.ToRegularWavefunction().GetProbabilityDensity();

            // Get a one-particle probability by marginalizing over the joint probability
            float[][][] oneParticleProbs = TdseUtils.Misc.Allocate3DArray(psz, psy, psx);

            if (particleIndex == 1)
            {
                for (int z1 = 0; z1 < psz; z1++)
                {
                    // Precompute the center-of-mass wavefunction probabilities
                    float[] ZExp = new float[psz];
                    for (int z2 = 0; z2 < psz; z2++)
                    {
                        double RzArg = (fm1*z1 + fm2*z2)*m_latticeSpacing - RzOffset;
                        ZExp[z2] = (float)(RnormZ * Math.Exp(RzScale*RzArg*RzArg));
                    }

                    TdseUtils.Misc.ForLoop(0, psy, y1 =>
                    {
                        // Precompute the center-of-mass wavefunction probabilities
                        float[] YExp = new float[psy];
                        for (int y2 = 0; y2 < psy; y2++)
                        {
                            double RyArg = (fm1*y1 + fm2*y2)*m_latticeSpacing - RyOffset;
                            YExp[y2] = (float)(RnormY * Math.Exp(RyScale*RyArg*RyArg));
                        }

                        for (int x1 = 0; x1 < psx; x1++)
                        {
                            float[] XExp = new float[psx];
                            for (int x2 = 0; x2 < psx; x2++)
                            {
                                double RxArg = (fm1*x1 + fm2*x2)*m_latticeSpacing - RxOffset;
                                XExp[x2] = (float)(RnormX * Math.Exp(RxScale*RxArg*RxArg));
                            }
                            fixed (float *pXExp = XExp)
                            {
                                float prob = 0.0f;
                                for (int z2 = 0; z2 < psz; z2++)
                                {
                                    int xOffset = x1 + psx - 1;
                                    float[][] relProbsZ = relDensity.Data[(z1 - z2) + psz - 1];

                                    for (int y2 = 0; y2 < psy; y2++)
                                    {
                                        float yzExpFactor = YExp[y2] * ZExp[z2];
                                        fixed ( float* pRelProbsZY = &(relProbsZ[(y1 - y2) + psy - 1][xOffset]) )
                                        {
                                            float sum = 0.0f;
                                            for (int x2 = 0; x2 < psx; x2++)
                                            {
                                                sum += pXExp[x2] * pRelProbsZY[-x2];
                                            }
                                            prob += sum * yzExpFactor;
                                        }
                                    }
                                }
                                oneParticleProbs[z1][y1][x1] = prob * (m_latticeSpacing * m_latticeSpacing * m_latticeSpacing);
                            }
                        }

                    }, m_multiThread );

                    CheckForPause();
                    if (IsCancelled) { return null; }
                }
            }
            else
            {
                for (int z2 = 0; z2 < psz; z2++)
                {
                    // Precompute the center-of-mass wavefunction probabilities
                    float[] ZExp = new float[psz];
                    for (int z1 = 0; z1 < psz; z1++)
                    {
                        double RzArg = (fm1*z1 + fm2*z2)*m_latticeSpacing - RzOffset;
                        ZExp[z1] = (float)(RnormZ * Math.Exp(RzScale*RzArg*RzArg));
                    }

                    TdseUtils.Misc.ForLoop(0, psy, y2 =>
                    {
                        // Precompute the center-of-mass wavefunction probabilities
                        float[] YExp = new float[psy];
                        for (int y1 = 0; y1 < psy; y1++)
                        {
                            double RyArg = (fm1*y1 + fm2*y2)*m_latticeSpacing - RyOffset;
                            YExp[y1] = (float)(RnormY * Math.Exp(RyScale*RyArg*RyArg));
                        }

                        for (int x2 = 0; x2 < psx; x2++)
                        {
                            float[] XExp = new float[psx];
                            for (int x1 = 0; x1 < psx; x1++)
                            {
                                double RxArg = (fm1*x1 + fm2*x2)*m_latticeSpacing - RxOffset;
                                XExp[x1] = (float)(RnormX * Math.Exp(RxScale*RxArg*RxArg));
                            }
                            fixed (float *pXExp = XExp)
                            {
                                float prob = 0.0f;
                                for (int z1 = 0; z1 < psz; z1++)
                                {
                                    float[][] relProbsZ = relDensity.Data[(z1 - z2) + psz - 1];

                                    int xOffset = -x2 + psx - 1;
                                    for (int y1 = 0; y1 < psy; y1++)
                                    {
                                        float yzExpFactor = YExp[y1] * ZExp[z1];
                                        fixed ( float* pRelProbsZY = &(relProbsZ[(y1 - y2) + psy - 1][xOffset]) )
                                        {
                                            float sum = 0.0f;
                                            for (int x1 = 0; x1 < psx; x1++)
                                            {
                                                sum += pXExp[x1] * pRelProbsZY[x1];
                                            }
                                            prob += sum * yzExpFactor;
                                        }
                                    }
                                }
                                oneParticleProbs[z2][y2][x2] = prob * (m_latticeSpacing * m_latticeSpacing * m_latticeSpacing);
                            }
                        }

                    }, m_multiThread );

                    CheckForPause();
                    if (IsCancelled) { return null; }
                }
            }

            return new ProbabilityDensity(oneParticleProbs, m_visscherWf.LatticeSpacing);
        }
    
    }
}
