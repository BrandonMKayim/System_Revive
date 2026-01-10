// [Host Only] Command '/AddAdmin <targ> <level> <auto?>' - Makes someone an Administator on the server.
function serverCmdAddAdmin(%client, %targetNameToPromote, %level, %useAuto)
{
    if(bl_getEditLevel(%client) < 3)
    {
        %client.chatMessage("You do not have permission to use this command.");
        return;
    }

    %targetToPromote = findClientByBL_ID(%targetNameToPromote);
    
    if(!isObject(%targetToPromote))
    {
        %client.chatMessage("Invalid person to promote.");
        return;
    }

    if(bl_getEditLevel(%targetToPromote) >= %level)
    {
        %client.chatMessage("Person is already this rank or higher.");
        return;
    }

    if(%level == 1)
    {
        messageAll('MsgAdminForce', '\c2%1 has been promoted to Admin (Manual) [by \'%2\']', %targetToPromote.getPlayerName(), %client.getPlayerName());
        %isAdmin = true;
        %isSuperAdmin = false;
    }
    else if(%level == 2)
    {
        messageAll('MsgAdminForce', '\c2%1 has been promoted to Super Admin (Manual) [by \'%2\']', %targetToPromote.getPlayerName(), %client.getPlayerName());
        %isAdmin = true;
        %isSuperAdmin = true;
    }
    else
    {
        %client.chatMessage("Invalid rank. Must be either 1, or 2 (/AddAdmin <BLID> <rank>).");
        return;
    }

    %targetToPromote.isAdmin = %isAdmin;
    %targetToPromote.isSuperAdmin = %isSuperAdmin;
    commandToClient(%targetToPromote, 'setAdminLevel', %targetToPromote.isAdmin + %targetToPromote.isSuperAdmin);
}

// [Host Only] Command /RemoveAdmin <targ>/<id> - Completely removes target's administration.
function serverCmdRemoveAdmin(%client, %targetNameOrID)
{
    if(bl_getEditLevel(%client) < 3)
    {
        %client.chatMessage("You do not have permission to use this command.");
        return;
    }
}