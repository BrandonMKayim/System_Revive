exec("./src/client/joystick.cs");
// activateDirectInput();
// enableJoystick();
// $CustomCDN::CDN_default = "http://blobs.bcs.place";

function clientCmdbl_receive(%msg, %etc1, %etc2, %etc3)
{

}

if(!isObject(BlocklandReviveGui))
    exec("./src/client/BlocklandReviveGui.gui");

if(!isObject(BlocklandReviveServerGui))
    exec("./src/client/BlocklandReviveServerGui.gui");
  