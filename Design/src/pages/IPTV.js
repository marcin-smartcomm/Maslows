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