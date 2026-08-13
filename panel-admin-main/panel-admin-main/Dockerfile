FROM git.faramaham.com:5050/shared/docker_repository/node:22.9.0-alpine

RUN mkdir -p /app
WORKDIR /app

COPY . /app

RUN npm config set registry https://npm.faramaham.com
RUN npm install
RUN npm run  build
#RUN npm run generate 

EXPOSE 3000


CMD [ "npm","run", "preview" ]
