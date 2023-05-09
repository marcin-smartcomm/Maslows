let prevColor = "";

let btnCount = 31;
function InitializeIPTVVariables()
{
  document.getElementById("iptvReturn").addEventListener('click', function() {
    openSubpage("Home");
  })

  for(let i = 0; i < 9; i ++)
  {
    document.getElementById(`btn${i}`).addEventListener('click', function() {
      sendMessage(`SourceBtn:${i}`);
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

  document.getElementById("channelSelect").addEventListener('click', function()
  {
    openSubpage("IPTVNumpad");
    InitializeIPTVNumpad();
  })
}

function InitializeIPTVNumpad()
{
  for(let i = 9; i < 19; i ++)
  {
    document.getElementById(`btn${i}`).addEventListener('click', function() {
      sendMessage(`SourceBtn:${i}`);
    })

      document.getElementById(`btn${i}`).addEventListener('touchstart', function() {
        document.getElementById(`btn${i}`).classList.add("btn-pressed")
        document.getElementById(`btn${i}`).classList.remove("iptv-red")
        document.getElementById(`btn${i}`).classList.remove("iptv-green")
        document.getElementById(`btn${i}`).classList.remove("iptv-blue")
        document.getElementById(`btn${i}`).classList.remove("iptv-yellow")
      })
      document.getElementById(`btn${i}`).addEventListener('touchend', function() {
        document.getElementById(`btn${i}`).classList.remove("btn-pressed")
        if(i == 19)
          document.getElementById(`btn${i}`).classList.add("iptv-red")
        if(i == 20)
          document.getElementById(`btn${i}`).classList.add("iptv-green")
        if(i == 21)
          document.getElementById(`btn${i}`).classList.add("iptv-yellow")
        if(i == 22)
          document.getElementById(`btn${i}`).classList.add("iptv-blue")
      })
  }

  document.getElementById("channelSelect").addEventListener('click', function()
  {
    openSubpage("TV");
  })
}