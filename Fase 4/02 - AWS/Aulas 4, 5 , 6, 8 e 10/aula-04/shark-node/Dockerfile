FROM node:18-slim

RUN mkdir -p /home/node/app/node_modules 

WORKDIR /home/fiap/app

COPY package*.json ./

RUN npm install

COPY . .

EXPOSE 80

CMD [ "node", "app.js" ]