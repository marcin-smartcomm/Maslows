let tvConnStatus = "Trying..."
let iptvConnStatus = "Trying..."

let SourcesList = {}
let SourceBtns = []
let sourceSelected = -1;
let nextRoom = 0;

let volLabel;
let volLevel = 0;

let homePageInitialized = false;

function InitializeHomeVariables()
{
    if (homePageInitialized)
        return
        
   PopulateInterface();
    for(let i = 0; i < SourcesList.length; i++)
    {
        document.getElementById(`${i}`).addEventListener('click', function(e){
            if(sourceSelected != i)
            {
                sendMessage(`SetSourceSelected:${i}`);
                ClearSourceFb();
            }
            else
            {
                if(SourcesList[sourceSelected] == "IPTV")
                {
                    openSubpage("IPTV");
                }
            }
        })
    }
    document.getElementById("roomOffBtn").addEventListener('click', function(e)
    {
        sendMessage("RoomOff")

        ClearSourceFb();
        e.target.classList.add('off-btn-active');
    })

    //Initialize Footer
    document.getElementById("volUpBtn").addEventListener('click', function()
    {
        sendMessage("Volume:+");
    })
    document.getElementById("volDownBtn").addEventListener('click', function()
    {
        sendMessage("Volume:-");
    })
    volLabel = document.getElementById("volLabel");
    volLabel.innerHTML = volLevel + "%";

    if(previousSubpage == "ScreenSaver") {
        ConnectRoom();
    }
    else
    {
        //console.log(document.getElementById("tvStatus").innerHTML);
        //in crCom.js
        //if(tvConnStatus == "Connected")
        //    connStatus('tvStatus', 'green', tvConnStatus);
        //else if( tvConnStatus == "Trying...")
        //    connStatus('tvStatus', 'black', tvConnStatus);
        //else
        //    connStatus('tvStatus', 'red', tvConnStatus);
            
        //if(iptvConnStatus == "Connected")
         //   connStatus('iptvStatus', 'green', iptvConnStatus);
        //else if( tvConnStatus == "Trying...")
         //   connStatus('iptvStatus', 'black', iptvConnStatus);
        //else
          //  connStatus('iptvStatus', 'red', iptvConnStatus);
    }

    homePageInitialized = true;
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

function AddSourcesToInterface()
{
    let btnsContainer = document.getElementById("Home-sources-container");
    let i = 0;
    SourcesList.forEach(source => {
        let sourceBtn = document.createElement("div");

        let sourceBtnLabel = document.createElement("div")
        sourceBtnLabel.innerHTML = source;
        sourceBtnLabel.style.width = "100%";

        sourceBtn.appendChild(sourceBtnLabel);
        sourceBtn.classList.add('source-btn', 'grey-btn');
        sourceBtn.id = i;
      
        btnsContainer.appendChild(sourceBtn);
        SourceBtns.push(sourceBtn);
        i++;
      });

      let sourceBtn = document.createElement("div");
      sourceBtn.innerHTML = "Room Off"
      sourceBtn.classList.add('source-btn', 'off-btn');
      sourceBtn.id = "roomOffBtn"
      btnsContainer.appendChild(sourceBtn);

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
      document.getElementById("roomOffBtn").classList.add('off-btn-active');
  }
}

function AddActiveSourceFb()
{
    if(sourceSelected > -1)
    {
        let moreOptionsMessage = document.createElement("div");
        let selectedSourceBtn = document.getElementById(sourceSelected);
    
        selectedSourceBtn.classList.remove('grey-btn-not-active');
        selectedSourceBtn.classList.add('grey-btn-active');
        if(SourcesList[sourceSelected] == "IPTV")
            moreOptionsMessage.innerHTML += "Press Again for Control";
        else
            moreOptionsMessage.innerHTML += "No Extra Options";
    
        moreOptionsMessage.style.fontSize = "20px";
    
        selectedSourceBtn.appendChild(moreOptionsMessage);
    }
}

function ClearSourceFb()
{
    try
    {
        if(sourceSelected > -1 && currentSubpage == "Home")
        {
            document.getElementById(sourceSelected).classList.remove('grey-btn-active');
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
        let roomNameContainer = document.getElementById("roomNameContainer");
        let swipeMessage = "<< Swipe left for other Room"
        
        let nextRoomMessage = document.createElement("div")
        nextRoomMessage.innerHTML = swipeMessage;
        nextRoomMessage.style.fontSize = "24px"

        roomNameContainer.appendChild(nextRoomMessage);

        nextRoom = parseInt(neighbourRoom);
    }
    if(currentSubpage == "Home")
    {
        InitializeHomeVariables()
    }
}