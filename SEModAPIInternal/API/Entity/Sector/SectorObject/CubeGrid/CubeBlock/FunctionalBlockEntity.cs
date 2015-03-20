using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using SEModAPIInternal.API.Common;
using SEModAPIInternal.Support;

namespace SEModAPIInternal.API.Entity.Sector.SectorObject.CubeGrid.CubeBlock
{
	[DataContract( Name = "FunctionalBlockEntityProxy" )]
	public class FunctionalBlockEntity : TerminalBlockEntity
	{
		#region "Attributes"

		private PowerReceiver m_powerReceiver;
		private bool m_enabled;

		public static string FunctionalBlockNamespace = "";
		public static string FunctionalBlockClass = "Sandbox.Game.Entities.Cube.MyFunctionalBlock";

		//public static string FunctionalBlockGetEnabledMethod = "89B34B01DCC6C8596E80023078BB9541";
		public static string FunctionalBlockGetEnabledMethod = "get_Enabled";

		public static string FunctionalBlockSetEnabledMethod = "set_Enabled";

		//public static string FunctionalBlockBroadcastEnabledMethod = "D979DB9AA474782929587EC7DE5E53AA";
		public static string FunctionalBlockBroadcastEnabledMethod = "RequestEnable";

		public static string FunctionalBlockGetPowerReceiverMethod = "get_PowerReceiver";
		public static string FunctionalBlockCheckIsWorkingMethod = "CheckIsWorking";

		#endregion "Attributes"

		#region "Constructors and Initializers"

		public FunctionalBlockEntity( CubeGridEntity parent, MyObjectBuilder_FunctionalBlock definition )
			: base( parent, definition )
		{
			m_enabled = definition.Enabled;
		}

		public FunctionalBlockEntity( CubeGridEntity parent, MyObjectBuilder_FunctionalBlock definition, Object backingObject )
			: base( parent, definition, backingObject )
		{
			m_enabled = definition.Enabled;

			m_powerReceiver = new PowerReceiver( ActualObject, Parent.PowerManager, InternalGetPowerReceiver( ), new Func<float>( InternalPowerReceiverCallback ) );
		}

		#endregion "Constructors and Initializers"

		#region "Properties"

		[IgnoreDataMember]
		[Category( "Functional Block" )]
		[Browsable( false )]
		[ReadOnly( true )]
		internal new MyObjectBuilder_FunctionalBlock ObjectBuilder
		{
			get
			{
				try
				{
					if ( m_objectBuilder == null )
						m_objectBuilder = new MyObjectBuilder_FunctionalBlock( );

					return (MyObjectBuilder_FunctionalBlock)base.ObjectBuilder;
				}
				catch ( Exception )
				{
					return new MyObjectBuilder_FunctionalBlock( );
				}
			}
			set
			{
				base.ObjectBuilder = value;
			}
		}

		[DataMember]
		[Category( "Functional Block" )]
		public bool Enabled
		{
			get
			{
				if ( BackingObject == null || ActualObject == null )
					return ObjectBuilder.Enabled;

				return GetFunctionalBlockEnabled( );
			}
			set
			{
				if ( Enabled == value ) return;
				ObjectBuilder.Enabled = value;
				m_enabled = value;
				Changed = true;

				if ( BackingObject != null && ActualObject != null )
				{
					Action action = InternalUpdateFunctionalBlock;
					SandboxGameAssemblyWrapper.Instance.EnqueueMainGameAction( action );
				}
			}
		}

		[DataMember]
		[Category( "Functional Block" )]
		[ReadOnly( true )]
		public float CurrentInput
		{
			get
			{
				return PowerReceiver.CurrentInput;
			}
			private set
			{
				//Do nothing!
			}
		}

		[IgnoreDataMember]
		[Category( "Functional Block" )]
		[Browsable( false )]
		[ReadOnly( true )]
		internal PowerReceiver PowerReceiver
		{
			get { return m_powerReceiver; }
		}

		#endregion "Properties"

		#region "Methods"

		new public static bool ReflectionUnitTest( )
		{
			try
			{
				bool result = true;

				Type type = SandboxGameAssemblyWrapper.Instance.GetAssemblyType( FunctionalBlockNamespace, FunctionalBlockClass );
				if ( type == null )
					throw new Exception( "Could not find internal type for FunctionalBlockEntity" );

				result &= HasMethod( type, FunctionalBlockGetEnabledMethod );
				result &= HasMethod( type, FunctionalBlockSetEnabledMethod );
				result &= HasMethod( type, FunctionalBlockBroadcastEnabledMethod );
				result &= HasMethod( type, FunctionalBlockCheckIsWorkingMethod );
				//result &= HasMethod(type, FunctionalBlockGetPowerReceiverMethod);

				return result;
			}
			catch ( Exception ex )
			{
				LogManager.APILog.WriteLine( ex );
				return false;
			}
		}

		protected bool GetFunctionalBlockEnabled( )
		{
			Object rawResult = InvokeEntityMethod( ActualObject, FunctionalBlockGetEnabledMethod );
			if ( rawResult == null )
				return false;
			bool result = (bool)rawResult;
			return result;
		}

		protected void InternalUpdateFunctionalBlock( )
		{
			InvokeEntityMethod( ActualObject, FunctionalBlockSetEnabledMethod, new object[ ] { m_enabled } );
			InvokeEntityMethod( ActualObject, FunctionalBlockBroadcastEnabledMethod, new object[ ] { m_enabled } );
		}

		public static void SetState( IMyEntity entity, bool enabled )
		{
			if ( !( entity is Sandbox.ModAPI.Ingame.IMyTerminalBlock ) )
				return;

			SandboxGameAssemblyWrapper.Instance.GameAction( ( ) =>
			{
				InvokeEntityMethod( entity, FunctionalBlockSetEnabledMethod, new object[ ] { enabled } );
				InvokeEntityMethod( entity, FunctionalBlockBroadcastEnabledMethod, new object[ ] { enabled } );
			} );
		}

		public static bool GetState( IMyEntity entity )
		{
			if ( !( entity is Sandbox.ModAPI.Ingame.IMyTerminalBlock ) )
				return false;

			Object rawResult = InvokeEntityMethod( entity, FunctionalBlockGetEnabledMethod );
			if ( rawResult == null )
				return false;
			bool result = (bool)rawResult;
			return result;
		}

		protected virtual float InternalPowerReceiverCallback( )
		{
			return 0;
		}

		protected virtual Object InternalGetPowerReceiver( )
		{
			bool oldDebuggingSetting = SandboxGameAssemblyWrapper.IsDebugging;
			SandboxGameAssemblyWrapper.IsDebugging = false;
			bool hasPowerReceiver = HasMethod( ActualObject.GetType( ), FunctionalBlockGetPowerReceiverMethod );
			SandboxGameAssemblyWrapper.IsDebugging = oldDebuggingSetting;
			if ( !hasPowerReceiver )
				return null;

			return InvokeEntityMethod( ActualObject, FunctionalBlockGetPowerReceiverMethod );
		}

		public bool CheckIsWorking( )
		{
			object rawResult = InvokeEntityMethod( ActualObject, FunctionalBlockCheckIsWorkingMethod );
			if ( rawResult == null )
				return false;
			return (bool)rawResult;
		}

		#endregion "Methods"
	}
}