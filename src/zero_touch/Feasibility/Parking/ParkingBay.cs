using Autodesk.DesignScript.Geometry;
using DSCore;
using Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parking;
using Common;
using Parking.Accessories;

namespace Parking
{
    /// <summary>
    /// Wrapper class for the parking bay.
    /// </summary>
    public class ParkingBay
    {
        /// <summary>
        /// The target position of the parking bay.
        /// </summary>
        internal Point TargetPosition {  private get; set; }

        /// <summary>
        /// The width of the parking bay.
        /// </summary>
        public float BayWidth { get; set; }

        /// <summary>
        /// The length of the parking bay.
        /// </summary>
        public float BayLength { get; set; }

        /// <summary>
        /// The angle of the parking bay.
        /// </summary>
        public float BayAngle { get; set; }

        /// <summary>
        /// The number of the parking bay.
        /// </summary>
        public float BayNumber { get; set; }

        /// <summary>
        /// The parking bay rectangle geometry.
        /// </summary>
        internal Rectangle Geometry { get; private set; }

        /// <summary>
        /// Flip the parking bay horizontally.
        /// </summary>
        public Boolean FlipHorizontal { private get; set; }

        /// <summary>
        /// Flip the parking bay vertically.
        /// </summary>
        public Boolean FlipVertical { private get; set; }

        /// <summary>
        /// To set the parking type.
        /// </summary>
        public ParkingType ParkingType { private get; set; }

        /// <summary>
        /// The required parking accessory configuration.
        /// </summary>
        internal Accessories.Accessories Accessories { get; set; }


        /// <summary>
        /// Creates a parking bay instance.
        /// </summary>
        /// <param name="bayWidth">the width of the parking bay.</param>
        /// <param name="bayLength">the length of the parking bay.</param>
        /// <param name="bayAngle">the angle of the parking bay.</param>
        /// <param name="bayNumber">the number of the parking bay.</param>
        /// <param name="flipHorizontal">flip the parking bay horizontally.</param>
        /// <param name="flipVertical">flip the parking bay vertically</param>
        /// <param name="parkingType">The type of parking bay. Note that this parameter controls the displayed signage.</param>
        /// <param name="accessories">The required parking accessory configuration.</param>
        public ParkingBay(
            //Point targetPosition,
            //Point patternCenter,
            float bayWidth = (float)2.5,
            float bayLength = 5,
            float bayAngle = 30,
            float bayNumber = 10,
            bool flipHorizontal = false,
            bool flipVertical = false,
            ParkingType parkingType = ParkingType.EV,
            Accessories.Accessories accessories = null)
        { 
            //TargetPosition = targetPosition;
            BayWidth = bayWidth;
            BayLength = bayLength;
            BayAngle = bayAngle;
            BayNumber = bayNumber;
            Geometry = CreateRectangle();
            FlipHorizontal = flipHorizontal;
            FlipVertical = flipVertical;
            ParkingType = parkingType;
            Accessories = accessories;
        }


        /// <summary>
        /// To add the required rotation and mirror transformations to the parking bay.
        /// </summary>
        /// <param name="bayRectangle">The parking bay rectangle.</param>
        /// <returns name="transformedRectangle">The transformed parking bay rectangle.</returns>
        private Rectangle ParkingBayTransformations(Rectangle bayRectangle) 
        {
            // get the start point of the base rectangle.
            Point startPoint = bayRectangle.StartPoint as Point;

            // create plane at rectangle start point for rotation.
            Plane rotatePlane = Plane.ByOriginNormal(startPoint, Vector.ZAxis());

            // rotate the rectangle. 
            Rectangle rotatedRectangle = bayRectangle.Rotate(rotatePlane, BayAngle) as Rectangle;

            // get the coordinate system of the rotation plane.
            CoordinateSystem planeCS = CoordinateSystem.ByPlane(rotatePlane);

            // get the x and y axis of the rotation plane coordinate system.
            Vector coordx = planeCS.XAxis;
            Vector coordy = planeCS.YAxis;

            // create the vertical and horizontal mirror planes.
            Plane horizotalMirrorPlane = Plane.ByOriginNormal(startPoint, coordx);
            Plane verticalMirroPlane = Plane.ByOriginNormal(startPoint, coordy);

            // mirror the parking bay horizontally.
            Rectangle bayHorizontalMirror;

            if (FlipHorizontal == true)
            {
                bayHorizontalMirror = rotatedRectangle.Mirror(horizotalMirrorPlane) as Rectangle;
            }
            else
            {
                bayHorizontalMirror = rotatedRectangle;
            }

            // mirror the parking bay vertically.
            Rectangle bayVerticalMirror;

            if (FlipVertical == true)
            {
                bayVerticalMirror = bayHorizontalMirror.Mirror(verticalMirroPlane) as Rectangle;
            }
            else
            {
                bayVerticalMirror = bayHorizontalMirror;
            }

            // set a temporary target position if the input is null.
            Point _targetPosition = null;
            if (TargetPosition == null)
            {
                _targetPosition = Point.ByCoordinates(0, 0);
            }
            else 
            { 
                _targetPosition = TargetPosition;
            }

            // create a plane at the target position.
            Plane targetPlane = Plane.ByOriginNormal(_targetPosition, Vector.ZAxis());

            // get the coordinate system of the target plane.
            CoordinateSystem targetCS = CoordinateSystem.ByPlane(targetPlane);

            // transform the rectangle to the target plane.
            Rectangle transRectangle = bayVerticalMirror.Transform(planeCS, targetCS) as Rectangle;

            return transRectangle;
        }


        /// <summary>
        /// Creates the parking rectangle geometry.
        /// </summary>
        /// <returns name="parkingRectangle">The parking rectangle geometry.</returns>
        public Rectangle CreateRectangle() 
        {
            // create the base rectangle.
            Rectangle baseRectangle = Rectangle.ByWidthLength(BayLength, BayWidth);

            // add the transformations to the base rectangle.
            Rectangle transformedRectangle = ParkingBayTransformations(baseRectangle);;

            return transformedRectangle;
        }


        /// <summary>
        /// Creates the elongated parking rectangle for cutting the island surface.
        /// </summary>
        /// <returns></returns>
        public Rectangle CreateElongatedRectangle()
        {
            // calculate the additional length to extend the parking bay.
            float additionalLength = (float)(BayWidth * DSCore.Math.Tan(BayAngle));

            // create the parking rectangle.
            Rectangle baseRectangle = Rectangle.ByWidthLength(BayLength + additionalLength, BayWidth);

            // add the transformations to the rectangle.
            Rectangle transformedRectangle = ParkingBayTransformations(baseRectangle);

            return transformedRectangle;
        }


        /// <summary>
        /// Creates the parking stripe surface geometry.
        /// </summary>
        /// <param name="stripeThickness">The thickness of the parking stripe.</param>
        /// <param name="stripeOpeningWidth">The opening width of the parking stripe.</param>
        /// <returns name="parkingStripeSurface">The parking stripe surface.</returns>
        public Surface CreateStripeSurface(float stripeThickness=(float)0.1, float stripeOpeningWidth = (float)1.8) 
        {
            // create the parking rectangle. 
            Rectangle parkingRectangle = CreateRectangle();

            // try catch block to ensure internal surface is smaller than the parking surface.
            Surface subtractedSurface;
            try 
            {
                // offset the parking rectangle by the strip thickness.
                Curve[] stripeOffset = parkingRectangle.OffsetMany(-stripeThickness, Vector.ZAxis()) as Curve[];

                // join the offset curves.
                Curve joinedCurves = PolyCurve.ByJoinedCurves(stripeOffset, 0.01, false, 0) as Curve;

                // create the parking spot surface.
                Surface parkingSurface = Surface.ByPatch(parkingRectangle);

                // create the internal surface for subtraction from parking surface.
                List<Surface> internalSurface = new List<Surface> { Surface.ByPatch(joinedCurves) };

                // subtract the internal surface from the parking surface.
                subtractedSurface = parkingSurface.Difference(internalSurface);
            }
            catch 
            {
                // offset the parking rectangle by the strip thickness.
                Curve[] stripeOffset = parkingRectangle.OffsetMany(stripeThickness, Vector.ZAxis()) as Curve[];

                // join the offset curves.
                Curve joinedCurves = PolyCurve.ByJoinedCurves(stripeOffset, 0.01, false, 0) as Curve;

                // create the parking spot surface.
                Surface parkingSurface = Surface.ByPatch(parkingRectangle);

                // create the internal surface for subtraction from parking surface.
                List<Surface> internalSurface = new List<Surface> { Surface.ByPatch(joinedCurves) };

                // subtract the internal surface from the parking surface.
                subtractedSurface = parkingSurface.Difference(internalSurface);
            }

            // get the center of the parking spot.
            Point parkingCenter = parkingRectangle.Center();

            // create a plane at the center point.
            Plane centerPlane = Plane.ByOriginNormal(parkingCenter, Vector.ZAxis());

            // create the parking stripe entry cut rectangle.
            Rectangle entryRectangle = Rectangle.ByWidthLength(centerPlane, stripeOpeningWidth, stripeThickness * 3);

            // rotate the entry cut rectangle.
            Rectangle rotateRectangle = entryRectangle.Rotate(centerPlane, -GetRotationAngle()) as Rectangle;

            // move the entry rectangle to the parking bay entrance.
            Rectangle moveRectangle = rotateRectangle.Translate(GetParkingDirection(), parkingRectangle.Width/2) as Rectangle;

            // create a surface from the entry rectangle for subtraction.
            List<Surface> entrySurface = new List<Surface> { Surface.ByPatch(moveRectangle) };

            // subtract the entry rectangle from the subtracted surface.
            Surface stripeSurface = subtractedSurface.Difference(entrySurface);

            return stripeSurface;
        }


        /// <summary>
        /// Creates the parking stripe outline curve geometry.
        /// </summary>
        /// <param name="stripeThickness">The thickness of the parking stripe.</param>
        /// <param name="stripeOpeningWidth">The opening width of the parking stripe.</param>
        /// <returns name="parkingStripeOutline">The parking stripe outline.</returns>
        public PolyCurve CreateStripeOutline(float stripeThickness = (float)0.1, float stripeOpeningWidth = (float)1.8) 
        {
            // create the parking stripe surface.
            Surface stripeSurface = CreateStripeSurface(stripeThickness, stripeOpeningWidth);

            // get the perimeter curve of the stripe surface.
            PolyCurve stripeCurve = PolyCurve.ByJoinedCurves(stripeSurface.PerimeterCurves(), 0.001, false, 0);

            return stripeCurve;
        }


        /// <summary>
        /// Get the vector along the length of the parking spots.
        /// </summary>
        /// <returns name="lengthVector">vector along the length of the parking bay.</returns>
        public Vector GetParkingDirection() 
        {
            // get the parking bay rectangle.
            Rectangle parkingRectangle = CreateRectangle();

            // explode the parking rectangle to get an array of geometry.
            Geometry[] rectangleGeometries = parkingRectangle.Explode();

            // convert the array of Geometry to a list of Line.
            List<Line> rectangleLines = rectangleGeometries.OfType<Line>().ToList();

            // get the vector of a line at the length of the parking bay.
            Vector lengthVector = rectangleLines[1].Direction;

            return lengthVector;
        }


        /// <summary>
        /// Gets the rotation angle of the bays from the y axis.
        /// </summary>
        /// <returns name="rotationAngle"></returns>
        public float GetRotationAngle() 
        {
            // compute the rotation angle of the parking bay.
            float rotationAngle = (float)GetParkingDirection().AngleAboutAxis(Vector.YAxis(), Vector.ZAxis());

            return rotationAngle;
        }


        /// <summary>
        /// Gets the center points of the placed parking bays.
        /// </summary>
        /// <returns name="parkingCenter">the center point of the parking bay.</returns>
        public Point GetCenterPoint()
        {
            // get the parking bay rectangle.
            Rectangle parkingRectangle = CreateRectangle();

            // get the center of the parking bay.
            Point parkingCenter = parkingRectangle.Center();

            return parkingCenter;
        }


        /// <summary>
        /// Adds a signage location point to the parking bay.
        /// </summary>
        /// <param name="centerOffsetPercentage">The offset distance percentage in proportion to half the parking bay length.</param>
        /// <returns name="signageOutline">The signage outline curves.</returns>
        public List<Curve[]> AddSignageOutline(float centerOffsetPercentage = 50) 
        {
            // add the parking bay center point.
            Point parkingCenter = GetCenterPoint();

            // calculate the signage center offset distance. 
            float centerOffset = ((BayLength/2) / 100) * centerOffsetPercentage;

            // move the point to locate the signage.
            Point movedPoint = parkingCenter.Translate(GetParkingDirection(), centerOffset) as Point;

            // add a plane at the moved point.
            Plane plane = Plane.ByOriginNormal(movedPoint, Vector.ZAxis());

            // get the signage resource name.
            string resourcename = SignageResources.ResourceMap[ParkingType];

            // add the signage required signage type to the plane.  
            List<Curve[]> signageOutline = Parking.Signage.ParkingSymbol2D(
                plane,
                -GetRotationAngle() - 180,
                (float)0.01,
                (float)2.5,
                BayWidth,
                resourcename);

            return signageOutline;
        }


        /// <summary>
        /// Adds the parking numbering to the parking bay.
        /// </summary>
        /// <param name="numberPrefix"></param>
        /// <param name="numberingDiameter"></param>
        /// <param name="numberBorderOffset"></param>
        /// <param name="centerOffsetPercentage"></param>
        /// <returns name="numberingCurves"></returns>
        public List<Curve> AddNumberingOutline(
            string numberPrefix = "",
            float numberingDiameter = 1f,
            float numberBorderOffset = 0.1f,
            float centerOffsetPercentage = 50) 
        {
            // Get the center point of the parking bay.
            Point parkingCenter = GetCenterPoint();

            // Calculate the signage center offset distance. 
            float centerOffset = ((BayLength / 2) / 100) * centerOffsetPercentage;

            // Move the point to locate the numbering.
            Point movedPoint = parkingCenter.Translate(GetParkingDirection().Reverse(), centerOffset) as Point;

            // Add a plane at the moved point.
            Plane plane = Plane.ByOriginNormal(movedPoint, Vector.ZAxis()) as Plane;

            // Add the text at the plane.
            Dictionary<string, object> text = Text.ByStringPlaneAndScale(
                $"{numberPrefix}{BayNumber}",
                1,
                plane,
                -GetRotationAngle() - 180,
                0,
                0.6,
                "Arial",
                "Normal",
                "Normal"
            );

            // Get the text surfaces.
            List<Geometry> textSurfaces = text["textSurfaces"] as List<Geometry>;

            // cast the text surface geometry to surfaces.
            List<Surface> castSurfaces = new List<Surface>();  
            foreach (var surface in textSurfaces) 
            {
                castSurfaces.Add(surface as Surface);
            }

            // Join the surfaces and get the bounding box.
            PolySurface joinedSurfaces = PolySurface.ByJoinedSurfaces(castSurfaces);
            BoundingBox boundingBox = joinedSurfaces.BoundingBox;

            // Get the length of the bounding box diagonal line.
            Curve diagonalCurve = Line.ByStartPointEndPoint(boundingBox.MinPoint, boundingBox.MaxPoint) as Curve;
            float curveLength = (float)diagonalCurve.Length;

            // Calculate text scale factor.
            float textScaleFactor = (numberingDiameter - (numberBorderOffset * 2)) / curveLength;

            // Get the text polycurves.
            List<Geometry> textPolyCurves = text["textPolyCurves"] as List<Geometry>;

            // Cast the text polycurves to curves.
            List<Curve> castCurves = new List<Curve>();
            foreach (Geometry curve in textPolyCurves)
            {
                castCurves.Add(curve as  Curve);
            }

            // Scale the polycurves using the scale factor.
            List<Curve> scaledCurves = new List<Curve>();
            foreach(Curve curve in castCurves) 
            {
                scaledCurves.Add(curve.Scale(plane, textScaleFactor, textScaleFactor, textScaleFactor) as Curve);
            }

            // Create the circular border of the numbering.
            Curve border = Circle.ByCenterPointRadius(movedPoint, numberingDiameter / 2) as Curve;

            // Add all the curves to a list.
            List<Curve> numberingCurves = new List<Curve>();
            numberingCurves.Add(border);
            foreach (Curve curve in scaledCurves) 
            { 
                numberingCurves.Add(curve);
            }

            return numberingCurves;
        }


        public object CreateBicycleRack() 
        {
            object rackSolid;
            if (Accessories.BicycleRack.BicycleRackType == BicycleRackTypes.NoRack)
            {
                rackSolid = "No rack required";
            }
            else 
            {
                throw new Exception("This rack type has not been specified.");
            }

            return rackSolid;
        }
    }
}
