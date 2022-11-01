# LOKI Data Structures
## TN (Track Number): string, maybe struct eventually
- number or code, hexadecimal?
    - if hex, needs to be limited to a certain range
- basically a string or an int, nothing complicated
- should be overridden by IU# if available (moot for DCS?) and correlated between DL track and raw track
- (maybe internal TN can be retained as a second layer of data)
- maybe stored as a formatted string? "ITN{itn?itn:'-'}/IU{IU?IU:'-'}/..."
    - would be easier to compare
    - different kinds of TN:
        - Internal, ITN
        - External, ETN, based on other scopes and data sources
        - Datalink/Interface Unit #, IU

## Uncorrelated Data: struct
- Position (v3, cartesian)
- Coordinates (v3, polar, derived from Position)
- velocity (v3)
- IFF Returns
- IU#, if available
- Timestamp

## Track: struct?
- Position (v3, cartesian)
- Coordinates (v3, polar, derived from Position)
- velocity (v3)
    - needs to be computable from position, heading (stored as true), speed, vertical velocity
    - needs to compute into heading (true, magdev will be added as needed), speed, vvel
- IFF Returns
- IU# -> should become the main TN based on IFF and physical position/other correlation characteristics
- Last update timestamp
- Amp Data
    - Friend-Foe status - FND/ASF/NEU/SUS/HOS/UNK/PND
    - Callsign/VCS data
    - Specific type
        - ties into displayed type, preferred over Friend-Foe status
- Track history
    - previous positions every N seconds back some length of time
    - Next drop time or maximum number of positions
    - Displayed boolean

## Track Database: static class
- Key: TN
- Value: Track
- Behaviors:
    - Correlate new data to existing tracks
        - Broadphase, collect by proximity according to correlation parameters + some fudge factor?
        - Fine comparison by IFF/IU#, then by kinematics
    - Update existing track data (e.g. Amp Data) based on user input
    - Update kinematics
        - move tracks based on velocities and time step
    - Drop tracks if exceeding age limit w/o new data

## Global Settings
- History time/quantity
- Correlation settings
    - Adjustable priority and limits
    - horizontal distance
    - vertical distance
    - velocity difference
    - heading difference
    - IFF
    - IU#
- Theme and color settings

## Scope Stuff: a form/window/etc, or a static class tied into that
- Viewed position (v3, polar) where z or h => width of the display, in... units (NM, KM, etc)
- Selected Track(s) - will be used to display current track data
- Based on Viewed position, only show Tracks from the DB that are within the screen area (store references for later use...)

# Other Critical Utilities
## Conversions
- m/s (stored) to knots, ft/s
- meters (stored) to nm, feet

## Spherical Earth Math
- Cartesian to Polar coordinate conversions (done)
- Great Circle Distance
- Tangent Plane calculation (normal is easy to find), that plane is thus ax+by+cz+d=0 (DONE)
    - we need "up" and "right" vectors along the plane to convert from tangent space to world space
    - those can go into a fun lil matrix that can be used later
        - this will kinda look like...
          [[a b c px]
           [d e f py]
           [g h k pz]
           [0 0 0 1 ]]
           where abc px => x-plane (out), def py => y-plane (right), ghk pz => z-plane (up)
        - for reference, world space looks like
          [[1 0 0 0]
           [0 1 0 0]
           [0 0 1 0]
           [0 0 0 1]]
        - csc(lat) * (0, 0, 1) = north vector?
        - z cross x = y-vector?
    - includes dot and cross product operations
- Heading vector
    - (0, 1) = North, the heading vector is some angle rotated around (0,0) clockwise in tangent space
    - heading vector in tangent space then gets converted to worldspace
- Ground Speed Vector - Heading vector * speed
- V_Vel vector - Heading vector + normal * V_Vel (in proper units)
- Velocity into these components
    - first separate out V_Vel
    - Magnitude of GSV = speed
    - Heading vector from world space to tangent space, then get angle to north vector