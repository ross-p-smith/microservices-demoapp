#
# Build Angular app using @angular/cli
#
FROM node:8-alpine as angularapp
LABEL version="2.0.0" 
ARG angular_root="angular"

WORKDIR /build

# Install all the Angular dev tools & CLI
COPY ${angular_root}/package.json .
RUN npm install --silent

# Copy in the Angular source
COPY ${angular_root}/.angular-cli.json .
COPY ${angular_root}/tsconfig.json .
COPY ${angular_root}/src ./src

# Run Angular CLI build & bundle in prod mode, and output to ./dist
RUN node node_modules/@angular/cli/bin/ng build --prod

# ===================================================================== #

#
# Build Node.js frontend service, pulling in output from previous image
#
FROM node:8-alpine
LABEL version="2.0.0" 
ARG basedir="node/frontend"

# Node.js setup for the frontend
ENV NODE_ENV production
WORKDIR /home/app

# For efficient layer caching with NPM, this *really* speeds things up
COPY ${basedir}/package.json .

# NPM install for the server packages
RUN npm install --production --silent

# NPM is done, now copy in the the whole project to the workdir
COPY ${basedir}/ .

# Copy in Angular app, uses previous image in Dockerfile
COPY --from=angularapp /build/dist .

# Install bash inside container as it's good for debugging 
RUN apk update && apk add bash

EXPOSE 3000
CMD npm start