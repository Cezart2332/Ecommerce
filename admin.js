async function uploadProduct(){


    const fileInput = document.getElementById("image")
    const file = fileInput.files[0];

    const formData = new FormData();
    formData.append("productName", document.getElementById("productName").value)
    formData.append("productDescription", document.getElementById("productDescription").value)
    formData.append("price", document.getElementById("price").value)
    formData.append("image", file)
    formData.append("category", document.getElementById("category").value)
    formData.append("category", document.getElementById("stock").value)
    const response = await fetch("http://localhost:5247/api/products/upload",{
        method: "POST",
        body: formData,
    })
    try{
        if(!response.ok) throw new Error("Eroare la incarcare")
        const data = await response.json()
        console.log(`Succes: ${data}`)
    }catch(error){
        console.log(`Eroare: ${error}`)
    }

    

}