#!/bin/bash

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
EVALUATIONS_DIR="$(dirname "$SCRIPT_DIR")"
TESTS_DIR="$EVALUATIONS_DIR/tests"

DEFAULT_HOST="http://localhost:8080"
HOST="${HOST:-$DEFAULT_HOST}"

echo "========================================"
echo "Authorization Ownership - Black-Box Tests"
echo "========================================"
echo "Target Host: $HOST"
echo ""

cd "$TESTS_DIR"
export TEST_HOST="$HOST"

# Disable cache explicitly to avoid stale clean/buggy results.
# Run all tests in this suite by shared prefix.
go test -v -count=1 -run "^TestAuthorizationOwnership_" ./...