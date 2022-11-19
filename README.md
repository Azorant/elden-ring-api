# Elden Ring API
[![Docker Image Version (latest semver)](https://img.shields.io/docker/v/korrdyn/elden-ring-api)](https://hub.docker.com/r/korrdyn/elden-ring-api)
[![Codacy branch grade](https://img.shields.io/codacy/grade/46e4c7c95ebd4e9f923d3de5b5e03b3d/master)](https://app.codacy.com/gh/Korrdyn/elden-ring-api/dashboard)
![GitHub Workflow Status](https://img.shields.io/github/workflow/status/Korrdyn/elden-ring-api/Publish%20Docker%20image)
[![Discord](https://discord.com/api/guilds/918704583717572639/widget.png)](https://discord.gg/66dp9gxMZx)

This project is work in progress, more categories and other features will be added over time.

## Documentation
The documentation can be viewed at [eldenring.cubed.gg/swagger](https://eldenring.cubed.gg/swagger)

You can view the [Trello board](https://trello.com/b/DFMNpb0w) to see what's planned and in the works.

## Getting started
You will need [Docker](https://docs.docker.com/get-docker/) and [MongoDB](https://www.mongodb.com/) installed on your system.

You can get a free MongoDB instance from [MongoDB Atlas](https://www.mongodb.com/atlas/database).

```
docker run -d -p 8080:80 -e MONGO_URI=mongodb://user:pass@host --name EldenRingAPI korrdyn/elden-ring-api
```
