local JSON = loadfile("Scripts\\JSON.lua")
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

LuaExportActivityNextEvent = function(tCurrent)
    local commands = LOKI.UdpCommand:receive()
    local utils = LOKI.UdpUtility:receive()

    if(commands == nil) then
        commands = "none"
    end
    if (utils == nil) then
        utils = "none"
    end

    local message = "Test: \n"

    socket.try(LOKI.UdpExport:sendto(message, "127.0.0.1", LOKI.ExportPort))

    return tCurrent + 1.0
end
