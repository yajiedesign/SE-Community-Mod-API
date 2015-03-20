using SEModAPIInternal.API.Entity.Sector.SectorObject.CubeGrid;

namespace SEModAPIExtensions.API.Plugin.Events
{
	public interface ICubeBlockEventHandler
	{
		void OnCubeBlockCreated(CubeBlockEntity entity);
		void OnCubeBlockDeleted(CubeBlockEntity entity);
	}
}
