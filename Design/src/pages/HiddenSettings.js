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
}