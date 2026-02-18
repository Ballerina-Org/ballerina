#! /bin/bash

docker compose build --no-cache ballerina-core-frontend-dependency-installer --build-arg UID=$(id -u) --build-arg GID=$(id -g)  && 
 docker compose run --rm --remove-orphans ballerina-core-frontend-dependency-installer &&
 docker compose build --no-cache ballerina-core-tsc --build-arg UID=$(id -u) --build-arg GID=$(id -g)  && 
 docker compose run --rm --remove-orphans ballerina-core-tsc
