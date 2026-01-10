// Note: Credit goes to Author of this code below.
// -----------------------------------------------

//////# ENHANCED CONTROLS - Table of Contents
//////1. Prefs
//////2. Joystick GUI code
//////3. Packaged
//////4. Joystick In-Game Code
//////5. Misc.
//////6. Steam Controller Support (defaultActionMap)

//Tags:
//EXPERIMENTAL
//INCOMPLETE
//TEMPORARY
//TODO

//2016-1-24

//////# 1. Prefs
//$Pref::Input::JoystickDisableGUIControls = 1; // Disabled until joystickGuiLoop is "properly" implemented.
$Pref::Input::JSDoubleClick = 0; // Double-click to join servers?

// Need to fix these so they don't overwrite changes set in prefs.cs (Not sure what the "proper" way to do this is, if there is one)
$Pref::Input::JoystickDeadZone = "-0.1 0.1"; //original: -0.26 0.26
$Pref::Input::JoystickEnabled = true; // Does nothing
$Pref::Input::JoystickInvert = false;
$Pref::Input::JoystickSensitivity = 0.1;
$Pref::Input::JoystickSouthPaw = false;
//$Pref::Input::JoystickDisableAutoGUICheck // Disable automatic search for a scroll control when a GUI opens (Disabling may improve performance when opening guis)

//////2. Joystick GUI code
if(isObject(guiMap))
{
	guiMap.delete();
	joyUpdateGUI.delete();
}

new actionMap(guiMap);
new GuiControl(joyUpdateGUI); // GUI that allows joy-controlled cursor update buttons. not sure if there's a better way to do this. (I'll rephrase this in English later)

// This fixes the problem where GUI controls do nothing until you start or join a server.
if(!playGui.isAwake())
{
	canvas.pushDialog(playGui);
	canvas.popDialog(playGui);
}

// Joystick GUI controls

guiMap.bind(joystick, "xaxis", "D", $Pref::Input::JoystickDeadZone, joystickGuiX);
guiMap.bind(joystick, "yaxis", "D", $Pref::Input::JoystickDeadZone, joystickGuiY);
guiMap.bind(joystick, "rxaxis", "D", $Pref::Input::JoystickDeadZone, joystickGuiX2);
guiMap.bind(joystick, "ryaxis", "D", $Pref::Input::JoystickDeadZone, joystickGuiY2);
//guiMap.bind(joystick, "zaxis", "D", $Pref::Input::JoystickDeadZone, joystickGuiClick);
guiMap.bindCmd(joystick0, "button0", "joystickGUIClick(1);", "joystickGUIClick(0);");
guiMap.bind(joystick0,"upov",joyGUIup); //D-Pad Up
guiMap.bind(joystick0,"dpov",joyGUIdown); //D-Pad Down
//guiMap.bind(joystick0,"lpov",joyGUIleft); //D-Pad Left
//guiMap.bind(joystick0,"rpov",joyGUIright); //D-Pad Right

// Keyboard and Steam Controller support
guiMap.bindCmd(keyboard, "up", 							"joystickGuiY(-1);",          	"joystickGuiY(0);");
guiMap.bindCmd(keyboard, "down", 						"joystickGuiY(1);",           	"joystickGuiY(0);");
guiMap.bindCmd(keyboard, "left", 						"joystickGuiX(-1);",          	"joystickGuiX(0);");
guiMap.bindCmd(keyboard, "right",  						"joystickGuiX(1);",           	"joystickGuiX(0);");

guiMap.bindCmd(keyboard, "w", 						"joystickGuiY2(-1);",          	"joystickGuiY2(0);");
guiMap.bindCmd(keyboard, "s", 						"joystickGuiY2(1);",           	"joystickGuiY2(0);");
guiMap.bindCmd(keyboard, "a", 						"joystickGuiX2(-1);",          	"joystickGuiX2(0);");
guiMap.bindCmd(keyboard, "d",  						"joystickGuiX2(1);",           	"joystickGuiX2(0);");

// TODO: Bind this to a class?
function uiObj_doScroll(%obj,%x,%y)
{
	//echo(getWord(%obj.position, 0)+%x SPC getWord(%obj.position, 1)+%y SPC getWord(%obj.extent, 0) SPC getWord(%obj.extent, 1));
	if(!isObject(%obj))
		return;

	%obj.resize(getWord(%obj.getPosition(), 0)+%x, getWord(%obj.getPosition(), 1)+%y, getWord(%obj.getExtent(), 0), getWord(%obj.getExtent(), 1));
}

// This is WIP but it sort of works
function joystickGuiSearch(%gui,%noclick)
{
	// If we're in a pop-up menu, all we need to do is close said menu.
	if($Joystick::ActivePopUpMenu != 0)
	{
		echo("closing pop-up menu");
		$Joystick::ActivePopUpMenu.forceClose();
		$Joystick::ActivePopUpMenu = 0;
		$Joystick::ActivePopUpFound = 0;
		return;
	}

	// Otherwise, it's time to start searching. Yes, we're going to search the entire GUI. Fun, right?
	for(%i = 0; %i < %gui.getCount(); %i++)
	{
		%clicked = 0;
		%obj = %gui.getObject(%i);
		//%posX = getWord(%obj.position,0);
		//%posY = getWord(%obj.position,1);
		//%posX = getWord(%obj.getCanvasPosition(), 0);
		//%posY = getWord(%obj.getCanvasPosition(), 1);
		%posX = getWord(%obj.getScreenPosition(), 0); //EXPERIMENTAL: Using screen position instead as a fix attempt.
		%posY = getWord(%obj.getScreenPosition(), 1);
		%extX = getWord(%obj.getExtent(), 0); //EXPERIMENTAL: Using getExtent() instead of .extent as a fix attempt. (Probably the proper way anyway)
		%extY = getWord(%obj.getExtent(), 1);
		%curX = getWord(canvas.getCursorPos(), 0);
		%curY = getWord(canvas.getCursorPos(), 1);
		//TODO: None of these appear to be in use later, should be able to package it all as a function
		//uiObj_isUnderCursor();
		//For efficiency, maybe make a way to return all these/specify them ourselves in case we already have it

		//echo("checking object " @ %obj);

		//echo("if((" @ %curX @ " >= " @ %posX @ " && " @ %curX @ " <= " @ %extX @ " + " @ %posX @ ") && (" @ %curY @ " >= " @ %posY @ " && " @ %curY @ " <= " @ %extY @ " + " @ %posY @ "))");
		//echo("if((" @ %curX @ " >= " @ %posX @ " && " @ %curX @ " <= " @ %extX+%posX @ ") && (" @ %curY @ " >= " @ %posY @ " && " @ %curY @ " <= " @ %extY+%posY @ "))");
		if((%curX >= %posX && %curX <= %extX+%posX) && (%curY >= %posY && %curY <= %extY+%posY) || %noClick) // Make sure the object is under the cursor.
		{
			//echo("Picked object: " @ %obj SPC isObject(%obj) SPC %obj.isAwake() SPC %obj.isVisible());
			if(!isObject(%obj))
				continue;
			if(!%obj.isAwake() || !%obj.isVisible())
				continue;
			%c = %obj.getClassName();

			if(%c $= "GuiScrollCtrl" && %obj.isVisible())
				$Joystick::ActiveScroll = %obj;

			if(%c $= "GuiTextListCtrl" && %obj.isVisible())
				$Joystick::ActiveList = %obj;

			//echo("checking " @ %c);
			// Something else that needs to be searched.
			if(%c $= "GuiControl" || %c $= "GuiWindowCtrl" || %c $= "GuiSwatchCtrl" || %c $= "GuiScrollCtrl" || %c $= "GuiBitmapCtrl")
			{
				//echo("found " @ %c SPC %obj @ " while searching " @ %gui.getClassName() SPC %gui);
				if(%obj.command $= "" || %c $= "GuiWindowCtrl") // If there's no command, search for something in the object (GuiWindowCtrl excepted)
					joystickGuiSearch(%obj,%noClick);
				else
				{
					if(!%noclick)
					{
						$Joystick::ClickObj = %obj;
						%obj.setValue(1); // Gives the "clicked" effect. (Sometimes glitchy. Doesn't play sound.)
						%clicked = 1;
					}
				}
			}

			if(%c $= "GuiBitmapButtonCtrl" || %c $= "GuiRadioCtrl" || %c $= "GuiButtonCtrl") // If it's a button, click it. (Missing a lot of things. CheckBoxCtrl and RadioCtrl don't work right.)
			{
				if(!%noclick)
				{
					$Joystick::ClickObj = %obj;
					%obj.setValue(1); // Gives the "clicked" effect. (Sometimes glitchy. Doesn't play sound.)
					%clicked = 1;
				}
			}

			if(%c $= "GuiCheckBoxCtrl") // Toggle a checkbox
			{
				if(!%noclick)
				{
					%obj.setValue(!%obj.getValue());
					%clicked = 1;
				}
			}

			if(%c $= GuiPopUpMenuCtrl) // Open a drop-down menu
			{
				if(!%noclick)
				{
					//$Joystick::ClickObj = %obj;
					echo("clicked pop-up menu" SPC %obj);
					$Joystick::ActivePopUpMenu = %obj; // Mark this object so we can call forceClose(); later.
					echo($Joystick::ActivePopUpMenu);
					%obj.forceOnAction();
					%clicked = 1;
				}
			}
		}
		if(%clicked)
			return; // Only click one object
	}
}

//TODO: Package elsewhere
//%x: -1 to set as un-clicked, 0/NaN to click, 1 to set as clicking
//Probably should make this friendly for devs to package (for custom GUI things? not sure if this would really be necessary)
//uiObject_click(%x)
function joystickGuiClick(%x)
{
	if(!%x) // Button release
	{
		if($Joystick::ClickObj)
		{
			%obj = $Joystick::ClickObj; // Remove the click status
			%obj.setValue(0);

			%obj.performClick(); // This doesn't work properly for some things
			$Joystick::ClickObj = 0;
		}
		return;
	}
	else if(%x == 1 || %x > 0.85) // Button press (Works with trigger
	{
		%gui = canvas.getObject(canvas.getCount()-1); // Get the menu that is on top and start a "search" for buttons.
		echo("clicked in " @ %gui);
		joystickGuiSearch(%gui);
	}
}

function joystickGuiX(%this)
{
	$Joystick::GuiX = %this;
}

function joystickGuiY(%this)
{
	$Joystick::GuiY = %this;
}

function joystickGuiX2(%this)
{
	$Joystick::GuiX2 = %this;
}

function joystickGuiY2(%this)
{
	$Joystick::GuiY2 = %this;
}

function JoystickGUILoop()
{
	if($Pref::Input::JoystickDisableGUIControls)
	{
		guiMap.pop(); // Disable the gui controls.
		canvas.popDialog(joyUpdateGui); // Disable the update gui just in case. This probably isn't necessary.
		lockMouse(0);
		return;
	}

	%oldPosX = getWord(canvas.getCursorPos(), 0);
	%oldPosY = getWord(canvas.getCursorPos(), 1);

	//if(consoleDlg.isAwake() || !escapeMenu.isAwake() && playGui.isAwake()) // This stops us screwing up playGui and consoleDlg controls (and pretty much everything else)
	if(canvas.getObject(canvas.getCount()-1).getName() $= NewChatHud) // This stops us screwing up playGui and consoleDlg controls (and pretty much everything else)
	{
		guiMap.pop(); // Disable the gui controls.
		canvas.popDialog(joyUpdateGui); // Disable the update gui just in case. This probably isn't necessary.
		if(!$Joystick::GUILockedA)
		{
			if(!playgui.isAwake())
				lockMouse(0); // Unlock the mouse if game isn't active
			$Joystick::GuiX = 0;
			$Joystick::GuiY = 0;
			$Joystick::GUILockedA = 1;
		}

		%realPosX = getWord(canvas.getCursorPos(), 0);
		%realPosY = getWord(canvas.getCursorPos(), 1);
		$Joystick::CurPosX = %realPosX;
		$Joystick::CurPosY = %realPosY;
		$Joystick::GUILoop = schedule(32,0,joystickGUILoop);
		return;
	}

	////RS////
	if(canvas.getObject(canvas.getCount()-1).getName() $= AvatarGui) // In the avatar GUI, left stick moves the preview
	{
		%a = AvatarGui;
		if(!%a.cameraX)
		{
			%a.cameraX = 0.3;
			%a.cameraY = 0.6;
			%a.cameraZ = 2.52;
		}

		%a.cameraX = %a.cameraX+($Joystick::GuiY2/6);
		%a.cameraZ = %a.cameraZ+($Joystick::GuiX2/6);

		Avatar_Preview.setCameraRot(%a.cameraX,%a.cameraY,%a.cameraZ);
	}
	else if(canvas.getObject(canvas.getCount()-1).getName() $= MainMenuButtonsGui)
	{
		%menuGui = MainMenuButtonsGui.getObject(0);
		%menuGui.resize(6+$Joystick::GuiX2*8, 2+$Joystick::GuiY2*8, getWord(%menuGui.extent, 0), getWord(%menuGui.extent, 1));
	}
	else
	{
		if(isObject($Joystick::ActiveScroll) && $Joystick::GuiY2 != 0)
			uiObj_doScroll($Joystick::ActiveScroll.getObject(0),0,-$Joystick::GuiY2*14);
	}

	guiMap.push(); // I'm not sure if/why this is necessary in the loop.

	if(canvas.getObject(canvas.getCount()-1).getName() !$= ConsoleDlg) // Temporary fix
	{
		canvas.pushDialog(joyUpdateGUI);
		canvas.popDialog(joyUpdateGUI); // Doing this allows the "fake" cursor movements to highlight buttons. Doesn't seem to break anything (yet). Is there a better solution?
	}

	lockMouse(1);
	$Joystick::GUILockedA = 0;

	%newPosX = getWord(canvas.getCursorPos(), 0)+$Joystick::GuiX*8;
	%newPosY = getWord(canvas.getCursorPos(), 1)+$Joystick::GuiY*8;
	canvas.setCursorPos(%newPosX, %newPosY);

	%curX = getWord(canvas.getCursorPos(), 0);
	%curY = getWord(canvas.getCursorPos(), 1);

	if($Joystick::ClickObj) // Object check
	{
		%obj = $Joystick::ClickObj;
		%posX = getWord(%obj.getCanvasPosition(), 0);
		%posY = getWord(%obj.getCanvasPosition(), 1);
		%extX = getWord(%obj.extent, 0);
		%extY = getWord(%obj.extent, 1);

		if(%obj.visible && (%curX >= %posX && %curX <= %extX+%posX) && (%curY >= %posY && %curY <= %extY+%posY)) {} // We're not over the button anymore, remove its "click" status.
		else
		{
			%obj.setValue(0);
			$Joystick::ClickObj = 0;
		}
	}

	$Joystick::CurPosX = %curX;
	$Joystick::CurPosY = %curY;

	$Joystick::GUILoop = schedule(12,0,joystickGUILoop);

	//The gui loop needs to be cancelled when disabling joystick gui controls (also unlock the mouse)
}

//TODO: Package?
function joyGUIup(%x) // Move up in the list if one is selected.
{
	if($Joystick::ActivePopUpMenu && !$Joystick::ActivePopUpFound) // If there's a pop-up menu open that we need to find
	{
		%newList = %gui = canvas.getObject(canvas.getCount()-1);

		if(%newlist.getCount() == 1)
			$Joystick::ActiveList = %newList.getObject(0).getObject(0);

		$Joystick::ActivePopUpFound = 1;
	}

	%list = $Joystick::ActiveList;
	if(%x && isObject(%list))
	{
		if(!$Joystick::ListCount[%list])
			$Joystick::ListCount[%list] = 0;

		if(%list.isAwake() && %list.getRowId($Joystick::ListCount[%list]-1) != -1)
			$Joystick::ListCount[%list]--;

		%list.setSelectedRow($Joystick::ListCount[%list]);
	}
}

function joyGUIdown(%x)
{
	if($Joystick::ActivePopUpMenu && !$Joystick::ActivePopUpFound) // If there's a pop-up menu open that we need to find
	{
		%newList = %gui = canvas.getObject(canvas.getCount()-1);

		if(%newlist.getCount() == 1)
			$Joystick::ActiveList = %newList.getObject(0).getObject(0);

		$Joystick::ActivePopUpFound = 1;
	}

	%list = $Joystick::ActiveList;
	if(%x && isObject(%list))
	{
		if(!$Joystick::ListCount[%list])
			$Joystick::ListCount[%list] = 0;

		if(%list.isAwake() && %list.getRowId($Joystick::ListCount[%list]+1) != -1)
			$Joystick::ListCount[%list]++;

		%list.setSelectedRow($Joystick::ListCount[%list]);
	}
}

if(!$Joystick::GUILoop && $Pref::Input::JoystickEnableGUIControls)
	joystickGUILoop(); //this needs to be called when enabling joystick gui controls

//////3. Packaged
deactivatePackage("JoystickControls");
package JoystickControls
{
	function directSelectInv(%this)
	{
		Parent::directSelectInv(%this);
	}

	function GameModeGui::ClickGameMode(%gui,%this)
	{
		GameModeGui.clicked = %this;
		Parent::ClickGameMode(%gui,%this);
	}

	function Canvas::PushDialog(%canvas,%dialog,%b,%c,%d) //uses a and b
	{
		Parent::PushDialog(%canvas,%dialog,%b,%c,%d);

		if(%dialog !$= "joyUpdateGui" && !$Pref::Input::JoystickDisableGUIControls && !$Pref::Input::JoystickDisableAutoGUICheck) // Search the GUI (for scrolling)
			joystickGuiSearch(%dialog,1);
	}

	function Canvas::PopDialog(%dialog,%a,%b,%c,%d)
	{
		//echo("Canvas::PopDialog" SPC %dialog SPC %a SPC %b SPC %c SPC %d);
		Parent::PopDialog(%dialog,%a,%b,%c,%d);
	}
};
activatePackage("JoystickControls");

//////4. Joystick In-Game Code
//get proper disable/enable functionality?
disableJoystick();
enableJoystick();

//Main; Build; Alt; Emote
function joyA(%x) //Jump; Mouse Fire; Drop Camera at Player; Love Emote
{
	if(HUD_BrickActive.visible==1)
	{
		mouseFire(%x);
	}
	else if($joyaltmode==1)
	{
		if($joyaltemote && %x==1)
		{
			commandToServer('love');
		}
		else
		{
			dropCameraAtPlayer(%x);
		}
	}
	else
	{
		jump(%x);
	}
}

function joyB(%x) //Crouch, Undo Brick, Drop Player at Camera
{
	if(HUD_BrickActive.visible==1)
	{
		undoBrick(%x);
	}
	else if($joyaltmode==1)
	{
		if($joyaltemote && %x==1)
		{
			commandToServer('hate');
		}
		else
		{
			dropPlayerAtCamera(%x);
		}
	}
	else
	{
		crouch(%x);
	}
}

function joyX(%x) //Light; Rotate Brick; Drop Tool
{
	if(HUD_BrickActive.visible==1)
	{
		RotateBrickCW(%x);
	}
	else if($joyaltmode==1)
	{
		if($joyaltemote && %x==1)
		{
			commandToServer('confusion');
		}
		else
		{
			dropTool(%x);
		}
	}
	else
	{
		useLight(%x);
	}
}

function joyY(%x) //Toggle First/Third Person; Toggle Super Shift; Toggle Player Names/Crosshair
{
	if(HUD_BrickActive.visible==1)
	{
		toggleSuperShift(%x);
	}
	else if($joyaltmode==1)
	{
		if($joyaltemote && %x==1)
		{
			commandToServer('alarm');
		}
		else
		{
			toggleShapeNameHud(%x);
		}
	}
	else
	{
		togglefirstperson(%x);
	}
}

function joyLB(%x) //Jet; NONE; Hold for Emote Mode
{
	if($joyaltmode)
	{
		$joyaltemote = %x;

		if(%x==0)
		{
			newChatHud_AddLine("Joy - Emote Mode DISABLED");
		}
		else
		{
			newChatHud_AddLine("Joy - Emote Mode ENABLED");
		}
	}
	else
	{
		jet(%x);
	}
}

function joyRB(%x) //Use Tools; NONE; NONE
{
	$joyUseTools = %x;

	useTools(%x);
}

function joyBack(%x) //Toggle Alt Controls; NONE; NONE
{
	if(%x==1)
	{
		if($joyaltmode==1)
		{
			$jotaltemote = 0;
			$joyaltmode = 0;
			newChatHud_AddLine("Joy - Alt Mode DISABLED");
		}
		else
		{
			$joyaltmode = 1;
			newChatHud_AddLine("Joy - Alt Mode ENABLED");
		}
	}
}

function joyStart(%x) //Open Escape Menu; NONE; NONE
{
	if(%x==1)
	{
		escapeMenu.toggle();
	}
}

function joyLStick(%x) //Open Bricks Menu; NONE; Toggle Build Macro Recording
{
	if(%x==1)
	{
		if($joyaltmode==1)
		{
			toggleBuildMacroRecording(%x);
		}
		else
		{
			openBSD(%x);
		}
	}
}

function joyRStick(%x) //Open Chat Menu; Cancel Brick; NONE
{
	if(%x==1)
	{
		if(HUD_BrickActive.visible==1)
		{
			cancelBrick(%x);
		}
		else if($joyaltmode==1)
		{
			PlayBackBuildMacro(%x);
		}
		else
		{
			newChatHud_AddLine("Joy - WIP. This key will let you use chat!");
		}
	}
}

function joydup(%x) //Scroll Tools Up; Shift Brick Up 1/3; Scroll Chat Up; Zombie Emote
{
	if(HUD_BrickActive.visible==1)
	{
		if(HUD_SuperShift.visible) // We'll need to switch to regular shifting to use super shift
		{
			shiftBrickUp(%x);
			shiftBrickThirdUp(0);
		}
		else
		{
			shiftBrickThirdUp(%x);
			shiftBrickUp(0);
		}
	}
	else if($joyaltmode==1)
	{
		shiftBrickThirdUp(0);
		shiftBrickUp(0);
		if($joyaltemote && %x==1)
		{
			commandToServer('zombie');
		}
		else
		{
			PageUpNewChatHud(%x);
		}
	}
	else if(%x)
	{
		shiftBrickThirdUp(0);
		shiftBrickUp(0);
		scrollInventory(1);
	}
}

function joyddown(%x) //Scroll Tools Down; Shift Brick Down 1/3; Scroll Chat Down; Sit Emote
{
	if(HUD_BrickActive.visible==1)
	{
		if(HUD_SuperShift.visible) // We'll need to switch to regular shifting to use super shift
		{
			shiftBrickDown(%x);
			shiftBrickThirdDown(0);
		}
		else
		{
			shiftBrickThirdDown(%x);
			shiftBrickDown(0);
		}
	}
	else if($joyaltmode==1)
	{
		shiftBrickThirdDown(0);
		if($joyaltemote && %x==1)
		{
			commandToServer('sit');
		}
		else
		{
			PageDownNewChatHud(%x);
		}
	}
	else if(%x)
	{
		shiftBrickThirdDown(0);
		shiftBrickDown(0);
		scrollInventory(-1);
	}
}

function joydleft(%x) //Paint Column Left; Scroll Brick Bar Left; Previous Vehicle Seat
{
	if(HUD_BrickActive.visible==1 && %x)
	{
		scrollBricks(-1);
	}
	else if($joyaltmode==1)
	{
		PrevSeat(%x);

	}
	else if(%x)
	{
		shiftPaintColumn(-1);
		PrevSeat(0);
	}
}

function joydright(%x) //Paint Column Right; Scroll Brick Bar Right; Next Seat; Shift Brick Third Up
{
	if(HUD_BrickActive.visible==1 && %x)
	{
		scrollBricks(1);
	}
	else if($joyaltmode==1)
	{
		NextSeat(%x);
	}
	else if(%x)
	{
		shiftPaintColumn(1);
	}
}

//movement functions (Thanks to Resonance_Cascade's add-on)
function joystickMoveX(%x)
{
	if(HUD_BrickActive.visible) // If build mode is active
	{
		$mvLeftAction = 0; // This fixes the annoying glitch where you keep moving when you enter build mode.
		$mvRightAction = 0;
		if(%x > 0.5)
		{
			if(!$joyShiftedRight)
			{
				$joyShiftedRight = 1;
				shiftBrickRight(1);
			}
		}
		else
		{
			shiftBrickRight(0);
			$joyShiftedRight = 0;
		}

		if(%x < -0.5)
		{
			if(!$joyShiftedLeft)
			{
				$joyShiftedLeft = 1;
				shiftBrickLeft(1);
			}
		}
		else
		{
			shiftBrickLeft(0);
			$joyShiftedLeft = 0;
		}
	}
	else
	{
		shiftBrickRight(0);
		shiftBrickLeft(0);
		$joyShiftedRight = 0;
		$joyShiftedLeft = 0;
		if(%x > 0)
		{
			$mvRightAction = %x * $movementSpeed;
			$mvLeftAction = 0;
		}
		else
		{
			$mvLeftAction = -%x * $movementSpeed;
			$mvRightAction = 0;
		}
	}
}

function joystickMoveY(%x)
{
	if(HUD_BrickActive.visible) // If build mode is active
	{
		$mvForwardAction = 0; // This fixes the annoying glitch where you keep moving when you enter build mode.
		$mvBackwardAction = 0;
		if(%x > 0.5)
		{
			if(!$joyShiftedtowards)
			{
				$joyShiftedtowards = 1;
				shiftBrickTowards(1);
			}
		}
		else
		{
			shiftBrickTowards(0);
			$joyShiftedtowards = 0;
		}

		if(%x < -0.5)
		{
			if(!$joyShiftedAway)
			{
				$joyShiftedaway = 1;
				shiftBrickAway(1);
			}
		}
		else
		{
			shiftBrickAway(0);
			$joyshiftedaway = 0;
		}
	}
	else
	{
		shiftBrickTowards(0);
		shiftBrickAway(0);
		$joyShiftedTowards = 0;
		$joyShiftedAway = 0;
		if(%x > 0)
		{
			$mvBackwardAction = %x * $movementSpeed;
			$mvForwardAction = 0;
		}
		else
		{
			$mvForwardAction = -%x * $movementSpeed;
			$mvBackwardAction = 0;
		}
	}
}

function getJoystickAdjustAmount(%x)
{
   // based on a default camera FOV of 90'
   return(%x * ($cameraFov / 90) * $Pref::Input::JoystickSensitivity);
}

function joystickYaw(%x)
{
	if(HUD_BrickActive.visible) // If build mode is active
	{
		$mvYawLeftSpeed = 0; // This fixes the annoying glitch where you keep moving when you enter build mode.
		$mvYawRightSpeed = 0;
		if(%x > 0.5)
		{
			if(!$joyShiftedRight)
			{
				$joyShiftedRight = 1;
				shiftBrickRight(1);
			}
		}
		else
		{
			shiftBrickRight(0);
			$joyShiftedRight = 0;
		}

		if(%x < -0.5)
		{
			if(!$joyShiftedLeft)
			{
				$joyShiftedLeft = 1;
				shiftBrickLeft(1);
			}
		}
		else
		{
			shiftBrickLeft(0);
			$joyShiftedLeft = 0;
		}
	}
	else // If build mode isn't active
	{
		shiftBrickRight(0);
		shiftBrickLeft(0);
		$joyShiftedRight = 0;
		$joyShiftedLeft = 0;

		if(%x > 0)
		{
			$mvYawLeftSpeed = getJoystickAdjustAmount(%x);
			$mvYawRightSpeed = 0;
		}
		else
		{
			$mvYawRightSpeed = getJoystickAdjustAmount(-%x);
			$mvYawLeftSpeed = 0;
		}
	}
}

function joystickPitch(%x)
{
	if(HUD_BrickActive.visible) // If build mode is active
	{
		$mvPitchUpSpeed = 0; // This fixes the annoying glitch where you keep moving when you enter build mode.
		$mvPitchDownSpeed = 0;
		if(%x > 0.5)
		{
			if(!$joyShiftedtowards)
			{
				$joyShiftedtowards = 1;
				shiftBrickTowards(1);
			}
		}
		else
		{
			shiftBrickTowards(0);
			$joyShiftedtowards = 0;
		}

		if(%x < -0.5)
		{
			if(!$joyShiftedAway)
			{
				$joyShiftedaway = 1;
				shiftBrickAway(1);
			}
		}
		else
		{
			shiftBrickAway(0);
			$joyshiftedaway = 0;
		}
	}
	else // If build mode isn't active
	{
		shiftBrickTowards(0);
		shiftBrickAway(0);
		$joyShiftedTowards = 0;
		$joyShiftedAway = 0;

		if($Pref::Input::JoystickInvert)
			%x = -%x;

		if(%x > 0)
		{
			$mvPitchUpSpeed = getJoystickAdjustAmount(%x);
			$mvPitchDownSpeed = 0;
		}
		else
		{
			$mvPitchDownSpeed = getJoystickAdjustAmount(-%x);
			$mvPitchUpSpeed = 0;
		}
	}
}

function joystickFire(%x)
{
	if(isObject(serverConnection.getControlObject().getObjectMount()))
	{
		if(serverConnection.getControlObject().getObjectMount().getClassName() $= "WheeledVehicle" || serverConnection.getControlObject().getObjectMount().getClassName() $= "FlyingVehicle")
		{
			moveForward(-%x);
			return;
		}
	}

	if(%x < -0.85) // Left trigger
	{
		if(!$joystickTrigger) // Make sure we haven't already done this
		{
			$joystickTrigger = 1; // Set this to confirm we've already called the functions
			if(HUD_BrickActive.visible==1)
			{
				plantBrick(%x);
			}
			else
			{
				mouseFire(1);
			}
		}
	}
	else if(%x > 0.85) // Right trigger
	{
		if(!$joystickTrigger) // Make sure we haven't already done this
		{
			$joystickTrigger = 1;
			useBricks(1);
		}
	}
	else
	{
		$joystickTrigger = 0;
		mouseFire(0);
		plantBrick(0);
	}

}

//NEW CONTROLS
movemap.bind(joystick0, "button0", joyA); //A
movemap.bind(joystick0, "button1", joyB); //B
movemap.bind(joystick0, "button2", joyX); //X
movemap.bind(joystick0, "button3", joyY); //Y
movemap.bind(joystick0, "button4", joyLB); //LB
movemap.bind(joystick0, "button5", joyRB); //RB
movemap.bind(joystick0, "button6", joyBack); //Back
movemap.bind(joystick0, "button7", joyStart); //Start
movemap.bind(joystick0, "button8", joyLStick); //Left Stick
moveMap.bind(joystick0, "button9", joyRStick); //Right Stick

//D-Pad
movemap.bind(joystick0,"upov",joydup); //D-Pad Up
movemap.bind(joystick0,"dpov",joyddown); //D-Pad Down
movemap.bind(joystick0,"lpov",joydleft); //D-Pad Left
movemap.bind(joystick0,"rpov",joydright); //D-Pad Right

//Sticks
function Joystick_BindSticks()
{
	moveMap.bind(joystick, "xaxis", "D", $Pref::Input::JoystickDeadZone, joystickMoveX);
	moveMap.bind(joystick, "yaxis", "D", $Pref::Input::JoystickDeadZone, joystickMoveY);
	moveMap.bind(joystick, "rxaxis", "D", $Pref::Input::JoystickDeadZone, joystickYaw);
	moveMap.bind(joystick, "ryaxis", "D", $Pref::Input::JoystickDeadZone, joystickPitch);
}
Joystick_BindSticks();

//Triggers
moveMap.bind(joystick, "zaxis", "D", $Pref::Input::JoystickDeadZone, joystickFire);

///////////////////////////////////////////////////
//OLD CONTROLS (NO TRIGGER SUPPORT)
//Buttons
//movemap.bind(joystick0,"button1",joyA); //A
//movemap.bind(joystick0,"button2",joyB); //B
//movemap.bind(joystick0,"button0",joyX); //X
//movemap.bind(joystick0,"button3",joyY); //Y
//movemap.bind(joystick0,"button4",joyLB); //LB
//movemap.bind(joystick0,"button5",joyRB); //RB
//movemap.bind(joystick0,"button6",joyLT); //LT
//movemap.bind(joystick0,"button7",joyRT); //RT
//movemap.bind(joystick0,"button10",joyLStick); //Left Stick
//movemap.bind(joystick0,"button11",joyRStick); //Right Stick
//moveMap.bindCmd(joystick0, "button9", "", "escapeMenu.toggle();"); //Test - Start
//movemap.bind(joystick0,"button8",joyBack); //Back

//D-Pad
//movemap.bind(joystick0,"upov",joydup); //D-Pad Up
//movemap.bind(joystick0,"dpov",joyddown); //D-Pad Down
//movemap.bind(joystick0,"lpov",joydleft); //D-Pad Left
//movemap.bind(joystick0,"rpov",joydright); //D-Pad Right

//Sticks
//moveMap.bind(joystick, "xaxis", "D", $Pref::Input::JoystickDeadZone, joystickMoveX);
//moveMap.bind(joystick, "yaxis", "D", $Pref::Input::JoystickDeadZone, joystickMoveY);
//moveMap.bind(joystick, "rxaxis", "D", $Pref::Input::JoystickDeadZone, joystickYaw);
//moveMap.bind(joystick, "ryaxis", "D", $Pref::Input::JoystickDeadZone, joystickPitch);

//???
//movemap.bind(joystick0,"slider",joyjet);

//function joyLT(%x) //jet
//{
//	if(HUD_BrickActive.visible==1)
//	{
//		shiftBrickUp(%x);
//	}
//	else
//	{
//		jet(%x);
//	}
//}

//function joyRT(%x) //crouch
//{
//	if(HUD_BrickActive.visible==1)
//	{
//		shiftBrickDown(%x);
//	}
//	else
//	{
//		crouch(%x);
//	}
//}

///////////////////////////////////////////////////
//Other stuff

//if($Pref::Input::JoystickSouthPaw)
//{
//	moveMap.bind(joystick, "rxaxis", "D", $Pref::Input::JoystickDeadZone, joystickMoveX);
//	moveMap.bind(joystick, "ryaxis", "D", $Pref::Input::JoystickDeadZone, joystickMoveY);
//	moveMap.bind(joystick, "xaxis", "D", $Pref::Input::JoystickDeadZone, joystickYaw);
//	moveMap.bind(joystick, "yaxis", "D", $Pref::Input::JoystickDeadZone, joystickPitch);
//}


//moveMap.bind(joystick, "button0", plantBrick);
//moveMap.bind(joystick, "button1", openBSD);
//moveMap.bind(joystick, "button2", useBricks);
//moveMap.bind(joystick, "button3", useTools);
//moveMap.bind(joystick, "button4", shiftBrickDown);
//moveMap.bind(joystick, "button5", shiftBrickUp);
//moveMap.bind(joystick, "button6", invLeft);
//moveMap.bind(joystick, "button7", invRight);
//moveMap.bind(joystick, "button8", RotateBrickCCW);
//moveMap.bind(joystick, "button9", RotateBrickCW);

//moveMap.bind(joystick, "lpov", shiftBrickLeft);
//moveMap.bind(joystick, "rpov", shiftBrickRight);
//moveMap.bind(joystick, "upov", shiftBrickAway);
//moveMap.bind(joystick, "dpov", shiftBrickTowards);;

//////# 5. Misc.
// Server list double-clicking
JS_serverList.altCommand = "if($Pref::Input::JSDoubleClick) JoinServerGui.join();";