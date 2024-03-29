# Load the Python Standard and DesignScript Libraries
import sys
import clr
clr.AddReference('ProtoGeometry')
from Autodesk.DesignScript.Geometry import *

clr.AddReference('GeometryColor')
from Modifiers import GeometryColor

clr.AddReference('DSCoreNodes')
from DSCore import *

sys.path.append(r'F:\02_Onile\05_Operations\Software and App Development\Feasibility\src\dynamo\python_scripts')
from common_tools import listtools 
from common_tools.geometrytools import sweep_profile as sp

from math import atan, degrees

# Inputs.
building_width = IN[0]
building_length = IN[1]
floor_height = IN[2]
floor_number = IN[3]
roof_height = IN[4]
plane = IN[5]
custom_path = IN[6]
profile = IN[7]


class Building: 
    """
    Represents a building with various sub classes, methods, and attributes.
    
    Subclasses:
        BuildingShapes: Defines various profiles, paths, and geometry for creating various 
                        required building types.        
        BuildingTypesPrimitive: Represents primitive building types.
        BuildingTypesComplex: Represents complex building types.
        BuildingPatterns: Defines patterns for arranging buildings.
        
    Attributes:
        name (str): The name of the building.
    """
  
    
    def __init__(
               self, 
               name = "default",
    ):
        """
        Initializes a new Building instance.
        
        Args:
            name (str): The name of the building.
        """
        
        # Raise errors if inputs are entered incorrectly.
        if not isinstance(name, str):
            raise TypeError("Name must be a string")
            
            
        self.name = name


class BuildingShapes(Building):
    """
    Defines various profiles, paths, and geometry for creating various 
    required building types.
    
    Subclasses:
        BasePaths: Defines generic sweep paths for the building base.
        BaseProfiles: Defines vertical generic profiles for the building base to be swept along the 
                      base paths.
        RoofProfiles: Defines various roof profile shapes.
        BaseShapes: Defines various generic solids for the building base. 
        RoofShapes: Defines various generic roof solid shapes. 
    """ 
    
    def __init__(self, name = "default"):
        """
        Initialize a new building shape instance.
        
        Args:
            name (str): The name of the building shape.
        """  
        
        super().__init__(name)
             
        
class BasePaths(BuildingShapes):
    """
    Defines horizontal generic sweep paths for the building base.
    
    Attributes:
        name (str): The name of the path.
        rotation (float): The rotation of the path around the z-axis.
        plane: The base horizontal plane of the path.
        path_placement (str): The placement location of the path on the
                              base plane.
        length (float): The length of the path.
        width (float): The width of the path.
        path_length (float): The full perimeter length of the path.
        area (float): The area of the path.
        
    Methods:
        square_path: Creates a square path.
        c_path: Creates a C-shaped path.
        straight_path: Creates a straight path.
    """
    
    def __init__(
                self,
                name = "default",
                rotation = 0,
                plane = Plane.XY(),
                path_placement = "center",
    ):
        """
        Initialize a new base path instance.
        
        
        Args:
            name (str): The name of the path.
            rotation (float): The rotation of the path around the z axis.
            plane: The base horizontal plane of the path.
            path_placement (str): To indicate the placement location of the
                                  path on the base plane. "center" of the 
                                  path's bounding box or "start" of the path's curve.     
        """
        
        # Raise errors if inputs are entered incorrectly.
        if not isinstance(plane, Plane):
            raise TypeError("The plane input must be a plane object")
        if not plane.Normal.Z == 1:
            raise TypeError("The building plane must be horizontal")
        if not isinstance(rotation, (int, float)):
            raise TypeError("Rotation value must be a number")
        if path_placement != "center" and path_placement != "start":
            raise AttributeError("Path placement must be 'center' or 'start'")
        
        super().__init__(name)
        
        self.rotation = rotation
        self.plane = plane
        self.path_placement = path_placement
        self.length = 0
        self.width = 0
        self.path_length = 0
        self.area = 0
       
        
    def square_path(self, width=5, length=10):
        """
        Creates a square path.
        
        Args:
            width (float): The width of the path.
            length (float): The length of the path.
        """
        
        # Raise errors if inputs are entered incorrectly.
        if not isinstance(width, (int, float)):
            raise TypeError("Width must be a number")
        if not isinstance(length, (int, float)):
            raise TypeError("Length must be a number") 
            
        # Create the path rectangle.
        base_rectangle = Rectangle.ByWidthLength(
            self.plane, 
            width, 
            length
        )
        
        # Allow for the selection of a center or start path placement point.
        if self.path_placement == "center":
            base_rectangle = base_rectangle
        elif self.path_placement == "start":
            # Transform the path.
            start_point = base_rectangle.PointAtParameter(0)
            start_plane = Plane.ByOriginNormal(start_point, Vector.ZAxis())
            base_rectangle = base_rectangle.Transform(
                                                  start_plane.ToCoordinateSystem(),
                                                  self.plane.ToCoordinateSystem()
            )
            
        # Rotate the path rectangle.
        rotated_rectangle = Geometry.Rotate(
            base_rectangle,
            self.plane,
            self.rotation
        )
        
        # Update the length attribute.
        self.length = length
        
        # Update the width attribute.
        self.width = width
        
        # Update the path length attribute.
        self.path_length = rotated_rectangle.Length
        
        # Update the area attribute.
        self.area = Surface.ByPatch(rotated_rectangle).Area
            
        return rotated_rectangle

    
    def c_path(self, width=5, length=10):
        """
        Creates a C-shaped path.
        
        Args:
            width (float): The width of the path.
            length (float): The length of the path.
        """
        
        # Raise errors if inputs are entered incorrectly.
        if not isinstance(width, (int, float)):
            raise TypeError("Width must be a number")
        if not isinstance(length, (int, float)):
            raise TypeError("Length must be a number")
            
        # Create the path rectangle.
        base_rectangle = self.square_path(
            width,
            length
        )
        
        # Explode the base rectangle.
        exploded_rectangle = listtools.list_flatten(base_rectangle.Explode())
        
        # Create the c path.
        c_path = PolyCurve.ByJoinedCurves(exploded_rectangle[1:])
        
        # Update the length attribute.
        self.length = length
        
        # Update the width attribute.
        self.width = width
        
        # Update the path length attribute.
        self.path_length = c_path.Length
        
        # Update the area attribute.
        self.area = 0
        
        # Return a polycurve.
        return c_path
        
    
    def straight_path(self, length=10):
        """
        Creates a straight path.
        
        Args:
            length (float): The length of the path.
        """
        
        # Raise errors if inputs are entered incorrectly.
        if not isinstance(length, (int, float)):
            raise TypeError("Length must be a number")
      
        # Create the path line.
        path_line = Line.ByStartPointDirectionLength(
                Point.ByCoordinates(),
                Vector.YAxis(),
                length
        )
        
        # Create a plane on the path for transformation.
        if self.path_placement == "center":
            path_plane = Plane.ByOriginNormal(
            path_line.PointAtParameter(0.5),
            Vector.ZAxis()
        )
        elif self.path_placement == "start":
            path_plane = Plane.ByOriginNormal(
            path_line.PointAtParameter(0),
            Vector.ZAxis()
        )
        
        # Get the coordinate systems for the transformation.
        from_coord = CoordinateSystem.ByPlane(self.plane)
        to_coord = CoordinateSystem.ByPlane(path_plane)
        
        # Transform the path line to the input plane.
        transformed_path = path_line.Transform(
                to_coord,
                from_coord
        )
        
        # Rotate the path.
        rotated_path = Geometry.Rotate(
            transformed_path,
            self.plane,
            self.rotation
        )
        
        # Update the length attribute.
        self.length = length
        
        # Update the width attribute.
        self.width = 0
        
        # Update the path length attribute.
        self.path_length = rotated_path.Length
        
        # Update the area attribute.
        self.area = 0
        
        return rotated_path


class BaseProfiles(BuildingShapes):
    """
    Defines vertical generic profiles for the building base to be swept along the 
    base paths.
    
    Attributes:
        name (str): The name of the profile.
        width (float): The width of the profile.
        height (float): The height of the profile.
        mirrored (bool): To determime if the profile was mirrored.
        profile_length (float): The full perimeter length of the profile.
        area (float): The area of the profile.
        pitch (float): The angle of the profile to the horizontal plane.
        
    Methods:
        square_profile: Creates a square building base profile.
        mono_pitch_profile: Creates a monopitch building base profile.
        double_pitch_profile: Creates a double pitch building base profile.
        flat_roof_profile: Creates a flat roof building base profile. 
    """ 
    
    def __init__(
                self,
                name = "default",
                width = 5,
                height = 3,
                mirrored = False
    ):
        """
        Initializes a new base profile instance.
        
        Args:
            name (str): The name of the profile.
            width (float): The width of the profile.
            height (float): The height of the profile.
            mirrored (bool): Mirror the profile.
        """ 
        
        # Raise errors if inputs are entered incorrectly.
        if not isinstance(width, (int, float)):
            raise TypeError("Width must be a number")
        if not isinstance(height, (int, float)):
            raise TypeError("Height must be a number")
        if not isinstance(mirrored, bool):
            raise TypeError("The mirrored input must be a boolean")
            
            
        super().__init__(name)
        
        self.width = width
        self.height = height
        self.mirrored = mirrored
        self.profile_length = 0
        self.area = 0
        self.pitch = 0
        
        
    def square_profile(self):
        """Creates a square building base profile"""
        
        # The base plane of the profile.
        base_plane = Plane.XZ()
        
        # Create the profile.
        profile = Rectangle.ByWidthLength(
            base_plane,
            self.height,
            self.width
        )
        
        # Move plane vertically to align base with XY plane.
        moved_profile_XY = profile.Translate(
                    Vector.ZAxis(), 
                    self.height/2
        )
        
        # Move plane vertically to align base with YZ plane.
        moved_profile_YZ = moved_profile_XY.Translate(
                    Vector.XAxis(), 
                    self.width/2
        )
        
        # Mirror the profile along the YZ axis if required.
        if self.mirrored == True:
            mirror_profile = moved_profile_YZ.Mirror(Plane.YZ())
        else:
            mirror_profile = moved_profile_YZ
            
        # Update the path length attribute.
        self.profile_length = mirror_profile.Length
        
        # Update the area attribute.
        self.area = Surface.ByPatch(mirror_profile).Area
        
        # Update the roof pitch value.
        self.pitch = 0
            
        return mirror_profile
        
        
    def mono_pitch_profile(self, roof_height=1.5):
        """
        Creates a monopitch building base profile.
        
        Args:
            roof_height (float): The height of the profile's roof zone.
        """
        
        # Raise errors if inputs are entered incorrectly.
        if not isinstance(roof_height, (int, float)):
            raise TypeError("The roof height must be a number")
        if roof_height > self.height:
            raise AttributeError("The roof height cannot be greater than the height parameter")
        
        # Create the first profile point.
        profile_point_1 = Point.ByCoordinates()
        
        # Create the second profile point.
        profile_point_2 = Point.ByCoordinates(self.width, 0, 0)
        
        # Create the third profile point.
        profile_point_3 = Point.ByCoordinates(self.width, 0, self.height - roof_height)
        
        # Create the fourth profile point.
        profile_point_4 = Point.ByCoordinates(0, 0, self.height)
        
        # Create the profile polycurve.
        points = [
                profile_point_1, 
                profile_point_2, 
                profile_point_3, 
                profile_point_4
        ]
        
        profile_curve = PolyCurve.ByPoints(Point.PruneDuplicates(points), True)
        
        # Mirror the profile along the YZ axis if required.
        if self.mirrored == True:
            mirror_profile = profile_curve.Mirror(Plane.YZ())
        else:
            mirror_profile = profile_curve
            
        # Update the path length attribute.
        self.profile_length = mirror_profile.Length
        
        # Update the area attribute.
        self.area = Surface.ByPatch(mirror_profile).Area
            
        # Update the roof pitch value.
        self.pitch = degrees(atan(roof_height / self.width))
            
        return mirror_profile
        
        
    def double_pitch_profile(self, roof_height=1.5):
        """
        Creates a double pitch building base profile.
        
        Args:
            roof_height (float): The height of the profile's roof zone. 
        """
        
        # Raise errors if inputs are entered incorrectly.
        if not isinstance(roof_height, (int, float)):
            raise TypeError("The roof height must be a number")
        if roof_height > self.height:
            raise AttributeError("The roof height cannot be greater than the height parameter")
            
        # Create the first profile point.
        profile_point_1 = Point.ByCoordinates()
        
        # Create the second profile point.
        profile_point_2 = Point.ByCoordinates(self.width, 0, 0)
        
        # Create the third profile point.
        profile_point_3 = Point.ByCoordinates(self.width, 0, self.height - roof_height)
        
        # Create the fourth profile point.
        profile_point_4 = Point.ByCoordinates(self.width/2, 0, self.height)
        
        # Create the fifth profile point.
        profile_point_5 = Point.ByCoordinates(0, 0, self.height - roof_height)
        
        # Create the profile polycurve.
        points = [
                profile_point_1, 
                profile_point_2, 
                profile_point_3,
                profile_point_4,
                profile_point_5
        ]
        
        profile_curve = PolyCurve.ByPoints(Point.PruneDuplicates(points), True)
        
        # Mirror the profile along the YZ axis if required.
        if self.mirrored == True:
            mirror_profile = profile_curve.Mirror(Plane.YZ())
        else:
            mirror_profile = profile_curve
            
        # Update the path length attribute.
        self.profile_length = mirror_profile.Length
        
        # Update the area attribute.
        self.area = Surface.ByPatch(mirror_profile).Area
            
        # Update the roof pitch value.
        self.pitch = degrees(atan(roof_height / self.width))
            
        return mirror_profile
        
        
    def flat_roof_profile(
                         self, 
                         parapet_thickness=0.25, 
                         roof_height=0.5
    ):
        """
        Creates a flat roof building base profile. 
        
        Args:
            parapet_thickness (float): The thickness of the parapet.
            roof_height (float): The height of the profile's roof zone.
        """
        
        # Raise errors if inputs are entered incorrectly.
        if not isinstance(parapet_thickness, (int, float)):
            raise TypeError("The parapet thickness must be a number")
        if not isinstance(roof_height, (int, float)):
            raise TypeError("The roof height must be a number")
        if roof_height > self.height:
            raise AttributeError("The roof height cannot be greater than the height parameter")
        if parapet_thickness >= self.width/2:
            raise AttributeError("The parapet thickness must be less than half the profile width")
            
        # Define the profile points.   
        # Create the first profile point.
        profile_point_1 = Point.ByCoordinates()
        
        # Create the second profile point.
        profile_point_2 = Point.ByCoordinates(self.width, 0, 0)
        
        # Create the third profile point.
        profile_point_3 = Point.ByCoordinates(self.width, 0, self.height)
        
        # Create the fourth profile point.
        profile_point_4 = Point.ByCoordinates(
                                        self.width - parapet_thickness,
                                        0, 
                                        self.height
        )
        
        # Create the fifth profile point.
        profile_point_5 = Point.ByCoordinates(
                                        self.width - parapet_thickness,
                                        0, 
                                        self.height - roof_height
        )
        
        # Create the sixth profile point.
        profile_point_6 = Point.ByCoordinates(
                                        parapet_thickness,
                                        0, 
                                        self.height - roof_height
        )
        
        # Create the seventh profile point.
        profile_point_7 = Point.ByCoordinates(
                                        parapet_thickness,
                                        0, 
                                        self.height
        )
        
        # Create the eighth profile point.
        profile_point_8 = Point.ByCoordinates(
                                        0,
                                        0, 
                                        self.height
        )
        
        # Create the profile polycurve.
        points = [
                profile_point_1, 
                profile_point_2, 
                profile_point_3,
                profile_point_4,
                profile_point_5,
                profile_point_6,
                profile_point_7,
                profile_point_8,
        ]
        profile_curve = PolyCurve.ByPoints(Point.PruneDuplicates(points), True)
        
        # Mirror the profile along the YZ axis if required.
        if self.mirrored == True:
            mirror_profile = profile_curve.Mirror(Plane.YZ())
        else:
            mirror_profile = profile_curve
            
        # Update the path length attribute.
        self.profile_length = mirror_profile.Length
        
        # Update the area attribute.
        self.area = Surface.ByPatch(mirror_profile).Area
            
        # Update the roof pitch value.
        self.pitch = 0
        
        return mirror_profile
        

class RoofProfiles(BuildingShapes):
    """
    Defines various roof profile shapes.
    
    Attributes:
        name (str): The name of the profile.
        width (float): The width of the profile.
        height (float): The height of the profile.
        mirrored (bool): To determime if the profile was mirrored.
        profile_length (float): The full perimeter length of the profile.
        area (float): The area of the profile.
        pitch (float): The angle of the profile to the horizontal plane.
        
    Methods:
        mono_pitch_profile: Creates a roof monopitch profile.
        double_pitch_profile: Creates a roof double pitch profile.
        flat_roof_profile: Creates a flat roof profile.
    """
    
    def __init__(
                self,
                name = "default",
                width = 5,
                height = 2.5,
                mirrored = False,
                pitch = 0
    ):
        """
        Initializes a new roof profile instance.
        
        Args:
            name (str): The name of the profile.
            width (float): The width of the profile.
            height (float): The height of the profile.
            mirrored (bool): Mirror the profile.
        """ 
        
        # Raise errors if inputs are entered incorrectly.
        if not isinstance(width, (int, float)):
            raise TypeError("Width must be a number")
        if not isinstance(height, (int, float)):
            raise TypeError("Height must be a number")
        if not isinstance(mirrored, bool):
            raise TypeError("The mirrored input must be a boolean")
        if width <= 0:
            raise AttributeError("Width must be greater than 0")
        if height <= 0:
            raise AttributeError("Height must be greater than 0")
            
            
        super().__init__(name)
        
        self.width = width
        self.height = height
        self.mirrored = mirrored
        self.profile_length = 0
        self.area = 0
        self.pitch = 0
        
    
    def mono_pitch_profile(self, fascia_height=0):
        """
        Creates a monopitch roof profile.
        
        Args:
            fascia_height (float): The height of the profile's fascia edge.
        """
        
        # Raise errors if inputs are entered incorrectly.
        if not isinstance(fascia_height, (int, float)):
            raise TypeError("Fascia height must be a number")
        
        # Create the first profile point.
        profile_point_1 = Point.ByCoordinates()
        
        # Create the second profile point.
        profile_point_2 = Point.ByCoordinates(self.width, 0, 0)
        
        # Create the third profile point.
        profile_point_3 = Point.ByCoordinates(self.width, 0, fascia_height)
        
        # Create the fourth profile point.
        profile_point_4 = Point.ByCoordinates(0, 0, self.height)
        
        # Create the profile polycurve.
        points = [
                profile_point_1, 
                profile_point_2, 
                profile_point_3, 
                profile_point_4
        ]
        profile_curve = PolyCurve.ByPoints(Point.PruneDuplicates(points), True)
        
        # Mirror the profile along the YZ axis if required.
        if self.mirrored == True:
            mirror_profile = profile_curve.Mirror(Plane.YZ())
        else:
            mirror_profile = profile_curve
        
        # Update the path length attribute.
        self.profile_length = mirror_profile.Length
        
        # Update the area attribute.
        self.area = Surface.ByPatch(mirror_profile).Area
            
        # Update the roof pitch value.
        self.pitch = degrees(atan((self.height - fascia_height) / self.width))
            
        return mirror_profile
        
        
    def double_pitch_profile(self, fascia_height=0):
        """
        Creates a double pitch roof profile.
        
        Args:
            fascia_height (float): The height of the profile's fascia edge.
        """
        
        # Raise errors if inputs are entered incorrectly.
        if not isinstance(fascia_height, (int, float)):
            raise TypeError("Fascia height must be a number")

        # Create the first profile point.
        profile_point_1 = Point.ByCoordinates()
        
        # Create the second profile point.
        profile_point_2 = Point.ByCoordinates(self.width, 0, 0)
        
        # Create the third profile point.
        profile_point_3 = Point.ByCoordinates(self.width, 0, fascia_height)
        
        # Create the fourth profile point.
        profile_point_4 = Point.ByCoordinates(self.width/2, 0, self.height)
        
        # Create the fifth profile point.
        profile_point_5 = Point.ByCoordinates(0, 0, fascia_height)
        
        # Create the profile polycurve.
        points = [
                profile_point_1, 
                profile_point_2, 
                profile_point_3,
                profile_point_4,
                profile_point_5
        ]
        profile_curve = PolyCurve.ByPoints(Point.PruneDuplicates(points), True)
        
        # Mirror the profile along the YZ axis if required.
        if self.mirrored == True:
            mirror_profile = profile_curve.Mirror(Plane.YZ())
        else:
            mirror_profile = profile_curve
            
        # Update the path length attribute.
        self.profile_length = mirror_profile.Length
        
        # Update the area attribute.
        self.area = Surface.ByPatch(mirror_profile).Area
            
        # Update the roof pitch value.
        self.pitch = degrees(atan(self.height / (self.width/2)))
        
        return mirror_profile
        
        
    def flat_roof_profile(
                        self,  
                        parapet_thickness=0.25,
                        roof_thickness=0.25
    ):
        """
        Creates a flat roof profile. The parapet height and width values 
        must be greater than zero to make parapets visible.
        
        Args:
            parapet_thickness (float): The thickness of the parapet.
        
        """
        
        # Raise errors if inputs are entered incorrectly.
        if not isinstance(parapet_thickness, (int, float)):
            raise TypeError("Parapet thickness must be a number")
        if not isinstance(roof_thickness, (int, float)):
            raise TypeError("Roof thickness must be a number")
        if parapet_thickness <= 0:
            raise AttributeError("Parapet thickness must be greater than 0")
        if roof_thickness <= 0:
            raise AttributeError("Roof thickness must be greater than 0")
        
        # Define the profile points.   
        # Create the first profile point.
        profile_point_1 = Point.ByCoordinates()
        
        # Create the second profile point.
        profile_point_2 = Point.ByCoordinates(self.width, 0, 0)
        
        # Create the third profile point.
        profile_point_3 = Point.ByCoordinates(self.width, 0, self.height)
        
        # Create the fourth profile point.
        profile_point_4 = Point.ByCoordinates(
                                        self.width - parapet_thickness,
                                        0, 
                                        self.height
        )
        
        # Create the fifth profile point.
        profile_point_5 = Point.ByCoordinates(
                                        self.width - parapet_thickness,
                                        0, 
                                        roof_thickness
        )
        
        # Create the sixth profile point.
        profile_point_6 = Point.ByCoordinates(
                                        parapet_thickness,
                                        0, 
                                        roof_thickness
        )
        
        # Create the seventh profile point.
        profile_point_7 = Point.ByCoordinates(
                                        parapet_thickness,
                                        0, 
                                        self.height
        )
        
        # Create the eighth profile point.
        profile_point_8 = Point.ByCoordinates(
                                        0,
                                        0, 
                                        self.height
        )
        
        # Create the profile polycurve.
        points = [
                profile_point_1, 
                profile_point_2, 
                profile_point_3,
                profile_point_4,
                profile_point_5,
                profile_point_6,
                profile_point_7,
                profile_point_8,
        ]
        profile_curve = PolyCurve.ByPoints(Point.PruneDuplicates(points), True)
        
        # Mirror the profile along the YZ axis if required.
        if self.mirrored == True:
            mirror_profile = profile_curve.Mirror(Plane.YZ())
        else:
            mirror_profile = profile_curve
            
        # Update the path length attribute.
        self.profile_length = mirror_profile.Length
        
        # Update the area attribute.
        self.area = Surface.ByPatch(mirror_profile).Area
            
        # Update the roof pitch value.
        self.pitch = 0
        
        return mirror_profile

#####CONTINUE HERE#####
# Remove cross width and cross height parameters and use bounding box dimensions to offset along path x axis for path intersection.
# Update to allow flexibility in the position and orientation of the custom profile.
# Complete roof shapes methods.

class BaseShapes(BuildingShapes):
    """
    Defines various generic solids for the building base.
    
    Attributes:
        name (str): The name of the shape.
        cross_width (float): The cross-sectional width of the shape.
        cross_height (float): The cross-sectional height of the shape.
        mirrored (bool): To determime if the shape's profile was mirrored
                         along the path.
        path_length (float): The length of the shape's path.
        path_width (float): The width of the shape's path.
        path_type (str): The type of path selected for the shape.
        path: The path along which the profile is swept.
        profile: The profile swept along the shape's path.
        profile_type (str): The type of profile selected for the shape.
        rotation (float): The rotation of the shape around the z-axis.
        plane: The base horizontal plane of the shape.
        color_alpha (int): The alpha value of the shape's colour.
        color_r (int): The red component of the shape's colour.
        color_g (int): The green component of the shape's colour.
        color_b (int): The blue component of the shape's colour.
        
    Methods:
        custom_shape: Creates a building base shape along a custom path.
        square_shape: Creates a square building base shape.
        c_shape: Creates a c-shaped building base shape.
        straight_shape: Creates a straight building base shape. 
    """
    
    def __init__(
                self,
                name = "default",
                cross_width = 5,
                cross_height = 2.5,
                mirrored = False,
                path_length = 20,
                rotation = 0,
                plane = Plane.XY(),
                color_alpha = 255,
                color_r = 0,
                color_g = 0,
                color_b = 0
    ):
        """
        Initializes a new base shape instance.
        
        Args:
            name (str): The name of the shape.
            cross_width (float): The cross-sectional width of the shape.
            cross_height (float): The cross-sectional height of the shape.
            mirrored (bool): Mirror the profile.
            path_length (float): The length of the shape's path.
            rotation (float): The rotation of the shape around the z-axis.
            plane: The base horizontal plane of the shape.
            color_alpha (int): The alpha value of the shape's colour.
            color_r (int): The red component of the shape's colour.
            color_g (int): The green component of the shape's colour.
            color_b (int): The blue component of the shape's colour.
        """ 
        
        # Raise errors if inputs are entered incorrectly.
        if not isinstance(cross_width, (int, float)):
            raise TypeError("Cross-sectional width must be a number")
        if not isinstance(cross_height, (int, float)):
            raise TypeError("Cross-sectional height must be a number")
        if not isinstance(mirrored, bool):
            raise TypeError("The mirrored input must be a boolean")
        if not isinstance(path_length, (int, float)):
            raise TypeError("Path length must be a number")
        if not isinstance(rotation, (int, float)):
            raise TypeError("Rotation value must be a number")
        if not isinstance(plane, Plane):
            raise TypeError("The plane input must be a plane object")
        if not plane.Normal.Z == 1:
            raise TypeError("The building plane must be horizontal")
        if not isinstance(color_alpha, int):
            raise TypeError("Color alpha value must be an integer")
        if not isinstance(color_r, int):
            raise TypeError("Color r value must be an integer")
        if not isinstance(color_g, int):
            raise TypeError("Color g value must be an integer")
        if not isinstance(color_b, int):
            raise TypeError("Color b value must be an integer")
        if cross_width <= 0:
            raise AttributeError("Cross-sectional width must be greater than 0")
        if cross_height <= 0:
            raise AttributeError("Cross-sectional height must be greater than 0")
        if path_length <= 0:
            raise AttributeError("Path length must be greater than 0")
        if not color_alpha in range(256):
            raise AttributeError("Invalid color alpha value")
        if not color_r in range(256):
            raise AttributeError("Invalid color r value")
        if not color_g in range(256):
            raise AttributeError("Invalid color g value")
        if not color_b in range(256):
            raise AttributeError("Invalid color b value")
        
        super().__init__(name)
        
        self.cross_width = cross_width
        self.cross_height = cross_height
        self.mirrored = mirrored
        self.path_length = path_length
        self.path_width = 0
        self.path_type = None
        self.path = None
        self.profile = None
        self.rotation = rotation
        self.plane = plane
        self.color_alpha = color_alpha
        self.color_r = color_r
        self.color_g = color_g
        self.color_b = color_b
        
        
    def custom_shape(
                    self, 
                    profile,
                    path_type="custom", 
                    path_width=20,
                    custom_path=Rectangle.ByWidthLength(50, 50),
                    path_placement="center"
    ):
        """
        Creates a building shape along a custom path. 
        
        Args:
            path_type (str): The type of path selected for the shape.
            path_width (float): The width of the shape's path.
            profile: The profile swept along the shape's path.
            custom_path: The custom curve path to be used if the path type
                         is set to "custom". The default path is set to a fixed 
                         50 x 50 rectangle.
            path_placement (str): To indicate the placement location of the
                                  path on the base plane. "center" of the 
                                  path's bounding box or "start" of the path's curve.
                       
        """
        
        # Raise errors if inputs are entered incorrectly.
        if not isinstance(path_type, str):
            raise TypeError("Path type must be a string")
        if not isinstance(path_width, (int, float)):
            raise TypeError("Path width must be a number")
        if path_width <= 0:
            raise AttributeError("Path width must be greater than 0")
        if not isinstance(profile, PolyCurve):
            raise TypeError("Profile must be polycurve object")
        if not profile.IsClosed == True:
            raise AttributeError("Profile must be a closed curve")
        if not profile.IsPlanar == True:
            raise AttributeError("Profile must be a planar curve")
       
        # Get the profile plane direction to check if the profile is
        # orientated correctly.
        profile_plane_dir = profile.BasePlane().Normal
        if profile_plane_dir.Dot(Vector.XAxis()) != 0:
            raise AttributeError(
                       "The profile must be horizontal or parallel to XZ plane"
            )
        
        if not isinstance(custom_path, (Line, PolyCurve, NurbsCurve)):
            raise TypeError("The custom path must be a curve")
        if path_placement != "center" and path_placement != "start":
            raise AttributeError("Path placement must be 'center' or 'start'")
        
        # Create the base path to sweep the profile.   
        path_instance = BasePaths(
                                rotation = self.rotation, 
                                plane = self.plane,
                                path_placement = path_placement
        )
        
        # Get the path instance bounding box.
        bounding_box = BoundingBox.ByGeometry(custom_path)
        
        if path_type == "custom":
            # Transform the custom path onto the input plane.
            
            # Create diagonal line at bounding box corners.
            box_diagonal = PolyCurve.ByPoints([
                                               bounding_box.MaxPoint,
                                               bounding_box.MinPoint
                                               ]
            )
            
            # Allow for selection of center or start path placement point.
            if path_placement == "center":
                placement_point = box_diagonal.PointAtParameter(0.5)
            elif path_placement == "start":
                placement_point = custom_path.PointAtParameter(0)
            
            # Transform path.                                                                   
            center_plane = Plane.ByOriginNormal(placement_point, Vector.ZAxis())
            rotate_path = custom_path.Rotate(center_plane, self.rotation)
            transform_path = rotate_path.Transform(
                                                  center_plane.ToCoordinateSystem(),
                                                  self.plane.ToCoordinateSystem()
            )
            base_path = transform_path 
        elif path_type == "straight":
            base_path = path_instance.straight_path(self.path_length)
        elif path_type == "c":
            base_path = path_instance.c_path(path_width, self.path_length)
        elif path_type == "square":
            base_path = path_instance.square_path(path_width, self.path_length)
        
        # Create a plane on the path for profile transformation.
        path_plane = base_path.PlaneAtParameter(0)
        
        # Get the path plane coordinate system.
        path_plane_coordsys = path_plane.ToCoordinateSystem()
        
        # Get the profile plane coordiante system.############################
        profile_plane_coordsys = Plane.XZ().ToCoordinateSystem()
        
        # Create a profile for sweeping.
        base_profile = profile
        
        
        
        # Function to sweep a profile along a path.
        def sweep_profile(base_profile, base_path, path_type, cross_width):
            """
            Sweep a profile along a path
        
            Args:
                base_profile: The profile to be swept along the path.
                base_path: The path to sweep the profile along.
                path_type (str): The type of path.
        
            Returns:
                solid at [0] and profile at [1]
            """
        
            # Raise errors if inputs are entered incorrectly.
            if not isinstance(path_type, str):
                raise TypeError("Path type must be a string")
            if not isinstance(base_profile, (Line, PolyCurve, NurbsCurve)):
                raise TypeError("The profile must be a curve")
            if not isinstance(base_path, (Line, PolyCurve, NurbsCurve)):
                raise TypeError("The path must be a curve")
        
            # Create a plane on the path for profile transformation.
            path_plane = base_path.PlaneAtParameter(0)
        
            # Get the path plane coordinate system.
            path_plane_coordsys = path_plane.ToCoordinateSystem()
        
            # Get the profile plane coordiante system.
            profile_plane_coordsys = Plane.XZ().ToCoordinateSystem()
        
            # Transfrom the profile to the sweep path's plane.
            trans_profile = base_profile.Transform(
                                                  profile_plane_coordsys,
                                                  path_plane_coordsys
            )
        
            # Center the profile on the path if the path is straight or an open custom curve.
            if path_type == "straight" or (path_type == "custom" and base_path.IsClosed == False):
                path_coord_sys = base_path.CoordinateSystemAtParameter(0)
                path_coord_x = path_coord_sys.XAxis
                trans_profile = trans_profile.Translate(
                                                        path_coord_x,
                                                        -cross_width / 2
                )
        
                # Check if the moved profile intersects with the path.
                profile_int = trans_profile.DoesIntersect(base_path)
        
                # Move the profile onto the path if it does not intersect.
                if profile_int == False:
                    trans_profile = trans_profile.Translate(
                                                        path_coord_x,
                                                        cross_width / 2
                    )
        
            # Sweep the profile.
            sweep_solid = trans_profile.SweepAsSolid(base_path)
        
            return sweep_solid, trans_profile
        
        
        
        
        # Sweep the profile along the path.
        try:
            # Sweep the profile.     
            sweep_solid = sweep_profile(
                                        base_profile, 
                                        base_path, 
                                        path_type, 
                                        self.cross_width
            )
        
        except:
            # Flip the profile if an exception is returned.
            if self.mirrored == True:
                profile_instance.mirrored = False
                
                # Create a profile for sweeping.
                base_profile = profile_instance.square_profile()
                
                # Sweep the profile.     
                sweep_solid = sweep_profile(
                                            base_profile, 
                                            base_path, 
                                            path_type, 
                                            self.cross_width
                )
                
        # Color the swept solid.
        color = Color.ByARGB(
                            self.color_alpha,
                            self.color_r,
                            self.color_g,
                            self.color_b
        )
        
        color_geometry = GeometryColor.ByGeometryColor(sweep_solid[0], color)
        
        # Update the path width value.########UPDATE TO REFELCT CUSTOM SHAPE BOUNDING BOX########
        if path_type == "custom":
            bounding_box_cuboid = bounding_box.ToCuboid()
            self.path_width = bounding_box_cuboid.Width
            self.path_length = bounding_box_cuboid.Length
        else:
            self.path_width = path_width
        
        # Update the path value.
        self.path = base_path
        
        # Update the path type value.
        self.path_type = "custom"
        
        # Update the path value.
        self.profile = sweep_solid[1]
        
        return sweep_solid, color_geometry, 
        
       
    def square_shape(
                    self,
                    profile, 
                    path_width=20,
                    path_placement = "center",
    ):
        """
        Creates a square building base shape.
            
        Args:
            profile : The profile swept along the shape's path.
            path_width (float): The width of the shape's path.
            path_placement (str): To indicate the placement location of the
                                  path on the base plane. "center" of the 
                                  path's bounding box or "start" of the path's curve.
        """
            
        # Raise errors if inputs are entered incorrectly.
        if not isinstance(path_width, (int, float)):
            raise TypeError("Path width must be a number")
            
        # Create the sweep solid, color geometry and base_path.
        elements = self.custom_shape(
                                    profile = profile,
                                    path_type = "square",
                                    path_width = path_width,
                                    path_placement = path_placement,
        )
        
        # Update the path type value.
        self.path_type = "square"
            
        return elements
           
            
    def c_shape(
               self,
               profile, 
               path_width=20,
               path_placement = "center",
    ):
        """
        Creates a c-shaped building base shape.
            
        Args:
            profile : The profile swept along the shape's path.
            path_width (float): The width of the shape's path.
            path_placement (str): To indicate the placement location of the
                                  path on the base plane. "center" of the 
                                  path's bounding box or "start" of the path's curve.
        """
            
        # Raise errors if inputs are entered incorrectly.
        if not isinstance(path_width, (int, float)):
            raise TypeError("Path width must be a number")
            
        # Create the sweep solid, color geometry and base_path.
        elements = self.custom_shape(
                                    profile = profile,
                                    path_type = "c",
                                    path_width = path_width,
                                    path_placement = path_placement,
        )
        
        # Update the path type value.
        self.path_type = "c"
            
        return elements
       
           
    def straight_shape(self, profile, path_placement = "center"):
        """
        Creates a straight building base shape.
        
        Args:
            profile : The profile swept along the shape's path.
            path_placement (str): To indicate the placement location of the
                                  path on the base plane. "center" of the 
                                  path's bounding box or "start" of the path's curve.
        """
            
        # Create the sweep solid, color geometry and base_path.
        elements = self.custom_shape(
                                    profile = profile, 
                                    path_type = "straight",
                                    path_placement = path_placement,
        )
        
        # Update the path type value.
        self.path_type = "straight"
        
        # Update the path width value.
        self.path_width = 0
            
        return elements
  
              
class RoofShapes(BuildingShapes):
    """
    Defines various generic roof solid shapes.
    
    Attributes:
        name (str): The name of the shape.
        cross_width (float): The cross-sectional width of the shape.
        cross_height (float): The cross-sectional height of the shape.
        mirrored (bool): To determime if the shape's profile was mirrored
                         along the path.
        path_length (float): The length of the shape's path.
        path_width (float): The width of the shape's path.
        path_type (str): The type of path selected for the shape.
        profile_type (str): The type of profile selected for the shape.
        rotation (float): The rotation of the shape around the z-axis.
        plane: The base horizontal plane of the shape.
        color_alpha (int): The alpha value of the shape's colour.
        color_r (int): The red component of the shape's colour.
        color_g (int): The green component of the shape's colour.
        color_b (int): The blue component of the shape's colour.
        
    Methods:
        custom_shape: Creates a roof shape along a custom path.
        square_shape: Creates a square roof shape.
        c_shape: Creates a c-shaped roof shape.
        straight_shape: Creates a straight roof shape.
    """
    
    def __init__(
                self,
                name = "default",
                cross_width = 5,
                cross_height = 2.5,
                mirrored = False,
                path_length = 20,
                rotation = 0,
                plane = Plane.XY(),
                color_alpha = 255,
                color_r = 0,
                color_g = 0,
                color_b = 0
    ):
        """
        Initializes a new roof shape instance.
        
        Args:
            name (str): The name of the shape.
            cross_width (float): The cross-sectional width of the shape.
            cross_height (float): The cross-sectional height of the shape.
            mirrored (bool): Mirror the profile.
            path_length (float): The length of the shape's path.
            profile_type (str): The type of profile selected for the shape.
            rotation (float): The rotation of the shape around the z-axis.
            plane: The base horizontal plane of the shape.
            color_alpha (int): The alpha value of the shape's colour.
            color_r (int): The red component of the shape's colour.
            color_g (int): The green component of the shape's colour.
            color_b (int): The blue component of the shape's colour.
        """ 
        
        # Raise errors if inputs are entered incorrectly.
        if not isinstance(cross_width, (int, float)):
            raise TypeError("Cross-sectional width must be a number")
        if not isinstance(cross_height, (int, float)):
            raise TypeError("Cross-sectional height must be a number")
        if not isinstance(mirrored, bool):
            raise TypeError("The mirrored input must be a boolean")
        if not isinstance(path_length, (int, float)):
            raise TypeError("Path length must be a number")
        if not isinstance(rotation, (int, float)):
            raise TypeError("Rotation value must be a number")
        if not isinstance(plane, Plane):
            raise TypeError("The plane input must be a plane object")
        if not plane.Normal.Z == 1:
            raise TypeError("The plane must be horizontal")
        if not isinstance(color_alpha, int):
            raise TypeError("Color alpha value must be an integer")
        if not isinstance(color_r, int):
            raise TypeError("Color r value must be an integer")
        if not isinstance(color_g, int):
            raise TypeError("Color g value must be an integer")
        if not isinstance(color_b, int):
            raise TypeError("Color b value must be an integer")
        if cross_width <= 0:
            raise AttributeError("Cross-sectional width must be greater than 0")
        if cross_height <= 0:
            raise AttributeError("Cross-sectional height must be greater than 0")
        if path_length <= 0:
            raise AttributeError("Path length must be greater than 0")
        if not color_alpha in range(256):
            raise AttributeError("Invalid color alpha value")
        if not color_r in range(256):
            raise AttributeError("Invalid color r value")
        if not color_g in range(256):
            raise AttributeError("Invalid color g value")
        if not color_b in range(256):
            raise AttributeError("Invalid color b value")
        
        super().__init__(name)
        
        self.cross_width = cross_width
        self.cross_height = cross_height
        self.mirrored = mirrored
        self.path_length = path_length
        self.path_width = 0
        self.path_type = None
        self.path = None
        self.profile = None
        self.rotation = rotation
        self.plane = plane
        self.color_alpha = color_alpha
        self.color_r = color_r
        self.color_g = color_g
        self.color_b = color_b
        
    
    def custom_shape(
                    self, 
                    path_type="custom", 
                    path_width=20,
                    profile = RoofProfiles().mono_pitch_profile(),
                    custom_path=Rectangle.ByWidthLength(50, 50),
                    path_placement="center"
    ):
        """
        Creates a roof shape along a custom path. 
        
        Args:
            path_type (str): The type of path selected for the shape.
            path_width (float): The width of the shape's path.
            custom_path: The custom curve path to be used if the path type
                         is set to "custom". The default path is set to a 
                         50 x 50 rectangle.
            path_placement (str): To indicate the placement location of the
                                  path. "center" of the path's bounding box 
                                  or "start" of the path's curve.
                       
        """
        
        # Create the roof sweep solid, color geometry and base_path.
        base_shape_instance = BaseShapes(
                                         cross_width = self.cross_width,
                                         cross_height = self.cross_height,
                                         mirrored = self.mirrored,
                                         path_length = self.path_length,
                                         rotation = self.rotation,
                                         plane = self.plane,
                                         color_alpha = self.color_alpha,
                                         color_r = self.color_r,
                                         color_g = self.color_g,
                                         color_b = self.color_b
         )
         
        elements = base_shape_instance.custom_shape(
                                                   path_type = path_type,
                                                   path_width = path_width,
                                                   profile = profile,
                                                   custom_path = custom_path,
                                                   path_placement = path_placement
        )
        
        # Update the path width value.########UPDATE TO REFELCT CUSTOM SHAPE BOUNDING BOX########
        self.path_width = path_width
        
        # Update the path value.
        self.path = base_shape_instance.path
        
        # Update the path type value.
        self.path_type = "custom"
        
        # Update the path value.
        self.profile = base_shape_instance.profile
            
        return elements


    def square_shape():
        pass
        
    def c_shape():
        pass
        
    def straight_shape():
        pass
        


###############################################################################
# Output.
base_shape_instance = BaseShapes(path_length=180, cross_width=15, plane=plane, rotation=0)
profile_instance = RoofProfiles(width=15, height=10, mirrored=True).flat_roof_profile()
base_shape_instance.square_shape(profile_instance, path_width=40, path_placement="center")

path_instance = BasePaths()
path_instance.straight_path(length=100) 

base_profile_instance = BaseProfiles(mirrored=True)
base_profile_instance.double_pitch_profile()

roof_profile_instance = RoofProfiles() 

OUT = roof_profile_instance.mono_pitch_profile(), roof_profile_instance.pitch