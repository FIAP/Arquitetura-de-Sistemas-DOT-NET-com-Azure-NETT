var axios = require("axios");

var helper = require("./infra/helper");

let url = 'https://dog.ceo/api/breeds/image/random';


axios.get(url).then((response) => {
    let urlImage = response.data.message;
    console.log(urlImage);
    console.log(urlImage.substring(urlImage.lastIndexOf('/')+1));
   // helper.downloadImageToUrl(response.data.message, "./temp/" + crypto.randomUUID())
}).catch((error) => console.dir(error));

