using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Vec2 = TdseUtils.Vec2;
using WaveFunction2D1P = TdseSolver_2D1P.WaveFunction;


namespace TdseSolver_2D2P_NS
{
    /// <summary>
    /// This class computes the time evolution of a wavefunction.
    /// </summary>
    class Evolver : TdseUtils.Proc
    {
        // Declare a delegate for computing the potential energy
        public delegate float VDelegate(float rx, float ry, float sx, float sy);


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
        private VDelegate      m_potential             = null;

        private Vec2           m_initialMomentum1      = Vec2.Zero;
        private Vec2           m_initialPosition1      = Vec2.Zero;
        private Vec2           m_initialSize1          = Vec2.Zero;
        private Vec2           m_initialPosition2      = Vec2.Zero;
        private float          m_sho_sigma             = 1.0f;
        private int            m_sho_N                 = 0;
        private int            m_sho_Lz                = 0;

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
            m_potential            = V;
            
            m_initialMomentum1     = new Vec2(parms.InitialWavePacketMomentum1);
            m_initialPosition1     = new Vec2(parms.InitialWavePacketCenter1);
            m_initialSize1         = new Vec2(parms.InitialWavePacketSize);

            m_initialPosition2     = new Vec2(parms.AtomCenter);
            m_sho_sigma            = parms.AtomSize;
            m_sho_N                = parms.Atom_N;
            m_sho_Lz               = parms.Atom_Lz;
            
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
        /// Worker method.
        /// </summary>
        protected override void WorkerMethod()
        {
            // Reset counters
            m_currentTimeStepIndex = 0;
            m_lastSavedFrame = -1;


            // Create a single-particle wf representing the incoming wavepacket
            WaveFunction2D1P initialWf1 = TdseSolver_2D1P.WaveFunctionUtils.CreateGaussianWavePacket(m_gridSizeX, m_gridSizeY, m_latticeSpacing, 
                m_mass1, m_initialPosition1.ToPointF(), m_initialSize1.ToPointF(), m_initialMomentum1.ToPointF(), m_multiThread
            );

            // Create a single-particle wf representing the stationary bound state
            WaveFunction2D1P initialWf2 = WaveFunctionUtils.GetSHOWaveFunction(m_gridSizeX, m_gridSizeY, m_latticeSpacing, 
                m_mass2, m_initialPosition2, m_sho_sigma, m_sho_N, m_sho_Lz, m_multiThread );

            // Precompute the potentials everywhere on the grid
            float[][] V1 = TdseUtils.Misc.Allocate2DArray(m_gridSizeY, m_gridSizeX);
            float[][] V2 = PrecomputeV2();

            // Create a VisscherWf from the direct product of the 1-particle wfs
            m_visscherWf = new VisscherWf(initialWf1, initialWf2, V1, V2, m_mass1, m_mass2, m_deltaT, m_multiThread);
            
            TimeStepCompleted();
            if (IsCancelled) { return; }

            float[][] Vrel = PrecomputeVrel();

            // Main loop: Evolve the relative wavefunction
            while (m_currentTimeStepIndex < m_totalNumTimeSteps)
            {
                // Evolve the wavefunction by one timestep
                EvolveByOneTimeStep(m_visscherWf, Vrel, V2);
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
        /// Computes the binding potential felt by particle 2.  
        /// (Currently hard-coded to be a SHO potential.)
        /// </summary>
        private float[][] PrecomputeV2()
        {
            float[][] V = TdseUtils.Misc.Allocate2DArray(m_gridSizeY, m_gridSizeX);

            float Vcoeff = (float) ( 1.0/(2*m_mass2*Math.Pow(m_sho_sigma,4)) );

            for (int y = 0; y < m_gridSizeY; y++)
            {
                float[] Vy = V[y];
                float dy = Math.Min(5*m_sho_sigma, Math.Abs(y*m_latticeSpacing - m_initialPosition2.Y) ); // Numerical instability if we let this get too large

                for (int x = 0; x < m_gridSizeX; x++)
                {
                    float dx = Math.Min( 5*m_sho_sigma, Math.Abs(x*m_latticeSpacing - m_initialPosition2.X) );
                    Vy[x] = Vcoeff * (dx*dx + dy*dy);
                }
            }

            return V;
        }



        /// <summary>
        /// Computes the relative potential at all grid locations. 
        /// </summary>
        private float[][] PrecomputeVrel()
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
                    Vy[x] = m_potential((x-hsx)*a, (y-hsy)*a, domainSizeX, domainSizeY);
                }
            }, m_multiThread);

            return V;
        }
        /// <summary>
        /// Evolves the wavefunction by a single time step
        /// </summary>
        private void EvolveByOneTimeStep(VisscherWf wf, float[][] Vrel, float[][] V2)
        {
            // Initialize locals
            int sx    = m_gridSizeX;
            int sy    = m_gridSizeY;
            int sxm1  = sx - 1;
            int sym1  = sy - 1;

            float alpha = 5.0f;
            float beta  = -4.0f / 3.0f;
            float delta = 1.0f / 12.0f;

            float keFactor1 = 1.0f / (2 * m_mass1 *m_latticeSpacing * m_latticeSpacing);
            float keFactor2 = 1.0f / (2 * m_mass2 *m_latticeSpacing * m_latticeSpacing);
    

            // Compute the next real part in terms of the current imaginary part
            TdseUtils.Misc.ForLoop(0, sy, (y2) =>
            {
                int y2p  = (y2  < sym1) ?  y2 + 1 : 0;
                int y2pp = (y2p < sym1) ? y2p + 1 : 0;
                int y2m  = (y2  > 0) ?  y2 - 1 : sym1;
                int y2mm = (y2m > 0) ? y2m - 1 : sym1;

                for (int x2 = 0; x2 < sx; x2++)
                {
                    int x2p  = (x2  < sxm1) ?  x2 + 1 : 0;
                    int x2pp = (x2p < sxm1) ? x2p + 1 : 0;
                    int x2m  = (x2  > 0) ?  x2 - 1 : sxm1;
                    int x2mm = (x2m > 0) ? x2m - 1 : sxm1;

                    for (int y1 = 0; y1 < sy; y1++)
                    {
                        int y1p  = (y1  < sym1) ?  y1 + 1 : 0;
                        int y1pp = (y1p < sym1) ? y1p + 1 : 0;
                        int y1m  = (y1  > 0) ?  y1 - 1 : sym1;
                        int y1mm = (y1m > 0) ? y1m - 1 : sym1;

                        float[] wfi_y2_x2_y1   = wf.ImagPartP[y2][x2][y1];
                        float[] wfi_y2_x2_y1m  = wf.ImagPartP[y2][x2][y1m];
                        float[] wfi_y2_x2_y1p  = wf.ImagPartP[y2][x2][y1p];
                        float[] wfi_y2_x2_y1mm = wf.ImagPartP[y2][x2][y1mm];
                        float[] wfi_y2_x2_y1pp = wf.ImagPartP[y2][x2][y1pp];

                        float[] wfi_y2_x2p_y1  = wf.ImagPartP[y2][x2p][y1];
                        float[] wfi_y2_x2m_y1  = wf.ImagPartP[y2][x2m][y1];
                        float[] wfi_y2_x2mm_y1 = wf.ImagPartP[y2][x2mm][y1];
                        float[] wfi_y2_x2pp_y1 = wf.ImagPartP[y2][x2pp][y1];

                        float[] wfi_y2m_x2_y1  = wf.ImagPartP[y2m][x2][y1];
                        float[] wfi_y2p_x2_y1  = wf.ImagPartP[y2p][x2][y1];
                        float[] wfi_y2mm_x2_y1 = wf.ImagPartP[y2mm][x2][y1];
                        float[] wfi_y2pp_x2_y1 = wf.ImagPartP[y2pp][x2][y1];
                        
                        float[] wfr_y2_x2_y1   = wf.RealPart[y2][x2][y1];

                        for (int x1 = 0; x1 < sx; x1++)
                        {
                            int x1p  = (x1  < sxm1) ?  x1 + 1 : 0;
                            int x1pp = (x1p < sxm1) ? x1p + 1 : 0;
                            int x1m  = (x1  > 0) ?  x1 - 1 : sxm1;
                            int x1mm = (x1m > 0) ? x1m - 1 : sxm1;

                            // Kinetic energy
                            float ke1 = keFactor1 * (
                                alpha * wfi_y2_x2_y1[x1] +
                                beta  * (wfi_y2_x2_y1[x1m] + wfi_y2_x2_y1[x1p] + wfi_y2_x2_y1m[x1] + wfi_y2_x2_y1p[x1]) +
                                delta * (wfi_y2_x2_y1[x1mm] + wfi_y2_x2_y1[x1pp] + wfi_y2_x2_y1mm[x1] + wfi_y2_x2_y1pp[x1])
                            );

                            float ke2 = keFactor2 * (
                                alpha * wfi_y2_x2_y1[x1] +
                                beta  * (wfi_y2_x2m_y1[x1]  + wfi_y2_x2p_y1[x1]  + wfi_y2m_x2_y1[x1]  + wfi_y2p_x2_y1[x1]) +
                                delta * (wfi_y2_x2mm_y1[x1] + wfi_y2_x2pp_y1[x1] + wfi_y2mm_x2_y1[x1] + wfi_y2pp_x2_y1[x1])
                            );

                            // Potential energy
                            float pe = (V2[y2][x2] + Vrel[y2-y1 + sym1][x2-x1 + sxm1]) * wfi_y2_x2_y1[x1];

                            wfr_y2_x2_y1[x1] += m_deltaT * (ke1 + ke2 + pe);
                        }
                    }
                }
            }, m_multiThread );
            
            


            // Swap prev and post imaginary parts
            float[][][][] temp = wf.ImagPartM;
            wf.ImagPartM = wf.ImagPartP;
            wf.ImagPartP = temp;


            // Compute the next imaginary part in terms of the current real part
            TdseUtils.Misc.ForLoop(0, sy, (y2) =>
            {
                int y2p  = (y2  < sym1) ?  y2 + 1 : 0;
                int y2pp = (y2p < sym1) ? y2p + 1 : 0;
                int y2m  = (y2  > 0) ?  y2 - 1 : sym1;
                int y2mm = (y2m > 0) ? y2m - 1 : sym1;

                for (int x2 = 0; x2 < sx; x2++)
                {
                    int x2p  = (x2  < sxm1) ?  x2 + 1 : 0;
                    int x2pp = (x2p < sxm1) ? x2p + 1 : 0;
                    int x2m  = (x2  > 0) ?  x2 - 1 : sxm1;
                    int x2mm = (x2m > 0) ? x2m - 1 : sxm1;

                    for (int y1 = 0; y1 < sy; y1++)
                    {
                        int y1p  = (y1  < sym1) ?  y1 + 1 : 0;
                        int y1pp = (y1p < sym1) ? y1p + 1 : 0;
                        int y1m  = (y1  > 0) ?  y1 - 1 : sym1;
                        int y1mm = (y1m > 0) ? y1m - 1 : sym1;

                        float[] wfr_y2_x2_y1   = wf.RealPart[y2][x2][y1];
                        float[] wfr_y2_x2_y1m  = wf.RealPart[y2][x2][y1m];
                        float[] wfr_y2_x2_y1p  = wf.RealPart[y2][x2][y1p];
                        float[] wfr_y2_x2_y1mm = wf.RealPart[y2][x2][y1mm];
                        float[] wfr_y2_x2_y1pp = wf.RealPart[y2][x2][y1pp];

                        float[] wfr_y2_x2p_y1  = wf.RealPart[y2][x2p][y1];
                        float[] wfr_y2_x2m_y1  = wf.RealPart[y2][x2m][y1];
                        float[] wfr_y2_x2mm_y1 = wf.RealPart[y2][x2mm][y1];
                        float[] wfr_y2_x2pp_y1 = wf.RealPart[y2][x2pp][y1];

                        float[] wfr_y2m_x2_y1  = wf.RealPart[y2m][x2][y1];
                        float[] wfr_y2p_x2_y1  = wf.RealPart[y2p][x2][y1];
                        float[] wfr_y2mm_x2_y1 = wf.RealPart[y2mm][x2][y1];
                        float[] wfr_y2pp_x2_y1 = wf.RealPart[y2pp][x2][y1];
                        

                        for (int x1 = 0; x1 < sx; x1++)
                        {
                            int x1p  = (x1  < sxm1) ?  x1 + 1 : 0;
                            int x1pp = (x1p < sxm1) ? x1p + 1 : 0;
                            int x1m  = (x1  > 0) ?  x1 - 1 : sxm1;
                            int x1mm = (x1m > 0) ? x1m - 1 : sxm1;

                            // Kinetic energy
                            float ke1 = keFactor1 * (
                                alpha * wfr_y2_x2_y1[x1] +
                                beta  * (wfr_y2_x2_y1[x1m] + wfr_y2_x2_y1[x1p] + wfr_y2_x2_y1m[x1] + wfr_y2_x2_y1p[x1]) +
                                delta * (wfr_y2_x2_y1[x1mm] + wfr_y2_x2_y1[x1pp] + wfr_y2_x2_y1mm[x1] + wfr_y2_x2_y1pp[x1])
                            );

                            float ke2 = keFactor2 * (
                                alpha * wfr_y2_x2_y1[x1] +
                                beta  * (wfr_y2_x2m_y1[x1]  + wfr_y2_x2p_y1[x1]  + wfr_y2m_x2_y1[x1]  + wfr_y2p_x2_y1[x1]) +
                                delta * (wfr_y2_x2mm_y1[x1] + wfr_y2_x2pp_y1[x1] + wfr_y2mm_x2_y1[x1] + wfr_y2pp_x2_y1[x1])
                            );

                            // Potential energy
                            float pe = (V2[y2][x2] + Vrel[y2-y1 + sym1][x2-x1 + sxm1]) * wfr_y2_x2_y1[x1];

                            wf.ImagPartP[y2][x2][y1][x1] = wf.ImagPartM[y2][x2][y1][x1]  -  m_deltaT * (ke1 + ke2 + pe);
                        }
                    }
                }
            }, m_multiThread );

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
            int sx = m_gridSizeX;
            int sy = m_gridSizeY;


            // Get a one-particle probability by marginalizing over the joint probability
            float[][] oneParticleProbs = TdseUtils.Misc.Allocate2DArray(sy, sx);

            if (particleIndex == 1)
            {
                for (int y1 = 0; y1 < sy; y1++)
                {
                    TdseUtils.Misc.ForLoop(0, sx, x1 =>
                    { 
                        float prob = 0.0f;
                        for (int y2 = 0; y2 < sy; y2++)
                        {
                            float[][][] Re_y2 = m_visscherWf.RealPart[y2]; 
                            float[][][] ImP_y2 = m_visscherWf.ImagPartP[y2]; 
                            float[][][] ImM_y2 = m_visscherWf.ImagPartM[y2]; 

                            for (int x2 = 0; x2 < sx; x2++)
                            {
                                float re  = Re_y2[x2][y1][x1];
                                float imP = ImP_y2[x2][y1][x1];
                                float imM = ImM_y2[x2][y1][x1];
                                prob += re*re + imP*imM;
                            }
                        }
                        oneParticleProbs[y1][x1] = prob * (m_latticeSpacing * m_latticeSpacing);
                    }, m_multiThread);

                    CheckForPause();
                    if (IsCancelled) { return null; }
                }
            }
            else
            {
                for (int y2 = 0; y2 < sy; y2++)
                {
                    TdseUtils.Misc.ForLoop(0, sx, x2 =>
                    {                        
                        float prob = 0.0f;
                        for (int y1 = 0; y1 < sy; y1++)
                        {
                            float[] Re_y2_x2_y1  = m_visscherWf.RealPart[y2][x2][y1]; 
                            float[] ImP_y2_x2_y1 = m_visscherWf.ImagPartP[y2][x2][y1]; 
                            float[] ImM_y2_x2_y1 = m_visscherWf.ImagPartM[y2][x2][y1]; 

                            for (int x1 = 0; x1 < sx; x1++)
                            {
                                float re = Re_y2_x2_y1[x1];
                                prob += re*re + ImP_y2_x2_y1[x1]*ImM_y2_x2_y1[x1];
                            }
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
