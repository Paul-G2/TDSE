using System;
using System.ComponentModel; 
using System.Threading;
using System.Threading.Tasks;


namespace TdseUtils
{
    /// <summary>
    /// This class is responsible for running a given process in a background thread.
    /// It provides progress and completion callbacks, and supports pausing, resuming, and stopping.
    /// </summary>
    public abstract class Proc
    {
        #region Class data
        Task                    m_workerTask;
        object                  m_lock;
        bool                    m_busy;
        bool                    m_cancelled;
        ManualResetEvent        m_pauseSemaphore;
        SynchronizationContext  m_callbackContext;
        #endregion Class data


        #region Events
        public delegate void Progress_Delegate(Proc sender);
        public event         Progress_Delegate ProgressEvent;
        public delegate void Completion_Delegate(Proc sender, RunWorkerCompletedEventArgs e);
        public event         Completion_Delegate CompletionEvent;
        #endregion Events


        /// <summary>
        /// Constructor.
        /// </summary>
        public Proc()
        {
            // Initialize data members
            m_workerTask = null;
            m_lock = new object();
            m_busy = false;
            m_cancelled = false;
            m_pauseSemaphore = new ManualResetEvent(true);
            m_callbackContext = SynchronizationContext.Current; // Will be the UI context if this ctor is invoked on the UI thread, otherwise null.
        }


        /// <summary>
        /// The worker method, to be provided by the concrete subclass.
        /// 
        /// It should divide the computation into chunks, and call ReportProgress() after each chunk is completed.
        /// Immediately after each such progress report, it should check the IsCancelled property, and 
        /// return if IsCancelled is true.
        /// </summary>
        protected abstract void WorkerMethod();
        

        /// <summary>
        /// Runs the worker method in the calling thread.
        /// </summary>
        public void Run()
        {
            if ( IsBusy )
            {
                throw new InvalidOperationException("Can't run a Proc that is already running.");
            }
            else
            {
                IsBusy = true;
                m_cancelled = false;
                
                WorkerMethod();

                IsBusy = false;
                if (CompletionEvent != null)
                {
                    RunWorkerCompletedEventArgs e = new RunWorkerCompletedEventArgs(null, null, IsCancelled);
                    CompletionEvent(this, e);
                }
            }
        }


        /// <summary>
        /// Runs the worker method in a background thread.
        /// </summary>
        public void RunInBackground()
        {
            if ( IsBusy )
            {
                throw new InvalidOperationException("Can't run a Proc that is already running.");
            }
            else
            {
                IsBusy = true;
                m_cancelled = false;

                m_workerTask = new Task(this.WorkerMethod);
                m_workerTask.ContinueWith(WorkerCompletionHandler, TaskContinuationOptions.OnlyOnRanToCompletion);
                m_workerTask.ContinueWith(WorkerExceptionHandler, TaskContinuationOptions.OnlyOnFaulted);
                m_workerTask.Start();
            }
        }


        /// <summary>
        /// This method runs when the background task completes successfully.
        /// </summary>
        void WorkerCompletionHandler(Task task)
        {
            // (This method runs on the background thread.)
            IsBusy = false;

            Completion_Delegate completionEvent = CompletionEvent; // Take a copy in case CompletionEvent is changed by another thread
            if (completionEvent != null)
            {
                RunWorkerCompletedEventArgs e = new RunWorkerCompletedEventArgs(null, null, IsCancelled);

                if (m_callbackContext == null)
                {
                    completionEvent(this, e);
                }
                else
                {
                    // Invoke the completion handler on the UI thread
                    m_callbackContext.Send(new SendOrPostCallback(delegate
                    {
                        completionEvent(this, e);
                    }), null);
                }
            }         
        }
        

        /// <summary>
        /// This method runs when the background task throws an unhandled exception.
        /// </summary>        
        void WorkerExceptionHandler(Task task)
        {
            // (This method runs on the background thread.)
            IsBusy = false;

            Completion_Delegate completionEvent = CompletionEvent; // Take a copy in case CompletionEvent is changed by another thread
            if (completionEvent != null)
            {
                RunWorkerCompletedEventArgs e = new RunWorkerCompletedEventArgs(null, task.Exception.Flatten(), false);

                if (m_callbackContext == null)
                {
                    completionEvent(this, e);
                }
                else
                {
                    // Invoke the completion handler on the UI thread
                    m_callbackContext.Send(new SendOrPostCallback(delegate
                    {
                        completionEvent(this, e);
                    }), null);
                }
            }         
        }


        /// <summary>
        /// This method is meant to be invoked by derived classes when progress has been made.
        /// </summary>
        protected void ReportProgress()
        {
            // (This method may run on a background thread.)

            // Pause, if we've been requested to do so.
            if ( !IsCancelled )
            {
                m_pauseSemaphore.WaitOne(); // Blocks until the pause semaphore is set.
            }            
            
            // Report progress to observers
            Progress_Delegate progressEvent = ProgressEvent; // Take a copy in case ProgressEvent is changed by another thread
            if (progressEvent != null)
            {
                if (m_callbackContext == null)
                {
                    progressEvent( this );
                }
                else
                {
                    // Invoke the progress handler on the UI thread (synchronously)
                    m_callbackContext.Send(new SendOrPostCallback(delegate
                    {
                        progressEvent( this );
                    }), null);
                }
            }
        }


        /// <summary>
        /// Gets or sets a flag indicating whether the process is running.
        /// (A paused process is considered to be running.)
        /// </summary>
        public bool IsBusy
        {
            get
            {
                lock (m_lock) 
                { 
                    return m_busy; 
                }
            }

            private set
            {
                lock (m_lock) 
                { 
                    m_busy = value; 
                }
            }
        }


        /// <summary>
        /// Gets or sets a flag indicating whether the process was cancelled.
        /// </summary>
        public bool IsCancelled
        {
            get
            {
                lock (m_lock) 
                { 
                    return m_cancelled; 
                }
            }
        }


        /// <summary>
        /// Stops background processing.
        /// </summary>
        public void Cancel()
        {
            if ( IsBusy )
            {
                lock (m_lock) 
                { 
                    m_cancelled = true; 
                }
            
                if ( IsPaused )
                {
                    Resume();
                }
            }
        }


        /// <summary>
        /// Pauses background processing.
        /// </summary>
        public void Pause()
        {
            if ( IsBusy )
            {
                m_pauseSemaphore.Reset();
            }
        }
        
        
        /// <summary>
        /// Resumes  background processing.
        /// </summary>
        public void Resume()
        {
            if ( IsBusy )
            {
                m_pauseSemaphore.Set();
            }
        }
       

        /// <summary>
        /// Indicates whether the Proc is currently paused 
        /// </summary>
        public bool IsPaused
        {
            get 
            { 
                return !m_pauseSemaphore.WaitOne(0); // Doesn't block; just immediately returns the state of m_pauseSemaphore
            }
        }


    }
}
