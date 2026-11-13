
const db = require('./config/db');
const axios = require('axios');
const cheerio = require('cheerio');

const helper = require('./infra/helper');
const characterRepository = require('./repository/characterRepository');

axios.get('https://www.hbo.com/game-of-thrones/cast-and-crew').then((response) => {

    if (response.status == 200) {
        let $ = cheerio.load(response.data);

        $('div.row div.col.col-custom img').each((index, element) => {

            const thumb = $(element).attr('src');
      

            const name = $(element).attr('alt');
            const newName = name.toLowerCase().trim().replace(' ', '_') + ".jpg";

            helper.downloadImageToUrl(thumb,newName);

            let characters = {
                name: name,
                thumb: name.toLowerCase().trim().replace(' ', '_') + ".jpeg"
            };

            characterRepository.create(characters);



        });






        // $('section div.w-100 a').each(function (i, element) {
        //     let a = $(this);
        //     let name = a.parent().find("span").first().text();
        //     let thumb = a.find('img').attr('src');
        //     helper.downloadImageToUrl('https://www.hbo.com' + thumb, './uploads/' + name.toLowerCase().trim().replace(' ', '_') + ".jpeg");

        //     let characters = {
        //         name: name,
        //         thumb: name.toLowerCase().trim().replace(' ', '_') + ".jpeg"
        //     };

        //     characterRepository.create(characters);
        // });
    }
}).catch((error) => console.error(error));
