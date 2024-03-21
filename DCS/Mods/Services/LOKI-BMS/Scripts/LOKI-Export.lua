local JSON = loadfile("Scripts\\JSON.lua")()
local socket = require("socket")

local LOKI = {}

LOKI.ExportPort = 11308
LOKI.CommandPort = 11309
LOKI.UtilityPort = 11310

LOKI.Hostname = "127.0.0.1"

LOKI.JSON = JSON
LOKI.UdpExport = socket.udp()
LOKI.UdpCommand = socket.udp()
LOKI.UdpUtility = socket.udp()

--LOKI.UdpExport:setsockname("*", LOKI.ExportPort)
--LOKI.UdpExport:settimeout(0)

LOKI.UdpCommand:setsockname("*", LOKI.CommandPort)
LOKI.UdpCommand:settimeout(0)

LOKI.UdpUtility:setsockname("*", LOKI.UtilityPort)
LOKI.UdpUtility:settimeout(0)

function LOKI.GetAllUnits ()
    --local bluefor = coalition.GetGroups(2, 0)
    --local redfor = coalition.GetGroups(1,0)
    --local neufor = coalition.GetGroups(0,0)

    --local bluredmerge = LOKI.MergeTables(bluefor, redfor)
    local rawUnits = LoGetWorldObjects()

    local units = {}
    for k,v in pairs(rawUnits) do
        local temp = {
            name = v.UnitName,
            group = v.GroupName,
            position = v.LatLongAlt,
        }
        table.insert(units, temp)
    end

    return units
end

function LOKI.GetUnitSet (_number, _offset)

end

function LOKI.MergeTables (_t1, _t2)
    local merged = {}

    for _, item in pairs(_t1) do
        table.insert(merged, item)
    end
    for _, item in pairs(_t2) do
        table.insert(merged, item)
    end

    return merged
end

LuaExportActivityNextEvent = function(tCurrent)
    local commands = LOKI.UdpCommand:receive()
    local utils = LOKI.UdpUtility:receive()

    if(commands == nil) then
        commands = "none"
    end
    if (utils == nil) then
        utils = "none"
    end

    local message = {
        Timestamp = tCurrent,
        isExportAllowed = LoIsObjectExportAllowed(),
        Units = {},
        OtherText = "some other text",
    }

    if (LoIsObjectExportAllowed()) then
        message.Units = LOKI.GetAllUnits()
    end

    local encoded = LOKI.JSON:encode(message)

    socket.try(LOKI.UdpExport:sendto(encoded .. " \n", "127.0.0.1", LOKI.ExportPort))

    return tCurrent + 1.0
end
