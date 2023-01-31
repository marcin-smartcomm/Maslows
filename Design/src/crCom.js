let _webSocket;
let webSocketServerIPAddress = "172.16.30.101"
//let webSocketServerIPAddress = "192.168.1.243"

const indexedDB = 
    window.indexedDB ||
    window.mozIndexedDB ||
    window.webkitIndexedDB ||
    window.msIndexedDB ||
    window.shimIndexedDB;

const addressDB = indexedDB.open("address", 2)

addressDB.onerror = function (event) {
    console.log("An error occured with IndexedDB")
}

addressDB.onupgradeneeded = function() {
    const db = addressDB.result
    const store = db.createObjectStore("address", { keyPath: "id"})
    store.createIndex("addressURL", ["url"], { unique: false})
}

addressDB.onsuccess = function() {
    const db = addressDB.result
    const transaction = db.transaction("address", "readwrite")

    const store = transaction.objectStore("address")
    const idQuery = store.get(1)

    idQuery.onsuccess = function () {
        if(idQuery.result == undefined)
            //_webSocket = new WebSocket("ws://172.16.30.101:50000")
            _webSocket = new WebSocket("ws://" + webSocketServerIPAddress + ":50000")
        else
        {
            _webSocket = new WebSocket(idQuery.result.url)
        }
            

        _webSocket.onmessage = function(e) {
            onMessage(e);
        }
        
        _webSocket.onopen = function(e) {
            ping();
            setInterval(ping, 10000);
            socketConnected = true;
        
            if(_webSocket.url != "ws://" + webSocketServerIPAddress + ":50000/")
            {
                RequestRoomData();
                setTimeout(() => {
                    openSubpage("ScreenSaver");
                }, 100);
            }
        }

        _webSocket.onerror = function(e)
        {
            console.log("error connecting");
            location.reload();
        }
    }
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

        if(connMessage != null)
        {
            connMessage.setAttribute("style", `color: ${color};`);
            connMessage.textContent = message;
        }
    }
}



function pong() {
    connStatus('controlSystemStatus', 'green', 'Connected');
    clearTimeout(tm);
}

let neighbourRoom = "";
let interval;

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
    {}
    else if(value.includes("TV Connected"))
    {
        connStatus('tvStatus', 'green', 'Connected'); 
        tvConnStatus = "Connected";
    }
    else if(value.includes("IPTV Disconnected"))
    {}
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

    else if (value.includes("AvailableLocations"))
    {
        var availableLocations = value.replace('AvailableLocations ', '')
        TPLocations = availableLocations.split('|')

        openSubpage("TPSelectionPage")
    }

    else if (value.includes("FireAlarm"))
    {
        if(value.includes("True"))
        {
            openSubpage("FireAlarm")

            //in app.js
            frieAlarmState = true;
            interval = setInterval(simulateTouch, 1000)
            audio.play()
        }
        if(value.includes("False"))
        {
            openSubpage("ScreenSaver")

            //in app.js
            frieAlarmState = false;
            clearInterval(interval)
            audio.pause()
        }
    }
}

var audio = new Audio('./audio/fireAlarm.mp3')
function simulateTouch()
{
    audio.play()
    document.getElementById("fireAlarmSection").click();
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