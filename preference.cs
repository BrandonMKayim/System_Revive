// ::bl_registerPreference() - Registers a preference into the game.
//  %name         [string] = Name of preference.
//  %description  [string] = Description of preference.
//  %category     [string] = Category of preference.
//  %typeString   [string] = Type of preference to render.
//  %defaultValue [  any ] = Value of preference.
//  %typeMachine  [string] = Client, common, or server.
//  %prefName     [string] = Name of preference to edit on game.
//  %whoCanEdit   [  int ] = Level of editing preference. (level 0 - CONSOLE, level 1 - privelage, level 2 - admin, level 3 - super admin, level 4 - host)
function bl_registerPreference(%name, %description, %category, %typeString, %defaultValue, %typeMachine, %prefName, %whoCanEdit, %callFunc, %callArg1, %calArg2, %callArg3) // -> bool
{
    %safeName = getSafeVariableName(%name);

    if($bl::preference::registered[%safeName])
    {
        echo("bl_registerPreference() - '" @ %safeName @ "' already exists");
        return false;
    }

    if(!$bl::preference::idx)
    {
        $bl::preference::idx = 0;
    }
    
    $bl::preference::registered[%safeName] = true;
    $bl::preference::default[%safeName] = %defaultValue;

    echo("  registering preference '" @ %name @ "' as $bl::preference::value[" @ %safeName @ "] ..");

    // creation of ::value
    if($bl::preference::value[%safeName] $= "")
    {
        echo("  - now newly registered!");
        $bl::preference::value[%safeName]               = $bl::preference::default[%safeName];
        $bl::preference::valueName[%safeName]           = %name;
        $bl::preference::valueType[%safeName]           = %typeString;
        $bl::preference::valueMachineType[%safeName]    = %typeMachine;
        $bl::preference::valueEdit[%safeName]           = %prefName;
        $bl::preference::valueRequirePts[%safeName]     = atoi(%whoCanEdit);
        if(%callFunc !$= "")
        {
            echo("    -> registered calls");
            // todo: presets
            $bl::preference::valueNameCall[%safeName]     = %callFunc;
            $bl::preference::valueNameCallA1[%safeName]   = %callArg1;
            $bl::preference::valueNameCallA2[%safeName]   = %callArg2;
            $bl::preference::valueNameCallA3[%safeName]   = %callArg3;
        }
    }
    
    $bl::preference::idxNum[$bl::preference::idx++]     = %safeName;

    schedule(33 * 100, 0, bl_saveServerPreferences);

    return true;
}

// ::bl_saveServerPreferences() - Saves all server preferences.
function bl_saveServerPreferences() // -> void
{
    %fileLocation = "config/bl/server/config.cs";

    export("$bl::preference::value*", %fileLocation, false);
}

// ::bl_setPreference() - Sets a preference.
// %name  [string] = Name of preference (full).
// %value [string] = Value of setting preference.
function bl_setPreference(%name, %value) // -> bool
{
    cancel($bl::preference::saveSchedule);
    %safeName = getSafeVariableName(%name);

    if(!$bl::preference::registered[%safeName])
    {
        return false;
    }

    $bl_setGlobalCheckState = false;
    $bl::preference::value[%safeName] = %value;
    %valueName = $bl::preference::valueEdit[%safeName];
    
    if(%valueName $= "")
    {
        return false;
    }
    
    eval("$Pref::Server::" @ %valueName @ " = \"" @ %value @ "\"; $Server::" @ %valueName @ " = \"" @ %value @ "\"; $bl_setGlobalCheckState = true;");

    $bl::preference::saveSchedule = schedule(500, 0, bl_saveServerPreferences);

    if($bl::preference::valueNameCall[%safeName] !$= "")
    {
        call($bl::preference::valueNameCall[%safeName], $bl::preference::valueNameCallA1[%safeName], $bl::preference::valueNameCallA2[%safeName], $bl::preference::valueNameCallA3[%safeName]);
    }

    return $bl_setGlobalCheckState;
}

// ::bl_getPreference(%name) - Returns value of preference.
function bl_getPreference(%name) // -> Value
{
    %safeName = getSafeVariableName(%name);

    if(!$bl::preference::registered[%safeName])
    {
        return "(n/a)";
    }
    
    %preference = $bl::preference::value[%safeName];

    return (%preference $= "" ? "(n/a)" : %preference);
}

// ::bl_getPreferenceRequireLevel(%name) - Returns restriction level of preference.
function bl_getPreferenceRequireLevel(%name) // -> int
{
    %safeName = getSafeVariableName(%name);
    %preferencePts = mFloor(atoi($bl::preference::valueRequirePts[%safeName]));

    return (%preferencePts <= 0 ? 0 : %preferencePts);
}

// ::bl_init() - Used for resetting preferences, packages, and more. Server-sided, only works once per load. (todo move to common...)
function bl_init()
{
    // todo: failsafe if corrupted
    if(isFile("config/bl/server/config.cs"))
    {
        exec("config/bl/server/config.cs");
    }

    bl_registerPreference("Name", "Name of this server.", "General", "string 30 25", "Blockland Server", "Server", "Name", 1, bl_updatePlayerLists);
    bl_registerPreference("Welcome Message", "Welcome display text on join.", "General", "string 120 30", "\c2Welcome to Blockland, %1.", "Server", "WelcomeMessage", 1);
    bl_registerPreference("Fall Damage", "Outside of minigame damage by fall?", "General", "string 120 30", true, "Server", "FallingDamage", 1);

    // %name, %description, %category, %typeString, %defaultValue, %typeMachine, %prefName, %whoCanEdit, %callFunc, %callArg1, %calArg2, %callArg3
}
schedule(33, 0, bl_init);

// Tool - gets level access of server (0 - 3)
function bl_getEditLevel(%obj)
{
    return (%obj.isAdmin + %obj.isSuperAdmin + (getNumKeyID() == %obj.getBLID())) | 0;
}

// hacky way, please give hints on how to fix? (just doesn't update in-game on player gui)
function bl_updatePlayerLists()
{
    webcom_postServer();

    for(%i = 0; %i < clientGroup.getCount(); %i++)
    {
        clientGroup.getObject(%i).sendPlayerListUpdate();
        clientGroup.getObject(%i).transmitServerName();
    }
}

function serverCmdSetPreference(%client, %preferenceName, %value)
{
    %preferenceRegistered = $bl::preference::registered[getSafeVariableName(%preferenceName)];

    if(!%preferenceRegistered)
    {
        %client.chatMessage("Invalid change (unregistered). Try again.");
        return;
    }

    if(bl_getEditLevel(%client) < bl_getPreferenceRequireLevel(%preferenceName))
    {
        %client.chatMessage("You do not have access to this command.");
        // todo: play custom sound
        return;
    }

    %oldValue        = bl_getPreference(%preferenceName);
    %newValueConfirm = bl_setPreference(%preferenceName, %value);

    %newValue  = bl_getPreference(%preferenceName);
    %valueName = $bl::preference::valueName[getSafeVariableName(%preferenceName)];
    messageAll('MsgAdminForce', '\c2%1 has changed a preference: \c3%4 \c7--> \c2\'\c0%2\c2\' \c7...', %client.getPlayerName(), %newValue, "", %valueName, (%newValueConfirm == true ? "\c1pass" : "\c0fail"));
}