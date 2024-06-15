using Autodesk.DesignScript.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking
{
    /// <summary>
    /// Wrapper class for the internal parking bays.
    /// </summary>
    public class InternalBays
    {
        private Surface _layoutArea;

        /// <summary>
        /// A horizontal planar surface representing the parking area.
        /// </summary>
        public Surface LayoutArea 
        {
            private get { return _layoutArea; }
            set 
            {
                Point minPoint = value.BoundingBox.MinPoint;
                Point maxPoint = value.BoundingBox.MaxPoint;

                if (maxPoint.Z > minPoint.Z) 
                {
                    throw new ArgumentOutOfRangeException(nameof(_layoutArea), "The layout surface must be horizontal and planar.");
                }
                _layoutArea = value;
            }
        }


        private Surface _exclusionArea;


        /// <summary>
        /// Horizontal planar surfaces representing the exlusion zones.
        /// </summary>
        public Surface ExclusionArea 
        {
            private get { return _exclusionArea; }
            set 
            {
                Point minPoint = value.BoundingBox.MinPoint;
                Point maxPoint = value.BoundingBox.MaxPoint;

                if (maxPoint.Z > minPoint.Z)
                {
                    throw new ArgumentOutOfRangeException(nameof(_exclusionArea), "The exclusion area surface must be horizontal and planar.");
                }
                _exclusionArea = value;
            }
        }


        private int _patternType;

        /// <summary>
        /// Set the parking pattern type.
        /// 1 for the non interlocking pattern.
        /// 2 for the interlocking pattern.
        /// 3 for the herringbone pattern.
        /// </summary>
        public int PatternType
        {
            private get { return _patternType; }
            set
            {
                if (value < 1 || value > 3)
                {
                    throw new ArgumentOutOfRangeException(nameof(PatternType), "PatternType must be between 1 an 3.");
                }
                _patternType = value;
            }
        }


        private int _aisleType;

        /// <summary>
        /// Set the internal parking aisle type.
        /// 1 for one way traffic.
        /// 2 for two way traffic.
        /// </summary>
        public int AisleType 
        {
            private get { return _aisleType; }
            set 
            {
                if (value < 1 || value > 2) 
                { 
                    throw new ArgumentOutOfRangeException(nameof(AisleType), "AisleType must be 1 or 2.");
                }
            }
        }
    }
}
