#! /bin/bash

docker compose run --rm  --build --remove-orphans  ballerina-core-frontend-dependency-installer &&
docker compose run --rm  --build --remove-orphans  ballerina-core-tsc
