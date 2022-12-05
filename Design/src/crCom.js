let _webSocket = new WebSocket('ws://192.168.1.13:50000');
//let _webSocket = new WebSocket('ws://10.10.23.134:50001');

_webSocket.onmessage = function(e) {
    onMessage(e);
}

_webSocket.onopen = function(e) {
    ping();
    setInterval(ping, 10000);
    //connStatus('controlSystemStatus', 'green', 'Connected');
    //const connMessage = document.getElementById("controlSystemStatus");
    //connMessage.setAttribute("style", `color: green;`);
    //connMessage.textContent = 'Connected';
    socketConnected = true;

    RequestRoomData();
}

function RequestRoomData()
{
    sendMessage("GetRoomName");
    sendMessage("GetSources");
    sendMessage("GetSourceSelected");
    sendMessage("GetNeighbourRoom");
    sendMessage("GetVolumeLevel");
}

function sendMessage(message)
{
    _webSocket.send("STRING[1,"+message+"]");
    //console.log(message);
}

_webSocket.onerror = function(e)
{
    console.log("error connecting");
    location.reload();
}

let socketConnected = false;
async function ping() {   
    //console.log("Websocket Ready state: "+_webSocket.readyState);
    if (_webSocket.readyState === 0 || _webSocket.readyState === 3)
    {
        socketConnected = false;
        location.reload();
    }
    
    if(socketConnected)
    {
        _webSocket.send('STRING[1,__ping__]');
    }

    tm = setTimeout(function () {
        connStatus('controlSystemStatus', 'red', 'Error');
    }, 3000);
}

function connStatus(elementID, color, message)
{
    if(currentSubpage != "ScreenSaver")
    {
        const connMessage = document.getElementById(`${elementID}`);
        connMessage.setAttribute("style", `color: ${color};`);
        connMessage.textContent = message;
    }
}



function pong() {
    connStatus('controlSystemStatus', 'green', 'Connected');
    clearTimeout(tm);
}

let neighbourRoom = "";

function onMessage(e) {
  const msg = e.data;
  const value = getBoundString_EndLastIndex(msg, ",", "]"); 
  console.log(e.data);
    if (value == '__pong__') {
        pong();
        return;
    }
    else if(value.includes("RoomName"))
    {
        let roomName = value.replace('RoomName ', '');

        //in app.js
        FilRoomName(roomName);
    }
    else if(value.includes("Sources"))
    {
        let roomSetupInfo = value.replace('Sources ', '');
        sources = roomSetupInfo.split(':');

        //in Home.js
        AddSourceBtns(sources);
    }
    else if(value.includes("SourceSelected"))
    {
        let sourceSelected = value.replace('SourceSelected ', '');

        //in Home.js
        ProcessSourceSelected(sourceSelected);
    }
    else if(value.includes("NeighbourRoom"))
    {
        neighbourRoom = value.replace('NeighbourRoom ', '');

        //in Home.js
        ProcessNeighbourRoom(neighbourRoom);
    }
    else if(value.includes("RoomSelected"))
    {
        //In app.js
        currentRoomSelected = value.split(':')[1];
    }
    else if(value.includes("RoomChanged"))
    {
        if(currentSubpage == "Home")
        {
            document.getElementById("Home-sources-container").innerHTML = "";
        }
        document.getElementById("roomNameContainer").innerHTML = "";   
        RequestRoomData();
    }
    else if(value.includes("IPTV Connected"))
    {
        connStatus('iptvStatus', 'green', 'Connected');
        iptvConnStatus = "Connected";
    }
    else if(value.includes("TV Connected"))
    {
        connStatus('tvStatus', 'green', 'Connected'); 
        tvConnStatus = "Connected";
    }
    else if(value.includes("IPTV Disconnected"))
    {
        connStatus('iptvStatus', 'red', 'Error');
        iptvConnStatus = "Error";
    }
    else if(value.includes("TV Disconnected"))
    {
        connStatus('tvStatus', 'red', 'Error'); 
        tvConnStatus = "Error";
    }
    else if(value.includes("Volume"))
    {
        let temp = value.replace('Volume ', '');

        //in Home.js
        UpdateVolumeLevel(temp);
    }
}
 
function getBoundString_EndLastIndex(msg, startChar, stopChar)
{
    let response = "";
         
    if (msg != null && msg.length > 0)
    {
        let start = msg.indexOf(startChar);
             
        if (start >= 0)
        {
            start += startChar.length;
                 
            let end = msg.lastIndexOf(stopChar);
             
            if (start < end)
            {
                response = msg.substring(start, end);
            }
        }
    }
         
    return response;
}