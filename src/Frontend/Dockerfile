FROM node:lts AS build

WORKDIR /project
ADD . .

RUN npm install
RUN npm run build

FROM nginx AS runtime

COPY --from=build /project/build /usr/share/nginx/html
