let TPLocations;

function InitializeTPSelectionVariables()
{
    var subpageSection = document.getElementById("tpSelectionBtnsContainer")
    
    for(let i = 0; i < TPLocations.length; i++)
    {
        let newBtn = document.createElement("div")
        newBtn.classList.add('tp-selction-btn')
        newBtn.innerHTML = TPLocations[i].split(':')[0]
        newBtn.id = TPLocations[i].split(':')[1]

        subpageSection.appendChild(newBtn);
        
        document.getElementById(newBtn.id).addEventListener('click', function()
        {
            newBtn.classList.add("tp-selction-btn-active")

            const db = addressDB.result
            const transaction = db.transaction("address", "readwrite")
            const store = transaction.objectStore("address")

            store.put({ id: 1, url: "ws://192.168.1.243:" + (50000 + parseInt(newBtn.id))})

            setTimeout(() => {
                location.reload();
            }, 1000);
        })
    }
}