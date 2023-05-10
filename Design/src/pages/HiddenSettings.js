function InitializeHiddenSettingsVariables()
{
    document.getElementById("roomOffBtn").addEventListener('click', function(e)
    {
        sendMessage("RoomOff")
        e.target.classList.add('off-btn-active');
    })

    document.getElementById("skyControl").addEventListener('click', function(e)
    {
        openSubpage("Sky-Main")
    })

    document.getElementById("homeBtn").addEventListener('click', function(e)
    {
        openSubpage("Home")
    })

    if(neighbourRoom > -1)
    {
        let skyControlBtn = document.getElementById("skyControl")
        let settingsOptionsContainer = document.getElementById("settingsOptionsContainer");
        let zoneDivision = document.createElement("div")
        zoneDivision.id = "zoneDivBtn"
        zoneDivision.innerHTML = "ZONE DIVISION"
        zoneDivision.classList.add('source-btn', 'grey-btn')

        settingsOptionsContainer.insertBefore(zoneDivision, skyControlBtn)

        document.getElementById("zoneDivBtn").addEventListener('touchend', function(){
            if(!sourceSelectionDisabled)
                openSubpage("Settings");
        })
    }
}