if(!isObject(EventScript))
{
    new ScriptGroup(EventScript)
    {
        class = "EventGroupSO";
    };
}

function fxDtsBrick::createScript(%brick, %name, %client)
{
    if(!%client.isAdmin)
    {
        return false;
    }

    %tokenHandler = new ScriptObject()
    {
        class = "Script";
        processString = "script " @ %name @ "\n";

        tokens = 0;

        isDone = false;
    };

    if(isObject(%brick.scriptHandler))
    {
        %brick.scriptHandler.delete();
    }

    if(isObject(%tokenHandler))
    {
        %brick.scriptHandler = %tokenHandler;
    }

    return true;
}
registerOutputEvent("fxDtsBrick", "createScript", "string 50 50");

function fxDtsBrick::editScript(%brick, %scriptString, %client)
{
    if(!%client.isAdmin)
    {
        return false;
    }

    if(isObject(%tokenHandler = %brick.scriptHandler))
    {
        %tokenHandler.processString = %tokenHandler.processString NL %scriptString;
    }

    return true;
}
registerOutputEvent("fxDtsBrick", "editScript", "string 200 200");

function fxDtsBrick::runScript(%brick, %client)
{
    %tokenHandler = %brick.scriptHandler;
    if(isObject(%tokenHandler))
    {
        %tokenHandler.tokenize();
        %tokenHandler.process();
        %tokenHandler.schedule(0, delete);
    }

    return true;
}
registerOutputEvent("fxDtsBrick", "runScript");

function GameConnection::runScript(%gc, %scriptString)
{
    %tokenHandler = eventScript_tokenize(%scriptString);
    %tokenHandler.process();
    %tokenHandler.schedule(0, delete);
}

function eventScript_tokenize(%scriptString)
{
    %event = new ScriptObject()
    {
        class = "Script";
        processString = %scriptString;

        tokens = 0;

        isDone = false;
    };

    %event.tokenize();

    return %event;
}

function Script::onAdd(%script)
{
    EventScript.add(%script);
}

function Script::tokenize(%script)
{
    // local (brick): .
    // global (group): !

    %script.tokens = 0;
    %words = getWordCount(%script.processString);

    for(%i = 0; %i < %words; %i++)
    {
        %word = getWord(%script.processString, %i);
        %nextWord = (%i + 1 >= %words ? "" : getWord(%script.processString, %i + 1));

        if(%skipNextWord == true)
        {
            %skipNextWord = false;
            continue;
        }

        if(getSubStr(%word, 0, 1) $= ".")
        {
            %token = getSubStr(%word, 1, 999);

            %script.token[%script.tokens] = %token;
            %script.tokenType[%script.tokens] = "local_var";
            %script.tokenValue[%script.tokens] = %nextWord;
            %script.tokens++;

            %skipNextWord = 1;
            talk("SETTING LOCAL_VAR '" @ %word @ "': " @ %nextWord);
        }
        else if(getSubStr(%word, 0, 1) $= "!")
        {
            %token = getSubStr(%word, 1, 999);

            %script.token[%script.tokens] = %token; 
            %script.tokenType[%script.tokens] = "group_var";
            %script.tokenValue[%script.tokens] = %nextWord;
            %script.tokens++;

            %skipNextWord = 1;
            talk("SETTING GROUP_VAR '" @ %word @ "': " @ %nextWord);
        }
        else if(%word $= "call")
        {
            talk("CALL '" @ %nextWord @ "' for EventScript");
            
            %script.token[%script.tokens] = "function"; 
            %script.tokenType[%script.tokens] = "function_call";
            %script.tokenValue[%script.tokens] = %nextWord;
            %script.tokens++;

            %skipNextWord = true;
        }
        else
        {
            // talk("UNKNOWN TOKEN '" @ %word @ "'");
        }
    }

    %script.isDone = true;
}

function Script::process(%script)
{
    talk("PROCESSING");
    talk("  script tokens: " @ %script.tokens);
    
    for(%l = 0; %l < getLineCount(%script.processString); %l++)
    {
        %processLine = getLine(%script.processString, %l);

        if(getWord(%processLine, 0) $= "script")
        {
            %script.name = getWords(%processLine, 1, 999);
            talk("  setting script name to: " @ %script.name);
        }
        else
        {
            for(%i = 0; %i < %script.tokens; %i++)
            {
                if(%script.tokenType[%i] $= "function_call")
                {
                    talk("   found - FUNC");
                }
                else if(%script.tokenType[%i] $= "group_var")
                {
                    talk("   found - group '" @ %script.tokenValue[%i] @ "'");
                }
                else if(%script.tokenType[%i] $= "local_var")
                {
                    talk("   found - local '" @ %script.tokenValue[%i] @ "'");
                }
            }
        }

        talk("    parseLine --> " @ %processLine);
    }
}