FROM node:22.15.0@sha256:a1f1274dadd49738bcd4cf552af43354bb781a7e9e3bc984cfeedc55aba2ddd8 AS global-dependencies

ARG UID=
ARG GID=

RUN groupmod -g $GID node && \
    usermod -u $UID -g $GID node && \
    mkdir -p /app && chown -R $UID:$GID /app

WORKDIR /app

COPY --chown=node:node package.json yarn.lock ./

USER node
