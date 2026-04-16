FROM node:25.9.0-alpine

WORKDIR /client

COPY package*.json ./

RUN npm install

COPY --chown=app:app . .

EXPOSE 5173

CMD ["npm", "run", "dev"]