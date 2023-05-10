let tvConnStatus = "Trying..."
let iptvConnStatus = "Trying..."

let SourcesList = {}
let SourceBtns = []
let sourceSelected = 0;
let previousSourceSelected = 0;
let sourceSelectionDisabled = false;

let volLabel;
let volLevel = 0;
let muteState = false;

let homePageInitialized = false;

function InitializeHomeVariables()
{
    if (homePageInitialized)
        return
        
   PopulateInterface();
    for(let i = 0; i < SourcesList.length; i++)
    {
        document.getElementById(`${i}`).addEventListener('click', function(e){
            if(sourceSelectionDisabled)
                return;
        
            if(sourceSelected != i)
            {
                sendMessage(`SetSourceSelected:${i}`);
                ClearSourceFb();
            }
            else
            {
                if(SourcesList[sourceSelected] == "TV")
                {
                    openSubpage("TV");
                }
            }
        })
    }
    
    const timeNow = new Date();
    if(
        (timeNow.getHours() >= 20 && timeNow.getMinutes() >= 30) ||
        timeNow.getHours() > 20 ||
        timeNow.getHours() <= 6
    )
    {
        document.getElementById("roomOffBtn").addEventListener('click', function(e)
        {
            if(sourceSelectionDisabled)
                return;
        
            sendMessage("RoomOff")

            ClearSourceFb();
            e.target.classList.add('off-btn-active');
        })
    }

    //Hidden Page
    document.getElementById("roomNameContainer").addEventListener("touchstart", function() {
        HoldTimer();
    })
    document.getElementById("roomNameContainer").addEventListener("touchend", function() {
        holding = false;
    })

    //Initialize Footer
    document.getElementById("volUpBtn").addEventListener('touchstart', function()
    {
        document.getElementById("volUpBtn").classList.remove("vol-btn-unpressed")
        document.getElementById("volUpBtn").classList.add("vol-btn-pressed")
    })
    document.getElementById("volUpBtn").addEventListener('touchend', function()
    {
        document.getElementById("volUpBtn").classList.remove("vol-btn-pressed")
        document.getElementById("volUpBtn").classList.add("vol-btn-unpressed")
        sendMessage("Volume:+");
    })

    document.getElementById("volDownBtn").addEventListener('touchstart', function()
    {
        document.getElementById("volDownBtn").classList.remove("vol-btn-unpressed")
        document.getElementById("volDownBtn").classList.add("vol-btn-pressed")
    })
    document.getElementById("volDownBtn").addEventListener('touchend', function()
    {
        document.getElementById("volDownBtn").classList.add("vol-btn-unpressed")
        document.getElementById("volDownBtn").classList.remove("vol-btn-pressed")
        sendMessage("Volume:-");
    })

    document.getElementById("muteBtn").addEventListener('touchstart', function()
    {
        sendMessage("Volume:Mute");
    })

    volLabel = document.getElementById("volLabel");
    volLabel.innerHTML = volLevel + "%";
    UpdateMuteStateUI();

    if(previousSubpage == "ScreenSaver") {
        ConnectRoom();
    }

    //ProcessNeighbourRoom(neighbourRoom);

    homePageInitialized = true;
}

let holding = false;

function HoldTimer()
{
    holding = true;
    let i = 0;
    const holdingInterval = setInterval(() => {
        if(i > 600)
        {
            if(holding)
            {
                if(!sourceSelectionDisabled)
                    openSubpage("HiddenSettings");
            }
            
            holding = false;
            clearInterval(holdingInterval);
        }
        else
        {
            if(holding)
            {
                //holding and timer not passed
                i++
            }
            else
            {
                //stopped holding and timer not passed
                clearInterval(holdingInterval);
            }
        }
    }, 7);
}


function ConnectRoom()
{
    //in crCom.js
    connStatus('tvStatus', 'black', "Trying...");
    connStatus('iptvStatus', 'black', "Trying...");

    //in app.js
    setTimeout(() => {
        sendMessage("ConnectEquipment");
    }, 300);
}

function UpdateVolumeLevel(level)
{
    volLevel = level;
    if(currentSubpage != "ScreenSaver")
        volLabel.innerHTML = level + "%";
}

function UpdateMuteState(newState)
{
    muteState = newState;
    if(currentSubpage != "ScreenSaver")
        UpdateMuteStateUI();
}

function UpdateMuteStateUI()
{
    if(muteState == "True")
    {
        document.getElementById("muteBtn").classList.remove('unmuted')
        document.getElementById("muteBtn").classList.add('muted')
    }
    else
    {
        document.getElementById("muteBtn").classList.remove('muted')
        document.getElementById("muteBtn").classList.add('unmuted')
    }
}

function AddSourcesToInterface()
{
    let btnsContainer = document.getElementById("Home-sources-container");
    let i = 0;
    SourcesList.forEach(source => {
        let sourceBtn = document.createElement("div");

        let sourceBtnLabel = document.createElement("div")
        sourceBtnLabel.innerHTML = source;
        sourceBtnLabel.style.width = "100%";
        sourceBtnLabel.style.color = "black";

        sourceBtn.appendChild(sourceBtnLabel);
        sourceBtn.classList.add('source-btn', 'grey-btn');
        sourceBtn.id = i;
      
        btnsContainer.appendChild(sourceBtn);
        SourceBtns.push(sourceBtn);
        i++;
      });

    const timeNow = new Date();
    if(
    (timeNow.getHours() >= 20 && timeNow.getMinutes() >= 30) ||
    timeNow.getHours() > 20 ||
    timeNow.getHours() <= 6
    )
    {
        let sourceBtn = document.createElement("div");
        sourceBtn.innerHTML = "ROOM OFF"
        sourceBtn.classList.add('source-btn', 'off-btn');
        sourceBtn.id = "roomOffBtn"
        btnsContainer.appendChild(sourceBtn);
    }

    AddActiveSourceFb();
}

function PopulateInterface()
{
    AddSourcesToInterface();

  if(parseInt(sourceSelected) != -1)
  {
      document.getElementById(sourceSelected).classList.add('grey-btn-active');
  }
  else
  {
    const timeNow = new Date();
    if(
    (timeNow.getHours() >= 20 && timeNow.getMinutes() >= 30) ||
    timeNow.getHours() > 20 ||
    timeNow.getHours() <= 6
    )
    {
        document.getElementById("roomOffBtn").classList.add('off-btn-active');
    }
  }
}

function AddActiveSourceFb()
{
    if(previousSourceSelected == -1 && sourceSelected != -1)
    {
        disableSourceSelection(sourceSelected);
    }

    if(sourceSelected > -1)
    {
        let selectedSourceBtn = document.getElementById(sourceSelected);
    
        selectedSourceBtn.classList.remove('grey-btn-not-active');

        if(!sourceSelectionDisabled)
            selectedSourceBtn.classList.add('grey-btn-active');
        else
        {
            selectedSourceBtn.classList.add('grey-btn-activating');
        }

        if(!sourceSelectionDisabled)
        {
            AddExtraText(selectedSourceBtn);
        }
    }
    previousSourceSelected = sourceSelected;
}

function AddExtraText(sourceBtn)
{
    let moreOptionsMessage = document.createElement("div");
    if(SourcesList[sourceSelected] == "TV")
        moreOptionsMessage.innerHTML += "PRESS AGAIN FOR CONTROL";
    else
        moreOptionsMessage.innerHTML += "NO EXTRA OPTIONS";

    moreOptionsMessage.classList.add('more-options-message-label')

    sourceBtn.appendChild(moreOptionsMessage);
}

function disableSourceSelection(sSelected)
{
    let currentSourceName = document.getElementById(sSelected).firstChild.innerHTML;
    document.getElementById(sSelected).firstChild.innerHTML = "SYSTEM INITIALIZING..";

    sourceSelectionDisabled = true;
    setTimeout(() => {
        document.getElementById(sSelected).firstChild.innerHTML = currentSourceName;
        AddExtraText(document.getElementById(sSelected))
        sourceSelectionDisabled = false;
    }, 60000);
}

function ClearSourceFb()
{
    try
    {
        if(sourceSelected > -1 && currentSubpage == "Home")
        {
            document.getElementById(sourceSelected).classList.remove('grey-btn-active');
            document.getElementById(sourceSelected).classList.remove('grey-btn-activating');
            document.getElementById(sourceSelected).classList.add('grey-btn-not-active');
            
            let sourceBtnLabel = document.createElement("div")
            sourceBtnLabel.innerHTML = SourcesList[sourceSelected];
            sourceBtnLabel.style.width = "100%";

            document.getElementById(sourceSelected).innerHTML = "";
            document.getElementById(sourceSelected).appendChild(sourceBtnLabel);
            
        }
        document.getElementById("roomOffBtn").classList.remove('off-btn-active')
    }
    catch
    {

    }
}

function AddSourceBtns(sources)
{
  SourcesList = sources;
}

function ProcessSourceSelected(ss)
{
    if(parseInt(ss) == -1 && currentSubpage == "Home")
    {
        ClearSourceFb();
        try
        {
            document.getElementById("roomOffBtn").classList.add('off-btn-active')
        }
        catch{}
    }
    else if (parseInt(ss) == -1 && currentSubpage != "Home")
    {
        previousSourceSelected = sourceSelected;
        openSubpage("Home");
        ClearSourceFb();
        try
        {
            document.getElementById("roomOffBtn").classList.add('off-btn-active')
        }
        catch{}
    }
    else
    {
        previousSourceSelected = sourceSelected;
        ClearSourceFb();
    }
    
    sourceSelected = parseInt(ss);
    if(currentSubpage == "Home" && document.getElementById(sourceSelected) != null)
        AddActiveSourceFb();
}

function ProcessNeighbourRoom(neighbourRoom)
{
    if(parseInt(neighbourRoom) > -1)
    {
        let projectBottom = document.getElementById("projectBottom");
        let settingsButton = document.createElement("div");
        settingsButton.id = "settingsButton"
        settingsButton.classList.add('centered', 'fa-solid', 'fa-gear', 'fa-4x')
        
        projectBottom.appendChild(settingsButton);
        document.getElementById("settingsButton").addEventListener('touchend', function(){
            if(!sourceSelectionDisabled)
                openSubpage("Settings");
        })
    }
    else
    {
        document.getElementById("volumeControlsContainer").style.marginLeft = "0px"
    }
}