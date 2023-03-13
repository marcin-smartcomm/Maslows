function InitializeSettingsVariables()
{
    document.getElementById("joinSeperateButton").addEventListener('click', function(){
        if(document.getElementById("joinSeperateButton").innerHTML.includes("Join"))
            sendMessage("JoinRooms")
        else if(document.getElementById("joinSeperateButton").innerHTML.includes("Seperate"))
            sendMessage("SeperateRooms")
    })
    document.getElementById("returnBtn").addEventListener('click', function(){
        openSubpage("Home")
    })

    document.getElementById("room1Box").innerHTML = CurrentRoomName;
    document.getElementById("room2Box").innerHTML = neighbourRoomName;

    updateJoinedState();
}

function updateJoinedState()
{
    joinSepBtn = document.getElementById("joinSeperateButton");

    if(JoinedState)
    {
        joinSepBtn.innerHTML = "Seperate Rooms"
        document.getElementById("room1Box").classList.add('joinedBottom');
        document.getElementById("room2Box").classList.add('joinedTop');

        document.getElementById("room1Box").classList.remove('seperateBottom');
        document.getElementById("room2Box").classList.remove('seperateTop');
    }
    else
    {
        joinSepBtn.innerHTML = "Join Rooms"
        document.getElementById("room1Box").classList.remove('joinedBottom');
        document.getElementById("room2Box").classList.remove('joinedTop');

        document.getElementById("room1Box").classList.add('seperateBottom');
        document.getElementById("room2Box").classList.add('seperateTop');
    }
}