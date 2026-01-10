if(!isObject(EventScript))
{
    new ScriptGroup(EventScript)
    {
        class = "EventGroupSO";
    };
}

function fxDtsBrick::runScript(%brick, %scriptString, %client)
{
    if(!%client.isAdmin)
    {
        return false;
    }

    %tokenHandler = eventScript_tokenize(%scriptString);
    if(isObject(%tokenHandler))
    {
        %tokenHandler.process();
        %tokenHandler.schedule(0, delete);
    }

    return true;
}
registerOutputEvent("fxDtsBrick", "runScript", "string 50 100");

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
            // talk("SETTING LOCAL_VAR '" @ %word @ "': " @ %nextWord);
        }
        else if(getSubStr(%word, 0, 1) $= "!")
        {
            %token = getSubStr(%word, 1, 999);

            %script.token[%script.tokens] = %token; 
            %script.tokenType[%script.tokens] = "group_var";
            %script.tokenValue[%script.tokens] = %nextWord;
            %script.tokens++;

            %skipNextWord = 1;
            // talk("SETTING GROUP_VAR '" @ %word @ "': " @ %nextWord);
        }
        else if(%word $= "call")
        {
            // talk("CALL '" @ %nextWord @ "' for EventScript");
            
            %script.token[%script.tokens] = "function"; 
            %script.tokenType[%script.tokens] = "function_call";
            %script.tokenValue[%script.tokens] = %nextWord;
            %script.tokens++;

            %skipNextWord = true;
        }
        else
        {
            talk("UNKNOWN TOKEN '" @ %word @ "'");
        }
    }

    %script.isDone = true;
}

function Script::process(%script)
{
    
}