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
      document.getElementById(`btn${i}`).classList.add("btn-pressed")
      document.getElementById(`btn${i}`).classList.remove("btn-unpressed")
    })
    document.getElementById(`btn${i}`).addEventListener('touchend', function() {
      document.getElementById(`btn${i}`).classList.remove("btn-pressed")
      document.getElementById(`btn${i}`).classList.add("btn-unpressed")
    })
  }

  document.getElementById("channelSelect").addEventListener('click', function()
  {
    openSubpage("IPTVNumpad");
    InitializeIPTVNumpad();
  })

//swipe read
/*
  document.getElementById("iptvSubpageSection").addEventListener('touchstart', e => {
    IPTVtouchstartX = e.changedTouches[0].screenX
  })
  
  document.getElementById("iptvSubpageSection").addEventListener('touchend', e => {
    IPTVtouchendX = e.changedTouches[0].screenX
  
    IPTVcheckDirection()
  })
  */
}


function InitializeIPTVNumpad()
{
  document.getElementById("iptvReturn").addEventListener('click', function() {
    openSubpage("Home");
  })

  for(let i = 9; i < 23; i ++)
  {
    document.getElementById(`btn${i}`).addEventListener('click', function() {
      sendMessage(`SourceBtn:${i}`);
    })

    if(!document.getElementById(`btn${i}`).classList.contains("iptv-round-btn"))
    {
      document.getElementById(`btn${i}`).addEventListener('touchstart', function() {
        document.getElementById(`btn${i}`).classList.add("btn-pressed")
        document.getElementById(`btn${i}`).classList.remove("btn-unpressed")
      })
      document.getElementById(`btn${i}`).addEventListener('touchend', function() {
        document.getElementById(`btn${i}`).classList.remove("btn-pressed")
        document.getElementById(`btn${i}`).classList.add("btn-unpressed")
      })
    }
    else
    {
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
  }

  document.getElementById("channelSelect").addEventListener('click', function()
  {
    openSubpage("TV");
  })

//swipe read
/*
  document.getElementById("iptvSubpageSection").addEventListener('touchstart', e => {
    IPTVtouchstartX = e.changedTouches[0].screenX
  })
  
  document.getElementById("iptvSubpageSection").addEventListener('touchend', e => {
    IPTVtouchendX = e.changedTouches[0].screenX
  
    IPTVcheckDirection()
  })
  */
}

//swipe read
/*
let IPTVtouchstartX = 0
let IPTVtouchendX = 0
    
function IPTVcheckDirection() {
  if (IPTVtouchendX < IPTVtouchstartX)
  {
    if(currentSubpage == "TV")
    {
      openSubpage("IPTVNumpad");
      InitializeIPTVNumpad();
    }
    else
      openSubpage("TV");
  }
}
*/