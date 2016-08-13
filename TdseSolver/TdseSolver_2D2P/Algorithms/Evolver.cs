using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Vec2 = TdseUtils.Vec2;


namespace TdseSolver_2D2P
{
    /// <summary>
    /// This class computes the time evolution of a wavefunction.
    /// </summary>
    class Evolver : TdseUtils.Proc
    {
        // Declare a delegate for computing the potential energy
        public delegate float VDelegate(float rx, float ry, float mass, float sx, float sy);


        // Class data
        private int            m_gridSizeX             = 0;
        private int            m_gridSizeY             = 0;
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

        private Vec2           m_initialMomentum1      = Vec2.Zero;
        private Vec2           m_initialMomentum2      = Vec2.Zero;
        private Vec2           m_initialPosition1      = Vec2.Zero;
        private Vec2           m_initialPosition2      = Vec2.Zero;
        private Vec2           m_sigmaRel              = Vec2.Zero;
        private Vec2           m_sigmaCm               = Vec2.Zero;

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
            m_gridSizeX            = parms.GridSize.Width;
            m_gridSizeY            = parms.GridSize.Height;
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
            
            m_initialMomentum1     = new Vec2(parms.InitialWavePacketMomentum1);
            m_initialMomentum2     = new Vec2(parms.InitialWavePacketMomentum2);
            m_initialPosition1     = new Vec2(parms.InitialWavePacketCenter1);
            m_initialPosition2     = new Vec2(parms.InitialWavePacketCenter2);
            m_sigmaRel             = parms.InitialWavePacketSize * Math.Sqrt(m_totalMass/m_mass2);
            m_sigmaCm              = parms.InitialWavePacketSize * Math.Sqrt(m_mass1/m_totalMass);
            
            m_dampingBorderWidth   = parms.DampingBorderWidth;
            m_dampingFactor        = parms.DampingFactor;

            m_numFramesToSave      = parms.NumFramesToSave;
            m_lastSavedFrame       = -1;
            m_outputDir            = outputDir;
            m_multiThread          = parms.MultiThread;
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
            float[][] V = PrecomputeV();

            // Create the initial relative wavefunction
            Vec2 r0 = m_initialPosition1 - m_initialPosition2;
            Vec2 p0 = (m_mass2/m_totalMass)*m_initialMomentum1 - (m_mass1/m_totalMass)*m_initialMomentum2;
            int sx = 2*m_gridSizeX - 1;  // The range of r = r1-r2 is twice the range of r1, r2
            int sy = 2*m_gridSizeY - 1;

            WaveFunction initialWf = WaveFunctionUtils.CreateGaussianWavePacket(
                sx, sy, m_latticeSpacing, true, m_reducedMass, r0, m_sigmaRel, p0, m_multiThread
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
        private float[][] PrecomputeV()
        {
            int sx  = 2*m_gridSizeX - 1; // The range of r = r1-r2 is twice the range of r1, r2
            int sy  = 2*m_gridSizeY - 1;
            int hsx = m_gridSizeX - 1;
            int hsy = m_gridSizeY - 1;

            float a = m_latticeSpacing;
            float domainSizeX = sx * a;
            float domainSizeY = sy * a;

            float[][] V = TdseUtils.Misc.Allocate2DArray(sy, sx);

            TdseUtils.Misc.ForLoop(0, sy, y =>
            {
                float[] Vy = V[y];
                for (int x = 0; x < sx; x++)
                {
                    Vy[x] = m_potential((x-hsx)*a, (y-hsy)*a, m_reducedMass, domainSizeX, domainSizeY);
                }
            }, m_multiThread);

            return V;
        }



        /// <summary>
        /// Evolves the wavefunction by a single time step
        /// </summary>
        private void EvolveByOneTimeStep(VisscherWf wf, float[][] V)
        {
            int sx = wf.GridSizeX;
            int sy = wf.GridSizeY;
            int sxm1 = sx - 1;
            int sym1 = sy - 1;
            float keFactor = 1.0f / (2 * m_reducedMass * wf.LatticeSpacing * wf.LatticeSpacing);

            float alpha = 5.0f;
            float beta  = -4.0f / 3.0f;
            float delta = 1.0f / 12.0f;

            // Compute the next real part in terms of the current imaginary part
            TdseUtils.Misc.ForLoop(0, sy, y =>
            {
                int yp  = (y  < sym1) ?  y + 1 : 0;
                int ypp = (yp < sym1) ? yp + 1 : 0;
                int ym  = (y  > 0) ?  y - 1 : sym1;
                int ymm = (ym > 0) ? ym - 1 : sym1;

                float[] V_y     = V[y];
                float[] wfR_y   = wf.RealPart[y];
                float[] wfI_y   = wf.ImagPartP[y];
                float[] wfI_ym  = wf.ImagPartP[ym];
                float[] wfI_yp  = wf.ImagPartP[yp];
                float[] wfI_ymm = wf.ImagPartP[ymm];
                float[] wfI_ypp = wf.ImagPartP[ypp];

                for (int x = 0; x < sx; x++)
                {
                    int xp  = (x  < sxm1) ?  x + 1 : 0;
                    int xpp = (xp < sxm1) ? xp + 1 : 0;
                    int xm  = (x  > 0) ?  x - 1 : sxm1;
                    int xmm = (xm > 0) ? xm - 1 : sxm1;

                    // A discretization of the 2nd derivative, whose error term is O(a^6)
                    float ke = keFactor * (
                        alpha * wfI_y[x] +
                        beta  * (wfI_y[xm] + wfI_y[xp] + wfI_ym[x] + wfI_yp[x]) +
                        delta * (wfI_y[xmm] + wfI_y[xpp] + wfI_ymm[x] + wfI_ypp[x])
                    );
                        
                    float pe = V_y[x] * wfI_y[x];

                    wfR_y[x] += m_deltaT * (ke + pe);
                }
            }, m_multiThread);


            // Swap prev and post imaginary parts
            float[][] temp = wf.ImagPartM;
            wf.ImagPartM = wf.ImagPartP;
            wf.ImagPartP = temp;


            // Compute the next imaginary part in terms of the current real part
            TdseUtils.Misc.ForLoop(0, sy, y =>
            {
                int yp  = (y  < sym1) ?  y + 1 : 0;
                int ypp = (yp < sym1) ? yp + 1 : 0;
                int ym  = (y  > 0) ?  y - 1 : sym1;
                int ymm = (ym > 0) ? ym - 1 : sym1;

                float[] V_y      =  V[y];
                float[] wfIM_y   =  wf.ImagPartM[y];
                float[] wfIP_y   =  wf.ImagPartP[y];
                float[] wfR_y    =  wf.RealPart[y];
                float[] wfR_ym   =  wf.RealPart[ym];
                float[] wfR_yp   =  wf.RealPart[yp];
                float[] wfR_ymm  =  wf.RealPart[ymm];
                float[] wfR_ypp  =  wf.RealPart[ypp];

                for (int x = 0; x < sx; x++)
                {
                    int xp  = (x  < sxm1) ?  x + 1 : 0;
                    int xpp = (xp < sxm1) ? xp + 1 : 0;
                    int xm  = (x  > 0) ?  x - 1 : sxm1;
                    int xmm = (xm > 0) ? xm - 1 : sxm1;

                    // A discretization of the 2nd derivative, whose error term is O(a^6)
                    float ke = keFactor * (
                        alpha * wfR_y[x] +
                        beta  * (wfR_y[xm] + wfR_y[xp] + wfR_ym[x] + wfR_yp[x]) +
                        delta * (wfR_y[xmm] + wfR_y[xpp] + wfR_ymm[x] + wfR_ypp[x])
                    );

                    float pe = V_y[x] * wfR_y[x];

                    wfIP_y[x] = wfIM_y[x] - m_deltaT * (ke + pe);
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
            int sx = wf.GridSizeX;
            int sy = wf.GridSizeY;
            int d = m_dampingBorderWidth;

            float[] factors = new float[d];
            for (int i=0; i<d; i++)
            {
                factors[i] = (float) ( 1.0 - m_dampingFactor*m_deltaT*(1.0 - Math.Sin( ((Math.PI/2)*i)/d )) ); 
            }

            // Top border
            for (int y=0; y<d; y++)
            {
                float[] wfDataIPy = wf.ImagPartP[y];
                float[] wfDataIMy = wf.ImagPartM[y];
                float[] wfDataRy  = wf.RealPart[y];
                float f = factors[y];
 
                for (int x=0; x<sx; x++)
                {
                    wfDataRy[x]  *= f;
                    wfDataIPy[x] *= f;
                    wfDataIMy[x] *= f;
                }
            }

            // Bottom border
            for (int y=sy-d; y<sy; y++)
            {
                float[] wfDataIPy = wf.ImagPartP[y];
                float[] wfDataIMy = wf.ImagPartM[y];
                float[] wfDataRy  = wf.RealPart[y];
                float f = factors[sy-1-y];

                for (int x=0; x<sx; x++)
                {
                    wfDataRy[x]  *= f;
                    wfDataIPy[x] *= f;
                    wfDataIMy[x] *= f;
                }
            }

            // Left and right borders
            for (int y=0; y<sy; y++)
            {
                float[] wfDataIPy = wf.ImagPartP[y];
                float[] wfDataIMy = wf.ImagPartM[y];
                float[] wfDataRy  = wf.RealPart[y];

                for (int x=0; x<d; x++)
                {
                    float f = factors[x];
                    wfDataRy[x]  *= f;
                    wfDataIPy[x] *= f;
                    wfDataIMy[x] *= f;
                }
                for (int x=sx-d; x<sx; x++)
                {
                    float f = factors[sx-1-x];
                    wfDataRy[x]  *= f;
                    wfDataIPy[x] *= f;
                    wfDataIMy[x] *= f;
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
        private ProbabilityDensity GetSingleParticleProbability(int particleIndex, double time)
        {
            int psx = m_gridSizeX;
            int psy = m_gridSizeY;

            // Precompute some constants we'll need
            double fm1             = m_mass1/m_totalMass;
            double fm2             = m_mass2/m_totalMass;
            double sigmaCmFactorX  = Math.Pow(m_sigmaCm.X, 4) + (time*time)/(m_totalMass*m_totalMass);
            double sigmaCmFactorY  = Math.Pow(m_sigmaCm.Y, 4) + (time*time)/(m_totalMass*m_totalMass);
            double RnormX          = m_sigmaCm.X / Math.Sqrt( Math.PI * sigmaCmFactorX );
            double RnormY          = m_sigmaCm.Y / Math.Sqrt( Math.PI * sigmaCmFactorY );
            Vec2   R0              = fm1*m_initialPosition1 + fm2*m_initialPosition2;
            Vec2   P0              = (m_initialMomentum1 + m_initialMomentum2);
            double RxOffset        = R0.X + time*(P0.X/m_totalMass);
            double RyOffset        = R0.Y + time*(P0.Y/m_totalMass);
            double RxScale         = -(m_sigmaCm.X*m_sigmaCm.X)/sigmaCmFactorX;
            double RyScale         = -(m_sigmaCm.Y*m_sigmaCm.Y)/sigmaCmFactorX;


            // Precompute the relative wavefunction probabilities
            ProbabilityDensity relDensity = m_visscherWf.ToRegularWavefunction().GetProbabilityDensity();


            // Get a one-particle probability by marginalizing over the joint probability
            float[][] oneParticleProbs = TdseUtils.Misc.Allocate2DArray(psy, psx);

            if (particleIndex == 1)
            {
                for (int y1 = 0; y1 < psy; y1++)
                {
                    // Precompute the center-of-mass wavefunction probabilities
                    float[] YExp = new float[psy];
                    for (int y2 = 0; y2 < psy; y2++)
                    {
                        double RyArg = (fm1*y1 + fm2*y2)*m_latticeSpacing - RyOffset;
                        YExp[y2] = (float) ( RnormY * Math.Exp(RyScale*RyArg*RyArg) );
                    }

                    TdseUtils.Misc.ForLoop(0, psx, x1 =>
                    { 
                        float[] XExp = new float[psx];
                        for (int x2 = 0; x2 < psx; x2++)
                        {
                            double RxArg = (fm1*x1 + fm2*x2)*m_latticeSpacing - RxOffset;
                            XExp[x2] = (float) ( RnormX * Math.Exp(RxScale*RxArg*RxArg) );
                        }

                        float prob = 0.0f;
                        for (int y2 = 0; y2 < psy; y2++)
                        {
                            float[] relProbsY = relDensity.Data[(y1 - y2) + psy - 1];

                            int xOffset = x1 + psx - 1;
                            float sum = 0.0f;
                            for (int x2 = 0; x2 < psx; x2++)
                            {
                                sum += XExp[x2] * relProbsY[xOffset - x2];
                            }
                            prob += sum * YExp[y2];
                        }
                        oneParticleProbs[y1][x1] = prob * (m_latticeSpacing * m_latticeSpacing);
                    }, m_multiThread);

                    CheckForPause();
                    if (IsCancelled) { return null; }
                }
            }
            else
            {
                for (int y2 = 0; y2 < psy; y2++)
                {
                    // Precompute the center-of-mass wavefunction probabilities
                    float[] YExp = new float[psy];
                    for (int y1 = 0; y1 < psy; y1++)
                    {
                        double RyArg = (fm1*y1 + fm2*y2)*m_latticeSpacing - RyOffset;
                        YExp[y1] = (float) ( RnormY * Math.Exp(RyScale*RyArg*RyArg) );
                    }

                    TdseUtils.Misc.ForLoop(0, psx, x2 =>
                    {                        
                        float[] XExp = new float[psx];
                        for (int x1 = 0; x1 < psx; x1++)
                        {
                            double RxArg = (fm1*x1 + fm2*x2)*m_latticeSpacing - RxOffset;
                            XExp[x1] = (float) ( RnormX * Math.Exp(RxScale*RxArg*RxArg) );
                        }

                        float prob = 0.0f;
                        for (int y1 = 0; y1 < psy; y1++)
                        {
                            float[] relProbsY = relDensity.Data[(y1 - y2) + psy - 1];

                            int xOffset = -x2 + psx - 1;
                            float sum = 0.0f;
                            for (int x1 = 0; x1 < psx; x1++)
                            {
                                sum += XExp[x1] * relProbsY[x1 + xOffset];
                            }
                            prob += sum * YExp[y1];
                        }
                        oneParticleProbs[y2][x2] = prob * (m_latticeSpacing * m_latticeSpacing);
                    }, m_multiThread);

                    CheckForPause();
                    if (IsCancelled) { return null; }
                }
            }

            return new ProbabilityDensity(oneParticleProbs, m_visscherWf.LatticeSpacing);
        }
    
    }
}
