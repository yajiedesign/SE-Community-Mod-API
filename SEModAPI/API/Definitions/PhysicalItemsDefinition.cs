using Sandbox.Common.ObjectBuilders.Definitions;
using VRage.Common;

namespace SEModAPI.API.Definitions
{
	public class PhysicalItemsDefinition : ObjectOverLayerDefinition<MyObjectBuilder_PhysicalItemDefinition>
	{
		#region "Constructors and Initializers"

        public PhysicalItemsDefinition(MyObjectBuilder_PhysicalItemDefinition definition): base(definition)
		{}

		#endregion

        #region "Properties"

		public string DisplayName
		{
			get { return m_baseDefinition.DisplayName; }
			set
			{
				if (m_baseDefinition.DisplayName == value) return;
				m_baseDefinition.DisplayName = value;
				Changed = true;
			}
		}

        public VRageMath.Vector3 Size
        {
            get { return m_baseDefinition.Size; }
            set
            {
                if (m_baseDefinition.Size == value) return;
                m_baseDefinition.Size = value;
                Changed = true;
            }
        }

		public float Mass
		{
            get { return m_baseDefinition.Mass; }
			set
			{
                if (m_baseDefinition.Mass == value) return;
                m_baseDefinition.Mass = value;
				Changed = true;
			}
		}

		public float Volume
		{
            get { return m_baseDefinition.Volume.GetValueOrDefault(0); }
			set
			{
                if (m_baseDefinition.Volume == value) return;
                m_baseDefinition.Volume = value;
				Changed = true;
			}
		}

		public string Model
		{
            get { return m_baseDefinition.Model; }
			set
			{
                if (m_baseDefinition.Model == value) return;
                m_baseDefinition.Model = value;
				Changed = true;
			}
		}

		public string Icon
		{
            get { return m_baseDefinition.Icon; }
			set
			{
                if (m_baseDefinition.Icon == value) return;
                m_baseDefinition.Icon = value;
				Changed = true;
			}
		}

		/*
		public MyTexts IconSymbol
		{
		    get
            {
                    //return m_baseDefinition.IconSymbol.GetValueOrDefault(MyTexts.IconSymbol_Stone);
            } 
			set
			{
                //if (m_baseDefinition.IconSymbol == value) return;
                //m_baseDefinition.IconSymbol = value;
				//Changed = true;
			}
		}
		*/
		#endregion

        #region "Methods"

        protected override string GetNameFrom(MyObjectBuilder_PhysicalItemDefinition definition)
        {
            return definition.DisplayName;
        }
		#endregion
	}

	public class PhysicalItemDefinitionsManager : SerializableDefinitionsManager<MyObjectBuilder_PhysicalItemDefinition, PhysicalItemsDefinition>
	{}
}
