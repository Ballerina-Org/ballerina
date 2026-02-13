#! /bin/bash

docker compose build --no-cache --build-arg UID=$(id -u) --build-arg GID=$(id -g) web && 
 docker compose run --rm --remove-orphans --publish 5001:5001 web
