using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Parking;
using ProtoCore.AST.ImperativeAST;

namespace Parking
{
    /// <summary>
    /// Wrapper class for the parking patterns.
    /// </summary>
    [IsVisibleInDynamoLibrary(true)]
    public class Patterns
    {   
        /// <summary>
        /// The input line indicating the location of the pattern.
        /// </summary>
        public Line LocationLine {  private get; set; }

        private int _patternType;

        /// <summary>
        /// Set the parking pattern type.
        /// 1 for the non interlocking pattern.
        /// 2 for the interlocking pattern.
        /// 3 for the herringbone pattern.
        /// </summary>
        public int PatternType
        {
            get { return _patternType; }
            set
            {
                if (value < 1 || value > 3)
                {
                    throw new ArgumentOutOfRangeException(nameof(PatternType), "PatternType must be between 1 an 3.");
                }
                _patternType = value;
            }
        }

        /// <summary>
        /// The width of the parking bays.
        /// </summary>
        public float BayWidth { private get; set; }

        /// <summary>
        /// Determines if the bay width is as per the input width or adjusted to fit the location line.
        /// </summary>
        public bool AdjustBayWidth { private get; set; }

        /// <summary>
        /// The length of the parking bays.
        /// </summary>
        public float BayLength { private get; set; }

        /// <summary>
        /// The angle of the parking bays.
        /// Note that the angle is not applied to the herringbone pattern.
        /// The herringbone pattern is always 45 degrees.
        /// </summary>
        public float BayAngle { get; set; }

        /// <summary>
        /// The rotation angle of the pattern.
        /// This will be overridden by the internal layout's rotation value(s).
        /// </summary>
        public float PatternRotation { private get; set; }

        /// <summary>
        /// The width of the non interlocking pattern island.
        /// </summary>
        public float IslandWidth { private get; set; }

        /// <summary>
        /// To set the parking signage type.
        /// </summary>
        public SignageType Signage { private get; set; }


        /// <summary>
        /// Creates instances of the parking pattern class.
        /// </summary>
        /// <param name="locationLine">The input line indicating the location of the pattern.</param>
        /// <param name="patternType">Set the parking pattern type.</param>
        /// <param name="bayWidth">the width of the parking bays.</param>
        /// <param name="adjustBayWidth">Adjust the parking bay width to fit the location line?</param>
        /// <param name="bayLength">the length of the parking bays.</param>
        /// <param name="bayAngle">the angle of the parking bays.</param>
        /// <param name="patternRotation">The rotation angle of the pattern.</param>
        /// <param name="islandWidth">The width of the non interlocking pattern island.</param>
        /// <param name="signage">The signage type to be placed on the parking bay.</param>
        [IsVisibleInDynamoLibrary(true)]
        public Patterns(
            Line locationLine,
            int patternType = 1,
            float bayWidth = (float)2.5,
            bool adjustBayWidth = true,
            float bayLength = 5,
            float bayAngle = 30,
            float patternRotation = 0,
            float islandWidth = 1,
            SignageType signage = SignageType.EV) 
        { 
            LocationLine = locationLine;
            PatternType = patternType;
            BayWidth = bayWidth;
            AdjustBayWidth = adjustBayWidth;
            BayLength = bayLength;
            BayAngle = bayAngle;
            PatternRotation = patternRotation;
            IslandWidth = islandWidth;
            Signage = signage;
        }


        /// <summary>
        /// Get the required bay angle based on the selected parking pattern.
        /// Since the herringbone pattern must always be 45 degrees.
        /// </summary>
        /// <returns name="requiredAngle"></returns>
        private float RequiredBayAngle()
        {
            // make the bay angle 45 degrees if the pattern type is herringbone.
            float bayAngle;
            if (PatternType == 3)
            {
                bayAngle = 45;
            }
            else
            {
                bayAngle = BayAngle;
            }

            return bayAngle;
        }


        /// <summary>
        /// The width of the parking bay against the location line.
        /// </summary>
        /// <returns name="actualWidth">The parking bay width against the location line.</returns>
        private float BayLocationLineWidth() 
        {
            // get the parking bay angle..
            float bayAngle = RequiredBayAngle();
            
            // calculate the actual bay width against the pattern location line.
            float actualWidth = (float)BayWidth / (float)DSCore.Math.Cos((float)bayAngle);

            return actualWidth;
        }


        /// <summary>
        /// Calculates the number of parking bays to copy along the location line.
        /// </summary>
        /// <returns></returns>
        private int ParkingCopyNumber() 
        {
            // calculate the number of bays to copy along the location line.
            int copyNumber;
            if (LocationLine.Length <= BayLocationLineWidth()) 
            {
                copyNumber = 1;
            }
            else 
            {
                if (AdjustBayWidth == true)
                {
                    copyNumber = (int)DSCore.Math.Floor(LocationLine.Length / BayLocationLineWidth());
                }
                else
                {
                    copyNumber = (int)DSCore.Math.Ceiling(LocationLine.Length / BayLocationLineWidth());
                }
            }
             
            return copyNumber;
        }


        /// <summary>
        /// Calculate the actual required parking width based on the length of the location line. 
        /// </summary>
        /// <returns name="actualBayWidth">The actual width of the parking bays.</returns>
        private float ActualBayWidth()
        {
            float actualBayWidth;
            if (LocationLine.Length <= BayLocationLineWidth()) 
            {
                actualBayWidth = (float)LocationLine.Length;
            }
            else 
            {
                if (AdjustBayWidth == true)
                {
                    // divide the location line by the copy number.
                    float actualLocationLineWidth = (float)LocationLine.Length / ParkingCopyNumber();

                    actualBayWidth = (float)DSCore.Math.Cos(RequiredBayAngle()) * actualLocationLineWidth;
                }
                else
                {
                    actualBayWidth = (float)DSCore.Math.Cos(RequiredBayAngle()) * BayLocationLineWidth();
                }
            }
            
            return actualBayWidth;    
        }


        /// <summary>
        /// Creates half of the parking pattern points.
        /// </summary>
        /// <param name="patternOffset">The offset distance of the pattern points from the location line.</param>
        /// <returns name="locationPoints">A list of points to host parking bays.</returns>
        private List<Point> HalfPoints(float patternOffset = 1) 
        {
            // make the bay angle 45 degrees if the pattern type is herringbone.
            float bayAngle = RequiredBayAngle();

            // get the location line start coordinate system.
            CoordinateSystem lineCoord = LocationLine.CoordinateSystemAtParameter(0);

            // get the x vector of the coordinate system.
            Vector coordVector = lineCoord.XAxis.Reverse();

            // extend the location line if required to ensure the bays fit accurately.
            Line extendedLine;
            if (AdjustBayWidth == true) 
            {
                extendedLine = LocationLine;
            }
            else 
            {
                float newLineLength = ((float)BayWidth / (float)DSCore.Math.Cos((float)bayAngle)) * ParkingCopyNumber();
                float extensionLength = (float)newLineLength - (float)LocationLine.Length;
                extendedLine = LocationLine.ExtendEnd(extensionLength) as Line;
            }
            
            // move the location line to offset the bays in relation to the location line.
            Line movedLine = extendedLine.Translate(coordVector, patternOffset) as Line;

            // add the parking bay location points to the moved line.
            List<Point> locationPoints = new List<Point>();
            if (LocationLine.Length <= BayLocationLineWidth()) 
            { 
                Point point = movedLine.PointAtParameter(1);
                locationPoints.Add(point);
            }
            else 
            {
                foreach (float number in Common.Math.Range(0, 1, ParkingCopyNumber() + 1))
                {
                    Point point = movedLine.PointAtParameter(number);
                    locationPoints.Add(point);
                }
            }

            // remove the last items from the location points list.
            if (locationPoints.Count > 0) 
            {
                locationPoints.RemoveAt(0);
            }
            
            return locationPoints;
        }


        /// <summary>
        /// Gets the rotation angle of the location line from the y axis.
        /// </summary>
        /// <returns name="rotationAngle"></returns>
        private float GetLineRotationAngle()
        {
            // get the direction of the location line.
            Vector lineDirection = LocationLine.Direction;

            // compute the rotation angle of the parking bay.
            float rotationAngle = (float)lineDirection.AngleAboutAxis(Vector.YAxis(), Vector.ZAxis());

            return rotationAngle;
        }


        /// <summary>
        /// Creates a mirror plane at the pattern location line.
        /// </summary>
        /// <returns name="mirrorPlane">The location line mirror plane.</returns>
        private Plane LocationLineMirrorPlane() 
        {
            // create the mirror plane to mirror the target points along the location line.
            Point lineStartPoint = LocationLine.StartPoint;
            CoordinateSystem lineCoord = LocationLine.CoordinateSystemAtParameter(0);
            Vector coordVector = lineCoord.XAxis;
            Plane mirrorPlane = Plane.ByOriginNormal(lineStartPoint, coordVector);

            return mirrorPlane;
        }


        /// <summary>
        /// Creates the non interlocking parking pattern.
        /// </summary>
        /// <returns name="parkingBays">The parking bay instances.</returns>
        private List<List<ParkingBay>> NonInterlockingPattern() 
        {
            // create the first half of the parking bay target points.
            List<Point> locationPoints = HalfPoints(IslandWidth / 2);

            // create the mirror plane to mirror the target points along the location line.
            Plane mirrorPlane = LocationLineMirrorPlane();

            // create the second half of the parking bay target points.
            List<Point> secondLocationPoints = new List<Point>();
            foreach (Point point in locationPoints) 
            { 
                Point mirrorPoint = point.Mirror(mirrorPlane) as Point;
                secondLocationPoints.Add(mirrorPoint);
            }

            // get the center of the location line.
            Point locationCenter = LocationLine.PointAtParameter(0.5);

            // add the parking bay instances to the first half target points.
            List<ParkingBay> firstParkingBays = new List<ParkingBay>();  
            foreach (Point point in locationPoints) 
            {
                ParkingBay bay = new ParkingBay(
                    point, 
                    locationCenter,
                    ActualBayWidth(), 
                    BayLength, 
                    BayAngle + GetLineRotationAngle(), 
                    PatternRotation,
                    false,
                    true,
                    Signage);
                firstParkingBays.Add(bay);
            }

            // add the parking bay instances to the second half target points.
            List<ParkingBay> secondParkingBays = new List<ParkingBay>();
            foreach (Point point in secondLocationPoints)
            {
                ParkingBay bay = new ParkingBay(
                    point,
                    locationCenter,
                    ActualBayWidth(),
                    BayLength,
                    BayAngle + GetLineRotationAngle(),
                    PatternRotation,
                    true,
                    true,
                    Signage);
                secondParkingBays.Add(bay);
            }

            // add the lists of parking bays to a single list.
            List<List<ParkingBay>> parkingBays = new List<List<ParkingBay>>();
            parkingBays.Add(firstParkingBays);
            parkingBays.Add(secondParkingBays);

            return parkingBays;
        }


        /// <summary>
        /// Creates the interlocking pattern.
        /// </summary>
        /// <returns name="parkingBays">The parking bay instances.</returns>
        private List<List<ParkingBay>> InterlockingPattern() 
        {
            // create the first half of the parking bay target points.
            List<Point> locationPoints = HalfPoints(-(float)(ActualBayWidth() * DSCore.Math.Sin(BayAngle)) / 2);

            // create the mirror plane to mirror the target points along the location line.
            Plane mirrorPlane = LocationLineMirrorPlane();

            // create the second half of the parking bay target points.
            List<Point> secondLocationPoints = new List<Point>();
            foreach (Point point in locationPoints)
            {
                Point mirrorPoint = point.Mirror(mirrorPlane) as Point;
                secondLocationPoints.Add(mirrorPoint);
            }

            // get the direction of the location line.
            Vector locationLineDir = LocationLine.Direction;

            // move the mirrored points along the pattern location line.
            float moveDistance = (float)(-(DSCore.Math.Sin(90 - BayAngle) * ActualBayWidth()));
            List<Point> movedPoints = new List<Point>();
            foreach (Point point in secondLocationPoints) 
            { 
                Point movedPoint = point.Translate(locationLineDir, moveDistance) as Point;
                movedPoints.Add(movedPoint);
            }

            // get the center of the location line.
            Point locationCenter = LocationLine.PointAtParameter(0.5);

            // add the parking bay instances to the first half target points.
            List<ParkingBay> firstParkingBays = new List<ParkingBay>();
            foreach (Point point in locationPoints)
            {
                ParkingBay bay = new ParkingBay(
                    point,
                    locationCenter,
                    ActualBayWidth(),
                    BayLength,
                    BayAngle + GetLineRotationAngle(),
                    PatternRotation,
                    false,
                    true,
                    Signage);
                firstParkingBays.Add(bay);
            }

            // add the parking bay instances to the second half target points.
            List<ParkingBay> secondParkingBays = new List<ParkingBay>();
            foreach (Point point in movedPoints)
            {
                ParkingBay bay = new ParkingBay(
                    point,
                    locationCenter,
                    ActualBayWidth(),
                    BayLength,
                    BayAngle + GetLineRotationAngle(),
                    PatternRotation,
                    true,
                    false,
                    Signage);
                secondParkingBays.Add(bay);
            }

            // add the lists of parking bays to a single list.
            List<List<ParkingBay>> parkingBays = new List<List<ParkingBay>>();
            parkingBays.Add(firstParkingBays);
            parkingBays.Add(secondParkingBays);

            return parkingBays;
        }


        /// <summary>
        /// Creates the herringbone pattern.
        /// </summary>
        /// <returns name="parkingBays">The parking bay instances.</returns>
        private List<List<ParkingBay>> HerringbonePattern() 
        {
            // create the first half of the parking bay target points.
            List<Point> locationPoints = HalfPoints(-(float)(ActualBayWidth() * DSCore.Math.Sin(45)) / 2);

            // create the mirror plane to mirror the target points along the location line.
            Plane mirrorPlane = LocationLineMirrorPlane();

            // create the second half of the parking bay target points.
            List<Point> secondLocationPoints = new List<Point>();
            foreach (Point point in locationPoints)
            {
                Point mirrorPoint = point.Mirror(mirrorPlane) as Point;
                secondLocationPoints.Add(mirrorPoint);
            } 

            // get the direction of the location line.
            Vector locationLineDir = LocationLine.Direction;

            // move the mirrored points along the pattern location line.
            float moveDistance = (float)(-(DSCore.Math.Sin(45) * ActualBayWidth()));
            List<Point> movedPoints = new List<Point>();
            foreach (Point point in secondLocationPoints)
            {
                Point movedPoint = point.Translate(locationLineDir, moveDistance) as Point;
                movedPoints.Add(movedPoint);
            }

            // get the center of the location line.
            Point locationCenter = LocationLine.PointAtParameter(0.5);

            // add the parking bay instances to the first half target points.
            List<ParkingBay> firstParkingBays = new List<ParkingBay>();
            foreach (Point point in locationPoints)
            {
                ParkingBay bay = new ParkingBay(
                    point,
                    locationCenter,
                    ActualBayWidth(),
                    BayLength,
                    45 + GetLineRotationAngle(),
                    PatternRotation,
                    false,
                    true,
                    Signage);
                firstParkingBays.Add(bay);
            }


            // add the parking bay instances to the second half target points.
            List<ParkingBay> secondParkingBays = new List<ParkingBay>();
            foreach (Point point in movedPoints)
            {
                ParkingBay bay = new ParkingBay(
                    point,
                    locationCenter,
                    ActualBayWidth(),
                    BayLength,
                    45 + GetLineRotationAngle(),
                    PatternRotation,
                    true,
                    true,
                    Signage);
                secondParkingBays.Add(bay);
            }

            // add the lists of parking bays to a single list.
            List<List<ParkingBay>> parkingBays = new List<List<ParkingBay>>();
            parkingBays.Add(firstParkingBays);
            parkingBays.Add(secondParkingBays);

            return parkingBays;
        }


        /// <summary>
        /// Creates the parking bay instances in a pattern.
        /// </summary>
        /// <returns name="parkingBays">The parking bay instances.</returns>
        public List<List<ParkingBay>> CreateParkingBays()
        {
            // add logic to switch between the parking patterns as required.
            List<List<ParkingBay>> parkingBays;
            if (PatternType == 1)
            {
                parkingBays = NonInterlockingPattern();
            }
            else if (PatternType == 2)
            {
                parkingBays = InterlockingPattern();
            }
            else 
            {
                parkingBays = HerringbonePattern();
            }

            return parkingBays;
        }


        /// <summary>
        /// Creates the extended parking bay rectangles for cutting the island surface.
        /// </summary>
        /// <returns name="extendedRectangles">The extended parking bay rectangles.</returns>
        public List<Rectangle> ExtendedRectangles()
        {
            // add the parking bays to a list.
            List<List<ParkingBay>> parkingBays = CreateParkingBays();

            // extend the parking bays.
            List<Rectangle> extendedRectangles = new List<Rectangle>();
            foreach (List<ParkingBay> bayList in parkingBays)
            {
                foreach (ParkingBay bay in bayList)
                {
                    extendedRectangles.Add(bay.CreateElongatedRectangle());
                }
            }

            return extendedRectangles;
        }


        /// <summary>
        /// Calculates the width of the patterns.
        /// </summary>
        /// <returns name="patternWidth">The width of the non interlocking pattern.</returns>
        public float PatternWidth
        {
            get
            {
                // calculate the overall pattern widths.
                float patternWidth;
                if (PatternType == 1)
                {
                    float width1 = (float)(ActualBayWidth() * DSCore.Math.Sin(BayAngle)); // closest triangle width to the center island.
                    float width2 = (float)(DSCore.Math.Cos(BayAngle) * BayLength); // furthermost trinagle wifth from the center island.
                    patternWidth = (float)((width1 + width2) * 2 + IslandWidth);
                }

                else if (PatternType == 2)
                {
                    float width1 = (float)(BayLength * DSCore.Math.Cos(BayAngle)); // width of the pattern from the center overlap zone.
                    float width2 = (float)(ActualBayWidth() * DSCore.Math.Sin(BayAngle) / 2);
                    patternWidth = (width1 + width2) * 2;
                }
                else
                {
                    float width1 = (float)(BayLength * DSCore.Math.Cos(45)); // width of the pattern from the center overlap zone.
                    float width2 = (float)(ActualBayWidth() * DSCore.Math.Sin(45) / 2);
                    patternWidth = (width1 + width2) * 2;
                }

                return patternWidth;
            }
        }


        /// <summary>
        /// Creates a rectangle covering the width of the pattern and length of the location line.
        /// </summary>
        /// <returns name="patternSurface">The pattern surface.</returns>
        public Surface PatternIslandSurface()
        {
            // create the surface rectangle.
            Plane centerPlane = Plane.ByOriginNormal(LocationLine.PointAtParameter(0.5), Vector.ZAxis());
            Rectangle surfaceRectangle = Rectangle.ByWidthLength(centerPlane, PatternWidth, LocationLine.Length);

            // rotate the rectangle.
            Rectangle rotatedRectangle = surfaceRectangle.Rotate(centerPlane, PatternRotation) as Rectangle;

            // create the surface.
            Surface patternSurface = Surface.ByPatch(rotatedRectangle);

            return patternSurface;
        }
    }
}
