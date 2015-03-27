﻿namespace SEModAPI.API.Definitions
{
	using Sandbox.Common.ObjectBuilders;
	using Sandbox.Common.ObjectBuilders.Definitions;
	using VRage.Utils;

	public class ContainerTypesDefinition : OverLayerDefinition<MyObjectBuilder_ContainerTypeDefinition>
    {
        #region "Attributes"

        private readonly ContainerTypeItemsManager _itemsManager;

        #endregion

        #region "Constructors and Initializers"

		public ContainerTypesDefinition(MyObjectBuilder_ContainerTypeDefinition definition)
			: base(definition)
        {
			_itemsManager = new ContainerTypeItemsManager();
			if(definition.Items != null)
				_itemsManager.Load(definition.Items);
		}

        #endregion

        #region "Properties"

        new public bool Changed
        {
            get
            {
				if (base.Changed) return true;
				foreach (ContainerTypeItem def in _itemsManager.Definitions)
                {
                    if (def.Changed)
                        return true;
                }

                return false;
            }
            private set
            {
                base.Changed = value;
            }
        }

		new public MyObjectBuilder_ContainerTypeDefinition BaseDefinition
		{
			get
			{
				m_baseDefinition.Items = _itemsManager.ExtractBaseDefinitions().ToArray();
				return m_baseDefinition;
			}
		}

		public MyObjectBuilderType TypeId
        {
            get { return m_baseDefinition.TypeId; }
        }

        public MyStringId SubtypeId
        {
            get { return m_baseDefinition.SubtypeId; }
        }

        public int CountMin
        {
            get { return m_baseDefinition.CountMin; }
            set
            {
                if (m_baseDefinition.CountMin == value) return;
                m_baseDefinition.CountMin = value;
                Changed = true;
            }
        }

        public int CountMax
        {
            get { return m_baseDefinition.CountMax; }
            set
            {
                if (m_baseDefinition.CountMax == value) return;
                m_baseDefinition.CountMax = value;
                Changed = true;
            }
        }

        public ContainerTypeItem[] Items
        {
            get { return _itemsManager.Definitions; }
        }

        #endregion

        #region "Methods"

        protected override string GetNameFrom(MyObjectBuilder_ContainerTypeDefinition definition)
        {
            return "Container Type";
        }

		public ContainerTypeItem NewEntry()
		{
			return _itemsManager.NewEntry();
		}

		public bool DeleteEntry(ContainerTypeItem source)
		{
			return _itemsManager.DeleteEntry(source);
		}

        #endregion
    }

    public class ContainerTypeItem : OverLayerDefinition<MyObjectBuilder_ContainerTypeDefinition.ContainerTypeItem>
    {
        #region "Constructors and Initializers"

        public ContainerTypeItem(MyObjectBuilder_ContainerTypeDefinition.ContainerTypeItem definition)
            : base(definition)
        {
        }

        #endregion

        #region "Properties"

        public SerializableDefinitionId Id
        {
            get { return m_baseDefinition.Id; }
            set
            {
                if (m_baseDefinition.Id.Equals(value)) return;
                m_baseDefinition.Id = value;
                Changed = true;
            }
        }

        public string AmountMin
        {
            get { return m_baseDefinition.AmountMin; }
            set
            {
                if (m_baseDefinition.AmountMin == value) return;
                m_baseDefinition.AmountMin = value;
                Changed = true;
            }
        }

		public string AmountMax
        {
            get { return m_baseDefinition.AmountMax; }
            set
            {
                if (m_baseDefinition.AmountMax == value) return;
                m_baseDefinition.AmountMax = value;
                Changed = true;
            }
        }

        public float Frequency
        {
            get { return m_baseDefinition.Frequency; }
            set
            {
                if (m_baseDefinition.Frequency == value) return;
                m_baseDefinition.Frequency = value;
                Changed = true;
            }
        }

        #endregion

        #region "Methods"

        protected override string GetNameFrom(MyObjectBuilder_ContainerTypeDefinition.ContainerTypeItem definition)
        {
            return definition.Id.ToString();
        }

        #endregion
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////

	public class ContainerTypesDefinitionsManager : SerializableDefinitionsManager<MyObjectBuilder_ContainerTypeDefinition, ContainerTypesDefinition>
	{
		#region "Methods"

		protected override MyObjectBuilder_ContainerTypeDefinition GetBaseTypeOf(ContainerTypesDefinition overLayer)
		{
			return overLayer.BaseDefinition;
		}

		#endregion
	}

	public class ContainerTypeItemsManager : SerializableDefinitionsManager<MyObjectBuilder_ContainerTypeDefinition.ContainerTypeItem, ContainerTypeItem>
    {
    }
}
