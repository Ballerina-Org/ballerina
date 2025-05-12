#!/bin/bash
poetry run ruff check &&
    poetry run ruff format &&
    poetry run mypy &&
    poetry run pytest
