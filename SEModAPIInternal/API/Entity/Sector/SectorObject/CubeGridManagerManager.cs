namespace SEModAPIInternal.API.Entity.Sector.SectorObject
{
	using System;
	using SEModAPIInternal.API.Common;
	using SEModAPIInternal.Support;

	public class CubeGridManagerManager
	{
		#region "Attributes"

		private CubeGridEntity m_parent;
		private Object m_backingObject;

		private PowerManager m_powerManager;
		private CubeGridThrusterManager m_thrusterManager;

		public static string CubeGridManagerManagerNamespace = "";
		public static string CubeGridManagerManagerClass = "Sandbox.Game.Entities.Cube.MyCubeGridSystems";

		public static string CubeGridManagerManagerGetPowerManagerMethod = "get_PowerDistributor";
		public static string CubeGridManagerManagerGetThrusterManagerMethod = "get_ThrustSystem";

		#endregion "Attributes"

		#region "Constructors and Initializers"

		public CubeGridManagerManager( CubeGridEntity parent, Object backingObject )
		{
			m_parent = parent;
			m_backingObject = backingObject;

			m_powerManager = new PowerManager( GetPowerManager( ) );
			m_thrusterManager = new CubeGridThrusterManager( GetThrusterManager( ), m_parent );
		}

		#endregion "Constructors and Initializers"

		#region "Properties"

		public static Type InternalType
		{
			get
			{
				Type type = SandboxGameAssemblyWrapper.Instance.GetAssemblyType( CubeGridManagerManagerNamespace, CubeGridManagerManagerClass );
				return type;
			}
		}

		public Object BackingObject
		{
			get { return m_backingObject; }
		}

		public PowerManager PowerManager
		{
			get { return m_powerManager; }
		}

		public CubeGridThrusterManager ThrusterManager
		{
			get { return m_thrusterManager; }
		}

		#endregion "Properties"

		#region "Methods"

		public static bool ReflectionUnitTest( )
		{
			try
			{
				Type type = InternalType;
				if ( type == null )
					throw new Exception( "Could not find internal type for CubeGridManagerManager" );
				bool result = true;
				result &= BaseObject.HasMethod( type, CubeGridManagerManagerGetPowerManagerMethod );
				result &= BaseObject.HasMethod( type, CubeGridManagerManagerGetThrusterManagerMethod );

				return result;
			}
			catch ( Exception ex )
			{
				LogManager.APILog.WriteLine( ex );
				return false;
			}
		}

		private Object GetPowerManager( )
		{
			Object manager = BaseObject.InvokeEntityMethod( BackingObject, CubeGridManagerManagerGetPowerManagerMethod );
			return manager;
		}

		private Object GetThrusterManager( )
		{
			Object manager = BaseObject.InvokeEntityMethod( BackingObject, CubeGridManagerManagerGetThrusterManagerMethod );
			return manager;
		}

		#endregion "Methods"
	}
}