using System;
using SEModAPIInternal.API.Common;
using SEModAPIInternal.API.Entity.Sector.SectorObject.CubeGrid;
using SEModAPIInternal.API.Entity.Sector.SectorObject.CubeGrid.CubeBlock;
using SEModAPIInternal.Support;

namespace SEModAPIInternal.API.Entity.Sector.SectorObject
{
	public class CubeGridThrusterManager
	{
		#region "Attributes"

		private Object m_thrusterManager;
		private CubeGridEntity m_parent;

		private bool m_dampenersEnabled;

		public static string CubeGridThrusterManagerNamespace = "";
		public static string CubeGridThrusterManagerClass = "=RQ6GT5VPdcFWeja6eus3dlb5mI=";

		public static string CubeGridThrusterManagerGetEnabled = "get_DampenersEnabled";
		public static string CubeGridThrusterManagerSetEnabled = "set_DampenersEnabled";
		public static string CubeGridThrusterManagerSetControlEnabled = "set_Enabled";

		#endregion "Attributes"

		#region "Constructors and Initializers"

		public CubeGridThrusterManager( Object thrusterManager, CubeGridEntity parent )
		{
			m_thrusterManager = thrusterManager;
			m_parent = parent;
		}

		#endregion "Constructors and Initializers"

		#region "Properties"

		public Object BackingObject
		{
			get { return m_thrusterManager; }
		}

		public bool DampenersEnabled
		{
			get { return InternalGetDampenersEnabled( ); }
			set
			{
				m_dampenersEnabled = value;

				Action action = InternalUpdateDampenersEnabled;
				SandboxGameAssemblyWrapper.Instance.EnqueueMainGameAction( action );
			}
		}

		#endregion "Properties"

		#region "Methods"

		public static bool ReflectionUnitTest( )
		{
			try
			{
				bool result = true;
				Type type = SandboxGameAssemblyWrapper.Instance.GetAssemblyType( CubeGridThrusterManagerNamespace, CubeGridThrusterManagerClass );
				if ( type == null )
					throw new Exception( "Could not find type for CubeGridThrusterManager" );
				result &= BaseObject.HasMethod( type, CubeGridThrusterManagerGetEnabled );
				result &= BaseObject.HasMethod( type, CubeGridThrusterManagerSetEnabled );
				result &= BaseObject.HasMethod( type, CubeGridThrusterManagerSetControlEnabled );

				return result;
			}
			catch ( Exception ex )
			{
				LogManager.APILog.WriteLine( ex );
				return false;
			}
		}

		protected bool InternalGetDampenersEnabled( )
		{
			bool result = (bool)BaseObject.InvokeEntityMethod( BackingObject, CubeGridThrusterManagerGetEnabled );
			return result;
		}

		protected void InternalUpdateDampenersEnabled( )
		{
			foreach ( CubeBlockEntity cubeBlock in m_parent.CubeBlocks )
			{
				if ( cubeBlock is CockpitEntity )
				{
					CockpitEntity cockpit = (CockpitEntity)cubeBlock;
					if ( cockpit.IsPassengerSeat )
						continue;

					cockpit.NetworkManager.BroadcastDampenersStatus( m_dampenersEnabled );
					break;
				}
			}

			BaseObject.InvokeEntityMethod( BackingObject, CubeGridThrusterManagerSetEnabled, new object[ ] { m_dampenersEnabled } );
			//BaseObject.InvokeEntityMethod(BackingObject, CubeGridThrusterManagerSetControlEnabled, new object[] { m_dampenersEnabled });
		}

		#endregion "Methods"
	}
}