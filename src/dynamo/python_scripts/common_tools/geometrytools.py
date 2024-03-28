# Import libraries.
import sys
import clr
clr.AddReference('ProtoGeometry')
from Autodesk.DesignScript.Geometry import *

clr.AddReference('GeometryColor')
from Modifiers import GeometryColor

clr.AddReference('DSCoreNodes')
from DSCore import *


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
