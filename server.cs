// -------------------------------------------------------------------------------------------------------------------------------------------------- //
// Name: System_Revive                                                                                                                                //
// Author: Visolator                                                                                                                                  //
//  Complete revival of Blockland ... from preferences to IRC.                                                                                        //
// -------------------------------------------------------------------------------------------------------------------------------------------------- //

exec("./preference.cs");

// current placeholder
$CustomCDN::CDN_to_clients = "http://blobs.bcs.place";

function GameConnection::bl_receive(%gc, %msg, %etc1, %etc2, %etc3)
{
    if(%gc.isAReviver)
    {
        commandToClient(%gc, 'bl_receive', %msg, %etc1, %etc2, %etc3);
    }
    else
    {
        messageClient(%gc, '', %msg, %etc1, %etc2, %etc3);
    }
}

exec("./src/server/administration.cs");