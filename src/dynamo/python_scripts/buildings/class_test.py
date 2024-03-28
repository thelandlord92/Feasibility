# testing classes
class Building:
    def __init__(self, name, location):
        self.name = name
        self.location = location

class BuildingShapes(Building):
    def __init__(self, name, location, shape):
        super().__init__(name, location)
        self.shape = shape

    def shape_type(self):
        return(f"This is an {self.name} {self.location.title()} {self.shape}")

class BuildingTypes(Building):
    def __init__(self, name, location, type):
        super().__init__(name, location)
        self.type = type
        self.building_shape = BuildingShapes(self.name, self.location, self.type)

    def building_type(self, top_shape):
        return (f"{self.building_shape.shape_type()} with a {top_shape} top")

    def meh(self):
        return self.building_type("crap")

building = BuildingShapes("ugly", "cape town", "square")
print(building.shape_type())
large_type = BuildingTypes("large", "joburg", "round")
print(large_type.building_type("oval"))
print(large_type.location)
