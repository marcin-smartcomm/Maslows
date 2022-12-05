//Project Variables
let currentRoomSelected = 0;

let touchstartX = 0
let touchendX = 0

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
  document.addEventListener('touchstart', function()
  {
    resetTimer();
  });
  function logout() {
    openSubpage("ScreenSaver");
    sendMessage("DisconnectEquipment");
  }
  function resetTimer() {
    clearTimeout(time);
    time = setTimeout(logout, 15000)
  }
};
document.onload = inactivityTime();
    
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