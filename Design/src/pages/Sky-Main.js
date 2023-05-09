function InitializeSkyMainVariables()
{
    document.getElementById("skyReturn").addEventListener('click', function() {
      openSubpage("HiddenSettings");
    })
  
    for(let i = 0; i < 50; i ++)
    {
        try{
            document.getElementById(`btn${i}`).addEventListener('click', function() {
              sendMessage(`SkyBtn:${i}`);
            })
            document.getElementById(`btn${i}`).addEventListener('touchstart', function() {
              prevColor = document.getElementById(`btn${i}`).style.backgroundColor;
              document.getElementById(`btn${i}`).classList.add("btn-pressed")
            })
            document.getElementById(`btn${i}`).addEventListener('touchend', function() {
              document.getElementById(`btn${i}`).classList.remove("btn-pressed")
              prevColor = document.getElementById(`btn${i}`).style.backgroundColor = prevColor;
            })
        }
        catch(ex)
        {}
    }
  
    document.getElementById("channelSelect").addEventListener('click', function()
    {
      openSubpage("Sky-Numpad");
    })
}

function InitializeSkyNumpadVariables()
{
  for(let i = 9; i < 19; i ++)
  {
    document.getElementById(`btn${i}`).addEventListener('click', function() {
      sendMessage(`SkyBtn:${i}`);
    })

      document.getElementById(`btn${i}`).addEventListener('touchstart', function() {
        document.getElementById(`btn${i}`).classList.add("btn-pressed")
      })
      document.getElementById(`btn${i}`).addEventListener('touchend', function() {
        document.getElementById(`btn${i}`).classList.remove("btn-pressed")
      })
  }

  document.getElementById("channelSelect").addEventListener('click', function()
  {
    openSubpage("Sky-Main");
  })
}