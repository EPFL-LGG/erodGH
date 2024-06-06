using System;
using Rhino.Geometry;

namespace ErodModelLib.Types
{
	public class Support
	{
		public Point3d InitialPosition { get; private set; }
        public Point3d Position { get; set; }
        public Point3d TargetPosition { get; private set; }
        public bool IsTemporary { get; private set; }
		public int[] LockedDoFs { get; private set; }

        public Support(Point3d position, int[] lockedDoFs, bool isTemporary=false, Point3d target=default)
		{
			InitialPosition = position;
			Position = position;
			TargetPosition = target==default || target==Point3d.Unset ? position : target;
			IsTemporary = isTemporary;
            LockedDoFs = lockedDoFs;
		}
	}
}

