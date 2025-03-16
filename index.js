function loadPage(){
    if(JSON.parse(localStorage.getItem("loggedIn"))){
        let client = JSON.parse(localStorage.getItem("client"))
        document.getElementById("account").innerHTML = `<i class="fa-solid fa-circle-user"></i>${client.firstName} ${client.lastName}`
        document.getElementById("account").setAttribute("href", "account.html")
    }
}
loadPage()