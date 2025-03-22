let cartProducts;
const client = JSON.parse(localStorage.getItem("client"))
async function loadCart(){
    let client = JSON.parse(localStorage.getItem("client"))
    const formData = new FormData()
    formData.append("email",client.email)
    document.getElementById("cart-products").innerHTML = ""
    const response = await fetch("http://localhost:5247/api/products/cartelements",{
            method: "POST",
            body: formData
        })
    try{
        if(!response.ok) throw new Error("Eroare la incarcare")
        cartProducts = await response.json()
        
    }catch(err){
        console.log(`Error ${err}`)
    } 
    cartProducts.forEach((cartProduct, index) => {
        document.getElementById("cart-products").innerHTML += `<div class="flex flex-row text-center items-center content-center bg-white rounded-lg mx-5 py-1 space-x-5" id=${index}>
                    <img src="data:image/png;base64,${cartProduct.productImage}" alt="" class="h-20 w-20 object-contain">
                    <h1 class="text-lg font-semibold">${cartProduct.productName}</h1>
                    
                    <div class="flex flex-col">
                        <p>Quantity:</p>
                        <div class="flex text-lg border-1 border-black/50 rounded-lg space-x-3 text-center items-center content-center">
                            <p id=quantity-${index}>${cartProduct.quantity}</p>
                            <div class="flex space-x-2 text-center">
                                <button class="p-0.5 font-bold cursor-pointer" onClick=removeQuantity(${index})>-</button>
                                <button class="p-0.5 font-bold cursor-pointer" onClick=addQuantity(${index})>+</button>
                            </div>
                        </div>
                    </div>
                    <p>${Intl.NumberFormat('de-DE', { style: 'currency', currency: 'RON' }).format(cartProduct.price)}</p>
                </div>  `
        
    });
    updatePrice()
}

function updatePrice(){
    let price = 0;
    cartProducts.forEach(cartProduct =>{
        price += cartProduct.price * cartProduct.quantity
    })
    document.getElementById("price").innerHTML = `You need to pay: ${Intl.NumberFormat('de-DE', { style: 'currency', currency: 'RON' }).format(price)}`
}

async function addQuantity(index) {
    const formData = new FormData()
    document.getElementById(`quantity-${index}`).innerHTML = cartProducts[index].quantity + 1;
    cartProducts[index].quantity++;
    formData.append("clientEmail", client.email);
    formData.append("productName", cartProducts[index].productName);
    formData.append("quantity", cartProducts[index].quantity)
    const response = await fetch("http://localhost:5247/api/products/changequantity", {
        method: "POST",
        body:formData
    })  
    try{
        if(!response.ok) throw Error("Eroare")   
    }catch(err){
        console.log(err);
    }
    updatePrice()
}
async function removeQuantity(index) {
    const formData = new FormData()
    document.getElementById(`quantity-${index}`).innerHTML = cartProducts[index].quantity - 1;
    cartProducts[index].quantity--;
    if(cartProducts[index].quantity == 0){
        document.getElementById("cart-products").removeChild(document.getElementById(`${index}`))  
    }
    formData.append("clientEmail", client.email);
    formData.append("productName", cartProducts[index].productName);
    formData.append("quantity", cartProducts[index].quantity)
    const response = await fetch("http://localhost:5247/api/products/changequantity", {
        method: "POST",
        body:formData
    })  
    try{
        if(!response.ok) throw Error("Eroare")   
    }catch(err){
        console.log(err);
    }
    updatePrice()
}
loadCart()
