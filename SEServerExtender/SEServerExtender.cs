namespace SEServerExtender
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Threading;
	using System.Windows.Forms;
	using Sandbox.Definitions;
	using SEModAPI.API;
	using SEModAPI.API.Definitions;
	using SEModAPI.Support;
	using SEModAPIExtensions.API;
	using SEModAPIExtensions.API.Plugin;
	using SEModAPIInternal;
	using SEModAPIInternal.API.Common;
	using SEModAPIInternal.API.Entity;
	using SEModAPIInternal.API.Entity.Sector.SectorObject;
	using SEModAPIInternal.API.Entity.Sector.SectorObject.CubeGrid;
	using SEModAPIInternal.API.Entity.Sector.SectorObject.CubeGrid.CubeBlock;
	using SEModAPIInternal.Support;
	using VRage.Utils;
	using VRageMath;
	using Timer = System.Windows.Forms.Timer;

	public sealed partial class SEServerExtender : Form
	{
		#region "Attributes"

		//General
		private static SEServerExtender m_instance;
		private readonly Server m_server;
		private List<BaseEntity> m_sectorEntities;
		private readonly List<CubeGridEntity> m_cubeGridEntities;
		private readonly List<CharacterEntity> m_characterEntities;
		private readonly List<VoxelMap> m_voxelMapEntities;
		private readonly List<FloatingObject> m_floatingObjectEntities;
		private readonly List<Meteor> m_meteorEntities;

		private int m_chatLineCount = 0;
		private int m_sortBy = 0;

		//Timers
		private Timer m_entityTreeRefreshTimer;
		private Timer m_chatViewRefreshTimer;
		private Timer m_factionRefreshTimer;
		private Timer m_pluginManagerRefreshTimer;
		private Timer m_statusCheckTimer;
		private Timer m_utilitiesCleanFloatingObjectsTimer;
		private Timer m_statisticsTimer;
		private Timer m_playersTimer;

		//Utilities Page
		private int m_floatingObjectsCount;

		#endregion

		#region "Constructors and Initializers"

		public SEServerExtender(Server server)
		{
			m_instance = this;
			m_server = server;
			m_sectorEntities = new List<BaseEntity>();
			m_cubeGridEntities = new List<CubeGridEntity>();
			m_characterEntities = new List<CharacterEntity>();
			m_voxelMapEntities = new List<VoxelMap>();
			m_floatingObjectEntities = new List<FloatingObject>();
			m_meteorEntities = new List<Meteor>();

			//Run init functionsS
			InitializeComponent();
			if (!SetupTimers())
				Close();
			if (!SetupControls())
				Close();
			if(!m_server.IsRunning)
				m_server.LoadServerConfig();
			UpdateControls();
			PG_Control_Server_Properties.SelectedObject = m_server.Config;

			//Update the title bar text with the assembly version
			Text = string.Format( "SEServerExtender {0}", Assembly.GetExecutingAssembly().GetName().Version );

			FormClosing += OnFormClosing;
		}

		private bool SetupTimers()
		{		
			m_entityTreeRefreshTimer = new Timer { Interval = 500 };
			m_entityTreeRefreshTimer.Tick += TreeViewRefresh;

			m_chatViewRefreshTimer = new Timer { Interval = 1000 };
			m_chatViewRefreshTimer.Tick += ChatViewRefresh;

			m_factionRefreshTimer = new Timer { Interval = 5000 };
			m_factionRefreshTimer.Tick += FactionRefresh;

			m_pluginManagerRefreshTimer = new Timer { Interval = 10000 };
			m_pluginManagerRefreshTimer.Tick += PluginManagerRefresh;

			m_statusCheckTimer = new Timer { Interval = 5000 };
			m_statusCheckTimer.Tick += StatusCheckRefresh;
			m_statusCheckTimer.Start();

			m_utilitiesCleanFloatingObjectsTimer = new Timer { Interval = 5000 };
			m_utilitiesCleanFloatingObjectsTimer.Tick += UtilitiesCleanFloatingObjects;

			m_statisticsTimer = new Timer { Interval = 1000 };
			m_statisticsTimer.Tick += StatisticsRefresh;

			/*
			m_playersTimer = new Timer { Interval = 2000 };
			m_playersTimer.Tick += PlayersRefresh;
			*/

			return true;
		}

		private bool SetupControls()
		{
			try
			{
				if (string.IsNullOrEmpty(m_server.CommandLineArgs.Path))
				{
					List<String> instanceList = SandboxGameAssemblyWrapper.Instance.GetCommonInstanceList();
					CMB_Control_CommonInstanceList.BeginUpdate();
					CMB_Control_CommonInstanceList.Items.AddRange(instanceList.ToArray());
					if (CMB_Control_CommonInstanceList.Items.Count > 0)
						CMB_Control_CommonInstanceList.SelectedIndex = 0;
					CMB_Control_CommonInstanceList.EndUpdate();
				}

				CB_Entity_Sort.SelectedIndex = 0;

				CMB_Control_AutosaveInterval.BeginUpdate();
				CMB_Control_AutosaveInterval.Items.Add(1);
				CMB_Control_AutosaveInterval.Items.Add(2);
				CMB_Control_AutosaveInterval.Items.Add(5);
				CMB_Control_AutosaveInterval.Items.Add(10);
				CMB_Control_AutosaveInterval.Items.Add(15);
				CMB_Control_AutosaveInterval.Items.Add(30);
				CMB_Control_AutosaveInterval.Items.Add(60);
				CMB_Control_AutosaveInterval.EndUpdate();
			}
			catch (AutoException)
			{
				return false;
			}

			return true;
		}

		private void OnFormClosing(object sender, EventArgs e)
		{
			m_entityTreeRefreshTimer.Stop();
			m_chatViewRefreshTimer.Stop();
			m_factionRefreshTimer.Stop();
			m_pluginManagerRefreshTimer.Stop();
			m_statusCheckTimer.Stop();
			m_utilitiesCleanFloatingObjectsTimer.Stop();
			m_statisticsTimer.Stop();
			m_playersTimer.Stop();
		}

		#endregion

		#region "Methods"

		#region "General"

		private void StatisticsRefresh(object sender, EventArgs e)
		{
			StringBuilder sb = new StringBuilder();
			Stats.Generic.WriteTo(sb);
			Stats.Network.WriteTo(sb);
			Stats.Timing.WriteTo(sb);					

			TB_Statistics.Text = sb.ToString();			
		}

		private void StatusCheckRefresh(object sender, EventArgs e)
		{
			UpdateControls();

			if (m_server.IsRunning)
			{
				if(!m_entityTreeRefreshTimer.Enabled)
					m_entityTreeRefreshTimer.Start();
				if (!m_chatViewRefreshTimer.Enabled)
					m_chatViewRefreshTimer.Start();
				if (!m_factionRefreshTimer.Enabled)
					m_factionRefreshTimer.Start();
				if (!m_pluginManagerRefreshTimer.Enabled)
					m_pluginManagerRefreshTimer.Start();

				if (!m_statisticsTimer.Enabled)
					m_statisticsTimer.Start();

				if (PG_Control_Server_Properties.SelectedObject != m_server.Config)
					PG_Control_Server_Properties.SelectedObject = m_server.Config;
			}
		}

		#endregion

		#region "Control"

		internal void BTN_ServerControl_Start_Click(object sender, EventArgs e)
		{
			m_entityTreeRefreshTimer.Start();
			m_chatViewRefreshTimer.Start();
			m_factionRefreshTimer.Start();
			m_pluginManagerRefreshTimer.Start();

			if (m_server.IsRunning)
				return;

			if (m_server.Config != null )
				m_server.SaveServerConfig();

			m_server.StartServer();
		}

		internal void BTN_ServerControl_Stop_Click(object sender, EventArgs e)
		{
			m_entityTreeRefreshTimer.Stop();
			m_chatViewRefreshTimer.Stop();
			m_factionRefreshTimer.Stop();
			m_pluginManagerRefreshTimer.Stop();
			m_utilitiesCleanFloatingObjectsTimer.Stop();

			m_server.StopServer();
		}

		private void CHK_Control_Debugging_CheckedChanged(object sender, EventArgs e)
		{
			SandboxGameAssemblyWrapper.IsDebugging = CHK_Control_Debugging.CheckState == CheckState.Checked;
		}

		private void CHK_Control_CommonDataPath_CheckedChanged(object sender, EventArgs e)
		{
			SandboxGameAssemblyWrapper.UseCommonProgramData = CHK_Control_CommonDataPath.CheckState == CheckState.Checked;
			CMB_Control_CommonInstanceList.Enabled = SandboxGameAssemblyWrapper.UseCommonProgramData;

			m_server.InstanceName = SandboxGameAssemblyWrapper.UseCommonProgramData ? CMB_Control_CommonInstanceList.Text : string.Empty;

			m_server.LoadServerConfig();

			PG_Control_Server_Properties.SelectedObject = m_server.Config;
		}

		private void BTN_Control_Server_Reset_Click(object sender, EventArgs e)
		{
			//Refresh the loaded config
			m_server.LoadServerConfig();
			UpdateControls();

			PG_Control_Server_Properties.SelectedObject = m_server.Config;
		}

		private void BTN_Control_Server_Save_Click(object sender, EventArgs e)
		{
			if (m_server.ServerHasRan)
				m_server.Config = (DedicatedConfigDefinition)PG_Control_Server_Properties.SelectedObject;

			//Save the loaded config
			m_server.SaveServerConfig();
			UpdateControls();
		}

		private void PG_Control_Server_Properties_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
		{
			UpdateControls();
		}

		internal void ChangeConfigurationName( string configurationName )
		{
			CMB_Control_CommonInstanceList.SelectedItem = configurationName;
		}

		private void CMB_Control_CommonInstanceList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!CMB_Control_CommonInstanceList.Enabled || CMB_Control_CommonInstanceList.SelectedIndex == -1) return;

			m_server.InstanceName = CMB_Control_CommonInstanceList.Text;
			SandboxGameAssemblyWrapper.Instance.InitMyFileSystem(CMB_Control_CommonInstanceList.Text);

			m_server.LoadServerConfig();

			PG_Control_Server_Properties.SelectedObject = m_server.Config;
		}

		private void CMB_Control_AutosaveInterval_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!CMB_Control_AutosaveInterval.Enabled || CMB_Control_AutosaveInterval.SelectedIndex == -1) return;

			double interval = 2;
			try
			{
				interval = double.Parse(CMB_Control_AutosaveInterval.Text);
			}
			catch
			{
				MessageBox.Show( this, "Invalid input for auto-save interval." );
			}

			m_server.AutosaveInterval = interval * 60000;
		}

		private void UpdateControls()
		{
			if (m_server.Config == null)
				SandboxGameAssemblyWrapper.UseCommonProgramData = true;

			if (m_server.InstanceName.Length != 0)
			{
				CHK_Control_CommonDataPath.Checked = true;
				foreach (object item in CMB_Control_CommonInstanceList.Items)
				{
					if (item.ToString().Equals(m_server.InstanceName))
					{
						CMB_Control_CommonInstanceList.SelectedItem = item;
						break;
					}
				}
			}

			CHK_Control_Debugging.Checked = SandboxGameAssemblyWrapper.IsDebugging;

			if (!CMB_Control_CommonInstanceList.ContainsFocus && m_server.InstanceName.Length > 0)
				CMB_Control_CommonInstanceList.SelectedText = m_server.InstanceName;

			BTN_ServerControl_Stop.Enabled = m_server.IsRunning;
			BTN_ServerControl_Start.Enabled = !m_server.IsRunning;
			BTN_Chat_Send.Enabled = m_server.IsRunning;
			if(!m_server.IsRunning)
				BTN_Entities_New.Enabled = false;
			BTN_Utilities_ClearFloatingObjectsNow.Enabled = m_server.IsRunning;

			if (CHK_Control_CommonDataPath.CheckState == CheckState.Checked)
				CMB_Control_CommonInstanceList.Enabled = !m_server.IsRunning;
			else
				CMB_Control_CommonInstanceList.Enabled = false;

			TXT_Chat_Message.Enabled = m_server.IsRunning;

			PG_Entities_Details.Enabled = m_server.IsRunning;
			PG_Factions.Enabled = m_server.IsRunning;
			PG_Plugins.Enabled = m_server.IsRunning;

			if (m_server.Config != null)
			{
				if (string.IsNullOrEmpty(m_server.CommandLineArgs.Path) && CMB_Control_CommonInstanceList.Items.Count > 0)
					CHK_Control_CommonDataPath.Enabled = !m_server.IsRunning;
				else
					CHK_Control_CommonDataPath.Enabled = false;
			}
			else
			{
				CHK_Control_CommonDataPath.Checked = true;

//				BTN_Control_Server_Save.Enabled = false;
//				BTN_Control_Server_Reset.Enabled = false;
			}

			if (!m_server.IsRunning)
			{
				//BTN_Plugins_Refresh.Enabled = false;
				//BTN_Plugins_Load.Enabled = false;
				//BTN_Plugins_Unload.Enabled = false;
				BTN_Plugins_Reload.Enabled = false;
				BTN_Plugins_Enable.Enabled = false;
			}
			else
			{
				//BTN_Plugins_Refresh.Enabled = true;
				BTN_Plugins_Reload.Enabled = true;
				BTN_Plugins_Enable.Enabled = true;
			}

			if (!CMB_Control_AutosaveInterval.ContainsFocus)
				CMB_Control_AutosaveInterval.SelectedItem = (int)Math.Round(m_server.AutosaveInterval / 60000.0);
		}

		#endregion

		#region "Entities"

		private void UpdateNodeInventoryItemBranch<T>(TreeNode node, List<T> source)
			where T : InventoryItemEntity
		{
			try
			{
				bool entriesChanged = (node.Nodes.Count != source.Count);
				if (entriesChanged)
				{
					node.Nodes.Clear();
					node.Text = node.Name + " (" + source.Count + ")";
				}

				int index = 0;
				foreach (T item in source)
				{
					TreeNode itemNode;
					if (entriesChanged)
					{
						itemNode = node.Nodes.Add(item.Name);
						itemNode.Tag = item;
					}
					else
					{
						itemNode = node.Nodes[index];
						itemNode.Text = item.Name;
						itemNode.Tag = item;
					}

					index++;
				}
			}
			catch (Exception ex)
			{
				ApplicationLog.BaseLog.Error(ex);
			}
		}

		private void TreeViewRefresh(object sender, EventArgs e)
		{
			m_entityTreeRefreshTimer.Enabled = false;

			SS_Bottom.Items[0].Text = string.Format("Updates Per Second: {0}", WorldManager.GetUpdatesPerSecond());

			try
			{
				if (!SandboxGameAssemblyWrapper.Instance.IsGameStarted)
					return;

				if (TAB_MainTabs.SelectedTab != TAB_Entities_Page)
					return;

				TRV_Entities.BeginUpdate();

				TreeNode sectorObjectsNode;
				TreeNode sectorEventsNode;

				if (TRV_Entities.Nodes.Count < 2)
				{
					sectorObjectsNode = TRV_Entities.Nodes.Add("Sector Objects");
					sectorEventsNode = TRV_Entities.Nodes.Add("Sector Events");

					sectorObjectsNode.Name = sectorObjectsNode.Text;
					sectorEventsNode.Name = sectorEventsNode.Text;
				}
				else
				{
					sectorObjectsNode = TRV_Entities.Nodes[0];
					sectorEventsNode = TRV_Entities.Nodes[1];
				}

				RenderSectorObjectChildNodes(sectorObjectsNode);
				sectorObjectsNode.Text = string.Format( "{0} ({1})", sectorObjectsNode.Name, SectorObjectManager.Instance.Count );
				sectorObjectsNode.Tag = SectorObjectManager.Instance;


				TRV_Entities.EndUpdate();
			}
			finally
			{
				m_entityTreeRefreshTimer.Interval = 500;
				m_entityTreeRefreshTimer.Enabled = true;
			}
		}

		private void RenderSectorObjectChildNodes(TreeNode objectsNode)
		{
			if (TRV_Entities.IsDisposed)
				return;

			TreeNode cubeGridsNode;
			TreeNode charactersNode;
			TreeNode voxelMapsNode;
			TreeNode floatingObjectsNode;
			TreeNode meteorsNode;

			if (objectsNode.Nodes.Count < 5)
			{
				objectsNode.Nodes.Clear();

				cubeGridsNode = objectsNode.Nodes.Add("Cube Grids");
				charactersNode = objectsNode.Nodes.Add("Characters");
				voxelMapsNode = objectsNode.Nodes.Add("Voxel Maps");
				floatingObjectsNode = objectsNode.Nodes.Add("Floating Objects");
				meteorsNode = objectsNode.Nodes.Add("Meteors");

				cubeGridsNode.Name = cubeGridsNode.Text;
				charactersNode.Name = charactersNode.Text;
				voxelMapsNode.Name = voxelMapsNode.Text;
				floatingObjectsNode.Name = floatingObjectsNode.Text;
				meteorsNode.Name = meteorsNode.Text;
			}
			else
			{
				cubeGridsNode = objectsNode.Nodes[0];
				charactersNode = objectsNode.Nodes[1];
				voxelMapsNode = objectsNode.Nodes[2];
				floatingObjectsNode = objectsNode.Nodes[3];
				meteorsNode = objectsNode.Nodes[4];
			}

			m_sectorEntities = SectorObjectManager.Instance.GetTypedInternalData<BaseEntity>();
			foreach (BaseEntity entry in m_sectorEntities)
			{
				CubeGridEntity cubeGridEntity = entry as CubeGridEntity;
				if (cubeGridEntity != null)
					m_cubeGridEntities.Add(cubeGridEntity);
				CharacterEntity characterEntity = entry as CharacterEntity;
				if (characterEntity != null)
					m_characterEntities.Add(characterEntity);
				VoxelMap voxelMap = entry as VoxelMap;
				if (voxelMap != null)
					m_voxelMapEntities.Add(voxelMap);
				FloatingObject floatingObject = entry as FloatingObject;
				if (floatingObject != null)
					m_floatingObjectEntities.Add(floatingObject);
				Meteor meteor = entry as Meteor;
				if (meteor != null)
					m_meteorEntities.Add(meteor);
			}

			RenderCubeGridNodes(cubeGridsNode);
			RenderCharacterNodes(charactersNode);
			RenderVoxelMapNodes(voxelMapsNode);
			RenderFloatingObjectNodes(floatingObjectsNode);
			RenderMeteorNodes(meteorsNode);
		}

		private void RenderCubeGridNodes(TreeNode rootNode)
		{
			if (rootNode == null)
				return;

			//Get cube grids
			List<CubeGridEntity> list = m_cubeGridEntities;			
			SortCubeGrids(list);

			//Cleanup and update the existing nodes
			foreach (TreeNode node in rootNode.Nodes)
			{
				try
				{
					if (node == null)
						continue;
					if (node.Tag == null)
					{
						node.Remove();
						continue;
					}

					CubeGridEntity item = (CubeGridEntity)node.Tag;
					bool foundMatch = false;
					foreach (CubeGridEntity listItem in list)
					{
						if (listItem.EntityId == item.EntityId)
						{
							foundMatch = true;
							string newNodeText = GenerateCubeNodeText(item);
							node.Text = newNodeText;
							list.Remove(listItem);

							break;
						}
					}

					if (!foundMatch)
					{
						node.Remove();
						continue;
					}
				}
				catch (Exception ex)
				{
					ApplicationLog.BaseLog.Error(ex);
				}
			}

			//Add new nodes
			foreach (CubeGridEntity item in list)
			{
				try
				{
					if (item == null)
						continue;

					string nodeKey = item.EntityId.ToString();

					TreeNode newNode = rootNode.Nodes.Add(nodeKey, GenerateCubeNodeText(item));
					newNode.Name = item.Name;
					newNode.Tag = item;
				}
				catch (Exception ex)
				{
					ApplicationLog.BaseLog.Error(ex);
				}
			}

			//Update node text
			rootNode.Text = string.Format( "{0} ({1})", rootNode.Name, rootNode.Nodes.Count );
		}

		private string GenerateCubeNodeText(CubeGridEntity item)
		{
			string text = item.DisplayName;

			int sortBy = CB_Entity_Sort.SelectedIndex;
			switch ( sortBy )
			{
				case 0:
					text += string.Format( " | {0}", item.Name );
					break;
				case 1:
					text += string.Format( " | ID: {0}", item.EntityId );
					break;
				case 4:
					text += string.Format( " | Mass: {0} kg", Math.Floor(item.Mass) );
					break;
			}
			
			text += string.Format( " | Dist: {0}m", Math.Round(((Vector3D)item.Position).Length(), 0) );

			return text;
		}

		public void SortCubeGrids(List<CubeGridEntity> list)
		{
			int sortBy = CB_Entity_Sort.SelectedIndex;

			if (sortBy == 0) // Name
			{
				list.Sort(delegate(CubeGridEntity x, CubeGridEntity y)
				          {
					          if (x.Name == null && y.Name == null) return 0;
					          if (x.Name == null) return -1;
					          if (y.Name == null) return 1;
					          return x.Name.CompareTo(y.Name);
				          } );
			}
			else if (sortBy == 1) // Entity ID
			{
				list.Sort(( x, y ) => x.EntityId.CompareTo(y.EntityId) );
			}
			else if (sortBy == 2) // Distance From Center
			{
				list.Sort(( x, y ) =>
					{
						if (x == null || x.IsDisposed || x.IsLoading)
							return -1;

						if (y == null || y.IsDisposed || y.IsLoading)
							return 1;

						return Vector3D.Distance(x.Position, Vector3D.Zero).CompareTo(Vector3D.Distance(y.Position, Vector3D.Zero));
					});
			}
			else if(sortBy == 3) // Display Name
			{
				list.Sort(( x, y ) => x.DisplayName.CompareTo(y.DisplayName) );
			}
		}

		private void RenderCharacterNodes(TreeNode rootNode)
		{
			if (rootNode == null)
				return;

			//Get entities from sector object manager
			List<CharacterEntity> list = m_characterEntities;

			//Cleanup and update the existing nodes
			foreach (TreeNode node in rootNode.Nodes)
			{
				try
				{
					if (node == null)
						continue;

					if (node.Tag != null && list.Contains(node.Tag))
					{
						CharacterEntity item = (CharacterEntity)node.Tag;

						if (!item.IsDisposed)
						{
							Vector3D rawPosition = item.Position;
							double distance = Math.Round(rawPosition.Length(), 0);
							string newNodeText = string.Format( "{0} | Dist: {1}m", item.Name, distance );
							node.Text = newNodeText;
						}
						list.Remove(item);
					}
					else
					{
						node.Remove();
					}
				}
				catch (Exception ex)
				{
					ApplicationLog.BaseLog.Error(ex);
				}
			}

			//Add new nodes
			foreach (CharacterEntity item in list)
			{
				try
				{
					if (item == null)
						continue;

					Vector3D rawPosition = item.Position;
					double distance = rawPosition.Length();

					string nodeKey = item.EntityId.ToString();

					TreeNode newNode = rootNode.Nodes.Add(nodeKey, string.Format( "{0} | Dist: {1}m", item.Name, distance ));
					newNode.Name = item.Name;
					newNode.Tag = item;
				}
				catch (Exception ex)
				{
					ApplicationLog.BaseLog.Error(ex);
				}
			}

			//Update node text
			rootNode.Text = string.Format( "{0} ({1})", rootNode.Name, rootNode.Nodes.Count );
		}

		private void RenderVoxelMapNodes(TreeNode rootNode)
		{
			if (rootNode == null)
				return;

			//Get entities from sector object manager
			List<VoxelMap> list = m_voxelMapEntities;

			//Cleanup and update the existing nodes
			foreach (TreeNode node in rootNode.Nodes)
			{
				try
				{
					if (node == null)
						continue;

					if (node.Tag != null && list.Contains(node.Tag))
					{
						VoxelMap item = (VoxelMap)node.Tag;

						if (!item.IsDisposed)
						{
							Vector3D rawPosition = item.Position;
							double distance = Math.Round(rawPosition.Length(), 0);
							string newNodeText = item.Name + " | Dist: " + distance + "m";
							node.Text = newNodeText;
						}
						list.Remove(item);
					}
					else
					{
						node.Remove();
					}
				}
				catch (Exception ex)
				{
					ApplicationLog.BaseLog.Error(ex);
				}
			}

			//Add new nodes
			foreach (VoxelMap item in list)
			{
				try
				{
					if (item == null)
						continue;

					Vector3D rawPosition = item.Position;
					double distance = rawPosition.Length();

					Type sectorObjectType = item.GetType();
					string nodeKey = item.EntityId.ToString();

					TreeNode newNode = rootNode.Nodes.Add(nodeKey, item.Name + " | Dist: " + distance + "m");
					newNode.Name = item.Name;
					newNode.Tag = item;
				}
				catch (Exception ex)
				{
					ApplicationLog.BaseLog.Error(ex);
				}
			}

			//Update node text
			rootNode.Text = rootNode.Name + " (" + rootNode.Nodes.Count + ")";
		}

		private void RenderFloatingObjectNodes(TreeNode rootNode)
		{
			if (rootNode == null)
				return;

			//Get entities from sector object manager
			List<FloatingObject> list = m_floatingObjectEntities;

			//Cleanup and update the existing nodes
			foreach (TreeNode node in rootNode.Nodes)
			{
				try
				{
					if (node == null)
						continue;

					if (node.Tag != null && list.Contains(node.Tag))
					{
						FloatingObject item = (FloatingObject)node.Tag;

						if (!item.IsDisposed && item.Item != null)
						{
							Vector3D rawPosition = item.Position;
							double distance = Math.Round(rawPosition.Length(), 0);
							string newNodeText = item.Name + " | Amount: " + item.Item.Amount + " | Dist: " + distance + "m";
							node.Text = newNodeText;
						}
						list.Remove(item);
					}
					else
					{
						node.Remove();
					}
				}
				catch (Exception ex)
				{
					ApplicationLog.BaseLog.Error(ex);
				}
			}

			//Add new nodes
			foreach (FloatingObject item in list)
			{
				try
				{
					if (item == null)
						continue;
					if (item.IsDisposed)
						continue;
					if (item.Item == null)
						continue;

					Vector3D rawPosition = item.Position;
					double distance = rawPosition.Length();

					string nodeKey = item.EntityId.ToString();

					TreeNode newNode = rootNode.Nodes.Add(nodeKey, string.Format( "{0} | Amount: {1} | Dist: {2}m", item.Name, item.Item.Amount, distance ));
					newNode.Name = item.Name;
					newNode.Tag = item;
				}
				catch (Exception ex)
				{
					ApplicationLog.BaseLog.Error(ex);
				}
			}

			//Update node text
			rootNode.Text = string.Format( "{0} ({1})", rootNode.Name, rootNode.Nodes.Count );

			// Update a var for the Utilities Floating object cleaner.
			m_floatingObjectsCount = rootNode.Nodes.Count;
		}

		private void RenderMeteorNodes(TreeNode rootNode)
		{
			if (rootNode == null)
				return;

			//Get entities from sector object manager
			List<Meteor> list = m_meteorEntities;

			//Cleanup and update the existing nodes
			foreach (TreeNode node in rootNode.Nodes)
			{
				try
				{
					if (node == null)
						continue;

					if (node.Tag != null && list.Contains(node.Tag))
					{
						Meteor item = (Meteor)node.Tag;

						if (!item.IsDisposed)
						{
							Vector3D rawPosition = item.Position;
							double distance = Math.Round(rawPosition.Length(), 0);
							string newNodeText = string.Format( "{0} | Dist: {1}m", item.Name, distance );
							node.Text = newNodeText;
						}
						list.Remove(item);
					}
					else
					{
						node.Remove();
					}
				}
				catch (Exception ex)
				{
					ApplicationLog.BaseLog.Error(ex);
				}
			}

			//Add new nodes
			foreach (Meteor item in list)
			{
				try
				{
					if (item == null)
						continue;

					Vector3D rawPosition = item.Position;
					double distance = rawPosition.Length();

					string nodeKey = item.EntityId.ToString();
					TreeNode newNode = rootNode.Nodes.Add(nodeKey, string.Format( "{0} | Dist: {1}m", item.Name, distance ));
					newNode.Name = item.Name;
					newNode.Tag = item;
				}
				catch (Exception ex)
				{
					ApplicationLog.BaseLog.Error(ex);
				}
			}

			//Update node text
			rootNode.Text = string.Format( "{0} ({1})", rootNode.Name, rootNode.Nodes.Count );
		}

		private void RenderCubeGridChildNodes(CubeGridEntity cubeGrid, TreeNode blocksNode)
		{
			TreeNode structuralBlocksNode;
			TreeNode containerBlocksNode;
			TreeNode productionBlocksNode;
			TreeNode energyBlocksNode;
			TreeNode conveyorBlocksNode;
			TreeNode utilityBlocksNode;
			TreeNode weaponBlocksNode;
			TreeNode toolBlocksNode;
			TreeNode lightBlocksNode;
			TreeNode miscBlocksNode;

			if (blocksNode.Nodes.Count < 9)
			{
				structuralBlocksNode = blocksNode.Nodes.Add("Structural");
				containerBlocksNode = blocksNode.Nodes.Add("Containers");
				productionBlocksNode = blocksNode.Nodes.Add("Refinement and Production");
				energyBlocksNode = blocksNode.Nodes.Add("Energy");
				conveyorBlocksNode = blocksNode.Nodes.Add("Conveyor");
				utilityBlocksNode = blocksNode.Nodes.Add("Utility");
				weaponBlocksNode = blocksNode.Nodes.Add("Weapons");
				toolBlocksNode = blocksNode.Nodes.Add("Tools");
				lightBlocksNode = blocksNode.Nodes.Add("Lights");
				miscBlocksNode = blocksNode.Nodes.Add("Misc");

				structuralBlocksNode.Name = structuralBlocksNode.Text;
				containerBlocksNode.Name = containerBlocksNode.Text;
				productionBlocksNode.Name = productionBlocksNode.Text;
				energyBlocksNode.Name = energyBlocksNode.Text;
				conveyorBlocksNode.Name = conveyorBlocksNode.Text;
				utilityBlocksNode.Name = utilityBlocksNode.Text;
				weaponBlocksNode.Name = weaponBlocksNode.Text;
				toolBlocksNode.Name = toolBlocksNode.Text;
				lightBlocksNode.Name = lightBlocksNode.Text;
				miscBlocksNode.Name = miscBlocksNode.Text;
			}
			else
			{
				structuralBlocksNode = blocksNode.Nodes[0];
				containerBlocksNode = blocksNode.Nodes[1];
				productionBlocksNode = blocksNode.Nodes[2];
				energyBlocksNode = blocksNode.Nodes[3];
				conveyorBlocksNode = blocksNode.Nodes[4];
				utilityBlocksNode = blocksNode.Nodes[5];
				weaponBlocksNode = blocksNode.Nodes[6];
				toolBlocksNode = blocksNode.Nodes[7];
				lightBlocksNode = blocksNode.Nodes[8];
				miscBlocksNode = blocksNode.Nodes[9];

				structuralBlocksNode.Nodes.Clear();
				containerBlocksNode.Nodes.Clear();
				productionBlocksNode.Nodes.Clear();
				energyBlocksNode.Nodes.Clear();
				conveyorBlocksNode.Nodes.Clear();
				utilityBlocksNode.Nodes.Clear();
				weaponBlocksNode.Nodes.Clear();
				toolBlocksNode.Nodes.Clear();
				lightBlocksNode.Nodes.Clear();
				miscBlocksNode.Nodes.Clear();
			}

			foreach (CubeBlockEntity cubeBlock in cubeGrid.CubeBlocks)
			{
				TreeNode newNode = new TreeNode(cubeBlock.Name);
				newNode.Name = newNode.Text;
				newNode.Tag = cubeBlock;

				Type cubeBlockType = cubeBlock.GetType();

				if (cubeBlockType == typeof(CubeBlockEntity))
				{
					structuralBlocksNode.Nodes.Add(newNode);
				}
				else if (cubeBlock is CargoContainerEntity)
				{
					containerBlocksNode.Nodes.Add(newNode);
				}
				else if (cubeBlock is ReactorEntity)
				{
					energyBlocksNode.Nodes.Add(newNode);
				}
				else if (cubeBlock is BatteryBlockEntity)
				{
					energyBlocksNode.Nodes.Add(newNode);
				}
				else if (cubeBlock is BeaconEntity)
				{
					utilityBlocksNode.Nodes.Add(newNode);
				}
				else if (cubeBlock is CockpitEntity)
				{
					utilityBlocksNode.Nodes.Add(newNode);
				}
				else if (cubeBlock is GravityGeneratorEntity)
				{
					utilityBlocksNode.Nodes.Add(newNode);
				}
				else if (cubeBlock is MedicalRoomEntity)
				{
					utilityBlocksNode.Nodes.Add(newNode);
				}
				else if (cubeBlock is DoorEntity)
				{
					utilityBlocksNode.Nodes.Add(newNode);
				}
				else if (cubeBlock is InteriorLightEntity)
				{
					lightBlocksNode.Nodes.Add(newNode);
				}
				else if (cubeBlock is ReflectorLightEntity)
				{
					lightBlocksNode.Nodes.Add(newNode);
				}
				else if (cubeBlock is RefineryEntity)
				{
					productionBlocksNode.Nodes.Add(newNode);
				}
				else if (cubeBlock is AssemblerEntity)
				{
					productionBlocksNode.Nodes.Add(newNode);
				}
				else if (cubeBlock is ConveyorBlockEntity)
				{
					conveyorBlocksNode.Nodes.Add(newNode);
				}
				else if (cubeBlock is ConveyorTubeEntity)
				{
					conveyorBlocksNode.Nodes.Add(newNode);
				}
				else if (cubeBlock is SolarPanelEntity)
				{
					energyBlocksNode.Nodes.Add(newNode);
				}
				else if (cubeBlock is GatlingTurretEntity)
				{
					weaponBlocksNode.Nodes.Add(newNode);
				}
				else if (cubeBlock is MissileTurretEntity)
				{
					weaponBlocksNode.Nodes.Add(newNode);
				}
				else if (cubeBlock is ShipGrinderEntity)
				{
					toolBlocksNode.Nodes.Add(newNode);
				}
				else if (cubeBlock is ShipWelderEntity)
				{
					toolBlocksNode.Nodes.Add(newNode);
				}
				else if (cubeBlock is ShipDrillEntity)
				{
					toolBlocksNode.Nodes.Add(newNode);	
				}
				else if (cubeBlock is InteriorTurretEntity)
				{
					weaponBlocksNode.Nodes.Add(newNode);
				}
				else
				{
					miscBlocksNode.Nodes.Add(newNode);
				}
			}

			structuralBlocksNode.Text = string.Format( "{0} ({1})", structuralBlocksNode.Name, structuralBlocksNode.Nodes.Count );
			containerBlocksNode.Text = string.Format( "{0} ({1})", containerBlocksNode.Name, containerBlocksNode.Nodes.Count );
			productionBlocksNode.Text = string.Format( "{0} ({1})", productionBlocksNode.Name, productionBlocksNode.Nodes.Count );
			energyBlocksNode.Text = string.Format( "{0} ({1})", energyBlocksNode.Name, energyBlocksNode.Nodes.Count );
			conveyorBlocksNode.Text = string.Format( "{0} ({1})", conveyorBlocksNode.Name, conveyorBlocksNode.Nodes.Count );
			utilityBlocksNode.Text = string.Format( "{0} ({1})", utilityBlocksNode.Name, utilityBlocksNode.Nodes.Count );
			weaponBlocksNode.Text = string.Format( "{0} ({1})", weaponBlocksNode.Name, weaponBlocksNode.Nodes.Count );
			toolBlocksNode.Text = string.Format( "{0} ({1})", toolBlocksNode.Name, toolBlocksNode.Nodes.Count );
			lightBlocksNode.Text = string.Format( "{0} ({1})", lightBlocksNode.Name, lightBlocksNode.Nodes.Count );
			miscBlocksNode.Text = string.Format( "{0} ({1})", miscBlocksNode.Name, miscBlocksNode.Nodes.Count );
		}

		private void TRV_Entities_NodeRefresh(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Clicks < 2)
				return;
			if (e.Node == null)
				return;
			if (e.Node.Tag == null)
				return;

			//Clear the child nodes
			e.Node.Nodes.Clear();

			//Call the main node select event handler to populate the node
			TreeViewEventArgs newEvent = new TreeViewEventArgs(e.Node);
			TRV_Entities_AfterSelect(sender, newEvent);
		}

		private void TRV_Entities_AfterSelect(object sender, TreeViewEventArgs e)
		{
			BTN_Entities_Export.Enabled = false;
			BTN_Entities_New.Enabled = false;
			BTN_Entities_Delete.Enabled = false;
			btnRepairEntity.Enabled = false;

			TreeNode selectedNode = e.Node;

			if (selectedNode == null)
				return;

			TreeNode parentNode = e.Node.Parent;

			if (parentNode == null)
				return;

			if ( parentNode.Tag is SectorObjectManager)
			{
				if (selectedNode == parentNode.Nodes[0])
				{
					BTN_Entities_New.Enabled = true;
				}
			}

			if ( parentNode.Tag is CubeGridEntity)
			{
				BTN_Entities_New.Enabled = true;
			}

			if (selectedNode.Tag == null)
				return;

			object linkedObject = selectedNode.Tag;
			PG_Entities_Details.SelectedObject = linkedObject;

			//Enable export for all objects that inherit from BaseObject
			if (linkedObject is BaseObject)
			{
				BTN_Entities_Export.Enabled = true;
			}

			//Enable delete for all objects that inherit from BaseEntity
			if (linkedObject is BaseEntity)
			{
				BTN_Entities_Delete.Enabled = true;
			}

			//Enable delete and repair for all objects that inherit from CubeBlockEntity
			CubeBlockEntity cubeBlockEntity = linkedObject as CubeBlockEntity;
			if (cubeBlockEntity != null)
			{
				BTN_Entities_Delete.Enabled = true;
				btnRepairEntity.Enabled = true;
			}

			CubeGridEntity cubeGridEntity = linkedObject as CubeGridEntity;
			if (cubeGridEntity != null)
			{
				BTN_Entities_New.Enabled = true;
				btnRepairEntity.Enabled = true;

				TRV_Entities.BeginUpdate();

				RenderCubeGridChildNodes(cubeGridEntity, e.Node);

				TRV_Entities.EndUpdate();
			}

			VoxelMap map = linkedObject as VoxelMap;
			if (map != null)
			{
				VoxelMap voxelMap = map;
				
				List<MyVoxelMaterialDefinition> materialDefs = new List<MyVoxelMaterialDefinition>(MyDefinitionManager.Static.GetVoxelMaterialDefinitions());

				ThreadPool.QueueUserWorkItem( state =>
				                             {
					                             Dictionary<MyVoxelMaterialDefinition, float> totalMaterials = voxelMap.Materials;

					                             Invoke(new Action(() =>
					                                                    {
						                                                    TRV_Entities.BeginUpdate();
						                                                    if (e.Node.Nodes.Count < materialDefs.Count)
						                                                    {
							                                                    e.Node.Nodes.Clear();

							                                                    foreach (MyVoxelMaterialDefinition material in materialDefs)
							                                                    {
								                                                    TreeNode newNode = e.Node.Nodes.Add(material.Id.SubtypeName);
								                                                    newNode.Name = newNode.Text;
								                                                    newNode.Tag = material;
							                                                    }
						                                                    }

						                                                    foreach (TreeNode node in e.Node.Nodes)
						                                                    {
							                                                    Object tag = node.Tag;
							                                                    if ( !(tag is MyVoxelMaterialDefinition))
								                                                    continue;
							                                                    MyVoxelMaterialDefinition material = (MyVoxelMaterialDefinition)tag;
							                                                    float total;
							                                                    if (totalMaterials.TryGetValue( material, out total ))
							                                                    {
								                                                    node.Text = string.Format( "{0} ({1})", node.Name, total );
							                                                    }
						                                                    }

						                                                    TRV_Entities.EndUpdate();
					                                                    }));
				                             });

			}

			CharacterEntity characterEntity = linkedObject as CharacterEntity;
			if (characterEntity != null)
			{
				CharacterEntity character = characterEntity;

				if (e.Node.Nodes.Count < 1)
				{
					TRV_Entities.BeginUpdate();

					e.Node.Nodes.Clear();
					TreeNode itemsNode = e.Node.Nodes.Add("Items");
					itemsNode.Name = itemsNode.Text;
					itemsNode.Tag = character.Inventory;

					TRV_Entities.EndUpdate();
				}
			}

			SmallGatlingGunEntity smallGatlingGunEntity = linkedObject as SmallGatlingGunEntity;
			if (smallGatlingGunEntity != null)
			{
				SmallGatlingGunEntity gun = smallGatlingGunEntity;

				if (e.Node.Nodes.Count < 1)
				{
					TRV_Entities.BeginUpdate();

					e.Node.Nodes.Clear();
					TreeNode itemsNode = e.Node.Nodes.Add("Ammo");
					itemsNode.Name = itemsNode.Text;
					itemsNode.Tag = gun.Inventory;

					TRV_Entities.EndUpdate();
				}
			}

			TurretBaseEntity turretBaseEntity = linkedObject as TurretBaseEntity;
			if (turretBaseEntity != null)
			{
				TurretBaseEntity gun = turretBaseEntity;

				if (e.Node.Nodes.Count < 1)
				{
					TRV_Entities.BeginUpdate();

					e.Node.Nodes.Clear();
					TreeNode itemsNode = e.Node.Nodes.Add("Ammo");
					itemsNode.Name = itemsNode.Text;
					itemsNode.Tag = gun.Inventory;

					TRV_Entities.EndUpdate();
				}
			}

			CockpitEntity cockpitEntity = linkedObject as CockpitEntity;
			if (cockpitEntity != null)
			{
				CockpitEntity cockpit = cockpitEntity;

				if (e.Node.Nodes.Count < 1)
				{
					TRV_Entities.BeginUpdate();

					e.Node.Nodes.Clear();
					TreeNode node = e.Node.Nodes.Add("Pilot");
					node.Name = node.Text;
					node.Tag = cockpit.PilotEntity;

					TRV_Entities.EndUpdate();
				}
				else
				{
					TRV_Entities.BeginUpdate();

					TreeNode node = e.Node.Nodes[0];
					node.Tag = cockpit.PilotEntity;

					TRV_Entities.EndUpdate();
				}
			}

			CargoContainerEntity cargoContainerEntity = linkedObject as CargoContainerEntity;
			if (cargoContainerEntity != null)
			{
				CargoContainerEntity container = cargoContainerEntity;

				if (e.Node.Nodes.Count < 1)
				{
					TRV_Entities.BeginUpdate();

					e.Node.Nodes.Clear();
					TreeNode itemsNode = e.Node.Nodes.Add("Items");
					itemsNode.Name = itemsNode.Text;
					itemsNode.Tag = container.Inventory;

					TRV_Entities.EndUpdate();
				}
			}

			ReactorEntity reactorEntity = linkedObject as ReactorEntity;
			if (reactorEntity != null)
			{
				ReactorEntity reactor = reactorEntity;

				if (e.Node.Nodes.Count < 1)
				{
					TRV_Entities.BeginUpdate();

					e.Node.Nodes.Clear();
					TreeNode itemsNode = e.Node.Nodes.Add("Items");
					itemsNode.Name = itemsNode.Text;
					itemsNode.Tag = reactor.Inventory;

					TRV_Entities.EndUpdate();
				}
			}

			ShipToolBaseEntity shipToolBaseEntity = linkedObject as ShipToolBaseEntity;
			if (shipToolBaseEntity != null)
			{
				ShipToolBaseEntity shipTool = shipToolBaseEntity;

				if (e.Node.Nodes.Count < 1)
				{
					TRV_Entities.BeginUpdate();

					e.Node.Nodes.Clear();
					TreeNode itemsNode = e.Node.Nodes.Add("Items");
					itemsNode.Name = itemsNode.Text;
					itemsNode.Tag = shipTool.Inventory;

					TRV_Entities.EndUpdate();
				}
			}

			ShipDrillEntity shipDrillEntity = linkedObject as ShipDrillEntity;
			if (shipDrillEntity != null)
			{
				ShipDrillEntity shipDrill = shipDrillEntity;

				if (e.Node.Nodes.Count < 1)
				{
					TRV_Entities.BeginUpdate();

					e.Node.Nodes.Clear();
					TreeNode itemsNode = e.Node.Nodes.Add("Items");
					itemsNode.Name = itemsNode.Text;
					itemsNode.Tag = shipDrill.Inventory;

					TRV_Entities.EndUpdate();
				}
			}

			ProductionBlockEntity productionBlockEntity = linkedObject as ProductionBlockEntity;
			if (productionBlockEntity != null)
			{
				ProductionBlockEntity productionBlock = productionBlockEntity;

				if (e.Node.Nodes.Count < 2)
				{
					TRV_Entities.BeginUpdate();

					e.Node.Nodes.Clear();
					TreeNode inputNode = e.Node.Nodes.Add("Input");
					inputNode.Name = inputNode.Text;
					inputNode.Tag = productionBlock.InputInventory;
					TreeNode outputNode = e.Node.Nodes.Add("Output");
					outputNode.Name = outputNode.Text;
					outputNode.Tag = productionBlock.OutputInventory;

					TRV_Entities.EndUpdate();
				}
			}

			InventoryEntity inventoryEntity = linkedObject as InventoryEntity;
			if (inventoryEntity != null)
			{
				BTN_Entities_New.Enabled = true;

				InventoryEntity inventory = inventoryEntity;

				UpdateNodeInventoryItemBranch(e.Node, inventory.Items);
			}

			if (linkedObject is InventoryItemEntity)
			{
				BTN_Entities_New.Enabled = true;
				BTN_Entities_Delete.Enabled = true;
			}
		}

		private void BTN_Entities_Delete_Click(object sender, EventArgs e)
		{
			try
			{
				Object linkedObject = TRV_Entities.SelectedNode.Tag;
				if (!(linkedObject is BaseObject))
					return;

				BaseObject baseObject = (BaseObject)linkedObject;
				baseObject.Dispose();

				TreeNode parentNode = TRV_Entities.SelectedNode.Parent;
				TRV_Entities.SelectedNode.Tag = null;
				TreeNode newSelectedNode = ( TRV_Entities.SelectedNode.NextVisibleNode ?? TRV_Entities.SelectedNode.PrevVisibleNode ) ?? parentNode.FirstNode;

				TRV_Entities.SelectedNode.Remove();
				if ( newSelectedNode != null )
				{
					TRV_Entities.SelectedNode = newSelectedNode;
					PG_Entities_Details.SelectedObject = newSelectedNode.Tag;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void BTN_Entities_New_Click(object sender, EventArgs e)
		{
			try
			{
				TreeNode selectedNode = TRV_Entities.SelectedNode;

				if (selectedNode == null)
					return;

				TreeNode parentNode = selectedNode.Parent;

				if (parentNode == null)
					return;

				SectorObjectManager sectorObjectManager = parentNode.Tag as SectorObjectManager;
				if ( sectorObjectManager != null)
				{
					if (selectedNode == parentNode.Nodes[0])
					{
						CreateCubeGridImportDialog();
						return;
					}
				}

				CubeGridEntity cubeGridEntity = parentNode.Tag as CubeGridEntity;
				if ( cubeGridEntity != null)
				{
					CubeBlockDialog dialog = new CubeBlockDialog { ParentCubeGrid = cubeGridEntity };
					dialog.ShowDialog(this);
					return;
				}

				if (selectedNode.Tag == null)
					return;

				if (!(selectedNode.Tag is BaseObject))
					return;

				BaseObject linkedObject = (BaseObject)selectedNode.Tag;

				InventoryEntity inventoryEntity = linkedObject as InventoryEntity;
				if (inventoryEntity != null)
				{
					InventoryItemDialog newItemDialog = new InventoryItemDialog { InventoryContainer = inventoryEntity };
					newItemDialog.ShowDialog(this);

					TreeViewEventArgs newEvent = new TreeViewEventArgs(selectedNode);
					TRV_Entities_AfterSelect(sender, newEvent);

					return;
				}

				InventoryItemEntity inventoryItemEntity = linkedObject as InventoryItemEntity;
				if (inventoryItemEntity != null)
				{
					InventoryItemDialog newItemDialog = new InventoryItemDialog { InventoryContainer = inventoryItemEntity.Container };
					newItemDialog.ShowDialog(this);

					TreeViewEventArgs newEvent = new TreeViewEventArgs(parentNode);
					TRV_Entities_AfterSelect(sender, newEvent);

					return;
				}

				if (linkedObject is CubeGridEntity)
				{
					CreateCubeGridImportDialog();
					return;
				}
			}
			catch (Exception ex)
			{
				ApplicationLog.BaseLog.Error(ex);
			}
		}

		private void BTN_Entities_Export_Click(object sender, EventArgs e)
		{
			try
			{
				if (TRV_Entities.SelectedNode == null)
					return;
				Object linkedObject = TRV_Entities.SelectedNode.Tag;
				if (linkedObject == null)
					return;
				if (!(linkedObject is BaseObject))
					return;

				BaseObject objectToExport = (BaseObject)linkedObject;

				SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = "sbc file (*.sbc)|*.sbc|All files (*.*)|*.*", InitialDirectory = GameInstallationInfo.GamePath };

				if (saveFileDialog.ShowDialog() == DialogResult.OK)
				{
					FileInfo fileInfo = new FileInfo(saveFileDialog.FileName);
					try
					{
						objectToExport.Export(fileInfo);
					}
					catch (Exception ex)
					{
						MessageBox.Show(this, ex.Message);
					}
				}
			}
			catch (Exception ex)
			{
				ApplicationLog.BaseLog.Error(ex);
			}
		}

		private void PG_Entities_Details_Click(object sender, EventArgs e)
		{
			TreeNode node = TRV_Entities.SelectedNode;
			if (node == null)
				return;
			object linkedObject = node.Tag;
			PG_Entities_Details.SelectedObject = linkedObject;
		}

		private void CreateCubeGridImportDialog()
		{
			try
			{
				OpenFileDialog openFileDialog = new OpenFileDialog
				{
					InitialDirectory = GameInstallationInfo.GamePath,
					DefaultExt = "sbc file (*.sbc)"
				};

				if (openFileDialog.ShowDialog(this) == DialogResult.OK)
				{
					FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
					if (fileInfo.Exists)
					{
						try
						{
							CubeGridEntity cubeGrid = new CubeGridEntity(fileInfo);

							SectorObjectManager.Instance.AddEntity(cubeGrid);
						}
						catch (Exception ex)
						{
							ApplicationLog.BaseLog.Error(ex);
						}
					}
				}
			}
			catch (Exception ex)
			{
				ApplicationLog.BaseLog.Error(ex);
			}
		}

		private void CB_Entity_Sort_SelectionChangeCommitted(object sender, EventArgs e)
		{
			TreeNode cubeNode = TRV_Entities.Nodes.Find("Cube Grids", true).FirstOrDefault();
			if(cubeNode == null)
				return;

			m_sortBy = CB_Entity_Sort.SelectedIndex;
			cubeNode.Nodes.Clear();
			TreeViewEventArgs newEvent = new TreeViewEventArgs(cubeNode);
			TRV_Entities_AfterSelect(sender, newEvent);			

		}

		#endregion

		#region "Chat"

		private void ChatViewRefresh(object sender, EventArgs e)
		{
			//Refresh the chat history
			List<ChatManager.ChatEvent> chatHistory = ChatManager.Instance.ChatHistory;
			if (chatHistory.Count != m_chatLineCount)
			{
				int pos = 0;
				foreach (ChatManager.ChatEvent entry in chatHistory)
				{
					if (pos >= m_chatLineCount)
					{

						string timestamp = entry.Timestamp.ToLongTimeString();
						string playerName = "Server";
						if (entry.SourceUserId != 0)
							playerName = PlayerMap.Instance.GetPlayerNameFromSteamId(entry.SourceUserId);
						string formattedMessage = timestamp + " - " + playerName + " - " + entry.Message + "\r\n";
						RTB_Chat_Messages.AppendText(formattedMessage);
					}

					pos++;
				}

				m_chatLineCount = chatHistory.Count;
			}

			//Refresh the connected players list
			LST_Chat_ConnectedPlayers.BeginUpdate();
			List<ulong> connectedPlayers = PlayerManager.Instance.ConnectedPlayers;
			
			if (connectedPlayers.Count != LST_Chat_ConnectedPlayers.Items.Count || CheckRequireNameUpdate())
			{
				int selected = LST_Chat_ConnectedPlayers.SelectedIndex;
				LST_Chat_ConnectedPlayers.DataSource = null;
				LST_Chat_ConnectedPlayers.Items.Clear();
				List<ChatUserItem> connectedPlayerList = new List<ChatUserItem>();
				foreach (ulong remoteUserId in connectedPlayers)
				{
					ChatUserItem item = new ChatUserItem();
					string playerName = PlayerMap.Instance.GetPlayerNameFromSteamId(remoteUserId);

					item.Username = playerName;
					item.SteamId = remoteUserId;
					connectedPlayerList.Add(item);
				}

				LST_Chat_ConnectedPlayers.DataSource = connectedPlayerList;
				LST_Chat_ConnectedPlayers.DisplayMember = "Username";

				if (selected >= connectedPlayerList.Count && connectedPlayerList.Count > 0)
					LST_Chat_ConnectedPlayers.SelectedIndex = 0;
				else
					LST_Chat_ConnectedPlayers.SelectedIndex = selected;

			}
			LST_Chat_ConnectedPlayers.EndUpdate();
		}

		private bool CheckRequireNameUpdate()
		{
			foreach (object item in LST_Chat_ConnectedPlayers.Items)
			{
				ChatUserItem chatItem = (ChatUserItem)item;

				if (chatItem.SteamId.ToString() == chatItem.Username)
					return true;
			}

			return false;
		}

		private void BTN_Chat_Send_Click(object sender, EventArgs e)
		{
			string message = TXT_Chat_Message.Text;
			if (!string.IsNullOrEmpty( message ))
			{
				ChatManager.Instance.SendPublicChatMessage(message);
				TXT_Chat_Message.Text = string.Empty;
			}
		}

		private void TXT_Chat_Message_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				string message = TXT_Chat_Message.Text;
				if (!string.IsNullOrEmpty( message ))
				{
					ChatManager.Instance.SendPublicChatMessage(message);
					TXT_Chat_Message.Text = string.Empty;
				}
			}
		}

		private void BTN_Chat_KickSelected_Click(object sender, EventArgs e)
		{
			if (LST_Chat_ConnectedPlayers.SelectedItem != null)
			{
				ChatUserItem item = (ChatUserItem)LST_Chat_ConnectedPlayers.SelectedItem;
				ChatManager.Instance.SendPublicChatMessage(string.Format( "/kick {0}", item.SteamId ));
			}
		}

		private void BTN_Chat_BanSelected_Click(object sender, EventArgs e)
		{
			if (LST_Chat_ConnectedPlayers.SelectedItem != null)
			{
				ChatUserItem item = (ChatUserItem)LST_Chat_ConnectedPlayers.SelectedItem;
				ChatManager.Instance.SendPublicChatMessage(string.Format( "/ban {0}", item.SteamId ));
			}
		}
		#endregion

		#region "Factions"

		private void FactionRefresh(object sender, EventArgs e)
		{
			try
			{
				if (SandboxGameAssemblyWrapper.Instance.IsGameStarted)
				{
					TRV_Factions.BeginUpdate();

					List<Faction> list = FactionsManager.Instance.Factions;

					//Cleanup and update the existing nodes
					foreach (TreeNode node in TRV_Factions.Nodes)
					{
						try
						{
							if (node == null)
								continue;
							if (node.Tag == null)
							{
								node.Remove();
								continue;
							}

							Faction item = (Faction)node.Tag;
							bool foundMatch = false;
							foreach (Faction faction in list)
							{
								if (faction.Id == item.Id)
								{
									foundMatch = true;

									string newNodeText = string.Format( "{0} ({1})", item.Name, item.Members.Count );
									node.Text = newNodeText;

									TreeNode membersNode = node.Nodes[0];
									TreeNode joinRequestsNode = node.Nodes[1];

									if (membersNode.Nodes.Count != item.Members.Count)
									{
										membersNode.Nodes.Clear();
										foreach (FactionMember member in item.Members)
										{
											TreeNode memberNode = membersNode.Nodes.Add(member.PlayerId.ToString(), member.PlayerId.ToString());
											memberNode.Name = member.PlayerId.ToString();
											memberNode.Tag = member;
										}
									}
									if (joinRequestsNode.Nodes.Count != item.JoinRequests.Count)
									{
										joinRequestsNode.Nodes.Clear();
										foreach (FactionMember member in item.JoinRequests)
										{
											TreeNode joinRequestNode = joinRequestsNode.Nodes.Add(member.PlayerId.ToString(), member.PlayerId.ToString());
											joinRequestNode.Name = member.PlayerId.ToString();
											joinRequestNode.Tag = member;
										}
									}

									list.Remove(faction);

									break;
								}
							}

							if (!foundMatch)
							{
								node.Remove();
								continue;
							}
						}
						catch (Exception ex)
						{
							ApplicationLog.BaseLog.Error(ex);
						}
					}

					//Add new nodes
					foreach (Faction item in list)
					{
						try
						{
							if (item == null)
								continue;

							string nodeKey = item.Id.ToString();

							TreeNode newNode = TRV_Factions.Nodes.Add(nodeKey, string.Format( "{0} ({1})", item.Name, item.Members.Count ));
							newNode.Name = item.Name;
							newNode.Tag = item;

							TreeNode membersNode = newNode.Nodes.Add("Members");
							TreeNode joinRequestsNode = newNode.Nodes.Add("Join Requests");

							foreach (FactionMember member in item.Members)
							{
								TreeNode memberNode = membersNode.Nodes.Add(member.PlayerId.ToString(), member.PlayerId.ToString());
								memberNode.Name = member.PlayerId.ToString();
								memberNode.Tag = member;
							}
							foreach (FactionMember member in item.JoinRequests)
							{
								TreeNode memberNode = membersNode.Nodes.Add(member.PlayerId.ToString(), member.PlayerId.ToString());
								memberNode.Name = member.PlayerId.ToString();
								memberNode.Tag = member;
							}
						}
						catch (Exception ex)
						{
							ApplicationLog.BaseLog.Error(ex);
						}
					}

					TRV_Factions.EndUpdate();
				}
			}
			catch (Exception ex)
			{
				ApplicationLog.BaseLog.Error(ex);
			}
		}

		private void TRV_Factions_AfterSelect(object sender, TreeViewEventArgs e)
		{
			BTN_Factions_Delete.Enabled = false;

			if (e.Node == null)
				return;
			if (e.Node.Tag == null)
				return;

			object linkedObject = e.Node.Tag;

			BTN_Factions_Delete.Enabled = true;

			PG_Factions.SelectedObject = linkedObject;

			//DEBUG
			if (e.Node.Text.Equals("Join Requests"))
				BTN_Factions_Delete.Enabled = false;
		}

		private void BTN_Factions_Delete_Click(object sender, EventArgs e)
		{
			TreeNode node = TRV_Factions.SelectedNode;
			if (node == null)
				return;
			if (node.Tag == null)
				return;

			object linkedObject = node.Tag;

			Faction faction = linkedObject as Faction;
			if (faction != null)
			{
				FactionsManager.Instance.RemoveFaction(faction.Id);
			}

			FactionMember factionMember = linkedObject as FactionMember;
			if (factionMember != null)
			{
				factionMember.Parent.RemoveMember( factionMember.PlayerId );
			}
		}

		#endregion

		#region "Plugins"

		private void PluginManagerRefresh(object sender, EventArgs e)
		{
			if (PluginManager.Instance.Initialized)
			{
				if (PluginManager.Instance.Plugins.Count == LST_Plugins.Items.Count)
					return;

                int selectedIndex = LST_Plugins.SelectedIndex;
                if (selectedIndex >= PluginManager.Instance.Plugins.Count)
                    return;

				LST_Plugins.BeginUpdate();			
				LST_Plugins.Items.Clear();
				foreach (Guid key in PluginManager.Instance.Plugins.Keys)
				{
					IPlugin plugin = (IPlugin)PluginManager.Instance.Plugins[key]; 
					LST_Plugins.Items.Add(string.Format( "{0} - {1}", plugin.Name, key ));
				}
				LST_Plugins.SelectedIndex = selectedIndex;
				LST_Plugins.EndUpdate();
			}
		}

		private void LST_Plugins_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (LST_Plugins.SelectedItem == null)
				return;

			int selectedIndex = LST_Plugins.SelectedIndex;
            if (selectedIndex >= PluginManager.Instance.Plugins.Count)
                return;

			Guid selectedItem = PluginManager.Instance.Plugins.Keys.ElementAt(selectedIndex);
			Object plugin = PluginManager.Instance.Plugins[selectedItem];

			// This section allows plugins to have a customized settings form inside the settings
			// panel of a plugin.  
			AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

			//foreach (var type in Assembly.GetAssembly(plugin.GetType()).GetTypes()) //??
			//{
			PropertyInfo info = plugin.GetType().GetProperty("PluginControlForm");
			if (info != null)
			{
				PG_Plugins.Visible = false;
				Form value = (Form)info.GetValue(plugin, null);

				foreach (Control control in SC_Plugins.Panel2.Controls)
				{
					if(control.Visible)
						control.Visible = false;
				}

				if (!SC_Plugins.Panel2.Controls.Contains(value))
				{
					value.TopLevel = false;
					SC_Plugins.Panel2.Controls.Add(value);
				}

				value.Dock = DockStyle.Fill;
				value.FormBorderStyle = FormBorderStyle.None;
				value.Visible = true;
			}
			else // Default PropertyGrid view
			{
				foreach (Control ctl in SC_Plugins.Panel2.Controls)
				{
					if (ctl.Visible)
					{
						ctl.Visible = false;
					}
				}

				PG_Plugins.Visible = true;
				PG_Plugins.SelectedObject = plugin;
			}
			//}

			// Set state
			bool pluginState = PluginManager.Instance.GetPluginState(selectedItem);
			if (pluginState)
			{
				BTN_Plugins_Reload.Enabled = true;
				BTN_Plugins_Enable.Text = "Disable";
			}
			else
			{
				BTN_Plugins_Reload.Enabled = false;
				BTN_Plugins_Enable.Text = "Enable";
			}
		}

		/// <summary>
		/// If a plugin uses reference .dlls and puts them in their plugin dir, the appdomain won't
		/// know where to find those dlls as the path won't be resolvable.  So we can just scan here
		/// and return the assembly if we have it
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			string modsPath = Path.Combine(Server.Instance.Path, "Mods");
			string[] subDirectories = Directory.GetDirectories(modsPath);
			foreach (string path in subDirectories)
			{
				string[] files = Directory.GetFiles(path);
				foreach (string file in files)
				{
					FileInfo fileInfo = new FileInfo(file);
					if (!fileInfo.Extension.ToLower().Equals(".dll"))
						continue;

					string[] names = args.Name.Split(new char[] {','});
					if (!fileInfo.Name.ToLower().Equals(names[0].ToLower().Trim() + ".dll"))
						continue;

					byte[] b = File.ReadAllBytes(file);
					return Assembly.Load(b);
				}
			}

			return null;
		}

		private void BTN_Plugins_Reload_Click(object sender, EventArgs e)
		{
			if (LST_Plugins.SelectedItem == null)
				return;

			int selectedIndex = LST_Plugins.SelectedIndex;
			if (selectedIndex >= PluginManager.Instance.Plugins.Count)
				return;

			Guid selectedItem = PluginManager.Instance.Plugins.Keys.ElementAt(selectedIndex);
			PluginManager.Instance.UnloadPlugin(selectedItem);
			PluginManager.Instance.LoadPlugins(true);
			PluginManager.Instance.InitPlugin(selectedItem);
			LST_Plugins_SelectedIndexChanged(this, EventArgs.Empty);
		}

		private void BTN_Plugins_Enable_Click(object sender, EventArgs e)
		{
			if (LST_Plugins.SelectedItem == null)
				return;

			int selectedIndex = LST_Plugins.SelectedIndex;
			if (selectedIndex >= PluginManager.Instance.Plugins.Count)
				return;

			Guid selectedItem = PluginManager.Instance.Plugins.Keys.ElementAt(selectedIndex);
			bool pluginState = PluginManager.Instance.GetPluginState(selectedItem);
			if (pluginState)
			{
				PluginManager.Instance.UnloadPlugin(selectedItem);
				PluginManager.Instance.LoadPlugins(true);
			}
			else
			{
				PluginManager.Instance.LoadPlugins(true);
				PluginManager.Instance.InitPlugin(selectedItem);
			}

			LST_Plugins_SelectedIndexChanged(this, EventArgs.Empty);
		}

		private void BTN_Plugins_Unload_Click(object sender, EventArgs e)
		{
			if (LST_Plugins.SelectedItem == null)
				return;

			int selectedIndex = LST_Plugins.SelectedIndex;
            if (selectedIndex >= PluginManager.Instance.Plugins.Count)
                return;

			Guid selectedItem = PluginManager.Instance.Plugins.Keys.ElementAt(selectedIndex);
			PluginManager.Instance.UnloadPlugin(selectedItem);
		}

		private void BTN_Plugins_Load_Click(object sender, EventArgs e)
		{
			if (LST_Plugins.SelectedItem == null)
				return;

			int selectedIndex = LST_Plugins.SelectedIndex;
            if (selectedIndex >= PluginManager.Instance.Plugins.Count)
                return;

			Guid selectedItem = PluginManager.Instance.Plugins.Keys.ElementAt(selectedIndex);
			PluginManager.Instance.InitPlugin(selectedItem);
		}

		private void BTN_Plugins_Refresh_Click(object sender, EventArgs e)
		{
			PluginManager.Instance.LoadPlugins(true);
		}

		#endregion

		#region "Utilities"

		// Start the Auto Clean timer if user checks the auto clean checkbox.
		private void CHK_Utilities_FloatingObjectAutoClean_CheckedChanged(object sender, EventArgs e)
		{
			if (CHK_Utilities_FloatingObjectAutoClean.Checked)
			{
				if(!m_utilitiesCleanFloatingObjectsTimer.Enabled)
					m_utilitiesCleanFloatingObjectsTimer.Start();
			}
			else
			{
				if (m_utilitiesCleanFloatingObjectsTimer.Enabled)
				{
					m_utilitiesCleanFloatingObjectsTimer.Enabled = false;
					m_utilitiesCleanFloatingObjectsTimer.Stop();
				}
			}
		}

		// Delete all floating objects when the count reaches the amount set.
		private void UtilitiesCleanFloatingObjects(object sender, EventArgs e)
		{
			if (m_floatingObjectsCount > TXT_Utilities_FloatingObjectAmount.IntValue)
			{
				ChatManager.Instance.SendPublicChatMessage("/delete all floatingobjects");
			}
		}

		// Delete all floating objects now
		private void BTN_Utilities_ClearFloatingObjectsNow_Click(object sender, EventArgs e)
		{
			ChatManager.Instance.SendPublicChatMessage("/delete all floatingobjects");
		}

		#endregion

		private void TSM_Kick_Click(object sender, EventArgs e)
		{
			if (LST_Chat_ConnectedPlayers.SelectedItem != null)
			{
				ChatUserItem item = (ChatUserItem)LST_Chat_ConnectedPlayers.SelectedItem;
				ChatManager.Instance.SendPublicChatMessage(string.Format( "/kick {0}", item.SteamId ));
			}
		}

		#endregion

		private void TSM_Ban_Click(object sender, EventArgs e)
		{
			if (LST_Chat_ConnectedPlayers.SelectedItem != null)
			{
				ChatUserItem item = (ChatUserItem)LST_Chat_ConnectedPlayers.SelectedItem;
				ChatManager.Instance.SendPublicChatMessage(string.Format( "/ban {0}", item.SteamId ));
			}
		}

		/// <summary>
		/// Repairs the selected <see cref="CubeBlockEntity"/> or <see cref="CubeGridEntity"/>
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnRepairEntity_Click( object sender, EventArgs e )
		{
			bool previousExportButtonState = BTN_Entities_Export.Enabled;
			bool previousNewButtonState = BTN_Entities_New.Enabled;
			bool previousDeleteButtonState = BTN_Entities_Delete.Enabled;
			BTN_Entities_Export.Enabled = false;
			BTN_Entities_New.Enabled = false;
			BTN_Entities_Delete.Enabled = false;
			btnRepairEntity.Enabled = false;

			TreeNode selectedNode = TRV_Entities.SelectedNode;

			if ( selectedNode == null )
			{
				MessageBox.Show( this, "Cannot repair that." );
				return;
			}

			TreeNode parentNode = TRV_Entities.SelectedNode.Parent;

			if ( parentNode == null )
			{
				MessageBox.Show( this, "Cannot repair that." );
				return;
			}

			if ( selectedNode.Tag == null )
			{
				MessageBox.Show( this, "Cannot repair that." );
				return;
			}

			object linkedObject = selectedNode.Tag;
			PG_Entities_Details.SelectedObject = linkedObject;

			CubeGridEntity cubeGridEntity = linkedObject as CubeGridEntity;
			if ( cubeGridEntity != null )
			{
				cubeGridEntity.Repair( );

				TRV_Entities.BeginUpdate( );

				RenderCubeGridChildNodes( cubeGridEntity, selectedNode );

				TRV_Entities.EndUpdate( );
			}

			CubeBlockEntity cubeBlockEntity = linkedObject as CubeBlockEntity;
			if ( cubeBlockEntity != null )
			{
				cubeBlockEntity.Repair( );
			}

			BTN_Entities_Export.Enabled = previousExportButtonState;
			BTN_Entities_New.Enabled = previousNewButtonState;
			BTN_Entities_Delete.Enabled = previousDeleteButtonState;
			btnRepairEntity.Enabled = true;
		}
	}
}
