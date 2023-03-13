function InitializeScreenSaverVariables()
{
  document.getElementById("screenSaverImg").addEventListener('touchstart', function() {
    document.getElementById("screenSaverImg").classList.add("blurred");
  })
  document.getElementById("screenSaverImg").addEventListener('touchend', function() {
    setTimeout(() => {
      openSubpage("Home");
    }, 200);
  })
}