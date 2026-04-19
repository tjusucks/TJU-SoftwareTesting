#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
APIURL="${APIURL:-http://localhost:8080/api}"

newman run "$SCRIPT_DIR/api-generated.postman_collection.json" \
  -e "$SCRIPT_DIR/api-generated.environment.json" \
  --env-var "APIURL=$APIURL" \
  --reporters cli,json \
  --reporter-json-export "$SCRIPT_DIR/api-run.json"
