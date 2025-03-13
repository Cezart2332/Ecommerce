async function registerClient(){
    firstName = document.getElementById("firstName").value
    lastName = document.getElementById("lastName").value
    email = document.getElementById("email").value
    password = document.getElementById("password").value

    if(firstName.trim() == "")
    {
        return alert("You need to enter a First Name")
    }
    if(lastName.trim() == "")
    {
        return alert("You need to enter a Last Name")
    }
    if(email.trim() == "")
    {
        return alert("You need to enter a email")
    }
    if(password.trim() == "")
    {
        return alert("You need to enter a password")
    }

    const newClient = {
        firstName : firstName,
        lastName : lastName,
        email : email,
        password : password
    }
    fetch("http://localhost:5247/api/clients", {
        method: "POST",
        headers:{
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(newClient)
    }).then(response => {
        if (!response.ok) {
          throw new Error('Network response was not ok');
        }
        return response.json();
      })
      .then(data => console.log('Success:', data))
      .catch(error => console.error('Error:', error));
}
async function loginClient()
{
    email = document.getElementById("email").value
    password = document.getElementById("password").value
    const loginModel = {
        email: email,
        password: password
    }
    fetch("http://localhost:5247/api/clients/login", {
        method: "POST",
        headers:{
            "Content-Type": "application/json"
        },
        body: JSON.stringify(loginModel)
    }).then(response =>{
        if(!response.ok){
            throw new Error("Eroare")
        }
        return response.json()
    })
    .then(data => 
        localStorage.setItem("client",JSON.stringify(data)),
        localStorage.setItem("loggedIn", JSON.stringify(true)))
    .catch(error => (console.error('Error', error)))
}