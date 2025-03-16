async function loadProducts()
{
    const response = await fetch("http://localhost:5247/api/products")
    products = await response.json()
    products.forEach((product,index) => {
        console.log(product)
        document.getElementById("products").innerHTML = `
         <div class="bg-black/20 shadow-lg shadow-black/40 w-50 h-80 rounded-3xl text-white">
            <div class="flex bg-amber-200 w-7 h-7 rounded-4xl translate-x-2 translate-y-4 items-center justify-center content-center    ">
                <a href=""><i class="fa-solid fa-thumbs-up h-4 w-4 "></i></a>
            </div>
            <img src="data:image/png;base64,${product.productImage}" alt="" class="w-60 h-40 object-contain">
            <h1 class="text-center mt-3 text-xl font-semibold">${product.productName}</h1>
            <div class="flex flex-col justify-center items-center">
                <p class="text-lg font-semibold">Pret : ${Intl.NumberFormat('de-DE', { style: 'currency', currency: 'RON' }).format(product.price)}</p>
                <button class="bg-amber-600 rounded-lg p-1 mt-3 px-2 cursor-pointer transition-all duration-400 hover:bg-amber-600/30 " onclick=addCart(${index})>Adauga in cos</button>
            </div>
        </div>
        `
    });
}
loadProducts()

const savedProducts = JSON.parse(localStorage.getItem("savedProcuts")) || []

function addCart(index){
    const isAlreadyInCart = savedProducts.some(
        (savedProduct) => savedProduct.productName === products[index].productName
    )
    if(isAlreadyInCart){
        return alert("Acest produs este deja in cos!")
    }
    savedProducts.push(products[index])
    localStorage.setItem("savedProducts", savedProducts)
    return alert("Ai adaugat cu succes un produs nou!")
}