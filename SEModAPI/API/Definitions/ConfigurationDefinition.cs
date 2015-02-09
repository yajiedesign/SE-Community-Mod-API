﻿using System;
using Sandbox.Common.ObjectBuilders.Definitions;

namespace SEModAPI.API.Definitions
{
	public class ConfigurationDefinition
	{
		#region "Attributes"

		private MyObjectBuilder_Configuration m_baseDefinition;

		#endregion

		#region "Constructors and Initializers"


		#endregion

		#region "Properties"

		public bool Changed
		{
			get;
			private set;
		}

		public float LargeCubeSize
		{
			get { return m_baseDefinition.CubeSizes.Large; }
			set
			{
				if (value < 0) throw new ArgumentOutOfRangeException();
				if (m_baseDefinition.CubeSizes.Large == value) return;
				m_baseDefinition.CubeSizes.Large = value;
				Changed = true;
			}
		}

		/*
		public float MediumCubeSize
		{
			get { return m_baseDefinition.CubeSizes.Medium; }
			set
			{
				if (value < 0) throw new ArgumentOutOfRangeException();
				if (m_baseDefinition.CubeSizes.Medium == value) return;
				m_baseDefinition.CubeSizes.Medium = value;
				Changed = true;
			}
		}
		*/

		public float SmallCubeSize
		{
			get { return m_baseDefinition.CubeSizes.Small; }
			set
			{
				if (value < 0) throw new ArgumentOutOfRangeException();
				if (m_baseDefinition.CubeSizes.Small == value) return;
				m_baseDefinition.CubeSizes.Small = value;
				Changed = true;
			}
		}

		public string SmallDynamic
		{
			get { return m_baseDefinition.BaseBlockPrefabs.SmallDynamic; }
			set
			{
				if (m_baseDefinition.BaseBlockPrefabs.SmallDynamic == value) return;
				m_baseDefinition.BaseBlockPrefabs.SmallDynamic = value;
				Changed = true;
			}
		}

		public string SmallStatic
		{
			get { return m_baseDefinition.BaseBlockPrefabs.SmallStatic; }
			set
			{
				if (m_baseDefinition.BaseBlockPrefabs.SmallStatic == value) return;
				m_baseDefinition.BaseBlockPrefabs.SmallStatic = value;
				Changed = true;
			}
		}

		/*
		public string MediumDynamic
		{
			get { return m_baseDefinition.BaseBlockPrefabs.MediumDynamic; }
			set
			{
				if (m_baseDefinition.BaseBlockPrefabs.MediumDynamic == value) return;
				m_baseDefinition.BaseBlockPrefabs.MediumDynamic = value;
				Changed = true;
			}
		}
		*/

		/*
		public string MediumStatic
		{
			get { return m_baseDefinition.BaseBlockPrefabs.MediumStatic; }
			set
			{
				if (m_baseDefinition.BaseBlockPrefabs.MediumStatic == value) return;
				m_baseDefinition.BaseBlockPrefabs.MediumStatic = value;
				Changed = true;
			}
		}
		*/

		public string LargeDynamic
		{
			get { return m_baseDefinition.BaseBlockPrefabs.LargeDynamic; }
			set
			{
				if (m_baseDefinition.BaseBlockPrefabs.LargeDynamic == value) return;
				m_baseDefinition.BaseBlockPrefabs.LargeDynamic = value;
				Changed = true;
			}
		}

		public string LargeStatic
		{
			get { return m_baseDefinition.BaseBlockPrefabs.LargeStatic; }
			set
			{
				if (m_baseDefinition.BaseBlockPrefabs.LargeStatic == value) return;
				m_baseDefinition.BaseBlockPrefabs.LargeStatic = value;
				Changed = true;
			}
		}

		#endregion

		#region "Methods"

		#endregion
	}
}
