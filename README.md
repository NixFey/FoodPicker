# FoodPicker

An application to make it easier to pick what to eat from your favorite meal delivery service (only one service currently supported).

## Project Status

This project is actively in development, but is currently completely usable from the UI. There is no manual work in the backend or in the database to get it running for a standard use-case.

Ideas for future features can be found in the project's issues section.

## Project Screen Shot(s)

TODO

## Installation and Setup Instructions

This project is available as a docker image, and that is the recommended way to run it. The project can also be run as a standalone .NET Core app.

### Docker Setup

Run the docker container using a command such as the following. This will expose the HTTP port, and create volumes for appsettings as well as the database so data and configuration is persisted after stopping the container.

```bash
docker pull ghcr.io/evanlihou/foodpicker:latest
docker run --name=foodpicker --restart=unless-stopped -p 8002:80 -v /path/to/foodpicker.db:/app/app.db -v /path/to/appsettings.json:/app/appsettings.json -d ghcr.io/evanlihou/foodpicker:latest
```

Once the container is running, go to <http://localhost:8002/Auth/Register> to create your first user. This registration page is intentionally disabled after creation of the first user. See below for how to create additional users.

This container supports being behind a reverse proxy and redirecting HTTP traffic to HTTPS when behind that reverse proxy. This can be enabled by setting the following things in the `appsettings.json` file. If the host machine of the docker container is also the load balancer, the IP is likely `172.17.0.1`.

- `KnownProxies`: an array of IP addresses allowed to pass in proxy headers
- `RedirectToHttps`: a boolean of whether the app should redirect the user

## Usage

### Administering the system

When logging into the site, admin users are given the option to enter their password. If they choose not to enter their password, their administrative abilities are limited. If they do enter their password, they will be able to do things such as destructive actions and creating/editing users. Administrators can confirm which mode their account is currently in next to their name in the navigation bar.

## Reflection

TODO

<!-- README template from https://gist.github.com/martensonbj/6bf2ec2ed55f5be723415ea73c4557c4 -->