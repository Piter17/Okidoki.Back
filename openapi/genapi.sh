#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
OPENAPI_FILE="Okidoki.Api.json"
CONFIG_FILE="config.json"
OUTPUT_DIR="Okidoki.Client"

# rm -f $OPENAPI_FILE
rm -rf $OUTPUT_DIR

dotnet build ../src/Okidoki.Api/Okidoki.Api.csproj -c Debug --tl:off

docker run --rm \
  --user "$(id -u):$(id -g)" \
  -v "$SCRIPT_DIR":/local \
  openapitools/openapi-generator-cli \
  generate \
    -i /local/$OPENAPI_FILE \
    -g csharp \
    -o /local/$OUTPUT_DIR \
    -c /local/$CONFIG_FILE
