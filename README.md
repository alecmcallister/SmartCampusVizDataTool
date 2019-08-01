# SmartCampusVizDataTool

### What it's used for
- [Website](https://smartcampus.ucalgary.ca/)
- [Visualization](https://smartcampus.ucalgary.ca/vis/)
___

### Data processing
#### Datapoints
Each row of the raw data corresponds to a specific user's GPS location (lat, long) at a certain point in time. Other factors, such as accuracy and temperature, were also recorded for each row.
Location and time are the main factors when calculating restpoint/ path output.
[List of all variables used.](https://github.com/alecmcallister/SmartCampusVizDataTool/blob/master/SSACCV_WPF/Logic/Affectors.cs)


#### Restpoints
A location that a user visited for a period of time (ex. classroom, office, favorite spot to eat lunch, etc.).
Made up of multiple restpoint "groups", which correspond to separate occasions the user has stayed there (i.e. same location, different date/ duration).

Variables used in calculating restpoints include:
- Radius (meters)
  - The radius of each restpoint
- Min duration (minutes)
  - The minimum amount of time a user must remain within the restpoint
- Min accuracy score
  - The minimum accuracy score that restpoint groups must have
- Min quantity
  - The minimum amount of datapoints that restpoint groups must have

Each user's restpoints are calculated as follows:
- Iterate over the user's datapoints (sorted by date)
- If the next datapoint overlaps with any existing restpoints, add it to the corresponding restpoint
- Otherwise, create a new restpoint centered on the next datapoint

The restpoint "groups" are then calculated as follows:
- Iterate over the user's restpoints
- If the next datapoint is within a certain time of the current group's last point, then add it to that group
- Otherwise, end (complete) the current group and start a new group from the next datapoint
- If the completed group fails the accuracy, quantity, or temporal tests, it's dropped from the final output


#### Paths
A series of locations denoting a user's movement over a period of time (ex. walking from classroom to classroom).

Variables used in calculating restpoints include:
- Min segments
  - The minimum number of segments (points - 1) that a path must have
- Max subsequent time (minutes)
  - The maximum amount of time between two adjacent points on the same path
- Max subsequent distance (meters)
  - The maximum amount of distance between two adjacent points on the same path

Each user's paths are calculated as follows:
- Iterate over the user's datapoints (sorted by date)
- If the next datapoint is within a certain time & distance from the current path's last point, then add it onto the path
- Otherwise, end (complete) the current path and start a new path originating from the next datapoint