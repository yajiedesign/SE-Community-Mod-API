using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Sandbox.Common.ObjectBuilders;
using SEModAPIInternal.Support;

namespace SEModAPIInternal.API.Entity.Sector.SectorObject.CubeGrid.CubeBlock
{
	[DataContract( Name = "CargoContainerEntityProxy" )]
	public class CargoContainerEntity : TerminalBlockEntity
	{
		#region "Attributes"

		private InventoryEntity m_Inventory;

		public static string CargoContainerNamespace = "";
		public static string CargoContainerClass = "=oYFCJscKDsNLb5ixFZYeII2thY=";

		public static string CargoContainerGetInventoryMethod = "GetInventory";

		#endregion "Attributes"

		#region "Constructors and Initializers"

		public CargoContainerEntity( CubeGridEntity parent, MyObjectBuilder_CargoContainer definition )
			: base( parent, definition )
		{
			m_Inventory = new InventoryEntity( definition.Inventory );
		}

		public CargoContainerEntity( CubeGridEntity parent, MyObjectBuilder_CargoContainer definition, Object backingObject )
			: base( parent, definition, backingObject )
		{
			m_Inventory = new InventoryEntity( definition.Inventory, InternalGetContainerInventory( ) );
		}

		#endregion "Constructors and Initializers"

		#region "Properties"

		[IgnoreDataMember]
		[Category( "Cargo Container" )]
		[Browsable( false )]
		[ReadOnly( true )]
		internal new MyObjectBuilder_CargoContainer ObjectBuilder
		{
			get
			{
				MyObjectBuilder_CargoContainer objectBuilder = (MyObjectBuilder_CargoContainer)base.ObjectBuilder;
				if ( objectBuilder == null )
				{
					objectBuilder = new MyObjectBuilder_CargoContainer( );
					ObjectBuilder = objectBuilder;
				}

				return objectBuilder;
			}
			set
			{
				base.ObjectBuilder = value;
			}
		}

		[DataMember]
		[Category( "Cargo Container" )]
		[Browsable( false )]
		public InventoryEntity Inventory
		{
			get
			{
				return m_Inventory;
			}
			private set
			{
				//Do nothing!
			}
		}

		[IgnoreDataMember]
		[Category( "Cargo Container" )]
		public long ItemCount
		{
			get
			{
				long count = 0;
				foreach ( MyObjectBuilder_InventoryItem item in ObjectBuilder.Inventory.Items )
				{
					count += item.Amount.RawValue;
				}
				return count;
			}
		}

		[IgnoreDataMember]
		[Category( "Cargo Container" )]
		public float ItemMass
		{
			get
			{
				float mass = 0;
				//foreach (var item in Inventory.Items)
				//{
				//	mass += item.Mass;
				//}
				return mass;
			}
		}

		[IgnoreDataMember]
		[Category( "Cargo Container" )]
		public float ItemVolume
		{
			get
			{
				float volume = 0;
				//foreach (var item in Inventory.Items)
				//{
				//	volume += item.Volume;
				//}
				return volume;
			}
		}

		#endregion "Properties"

		#region "Methods"

		#region "Internal"

		protected Object InternalGetContainerInventory( )
		{
			try
			{
				Object baseObject = BackingObject;
				Object actualObject = GetActualObject( );
				Object inventory = InvokeEntityMethod( actualObject, CargoContainerGetInventoryMethod, new object[ ] { 0 } );

				return inventory;
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