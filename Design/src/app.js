//Project Variables
let currentRoomSelected = 0;
let roomNametouchCount = 0;

let touchstartX = 0
let touchendX = 0

let frieAlarmState = false;

document.onload = openSubpage("ScreenSaver");

document.getElementById("projectTop").addEventListener('touchstart', e => {
  touchstartX = e.changedTouches[0].screenX
})

document.getElementById("projectTop").addEventListener('touchend', e => {
  touchendX = e.changedTouches[0].screenX

  checkDirection()
})

function FilRoomName(roomName)
{
  let roomNameContainer = document.getElementById("roomNameContainer")
  let roomNameTextContainer = document.createElement("div")
  roomNameTextContainer.innerHTML = roomName;
  roomNameTextContainer.style.width = "100%"
  roomNameTextContainer.style.textAlign = "center"

  roomNameContainer.appendChild(roomNameTextContainer);
}

let inactivityTime = function() {
  let time;
  document.addEventListener('touchstart', function(e)
  {
    if(_webSocket.url != "ws://" + webSocketServerIPAddress + ":50000/")
    {
      if(e.target.id == "roomNameContainer")
        roomNametouchCount = roomNametouchCount + 1;
      else
        roomNametouchCount = 0;

      if(roomNametouchCount == 10)
      {
        const db = addressDB.result
        const transaction = db.transaction("address", "readwrite")
        const store = transaction.objectStore("address")

        store.put({ id: 1, url: "ws://" + webSocketServerIPAddress + ":50000"})

        setTimeout(() => {
          location.reload();
      }, 1000);
      }

      resetTimer();
    }
  });
  function logout() {
    if(!frieAlarmState)
    {
      openSubpage("ScreenSaver");
      sendMessage("DisconnectEquipment");
    }
  }
  function resetTimer() {
    clearTimeout(time);
    time = setTimeout(logout, 120000)
  }
};
document.onload = PanelBoot();

function PanelBoot()
{
  inactivityTime();
}
    
function checkDirection() {
  if(parseInt(neighbourRoom) > -1)
  {
    if (touchendX < touchstartX)
    {
      homePageInitialized = false;
      sendMessage('DisconnectEquipment');
      sendMessage(`RoomChange:${nextRoom}`);
    }
  }
}