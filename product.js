document.getElementById("products").innerHTML = "";
let products;
const urlParams = new URLSearchParams(window.location.search);
const category = urlParams.get("category") || "all"; 
async function loadProducts()
{

    const response = await fetch("http://localhost:5247/api/products")
    products = await response.json()
    products.forEach((product,index) => {
        if(product.category === category || category === "all"){
            document.getElementById("products").innerHTML += `
          <div class="bg-black/30 shadow-lg shadow-black/20 w-50 rounded-3xl text-white pb-2">
           <div class="flex bg-amber-200 w-7 h-7 rounded-4xl translate-x-2 translate-y-4 items-center justify-center content-center    ">
               <a href=""><i class="fa-solid fa-thumbs-up h-4 w-4 "></i></a>
           </div>
           <img src="data:image/png;base64,${product.productImage}" alt="" class="w-60 h-40 object-contain">
           <h1 class="text-center mt-3 text-xl font-semibold">${product.productName} </h1>
           <div class="flex flex-col justify-center items-center">
           <p class="text-lg font-semibold">Pret : ${Intl.NumberFormat('de-DE', { style: 'currency', currency: 'RON' }).format(product.price)}</p>
               <p class="text-sm font-semibold text-gray-300">Rating: ${product.rating}</p>
               <p class="text-md font-semibold">Stock: ${product.stock}</p>
               <button class="bg-amber-600 rounded-lg p-1 mt-3 px-2 cursor-pointer transition-all duration-400 hover:bg-amber-600/30" onclick="addCart(${index})">Adauga in cos</button>
           </div>

       </div>
           `

        }
    });
  
}
loadProducts()

async function addCart(index){
    let client = JSON.parse(localStorage.getItem("client"))
    const formData = new FormData();
    formData.append("clientEmail",client.email)
    formData.append("productName",products[index].productName)
    formData.append("quantity",1);
    console.log(client.email)
    const response = await fetch("http://localhost:5247/api/products/cart",{
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

function filterProducts(){
    document.getElementById("products").innerHTML = "";
    let stock = document.getElementById("stock").checked;
    let lowerPrice = document.getElementById("lower-price").value
    let higherPrice = document.getElementById("higher-price").value
    let lowerRating = document.getElementById("lower-rating").value
    let higherRating = document.getElementById("higher-rating").value
    let filteredProducts = products;
    if(stock){
        filteredProducts = filteredProducts.filter((product) => product.stock > 0)
    }
    let minPrice = lowerPrice.trim() === "" ? 0 : parseInt(lowerPrice)
    let maxPrice = higherPrice.trim() === "" ? Infinity : parseInt(higherPrice)
    let minRating = lowerRating.trim() === "" ? 0 : parseInt(lowerRating)
    let maxRating = higherRating.trim() === "" ? Infinity : parseInt(higherRating)
    filteredProducts = filteredProducts.filter((product) => {
        return (product.price >= minPrice) &&
        (product.price <= maxPrice)
    })
    filteredProducts = filteredProducts.filter((product) => {
       return (product.rating >= minRating) &&
        (product.rating <= maxRating)
    })
    filteredProducts.forEach((product,index) => {
        console.log(product)
        if(product.category === category || category === "all"){
            document.getElementById("products").innerHTML += `
          <div class="bg-black/30 shadow-lg shadow-black/20 w-50 rounded-3xl text-white pb-2">
           <div class="flex bg-amber-200 w-7 h-7 rounded-4xl translate-x-2 translate-y-4 items-center justify-center content-center    ">
               <a href=""><i class="fa-solid fa-thumbs-up h-4 w-4 "></i></a>
           </div>
           <img src="data:image/png;base64,${product.productImage}" alt="" class="w-60 h-40 object-contain">
           <h1 class="text-center mt-3 text-xl font-semibold">${product.productName} </h1>
           <div class="flex flex-col justify-center items-center">
           <p class="text-lg font-semibold">Pret : ${Intl.NumberFormat('de-DE', { style: 'currency', currency: 'RON' }).format(product.price)}</p>
               <p class="text-sm font-semibold text-gray-300">Rating: ${product.rating}</p>
               <p class="text-md font-semibold">Stock: ${product.stock}</p>
               <button class="bg-amber-600 rounded-lg p-1 mt-3 px-2 cursor-pointer transition-all duration-400 hover:bg-amber-600/30" onclick="addCart(${index})">Adauga in cos</button>
           </div>
        </div>
           `
        }
    })
}