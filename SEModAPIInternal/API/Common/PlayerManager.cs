﻿using System;
using System.Collections.Generic;
using System.Linq;

using Sandbox.Common.ObjectBuilders;

using SEModAPIInternal.API.Entity;
using SEModAPIInternal.API.Utility;
using SEModAPIInternal.API.Server;
using SEModAPIInternal.Support;

using VRage.Serialization;

using Sandbox.ModAPI;
using System.Reflection;
using SteamSDK;

namespace SEModAPIInternal.API.Common
{
	public class PlayerMap
	{

		public struct InternalPlayerItem
		{
            public string name;
			public bool isDead;
			public ulong steamId;
			public string model;
            public long playerId;
            public long entityId;
		}

        public struct InternalIdentityItem
        {
            public string name;
            public string model;
            public long playerId;
            public long entityId;

            public InternalIdentityItem(Object source)
            {
                name = (string)BaseObject.GetEntityFieldValue(source, "1FB9EB403295AA9F17329DAB808AACC8");
                model = (string)BaseObject.GetEntityFieldValue(source, "98D5685D557109787E8C5BC033A1863F");
				playerId = (long)BaseObject.GetEntityFieldValue(source, "AFB0FA8889365232B6E34B6F527B304A");
                entityId = 0;
            }
        }

        public struct InternalClientItem : IComparable<InternalClientItem>
        {
            public ulong SteamId;
            public int SerialId;

            public InternalClientItem(Object source)
            {
                SteamId = (ulong)BaseObject.GetEntityFieldValue(source, "5FCD9FCE67409E4C896AAA6B61A0C33B");
                SerialId = (int)BaseObject.GetEntityFieldValue(source, "BC639949E46A6E11FEBEDF9BD08068A9");
            }

			public InternalClientItem(IMyPlayer player)
			{
				SteamId = player.SteamUserId;
				SerialId = 0;
			}

            public int CompareTo(InternalClientItem item)
            {
                if (SteamId < item.SteamId)
                {
                    return -1;
                }
                if (SteamId > item.SteamId)
                {
                    return 1;
                }
                if (SerialId < item.SerialId)
                {
                    return -1;
                }
                if (SerialId > item.SerialId)
                {
                    return 1;
                }
                return 0;
            }

            public override bool Equals(object item)
            {
                if (!(item is InternalClientItem))
                {
                    return false;
                }
                return (InternalClientItem)item == this;
            }

            public override int GetHashCode()
            {
                return SteamId.GetHashCode() * 571 ^ SerialId.GetHashCode();
            }

            public static InternalClientItem operator --(InternalClientItem item)
            {
                item.SerialId = item.SerialId - 1;
                return item;
            }

            public static bool operator ==(InternalClientItem item1, InternalClientItem item2)
            {
                if (item1.SteamId != item2.SteamId)
                {
                    return false;
                }
                return item1.SerialId == item2.SerialId;
            }

            public static InternalClientItem operator ++(InternalClientItem item)
            {
                item.SerialId = item.SerialId + 1;
                return item;
            }

            public static bool operator !=(InternalClientItem item1, InternalClientItem item2)
            {
                return !(item1 == item2);
            }

            public override string ToString()
            {
                return string.Concat(SteamId.ToString(), ":", SerialId.ToString());
            }
        }

		#region "Attributes"

		private static PlayerMap m_instance;

		public static string PlayerMapNamespace = "5F381EA9388E0A32A8C817841E192BE8";
		public static string PlayerMapClass = "E4C2964159826A46D102C2D7FDDC0733";

        public static string PlayerMapGetPlayerItemMappingField = "A343C8C883753AA13761B57BF93642B5";
        public static string PlayerMapGetSteamItemMappingField = "F8D0682ABF3074A0515A99D0E537D2E1";
		public static string PlayerMapGetFastPlayerIdFromSteamIdMethod = "664B2E5CBB958C38E24656118771D345";
		public static string PlayerMapGetFastIdentityFromPlayerIdMethod = "B5AF02B0769E7DFC7347A31BC9E6D3D1";
		public static string PlayerMapGetFastIdentityNameField = "1FB9EB403295AA9F17329DAB808AACC8";

		public static string PlayerMapSessionNamespace = "AAC05F537A6F0F6775339593FBDFC564";
		public static string PlayerMapSessionClass = "D580AE7552E79DAB03A3D64B1F7B67F9";
		public static string PlayerMapSessionCameraField = "B475119E5256E23CEA219027068578E6";
		public static string PlayerMapCameraDataClass = "1363E4575A0EAB9C07E1CC9EE70B0711";
		public static string PlayerMapGetCameraDataField = "3A4EC5D20D8A9F113F78FC05516F0D8B";

		public static string PlayerMapForceDisplaySpawnMenu = "BFF5EBE381CB1EE971FAE0A3DECAB00F";

        // SteamIdToPlayerId? public long 664B2E5CBB958C38E24656118771D345(ulong u00336ADF4D8C43635669729322024D2AD33, int u0032FA8049E153F637DEA99600B785ECCA0 = 0)


		//////////////////////////////////////////////////////

		public static string PlayerMapEntryNamespace = "";
		public static string PlayerMapEntryClass = "";

		public static string PlayerMapEntrySteamIdField = "208AE30D2628BD946A59F72F1A373ED4";

		#endregion

		#region "Constructors and Initializers"

		protected PlayerMap()
		{
			m_instance = this;

			Console.WriteLine("Finished loading PlayerMap");
		}

		#endregion

		#region "Properties"

		public static PlayerMap Instance
		{
			get
			{
				if (m_instance == null)
					m_instance = new PlayerMap();

				return m_instance;
			}
		}

		public Object BackingObject
		{
			get
			{
				Object backingObject = PlayerManager.Instance.InternalGetPlayerMap();
				return backingObject;
			}
		}

		#endregion

		#region "Methods"

		public static bool ReflectionUnitTest()
		{
			try
			{
				Type type1 = SandboxGameAssemblyWrapper.Instance.GetAssemblyType(PlayerMapNamespace, PlayerMapClass);
				if (type1 == null)
					throw new Exception("Could not find internal type for PlayerMap");
				bool result = true;
                result &= BaseObject.HasField(type1, PlayerMapGetPlayerItemMappingField);
                result &= BaseObject.HasField(type1, PlayerMapGetSteamItemMappingField);
				result &= BaseObject.HasMethod(type1, PlayerMapGetFastPlayerIdFromSteamIdMethod);
								
				Type type2 = SandboxGameAssemblyWrapper.Instance.GetAssemblyType(PlayerMapNamespace, PlayerMapCameraDataClass);
				if (type2 == null)
					throw new Exception("Could not find camera data type for PlayerMap");
				result &= BaseObject.HasField(type2, PlayerMapGetCameraDataField);

				Type type3 = WorldManager.InternalType;
				if(type3 == null)
					throw new Exception("Could not find world manager type for PlayerMap");
				result &= BaseObject.HasField(type3, PlayerMapSessionCameraField);

				return result;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return false;
			}
		}

        public string GetPlayerNameFromSteamId(ulong steamId)
        {
			if (steamId.ToString().StartsWith("9009"))
				return "Server";

            string playerName = steamId.ToString();

            Dictionary<ulong, InternalPlayerItem> steamDictionary = InternalGetSteamDictionary();
            if (steamDictionary.ContainsKey(steamId))
                return steamDictionary[steamId].name;

            return playerName;
        }

        public ulong GetSteamId(long playerEntityId)
        {
			if (BackingObject == null)
				return 0;

			if(!(BackingObject is IMyPlayerCollection))
				return 0;

            IMyPlayerCollection collection = (IMyPlayerCollection)BackingObject;

            List<IMyPlayer> players = new List<IMyPlayer>();
            //collection.GetPlayers(players, x => x.Controller.ControlledEntity.Entity.EntityId == playerEntityId);
			collection.GetPlayers(players, x => x.Controller != null && x.Controller.ControlledEntity != null && x.Controller.ControlledEntity.Entity != null && x.Controller.ControlledEntity.Entity.EntityId == playerEntityId);
            if (players != null && players.Count > 0)
            {
                return players.First().SteamUserId;
            }

            return 0;
        }

		/// <summary>
		/// Gets a List of InternalPlayerItems matching a players name
		/// Can be partial or full name
		/// </summary>
		/// <param name="playerName"> the name of the player</param>
		/// <returns>returns a list of matches</returns>
		public List<InternalPlayerItem> GetPlayerItemsFromPlayerName(string playerName)
		{
			string lowerName = playerName.ToLower();

			var playerItemstoReturn = new List<InternalPlayerItem>();

			foreach (var playerItem in InternalGetSteamDictionary().Values)
			{
				if (playerItem.name.ToLower().Contains(lowerName))
				{
					// if the playeritem occurs more than once, replace it.
					if (playerItemstoReturn.Exists(x => x.name.ToLower() == playerItem.name.ToLower()))
					{
						playerItemstoReturn[playerItemstoReturn.IndexOf(playerItemstoReturn.First(x => x.name.ToLower() == playerItem.name.ToLower()))] = playerItem;
					}
					else
						playerItemstoReturn.Add(playerItem);
				}
			}
			return playerItemstoReturn;
		}

        public MyObjectBuilder_Checkpoint.PlayerItem GetPlayerItemFromPlayerId(long playerId)
        {            
            MyObjectBuilder_Checkpoint.PlayerItem playerItem = new MyObjectBuilder_Checkpoint.PlayerItem();

            try
            {
                Dictionary<long, InternalPlayerItem> playerDictionary = InternalGetPlayerDictionary();
                if (!playerDictionary.ContainsKey(playerId))
                    return playerItem;

                InternalPlayerItem item = playerDictionary[playerId];
                playerItem.PlayerId = item.playerId;
                playerItem.SteamId = item.steamId;
                playerItem.Name = item.name;
                playerItem.Model = item.model;
                playerItem.IsDead = item.isDead;
            }
            catch (Exception ex)
            {
                LogManager.ErrorLog.WriteLine(ex);
            }

            return playerItem;
        }

        public ulong GetSteamIdFromPlayerName(string playerName, bool partial = false)
        {
            ulong steamId = 0;
            Dictionary<ulong, InternalPlayerItem> steamDictionary = InternalGetSteamDictionary();
			if(!partial)
				steamId = steamDictionary.FirstOrDefault(x => x.Value.name == playerName && x.Value.steamId != 0).Key;
			else
				steamId = steamDictionary.FirstOrDefault(x => x.Value.name.ToLower().Contains(playerName.ToLower()) && x.Value.steamId != 0).Key;

            if (steamId == 0)
            {
                try
                {
                    steamId = ulong.Parse(playerName);
                }
                catch (Exception ex)
                {
                    LogManager.ErrorLog.WriteLine(ex);
                }
            }

            return steamId;
        }

		public ulong GetSteamIdFromPlayerId(long playerId)
		{
			Dictionary<long, InternalPlayerItem> allPlayers = InternalGetPlayerDictionary();
			if (allPlayers.ContainsKey(playerId))
				return allPlayers[playerId].steamId;

			return 0;
		}

        public long GetPlayerEntityId(ulong steamId)
        {
            long result = 0;
            try
            {
                IMyPlayerCollection collection = (IMyPlayerCollection)BackingObject;

                List<IMyPlayer> players = new List<IMyPlayer>();
                collection.GetPlayers(players, x => x.SteamUserId == steamId);

                if (players.Count > 0)
                {
                    IMyPlayer player = players.FirstOrDefault();
                    if (player != null)
                    {
                        if (player.Controller != null && player.Controller.ControlledEntity != null && player.Controller.ControlledEntity.Entity != null)
                            result = player.Controller.ControlledEntity.Entity.EntityId;
                    }
                }
            }
            catch(Exception ex)
            {
                LogManager.ErrorLog.WriteLine(ex.ToString());
            }
            return result;
        }

        public List<long> GetPlayerIdsFromSteamId(ulong steamId, bool ignoreDead = true)
        {
            List<long> matchingPlayerIds = new List<long>();

            try
            {
                if (ignoreDead)
                {
                    Dictionary<ulong, InternalPlayerItem> steamDictionary = InternalGetSteamDictionary();
                    if (steamDictionary.ContainsKey(steamId))
                        matchingPlayerIds.Add(steamDictionary[steamId].playerId);
                }
                else
                {
                    foreach (InternalPlayerItem item in InternalGetPlayerList())
                    {
                        if (item.steamId == steamId)
                            matchingPlayerIds.Add(item.playerId);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.ErrorLog.WriteLine(ex);
            }

            return matchingPlayerIds;
        }

        public List<long> GetPlayerIds()
        {
            return InternalGetPlayerList().Select(x => x.playerId).ToList();
        }

		public long GetFastPlayerIdFromSteamId(ulong steamId)
		{
			int num = 0;
			long result = (long)BaseObject.InvokeEntityMethod(BackingObject, PlayerMapGetFastPlayerIdFromSteamIdMethod, new object[] {steamId, num });
			return result;
		}

		public string GetFastPlayerNameFromSteamId(ulong steamId)
		{
			long playerId = GetFastPlayerIdFromSteamId(steamId);
			if (playerId < 1)
				return "";

			object identity = BaseObject.InvokeEntityMethod(BackingObject, PlayerMapGetFastIdentityFromPlayerIdMethod, new object[] { playerId });
			object result = BaseObject.GetEntityFieldValue(identity, PlayerMapGetFastIdentityNameField);
			if (result != null)
				return (string)result;
			else
				return "";
		}

		public IMyIdentity GetFastPlayerIdentityFromPlayerId(long playerId)
		{
			//FieldInfo identitiesInfo = BaseObject.GetEntityField(BackingObject, PlayerMapGetPlayerItemMappingField);
			object identities = BaseObject.GetEntityFieldValue(BackingObject, PlayerMapGetPlayerItemMappingField);
			if(identities == null)
				return null;

			//return (IMyIdentity)identitiesInfo.DeclaringType.GetMethod("get_Item").Invoke(identities, new object[] { playerId });
			return (IMyIdentity)identities.GetType().GetMethod("get_Item").Invoke(identities, new object[] { playerId });
		}

		public void RemovePlayer(long playerId)
		{
			try
			{
				Object rawPlayerItemMapping = BaseObject.GetEntityFieldValue(BackingObject, PlayerMapGetPlayerItemMappingField);
				MethodInfo removeMethod = rawPlayerItemMapping.GetType().GetMethod("Remove");
				removeMethod.Invoke(rawPlayerItemMapping, new object[] { playerId });
			}
			catch (Exception ex)
			{
				LogManager.ErrorLog.WriteLineAndConsole(string.Format("RemovePlayer(): {0}", ex.ToString()));
			}
		}

		/*
		public static string PlayerMapSessionCameraField = "B475119E5256E23CEA219027068578E6";
		public static string PlayerMapGetCameraDataClass = "1363E4575A0EAB9C07E1CC9EE70B0711";
		public static string PlayerMapGetCameraDataField = "3A4EC5D20D8A9F113F78FC05516F0D8B";
		 */
 
		public void ClearCameraData()
		{
			try
			{
				Object cameraDataObject = BaseObject.GetEntityFieldValue(WorldManager.Instance.BackingObject, PlayerMapSessionCameraField);
				Object cameraDictionaryObject = BaseObject.GetEntityFieldValue(cameraDataObject, PlayerMapGetCameraDataField);
				MethodInfo removeMethod = cameraDictionaryObject.GetType().GetMethod("Clear");
				removeMethod.Invoke(cameraDictionaryObject, new object[] { });				
			}
			catch (Exception ex)
			{
				LogManager.ErrorLog.WriteLineAndConsole(string.Format("ClearCameraData(): {0}", ex.ToString()));
			}
		}

        // --
        protected Dictionary<long, Object> InternalGetPlayerItemMapping()
        {
            try
            {
                Object rawPlayerItemMapping = BaseObject.GetEntityFieldValue(BackingObject, PlayerMapGetPlayerItemMappingField);
                Dictionary<long, Object> allPlayersMapping = UtilityFunctions.ConvertDictionary<long>(rawPlayerItemMapping);
                return allPlayersMapping;
            }
            catch (Exception ex)
            {
                LogManager.ErrorLog.WriteLine(ex);
                return new Dictionary<long, Object>();
            }
        }

        protected Dictionary<object, long> InternalGetSteamIdMapping()
        {
            try
            {
                object rawPlayerItemMapping = BaseObject.GetEntityFieldValue(BackingObject, PlayerMapGetSteamItemMappingField);
                Dictionary<object, long> allSteamPlayersMapping = UtilityFunctions.ConvertDictionaryReverse<long>(rawPlayerItemMapping);
                return allSteamPlayersMapping;
            }
            catch (Exception ex)
            {
                LogManager.ErrorLog.WriteLine(ex);
                return new Dictionary<object, long>();
            }
        }

        protected Dictionary<long, InternalPlayerItem> InternalGetPlayerDictionary()
        {
            Dictionary<long, InternalPlayerItem> result = new Dictionary<long, InternalPlayerItem>();

            Dictionary<long, InternalClientItem> allSteamList;
            Dictionary<long, InternalIdentityItem> allPlayerList;
            InternalGetReferenceLists(out allSteamList, out allPlayerList);

            foreach (KeyValuePair<long, InternalIdentityItem> p in allPlayerList)
            {
                InternalPlayerItem item = new InternalPlayerItem();
                item.isDead = false;
                item.model = p.Value.model;
                item.name = p.Value.name;
                item.playerId = p.Value.playerId;
                item.steamId = 0;
                if (allSteamList.ContainsKey(p.Value.playerId))
                    item.steamId = allSteamList[p.Value.playerId].SteamId;

                if (result.ContainsKey(item.playerId))
                    result[item.playerId] = item;
                else
                    result.Add(item.playerId, item);
            }

            return result;
        }

        protected Dictionary<ulong, InternalPlayerItem> InternalGetSteamDictionary()
        {
            Dictionary<ulong, InternalPlayerItem> result = new Dictionary<ulong, InternalPlayerItem>();

            Dictionary<long, InternalClientItem> allSteamList;
            Dictionary<long, InternalIdentityItem> allPlayerList;
            InternalGetReferenceLists(out allSteamList, out allPlayerList);

            foreach (KeyValuePair<long, InternalIdentityItem> p in allPlayerList)
            {
                InternalPlayerItem item = new InternalPlayerItem();
                item.isDead = false;
                item.model = p.Value.model;
                item.name = p.Value.name;
                item.playerId = p.Value.playerId;
                item.steamId = 0;
                if (allSteamList.ContainsKey(p.Value.playerId))
                    item.steamId = allSteamList[p.Value.playerId].SteamId;

                if (result.ContainsKey(item.steamId))
                    result[item.steamId] = item;
                else
                    result.Add(item.steamId, item);
            }

            return result;
        }

        protected List<InternalPlayerItem> InternalGetPlayerList()
        {
            try
            {
                Dictionary<long, InternalClientItem> allSteamList;
                Dictionary<long, InternalIdentityItem> allPlayerList;
                InternalGetReferenceLists(out allSteamList, out allPlayerList);

                List<InternalPlayerItem> result = new List<InternalPlayerItem>();
                foreach (KeyValuePair<long, InternalIdentityItem> p in allPlayerList)
                {
                    for (int x = 0; x < result.Count; x++)
                    {
                        InternalPlayerItem test = result[x];
                        if (test.name == p.Value.name)
                            test.isDead = true;
                    }

                    InternalPlayerItem item = new InternalPlayerItem();
                    item.isDead = false;
                    item.model = p.Value.model;
                    item.name = p.Value.name;
                    item.playerId = p.Value.playerId;
                    item.steamId = 0;
                    if (allSteamList.ContainsKey(p.Value.playerId))
                        item.steamId = allSteamList[p.Value.playerId].SteamId;

                    result.Add(item);
                }

                return result;
            }
            catch (Exception ex)
            {
                LogManager.ErrorLog.WriteLine(ex.ToString());
                return new List<InternalPlayerItem>();
            }
        }

        private void InternalGetReferenceLists(out Dictionary<long, InternalClientItem> allSteamList, out Dictionary<long, InternalIdentityItem> allPlayerList)
        {			
			if (MyAPIGateway.Players == null)
			{
				allSteamList = new Dictionary<long, InternalClientItem>();
				allPlayerList = new Dictionary<long, InternalIdentityItem>();
				return;
			}
			/*
			List<IMyPlayer> players = new List<IMyPlayer>();
			List<IMyIdentity> identities = new List<IMyIdentity>();
//			SandboxGameAssemblyWrapper.Instance.GameAction(() =>
//			{
				MyAPIGateway.Players.GetPlayers(players);
				MyAPIGateway.Players.GetAllIdentites(identities);
//			});

			allSteamList = new Dictionary<long, InternalClientItem>();
			foreach (IMyPlayer player in players)
			{
				InternalClientItem item = new InternalClientItem(player);
				allSteamList.Add(player.PlayerID, item);
			}

			allPlayerList = new Dictionary<long, InternalIdentityItem>();
			foreach (IMyIdentity identity in identities)
			{
				InternalIdentityItem item = new InternalIdentityItem(identity);
				allPlayerList.Add(identity.PlayerId, item);
			}
			 */ 
            Dictionary<object, long> steamList = InternalGetSteamIdMapping();
            allSteamList = new Dictionary<long, InternalClientItem>();
            foreach (KeyValuePair<object, long> p in steamList)
            {
                InternalClientItem item = new InternalClientItem(p.Key);
                allSteamList.Add(p.Value, item);
            }
			
            Dictionary<long, Object> playerList = InternalGetPlayerItemMapping();
            allPlayerList = new Dictionary<long, InternalIdentityItem>();
            foreach (KeyValuePair<long, object> p in playerList)
            {
                InternalIdentityItem item = new InternalIdentityItem(p.Value);
                allPlayerList.Add(p.Key, item);
            }
        }

        /*

        protected Object GetPlayerFromSteamId(ulong steamId)
        {
            try
            {
                Object result = BaseObject.InvokeEntityMethod(BackingObject, PlayerMapSteamIdToPlayerMappingMethod, new object[] { steamId });

                return result;
            }
            catch (Exception ex)
            {
                LogManager.ErrorLog.WriteLine(ex);
                return null;
            }
        }

		protected ulong GetSteamIdFromPlayer(Object player)
		{
			try
			{
				ulong steamId = (ulong)BaseObject.GetEntityFieldValue(player, PlayerMapEntrySteamIdField);

				return steamId;
			}
			catch (Exception ex)
			{
				LogManager.ErrorLog.WriteLine(ex);
				return 0;
			}
		}

        /*
		protected ulong GetCharacterInternalEntityToSteamId(Object gameEntity)
		{
			try
			{
				Object mapEntry = BaseObject.InvokeEntityMethod(BackingObject, PlayerMapEntityToPlayerMappingMethod, new object[] { gameEntity });
				ulong steamId = GetSteamIdFromPlayer(mapEntry);

				return steamId;
			}
			catch (Exception ex)
			{
				LogManager.ErrorLog.WriteLine(ex);
				return 0;
			}
		}
		protected SerializableDictionary<long, ulong> InternalGetSerializableDictionary()
		{
			try
			{
				SerializableDictionary<long, ulong> result = (SerializableDictionary<long, ulong>)BaseObject.InvokeEntityMethod(BackingObject, PlayerMapGetSerializableDictionaryMethod);

				return result;
			}
			catch (Exception ex)
			{
				LogManager.ErrorLog.WriteLine(ex);
				return new SerializableDictionary<long, ulong>();
			}
		}

		protected Dictionary<long, Object> InternalGetPlayerItemMappping()
		{
			try
			{
				Object rawPlayerItemMapping = BaseObject.InvokeEntityMethod(BackingObject, PlayerMapGetPlayerItemMappingMethod);
				Dictionary<long, Object> allPlayersMapping = UtilityFunctions.ConvertDictionary<long>(rawPlayerItemMapping);

				return allPlayersMapping;
			}
			catch (Exception ex)
			{
				LogManager.ErrorLog.WriteLine(ex);
				return new Dictionary<long, Object>();
			}
		}

		public long GetServerVirtualPlayerId()
		{
			ulong serverSteamId = 0;
			foreach (var entry in WorldManager.Instance.Checkpoint.Players.Dictionary)
			{
				if (entry.Value.PlayerId == 0L)
				{
					serverSteamId = entry.Value.SteamID;
				}
			}

			long serverVirtualPlayerId = 0;
			List<long> playerIds = GetPlayerIdsFromSteamId(serverSteamId);
			if (playerIds.Count == 0)
			{
				serverVirtualPlayerId = BaseEntity.GenerateEntityId();
				if (serverVirtualPlayerId < 0)
					serverVirtualPlayerId = -serverVirtualPlayerId;

				MyObjectBuilder_Checkpoint.PlayerItem playerItem = new MyObjectBuilder_Checkpoint.PlayerItem();
				playerItem.Name = "Server";
				playerItem.IsDead = false;
				playerItem.Model = "";
				playerItem.PlayerId = serverVirtualPlayerId;
				playerItem.SteamId = serverSteamId;

				List<MyObjectBuilder_Checkpoint.PlayerItem> dummyList = new List<MyObjectBuilder_Checkpoint.PlayerItem>();
				dummyList.Add(playerItem);
				BaseObject.InvokeEntityMethod(BackingObject, PlayerMapAddPlayerItemMappingMethod, new object[] { dummyList });
			}
			else
			{
				serverVirtualPlayerId = playerIds[0];
			}

			return serverVirtualPlayerId;
		}
        */

        #endregion
    }

	public class PlayerManager
	{
		#region "Attributes"

		private static PlayerManager m_instance;
        private static Type m_internalType;

		//public static string PlayerManagerNamespace = "5F381EA9388E0A32A8C817841E192BE8";
		//public static string PlayerManagerClass = "08FBF1782D25BEBDA2070CAF8CE47D72";
        //public static string PlayerManagerPlayerMapField = "3F86E23829227B55C95CEB9F813578B2";

        public static string PlayerManagerNamespace = "5F381EA9388E0A32A8C817841E192BE8";
        public static string PlayerManagerClass = "0735AF1E7659DFDA65E92992C7ECBE13";
        public static string PlayerManagerPlayerMapField = "766455F8C87C6254FB177903415443F6";

        //5F381EA9388E0A32A8C817841E192BE8
        //0735AF1E7659DFDA65E92992C7ECBE13
        //766455F8C87C6254FB177903415443F6

		#endregion

		#region "Constructors and Initializers"

		protected PlayerManager()
		{
			m_instance = this;

			Console.WriteLine("Finished loading PlayerManager");
		}

		#endregion

		#region "Properties"

		public static PlayerManager Instance
		{
			get
			{
				if (m_instance == null)
					m_instance = new PlayerManager();

				return m_instance;
			}
		}

		public Object BackingObject
		{
			get
			{
				Object backingObject = WorldManager.Instance.InternalGetPlayerManager();

				return backingObject;
			}
		}

		public PlayerMap PlayerMap
		{
			get { return PlayerMap.Instance; }
		}

		public List<ulong> ConnectedPlayers
		{
			get
			{
				List<ulong> connectedPlayers = new List<ulong>(ServerNetworkManager.Instance.GetConnectedPlayers());

				return connectedPlayers;
			}
		}

		#endregion

		#region "Methods"

		public static bool ReflectionUnitTest()
		{
			try
			{
				Type type1 = SandboxGameAssemblyWrapper.Instance.GetAssemblyType(PlayerManagerNamespace, PlayerManagerClass);
				if (type1 == null)
					throw new Exception("Could not find internal type for PlayerManager");
				bool result = true;
                result &= BaseObject.HasMethod(type1, PlayerManagerPlayerMapField);

				return result;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return false;
			}
		}

		public Object InternalGetPlayerMap()
		{
            Type type = SandboxGameAssemblyWrapper.Instance.GetAssemblyType(PlayerManagerNamespace, PlayerManagerClass);
            MethodInfo playerMapHandler = BaseObject.GetStaticMethod(type, PlayerManagerPlayerMapField);
            Object playerMap = playerMapHandler.Invoke(type, new object[] { });
            
			return playerMap;
		}

		public void KickPlayer(ulong steamId)
		{
			ServerNetworkManager.Instance.KickPlayer(steamId);
		}

		public void BanPlayer(ulong steamId)
		{
			ServerNetworkManager.Instance.SetPlayerBan(steamId, true);
		}

		public void UnBanPlayer(ulong steamId)
		{
			ServerNetworkManager.Instance.SetPlayerBan(steamId, false);
		}

		public bool IsUserAdmin(ulong remoteUserId)
		{
			bool result = false;

			List<string> adminUsers = SandboxGameAssemblyWrapper.Instance.GetServerConfig().Administrators;
			foreach (string userId in adminUsers)
			{
				if (remoteUserId.ToString().Equals(userId))
				{
					result = true;
					break;
				}
			}

			return result;
		}

		#endregion
	}
}

