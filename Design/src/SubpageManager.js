let backBtn;
let previousSubpage;
let currentSubpage;
let blankOutBtnsVis = false;

function openSubpage(file)
{
  document.getElementById("subpageSection").classList.add("transitionIn")
  
  if(currentSubpage != null)
    previousSubpage = currentSubpage;
  else
    previousSubpage = "ScreenSaver";

  currentSubpage = file;

  var rawFile = new XMLHttpRequest();
  rawFile.open("GET", './pages/'+file+'.html', false);
  rawFile.onreadystatechange = function ()
  {
      if(rawFile.readyState === 4)
      {
          if(rawFile.status === 200 || rawFile.status == 0)
          {
              var allText = rawFile.responseText;
              document.querySelector('#subpageSection').innerHTML = allText;
          }
      }
  }
  rawFile.send(null);
  rawFile.DONE;
  
  InitializeSubpageVariables(file);

  setTimeout(ClearTransition, 500);
}

function showFooter(visibilityState)
{
  if(visibilityState)
  {
    var footer = new XMLHttpRequest();
    footer.open("GET", './pages/footer.html', false);
    footer.onreadystatechange = function ()
    {
        if(footer.readyState === 4)
        {
            if(footer.status === 200 || footer.status == 0)
            {
                var allText = footer.responseText;
                document.querySelector('#projectBottom').innerHTML = allText;
            }
        }
    }
    footer.send(null);
    footer.DONE;    
  }
  else
  {
    document.querySelector('#projectBottom').innerHTML = "";
  }
}

function ClearTransition()
{
  document.getElementById("subpageSection").classList.remove("transitionIn");
}

function InitializeSubpageVariables(pageToInitialize)
{
  if(pageToInitialize != "Home")
    homePageInitialized = false

  if(pageToInitialize == "ScreenSaver")
  {
    InitializeScreenSaverVariables();
    showFooter(false);
  }
  if(pageToInitialize == "Home")
  {
    showFooter(true);
    InitializeHomeVariables();

    //in crCom.js
    ping();
  }
  if(pageToInitialize == "IPTV")
  {
    InitializeIPTVVariables();
  }
}