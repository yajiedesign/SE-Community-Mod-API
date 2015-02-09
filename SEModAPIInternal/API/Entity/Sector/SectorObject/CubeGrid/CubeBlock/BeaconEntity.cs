﻿using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Sandbox.Common.ObjectBuilders;
using SEModAPIInternal.API.Common;
using SEModAPIInternal.Support;

namespace SEModAPIInternal.API.Entity.Sector.SectorObject.CubeGrid.CubeBlock
{
	[DataContract( Name = "BeaconEntityProxy" )]
	public class BeaconEntity : FunctionalBlockEntity
	{
		#region "Attributes"

		private RadioManager m_radioManager;

		public static string BeaconNamespace = "6DDCED906C852CFDABA0B56B84D0BD74";
		public static string BeaconClass = "BF2916D43CF7F023839AD13F50F31990";

		public static string BeaconGetRadioManagerMethod = "A80801D0C8F1D45AB0E81B934FB5EF90";

		#endregion "Attributes"

		#region "Constructors and Initializers"

		public BeaconEntity( CubeGridEntity parent, MyObjectBuilder_Beacon definition )
			: base( parent, definition )
		{
		}

		public BeaconEntity( CubeGridEntity parent, MyObjectBuilder_Beacon definition, Object backingObject )
			: base( parent, definition, backingObject )
		{
			Object internalRadioManager = InternalGetRadioManager( );
			m_radioManager = new RadioManager( internalRadioManager );
		}

		#endregion "Constructors and Initializers"

		#region "Properties"

		[IgnoreDataMember]
		[Category( "Beacon" )]
		[Browsable( false )]
		[ReadOnly( true )]
		internal new MyObjectBuilder_Beacon ObjectBuilder
		{
			get
			{
				MyObjectBuilder_Beacon beacon = (MyObjectBuilder_Beacon)base.ObjectBuilder;

				return beacon;
			}
			set
			{
				base.ObjectBuilder = value;
			}
		}

		[DataMember]
		[Category( "Beacon" )]
		public float BroadcastRadius
		{
			get
			{
				float result = ObjectBuilder.BroadcastRadius;

				if ( RadioManager != null )
					result = RadioManager.BroadcastRadius;

				return result;
			}
			set
			{
				if ( ObjectBuilder.BroadcastRadius == value ) return;
				ObjectBuilder.BroadcastRadius = value;
				Changed = true;

				if ( RadioManager != null )
					RadioManager.BroadcastRadius = value;
			}
		}

		[IgnoreDataMember]
		[Category( "Beacon" )]
		[Browsable( false )]
		[ReadOnly( true )]
		internal RadioManager RadioManager
		{
			get { return m_radioManager; }
		}

		#endregion "Properties"

		#region "Methods"

		new public static bool ReflectionUnitTest( )
		{
			try
			{
				bool result = true;

				Type type = SandboxGameAssemblyWrapper.Instance.GetAssemblyType( BeaconNamespace, BeaconClass );
				if ( type == null )
					throw new Exception( "Could not find internal type for BeaconEntity" );
				result &= HasMethod( type, BeaconGetRadioManagerMethod );

				return result;
			}
			catch ( Exception ex )
			{
				Console.WriteLine( ex );
				return false;
			}
		}

		#region "Internal"

		protected Object InternalGetRadioManager( )
		{
			try
			{
				Object result = InvokeEntityMethod( ActualObject, BeaconGetRadioManagerMethod );
				return result;
			}
			catch ( Exception ex )
			{
				LogManager.ErrorLog.WriteLine( ex );
				return null;
			}
		}

		#endregion "Internal"

		#endregion "Methods"
	}
}