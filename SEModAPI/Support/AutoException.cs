namespace SEModAPI.Support
{
	using System;
	using System.Runtime.Serialization;
	using System.Security.Permissions;

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// - Class to easily handle exceptions
    /// - The derived class must instantiate their exception
    ///   state representation as string for their exception state enum
	/// - See GameInstallationExceptionInfo for example
    /// </summary>
    [Serializable]
    public class AutoException : Exception
    {
        #region "Attributes"

        //Members
        protected string m_AdditionnalInfo;
        protected int m_ExceptionState;       

        //Must be redefined
        protected string[] StateRepresentation;

		#endregion

		#region "Properties"

		/// <summary>
		/// returns the value of the current state to string
		/// </summary>
		/// <returns></returns>
        public string StateToString() { return StateRepresentation[m_ExceptionState]; }

		/// <summary>
		/// Get the value representing the state of the exception
		/// </summary>
		public int ExceptionStateId { get { return m_ExceptionState; } }

        /// <summary>
        /// Get the CustomMessage
        /// </summary>
        public string AdditionnalInfo { get { return m_AdditionnalInfo; } }

        /// <summary>
        /// Get the Object context as string
        /// </summary>
        public string Object 
        {
            get
            {
                if (TargetSite.ReflectedType != null) return TargetSite.ReflectedType.FullName;
                return null;
            }
        }

        /// <summary>
        /// Get the Method context as string
        /// </summary>
        public string Method { get { return TargetSite.Name; } }

        #endregion

        #region "Constructors & Initializers"

        /// <summary>
        /// Class to handle exceptions to ease debbuging by handling their context
        /// </summary>
        /// <param name="exceptionState">An instance of a derived class of IExceptionState</param>
        /// <param name="additionnalInfo">Give additionnal information about the exception</param>
		protected AutoException(Enum exceptionState, string additionnalInfo = "")
        {
			m_ExceptionState = (int)Convert.ChangeType(exceptionState, typeof(int));
            m_AdditionnalInfo = additionnalInfo;
        }

        /// <summary>
        /// The serialization constructor of AutoException
        /// </summary>
        protected AutoException(SerializationInfo info, StreamingContext context): base(info, context){}

        #endregion

        #region "Methods"

        /// <summary>
        /// Method intended to return a standardized and simplified information about exceptions
        /// of the API to ease the debugging
        /// </summary>
        /// <returns>Simplified</returns>
        public string GetDebugString() { return Object + "->" + Method + "# " + StateToString() + "; " + m_AdditionnalInfo; }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

	        if (info == null)
	        {
				throw new ArgumentNullException("info"); 
	        }
            info.AddValue("Object", Object);
            info.AddValue("Method", Method);
            info.AddValue("AdditionnalInfo", m_AdditionnalInfo);
            info.AddValue("ExceptionState", StateToString());
        }

        #endregion

    }
}
